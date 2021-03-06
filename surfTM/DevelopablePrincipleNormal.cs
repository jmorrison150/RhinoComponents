﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
////using System.Threading.Tasks;

//using Rhino;
//using Rhino.Geometry;
//using Grasshopper.Kernel;


//namespace gsd {
//    public class DevelopablePrincipleNormal : Grasshopper.Kernel.GH_Component {
//        public DevelopablePrincipleNormal() : base(".Rectified Developable", ".Rectified Developable", "Principle Normal Developable", "Extra", "dev") { }

//        public override Guid ComponentGuid {
//            get { return new Guid("{65953886-ad61-4648-a9a1-35e52a23e01f}"); }
//        }

//        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
//            get {
//                return gsd.Properties.Resources.norm_icon;
//            }
//        }

//        protected override void RegisterInputParams(GH_InputParamManager pManager) {
//            pManager.AddCurveParameter("curves", "curves", "input space curves", GH_ParamAccess.list);
//            pManager.AddNumberParameter("distances", "distances", "offset distances", GH_ParamAccess.list, 10.0);
//            pManager.AddIntegerParameter("resolution", "resolution", "number of divisions", GH_ParamAccess.item, 100);
//            pManager.AddBooleanParameter("useCurvature", "useCurvature", "varies the width based on curvature", GH_ParamAccess.item,false);
//        }

//        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
//            pManager.Register_CurveParam("outCurves", "outCurves", "ruling lines", GH_ParamAccess.item);
//            pManager.Register_BRepParam("outBreps", "outBreps", "ruling Surfaces", GH_ParamAccess.item);
//            //pManager.Register_StringParam("debug", "debug", "debug");
//            //pManager.Register_SurfaceParam("outSurfaces", "outSurfaces", "Binormal Developable Surface", GH_ParamAccess.item);

//        }
//        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {


//            //lists
//            List<Curve> cvs = new List<Curve>();
//            DA.GetDataList<Curve>(0, cvs);
//            Curve[] curves = cvs.ToArray();
//            List<Curve> updateCurves = new List<Curve>();
//            List<Brep> updateBreps = new List<Brep>();
//            List<Point3d> updatePoints = new List<Point3d>();
//            Point3d[][] allPoints = new Point3d[curves.Length][];
//            List<Line> updateLines = new List<Line>();






//            //input
//            List<double> dists = new List<double>();
//            DA.GetDataList<double>(1, dists);
//            double[] distances = new double[curves.Length];
//            double currentDistance = 1.0;
//            for (int i = 0; i < curves.Length; ++i) {
//                try {
//                    currentDistance = dists[i];
//                    distances[i] = currentDistance;
//                } catch {
//                    distances[i] = currentDistance;
//                }
//            }

//            int divideByCount = 100;
//            DA.GetData<int>(2, ref divideByCount);

//            bool useCurvature = false;
//            DA.GetData<bool>(3,ref useCurvature);






//            //    i = Curve      j = point
//            for (int i = 0; i < allPoints.Length; ++i) {

//                //check for special cases
//                //needs a special case for circle and ellipse
//                //circle normal = circle
//                //circle offset
//                //make surface
//                bool closed = false;
//                int closedInt = 1;
//                if (curves[i].IsClosed || curves[i].IsEllipse() || curves[i].IsCircle()) { closed = true; closedInt = 0; } else { closed = false; closedInt = 1; }
//                if (curves[i].IsArc()) { closed = false; closedInt = 1; }
//                //check for polyline

//                //initialize point array
//                allPoints[i] = new Point3d[divideByCount + closedInt];
//                Curve[] rulingLines = new Curve[allPoints[i].Length];

//                //operate on a single curve at a time
//                for (int j = 0; j < allPoints[i].Length; ++j) {
//                    //divide by count
//                    double t = (curves[i].Domain.Length / divideByCount * j) + curves[i].Domain.Min;
//                    Point3d currentPoint = curves[i].PointAt(t);
//                    allPoints[i][j] = currentPoint;


//                    //get the reference plane
//                    Plane plane;
//                    curves[i].FrameAt(t, out plane);
//                    updatePoints.Add(currentPoint);

//                    //get curvature
//                    double cv = 1.0;
//                    if (useCurvature) {
//                    Vector3d curvature = curves[i].CurvatureAt(t);
//                    cv = curvature.Length;
//                    }


//                    //draw ruling lines
//                    Point3d[] pts = new Point3d[2];
//                    pts[0] = new Point3d(plane.Origin + (plane.YAxis * distances[i] * cv));
//                    pts[1] = new Point3d(plane.Origin + (plane.YAxis * distances[i] * cv * -1));
//                    rulingLines[j] = Curve.CreateControlPointCurve(pts, 1);

//                    updateLines.Add(new Line(pts[0], pts[1]));
//                }

//                Brep[] breps = Brep.CreateFromLoft(rulingLines, Point3d.Unset, Point3d.Unset, LoftType.Normal, closed);
//                //debugging += breps.Length.ToString();


//                for (int j = 1; j < breps.Length; ++j) {
//                    if (breps != null && breps.Length > 1) {
//                        breps[0].Append(breps[j]);
//                    }
//                }

//                if (breps != null && breps.Length >= 1) {
//                    updateBreps.Add(breps[0]);
//                }


//            }


//            DA.SetDataList(0, updateLines);
//            DA.SetDataList(1, updateBreps);
//            //DA.SetData(2, debugging);
//        }


//    }
//}
