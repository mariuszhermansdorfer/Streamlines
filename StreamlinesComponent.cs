using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Streamlines
{
    public class StreamlinesComponent : GH_Component
    {

        public StreamlinesComponent()
          : base("Streamlines", "Nickname",
            "Description",
            "Category", "Subcategory")
        {
        }

        private int _skip;
        private int _spacing;

        private PointCloud _probes;
        private List<Vector3d[]> _frames;
        private Polyline[] _streamlines;
        private List<double>[] _velocities;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("spacing", "spacing", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("skip", "skip", "", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("points", "points", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("velocities", "velocities", "", GH_ParamAccess.tree);
        }

    

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(0, ref _spacing))
                return;

            if (!DA.GetData(1, ref _skip))
                return;

            _probes = Core.CreateProbePoints(_spacing);
            var clippingBox = _probes.GetBoundingBox(false);

            _frames = new List<Vector3d[]>();
            for (int i = 0; i < 100; i++)
                _frames.Add(Core.CreateVelocityVectors(_spacing));


            _streamlines = new Polyline[_spacing * _spacing]; // TODO adapt to wind tunnel dimensions
            _velocities = new List<double>[_spacing * _spacing];


            Parallel.For(0, (_streamlines.Length - 1) / _skip, i =>
            //for (int i = 0; i < _streamlines.Length - 1; i += _skip)
            {
                i *= _skip; // Only needed for the Parallel.For loop

                // Create streamlines and assign their initial positions
                _streamlines[i] = new Polyline();
                _streamlines[i].Add(_probes[i].Location);
                _velocities[i] = new List<double>();
                _velocities[i].Add(_frames[0][i].Length);

                // Iterate over frames
                for (int j = 0; j < _frames.Count - 1; j++)
                {
                    var currentLocation = _streamlines[i].Last;
                    if (!clippingBox.Contains(currentLocation))
                        break;

                    var closestProbe = _probes.ClosestPoint(currentLocation);
                    var currentVelocity = _frames[j][closestProbe];
                    var destination = currentLocation + currentVelocity;
                    _streamlines[i].Add(destination);
                    _velocities[i].Add(currentVelocity.Length);
                }
            });

            var vertexBuffer = new DataTree<GH_Point>();
            var vertexColorBuffer = new DataTree<double>();

            for (int i = 0; i < _streamlines.Length; i++)
                if (_streamlines[i] == null)
                    continue;
                else
                {
                    GH_Path path = new GH_Path(i);
                    for (int j = 0; j < _streamlines[i].Count; j++)
                    {
                        vertexBuffer.Add(new GH_Point(_streamlines[i][j]), path);
                        vertexColorBuffer.Add(_velocities[i][j], path);
                    }
                }


            DA.SetDataTree(0, vertexBuffer);
            DA.SetDataTree(1, vertexColorBuffer);

        }

        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("2198AB5D-9B13-414E-B9E1-291F0025277D");
    }
}