using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace gsd {
    public class move:Grasshopper.Kernel.GH_Component {

        public move() : base(".move", ".move", "move geomtry down the y axis", "Extra", "surfTM") { }
        public override Guid ComponentGuid {
            get {
                return new Guid("{B9D2FB9B-6511-4E74-A364-621A7DFCA313}");
            }
        }
        protected override System.Drawing.Bitmap Icon {
            get {
                return gsd.Properties.Resources.gsd09;
            }
        }
        public override Grasshopper.Kernel.GH_Exposure Exposure {
            get {
                return Grasshopper.Kernel.GH_Exposure.tertiary;
            }
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager) {
            pManager.AddSurfaceParameter("surfaces", "surfaces", "surfaces", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddCurveParameter("curves", "curves", "curves", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddPointParameter("points", "points", "points", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddCurveParameter("text", "text", "text", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddNumberParameter("y", "y", "y", Grasshopper.Kernel.GH_ParamAccess.item, -100.0);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_SurfaceParam("surfaces", "surfaces", "surfaces",Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_CurveParam("curves", "curves", "curves",Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_PointParam("points", "points", "points", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_CurveParam("text", "text", "text", Grasshopper.Kernel.GH_ParamAccess.list);
        }


        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {
            List<Surface> inputSurfaces = new List<Surface>();
            List<Curve> inputCurves = new List<Curve>();
            List<Point3d> inputPoints = new List<Point3d>();
            List<Curve> inputText = new List<Curve>();
            double y = -100.0;


            DA.GetDataList<Surface>(0, inputSurfaces);
            DA.GetDataList<Curve>(1, inputCurves);
            DA.GetDataList<Point3d>(2, inputPoints);
            DA.GetDataList<Curve>(3, inputText);
            DA.GetData<double>(4, ref y);

            Transform moveY = Transform.Translation(Vector3d.YAxis * y);

            foreach (Surface s in inputSurfaces) { s.Transform(moveY); }
            foreach (Curve c in inputCurves) { c.Transform(moveY); }
            for (int i = 0; i < inputPoints.Count; i++) {
                inputPoints[i] = new Point3d(inputPoints[i].X, inputPoints[i].Y + y, inputPoints[i].Z);    }
            foreach (Curve c in inputText) { c.Transform(moveY); }


            DA.SetDataList(0, inputSurfaces);
            DA.SetDataList(1, inputCurves);
            DA.SetDataList(2, inputPoints);
            DA.SetDataList(3, inputText);


        }
    }
}
