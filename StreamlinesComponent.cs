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
        private Box _selectionBox;
        private bool _reset;
        
        private RTree _tree;
        private PointCloud _probes;
        private List<Vector3d[]> _frames;
        private List<int> _startingPointsIds;
        private Polyline[] _streamlines;
        private List<double>[] _velocityMagnitudes;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("spacing", "spacing", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("skip", "skip", "", GH_ParamAccess.item);
            pManager.AddBoxParameter("selectionBox", "selectionBox", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("reset", "reset", "", GH_ParamAccess.item, _reset);
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

            if (!DA.GetData(2, ref _selectionBox))
                return;

            DA.GetData(3, ref _reset);

            if (_reset)
            {
                _tree = null;
                GH_Document grasshopperDocument = OnPingDocument();
                List<IGH_DocumentObject> componentList = new List<IGH_DocumentObject>();

                var thumbnail = new ImagePreview();

                string path = "C:\\Users\\MRHE\\OneDrive - Ramboll\\Desktop\\DEV\\Template\\210506_GS_Showcase_embedded_files\\Lawn.png";
                thumbnail.AddVolatileData(new GH_Path(0), 0, new GH_String(path));

                thumbnail.CreateAttributes();
                thumbnail.Attributes.Pivot = new System.Drawing.PointF(Attributes.Pivot.X + 200, Attributes.Pivot.Y);
                thumbnail.Attributes.ExpireLayout();
                thumbnail.Attributes.PerformLayout();
                
                componentList.Add(thumbnail);

                foreach (var component in componentList)
                    grasshopperDocument.AddObject(component, false);

                grasshopperDocument.UndoUtil.RecordAddObjectEvent("Add image previews", componentList);

            }
                

            _probes = Core.CreateProbePoints(_spacing);
            if (_tree == null)
                _tree = RTree.CreatePointCloudTree(_probes);

            var clippingBox = _probes.GetBoundingBox(false);

            _frames = new List<Vector3d[]>();
            for (int i = 0; i < 100; i++)
                _frames.Add(Core.CreateVelocityVectors(_spacing));


            _startingPointsIds = new List<int>();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            _tree.Search(_selectionBox.BoundingBox, SearchCallback);
            Rhino.RhinoApp.WriteLine($"Search {sw.ElapsedMilliseconds} ms");
            sw.Stop();

            _streamlines = new Polyline[_startingPointsIds.Count];
            _velocityMagnitudes = new List<double>[_startingPointsIds.Count];

            for (int i = 0; i < _startingPointsIds.Count; i++)
            {
                int currentPoint = _startingPointsIds[i];
                _streamlines[i] = new Polyline();
                _streamlines[i].Add(_probes[currentPoint].Location);

                _velocityMagnitudes[i] = new List<double>();
                _velocityMagnitudes[i].Add(_frames[0][currentPoint].Length);
            }



            sw.Restart();
            Parallel.For(0, (_streamlines.Length - 1) / _skip, i =>
            //for (int i = 0; i < _streamlines.Length - 1; i += _skip)
            {
                i *= _skip; // Only needed for the Parallel.For loop

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
                    _velocityMagnitudes[i].Add(currentVelocity.Length);
                }
            });
            Rhino.RhinoApp.WriteLine($"Parallel For {sw.ElapsedMilliseconds} ms");
            var vertexBuffer = new DataTree<GH_Point>();
            var vertexColorBuffer = new DataTree<double>();
            
            sw.Restart();

            for (int i = 0; i < _streamlines.Length; i++)
                if (_streamlines[i] == null)
                    continue;
                else
                {
                    GH_Path path = new GH_Path(i);
                    for (int j = 0; j < _streamlines[i].Count; j++)
                    {
                        vertexBuffer.Add(new GH_Point(_streamlines[i][j]), path);
                        vertexColorBuffer.Add(_velocityMagnitudes[i][j], path);
                    }
                }
            Rhino.RhinoApp.WriteLine($"For {sw.ElapsedMilliseconds} ms");

            DA.SetDataTree(0, vertexBuffer);
            DA.SetDataTree(1, vertexColorBuffer);

        }

        private void SearchCallback(object sender, RTreeEventArgs e)
        {
            _startingPointsIds.Add(e.Id);
        }
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("2198AB5D-9B13-414E-B9E1-291F0025277D");
    }
}