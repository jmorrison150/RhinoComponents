//John Morrison 2014
//the top section sets up the grasshopper component display, inputs and outputs
//the "SolveInstance" contains the logic
//compiled with microsoft visual studio


//import the .dll libraries
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;


namespace geometry_lab {
    //set up the name of the component
    public class PlanarUnroll : Grasshopper.Kernel.GH_Component {
        //these are the names that show up in the grasshopper interface
        public PlanarUnroll() : base(".Planar Unroll", ".Planar Unroll", "Takes Planar Groups and Puts them on the Ground", "Extra", "dev") { }

        //each id is unique to each component, and needs to be generated. google "guid generator"
        public override Guid ComponentGuid {
            get { return new Guid("{AB86E489-3C4E-4C64-B4EB-156F6F312AC3}"); }
        }

        //get the icon
        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                return geometry_lab.Properties.Resources.square;
            }
        }

        //input
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddGroupParameter("groups", "groups", "3D Planar Groups", GH_ParamAccess.tree);
        }

        //output
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_GeometryParam("geometry", "geometry", "output on the Ground XY Plane",GH_ParamAccess.list);
        }











        //this is the section that contains all the logic of the component
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {

            //make empty lists that will hold our geometry
            List<Curve> cvs = new List<Curve>();
            List<Curve> updateCurves = new List<Curve>();
            List<Brep> updateBreps = new List<Brep>();
            List<Point3d> updatePoints = new List<Point3d>();
            Point3d[][] allPoints;
            List<Line> updateLines = new List<Line>();




            //bring in the inputs
            DA.GetDataList<Curve>(0, cvs);
            Curve[] curves = cvs.ToArray();

            int divideByCount = 100;
            DA.GetData<int>(2, ref divideByCount);

            bool useCurvature = false;
            DA.GetData<bool>(3, ref useCurvature);


            List<double> dists = new List<double>();
            DA.GetDataList<double>(1, dists);


            allPoints = new Point3d[curves.Length][];

            double[] distances = new double[curves.Length];
            double currentDistance = 1.0;
            for (int i = 0; i < curves.Length; ++i) {
                try {
                    currentDistance = dists[i];
                    distances[i] = currentDistance;
                } catch {
                    distances[i] = currentDistance;
                }
            }








            //work on one curve at a time
            //divide the curve
            //at each point, draw a ruling line based on the normal, binormal, or tangent
            //loft the lines

            //    i = Curve      j = point
            for (int i = 0; i < allPoints.Length; ++i) {

                //check for special cases
                bool closed = false;
                int closedInt = 1;
                if (curves[i].IsClosed || curves[i].IsEllipse() || curves[i].IsCircle()) { closed = true; closedInt = 0; } else { closed = false; closedInt = 1; }
                if (curves[i].IsArc()) { closed = false; closedInt = 1; }

                //initialize local variables
                allPoints[i] = new Point3d[divideByCount + closedInt];
                Curve[] rulingLines = new Curve[allPoints[i].Length];

                //divide the curve by count
                for (int j = 0; j < allPoints[i].Length; ++j) {
                    double t = (curves[i].Domain.Length / divideByCount * j) + curves[i].Domain.Min;
                    Point3d currentPoint = curves[i].PointAt(t);
                    allPoints[i][j] = currentPoint;


                    //get the reference plane
                    //this is the a convience function provided by rhino that 
                    //evaluates the curve at a point, and gives us a reference plane. 
                    //the axis of the plane correspond to normal, binormal, and tangent
                    Plane plane;
                    curves[i].FrameAt(t, out plane);
                    updatePoints.Add(currentPoint);


                    //provides an option for variable width ruling lines
                    //get curvature
                    double cv = 1.0;
                    if (useCurvature) {
                        Vector3d curvature = curves[i].CurvatureAt(t);
                        cv = curvature.Length;
                    }




                    //draw ruling lines
                    //the axis of the plane determines whether to use the tangent, normal, or binormal
                    //plane.XAxis == Tangent
                    //plane.YAxis == Normal
                    //plane.ZAxis == Binormal

                    Point3d[] pts = new Point3d[2];
                    pts[0] = new Point3d(plane.Origin + (plane.ZAxis * distances[i] * cv));
                    pts[1] = new Point3d(plane.Origin + (plane.ZAxis * distances[i] * cv * -1));
                    rulingLines[j] = Curve.CreateControlPointCurve(pts, 1);
                    updateLines.Add(new Line(pts[0], pts[1]));
                }


                //loft
                Brep[] breps = Brep.CreateFromLoft(rulingLines, Point3d.Unset, Point3d.Unset, LoftType.Normal, closed);

                //check the loft to make sure they're all together
                for (int j = 1; j < breps.Length; ++j) {
                    if (breps != null && breps.Length > 1) {
                        breps[0].Append(breps[j]);
                    }
                }
                if (breps != null && breps.Length >= 1) {
                    updateBreps.Add(breps[0]);
                }
            }

            //setoutputs
            DA.SetDataList(0, updateLines);
            DA.SetDataList(1, updateBreps);
        }
    }
}
