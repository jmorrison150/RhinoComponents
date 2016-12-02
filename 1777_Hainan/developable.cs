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


namespace gsd
{
    //set up the name of the component
    public class DevelopableBinormal : Grasshopper.Kernel.GH_Component
    {
        //these are the names that show up in the grasshopper interface
        public DevelopableBinormal() : base(".binormal", ".binormal", "Binormal Developable", "Extra", "dev") { }

        //each id is unique to each component, and needs to be generated. google "guid generator"
        public override Guid ComponentGuid
        {
            get { return new Guid("{8b56c818-0a70-48b7-a3d0-709fc1c8d238}"); }
        }

        //get the icon
        protected override System.Drawing.Bitmap Internal_Icon_24x24
        {
            get
            {
                return gsd.Properties.Resources.binormal_icon;
            }
        }

        //input
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("curves", "curves", "input space curves", GH_ParamAccess.list);
            pManager.AddNumberParameter("offset", "offset", "offset distances", GH_ParamAccess.list, 1.0);
            pManager.AddNumberParameter("angle", "angle", "rotate angle in degrees", GH_ParamAccess.item, 0.0);
            pManager.AddIntegerParameter("resolution", "resolution", "number of divisions", GH_ParamAccess.item, 100);
            pManager.AddBooleanParameter("perp", "perp", "uses perpendicular frames", GH_ParamAccess.item, true);
            //pManager.AddBooleanParameter("varyOffset", "useCurvature", "varies the width based on curvature", GH_ParamAccess.item, false);
        }

        //output
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_BRepParam("breps", "breps", "ruling Surfaces", GH_ParamAccess.item);
            //pManager.Register_PlaneParam("planes", "planes", "planes");
        }











        //this is the section that contains all the logic
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA)
        {

            //make empty lists that will hold our geometry
            List<Curve> cvs = new List<Curve>();
            List<Curve> updateCurves = new List<Curve>();
            List<Brep> updateBreps = new List<Brep>();
            List<Point3d> updatePoints = new List<Point3d>();
            Point3d[][] allPoints;
            List<Line> updateLines = new List<Line>();
            List<double> dists = new List<double>();
            List<Plane> updatePlanes = new List<Plane>();
            double angle = 0.0;
            int divideByCount = 100;
            bool useCurvature = false;
            bool usePerpendicularFrames = true;




            //bring in the inputs
            DA.GetDataList<Curve>(0, cvs);
            Curve[] curves = cvs.ToArray();
            DA.GetDataList<double>(1, dists);
            DA.GetData<double>(2, ref angle);
            DA.GetData<int>(3, ref divideByCount);
            DA.GetData<bool>(4, ref usePerpendicularFrames);
            //DA.GetData<bool>(4, ref useCurvature);


            allPoints = new Point3d[curves.Length][];

            double[] distances = new double[curves.Length];
            double currentDistance = 1.0;
            for (int i = 0; i < curves.Length; ++i)
            {
                try
                {
                    currentDistance = dists[i];
                    distances[i] = currentDistance;
                }
                catch
                {
                    distances[i] = currentDistance;
                }
            }









            //work on one curve at a time
            //divide the curve
            //at each point, draw a ruling line based on the normal, binormal, or tangent
            //loft the lines

            //    i = Curve      j = point
            for (int i = 0; i < allPoints.Length; ++i)
            {




                //check for special cases
                bool closed = false;
                int closedInt = 1;
                if (curves[i].IsClosed || curves[i].IsEllipse() || curves[i].IsCircle()) { closed = true; closedInt = 0; } else { closed = false; closedInt = 1; }
                if (curves[i].IsArc()) { closed = false; closedInt = 1; }

                //initialize local variables
                allPoints[i] = new Point3d[divideByCount + closedInt];
                Curve[] rulingLines = new Curve[allPoints[i].Length];

                //divide the curve by count
                for (int j = 0; j < allPoints[i].Length; ++j)
                {
                    double t = (curves[i].Domain.Length / divideByCount * j) + curves[i].Domain.Min;
                    Point3d currentPoint = curves[i].PointAt(t);
                    allPoints[i][j] = currentPoint;


                    //get the reference plane
                    //this is the a convience function provided by rhino that 
                    //evaluates the curve at a point, and gives us a reference plane. 
                    //the axis of the plane correspond to normal, binormal, and tangent

                    //the axis of the plane determines whether to use the tangent, normal, or binormal
                    //plane.XAxis == Tangent
                    //plane.YAxis == Normal
                    //plane.ZAxis == Binormal

                    Plane plane;
                    if (usePerpendicularFrames)
                    {
                        curves[i].PerpendicularFrameAt(t, out plane);
                    }
                    else
                    {
                        curves[i].FrameAt(t, out plane);
                        plane = new Plane(plane.Origin, plane.ZAxis, plane.YAxis);
                    }
                    plane.Rotate((angle * Math.PI / 180.0), plane.ZAxis);
                    updatePlanes.Add(plane);

                    //provides an option for variable width ruling lines
                    //get curvature
                    double cv = 1.0;
                    if (useCurvature)
                    {
                        Vector3d curvature = curves[i].CurvatureAt(t);
                        cv = curvature.Length;
                    }





                    //draw ruling lines
                    Point3d[] pts = new Point3d[2];
                    pts[0] = new Point3d(plane.Origin + (plane.XAxis * distances[i] * cv));
                    pts[1] = new Point3d(plane.Origin + (plane.XAxis * distances[i] * cv * -1));
                    rulingLines[j] = Curve.CreateControlPointCurve(pts, 1);

                    if (j > 0 && !Curve.DoDirectionsMatch(rulingLines[j - 1], rulingLines[j]))
                    {
                        rulingLines[j].Reverse();
                    }
                }


                //loft
                Brep[] breps = Brep.CreateFromLoft(rulingLines, Point3d.Unset, Point3d.Unset, LoftType.Normal, closed);



                //check the loft to make sure they're all together
                for (int j = 1; j < breps.Length; ++j)
                {
                    if (breps != null && breps.Length > 1)
                    {
                        breps[0].Append(breps[j]);
                    }
                }
                if (breps != null && breps.Length >= 1)
                {
                    updateBreps.Add(breps[0]);
                }
            }

            //setoutputs
            DA.SetDataList(0, updateBreps);
            //DA.SetDataList(1, updatePlanes);
        }
    }
}

