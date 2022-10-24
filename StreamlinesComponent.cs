using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private int _thickness = 2;
        private BoundingBox _clippingBox;
        private PointCloud _probes;
        private List<Vector3d[]> _frames;
        private Polyline[] _streamlines;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("spacing", "spacing", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("skip", "skip", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("thickness", "thickness", "", GH_ParamAccess.item, _thickness);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("sdf", "sdf", "", GH_ParamAccess.item);
        }

        protected override void BeforeSolveInstance()
        {
            _clippingBox = BoundingBox.Empty;
        }
    

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(0, ref _spacing))
                return;

            if (!DA.GetData(1, ref _skip))
                return;

            DA.GetData(2, ref _thickness);

            _probes = Core.CreateProbePoints(_spacing);
            _clippingBox.Union(_probes.GetBoundingBox(false));

            _frames = new List<Vector3d[]>();
            for (int i = 0; i < 100; i++)
                _frames.Add(Core.CreateVelocityVectors(_spacing));


            _streamlines = new Polyline[_spacing * _spacing]; // TODO adapt to wind tunnel dimensions

            Parallel.For(0, (_streamlines.Length - 1) / _skip, i =>
            // for (int i = 0; i < _streamlines.Length - 1; i += _skip)
            {
                i *= _skip; //Only needed for the Parallel.For loop

                // Create streamlines and assign their initial positions
                _streamlines[i] = new Polyline();
                _streamlines[i].Add(_probes[i].Location);

                // Iterate over frames
                for (int j = 0; j < _frames.Count - 1; j++)
                {
                    var currentLocation = _streamlines[i].Last;
                    if (!_clippingBox.Contains(currentLocation))
                        break;

                    var closestProbe = _probes.ClosestPoint(currentLocation);
                    var currentVelocity = _frames[j][closestProbe];
                    var destination = currentLocation + currentVelocity;
                    _streamlines[i].Add(destination);
                }
            });

            DA.SetData(0, new Point3d(-10, -10, -10));
            
        }

        public override BoundingBox ClippingBox
        {
            get { return _clippingBox; }
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            if (_probes == null)
                return;

            foreach (var polyline in _streamlines)
                args.Display.DrawPolyline(polyline, System.Drawing.Color.Aquamarine, _thickness);


            //args.Display.DrawPointCloud(_probes, (float)_size);

            //for (int i = 0; i < _probes.Count; i++)
            //    args.Display.DrawDirectionArrow(_probes[i].Location, _frames[0][i], System.Drawing.Color.Red);
        }

        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("2198AB5D-9B13-414E-B9E1-291F0025277D");
    }
}