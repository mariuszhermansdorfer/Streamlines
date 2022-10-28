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
    public class ReadProbes : GH_Component
    {

        public ReadProbes()
          : base("Streamlines", "Nickname",
            "Description",
            "Category", "Subcategory")
        {
        }

        private bool _run;

        private string _path;

        private RTree _tree;
        private PointCloud _probes;
        private List<Vector3d[]> _frames;
        private List<int> _startingPointsIds;
        private Polyline[] _streamlines;
        private List<double>[] _velocityMagnitudes;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("filePath", "filePath", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("run", "run", "", GH_ParamAccess.item, _run);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("points", "points", "", GH_ParamAccess.tree);
            pManager.AddNumberParameter("velocities", "velocities", "", GH_ParamAccess.tree);
        }



        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetData(0, ref _path))
                return;

            DA.GetData(1, ref _run);

            if (!_run)
                return;

            var test = Core.ReadProbeLocations(_path);
        }

        private void SearchCallback(object sender, RTreeEventArgs e)
        {
            _startingPointsIds.Add(e.Id);
        }
        protected override System.Drawing.Bitmap Icon => null;


        public override Guid ComponentGuid => new Guid("2198AB5D-9B13-616E-B9E1-291F0025277D");
    }
}