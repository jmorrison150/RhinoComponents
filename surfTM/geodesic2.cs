using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;

using System;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;



namespace gsd {
    public class Geodesic : Grasshopper.Kernel.GH_Component {
        public Geodesic() : base(".geodesic angle", ".geodesic angle", "draws an incremental line on the surface. the limit of the increments draws a straight line on the surface", "Extra", "dev") { }

        public override Guid ComponentGuid {
            get { return new Guid("{7DF41F74-978A-4D0E-BF3A-6B08C3AF1334}"); }
        }
        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                return gsd.Properties.Resources.tanDev;
            }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddSurfaceParameter("surface", "surface", "base surface", GH_ParamAccess.list);
            pManager.AddPointParameter("point", "point", "start point", GH_ParamAccess.list);
            pManager.AddNumberParameter("angle", "angle", "start angle", GH_ParamAccess.list);
            pManager.AddNumberParameter("length", "length", "length of the line", GH_ParamAccess.list);


            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;


        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_CurveParam("curves", "curves", "lines that unroll straight given this surface curvature", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            List<Surface> inputSurfaces = new List<Surface>();
            List<Point3d> inputPoints = new List<Point3d>();
            List<double> inputAngles = new List<double>();
            List<double> inputLengths = new List<double>();
            List<Curve> outCurves = new List<Curve>();
            updateCurves = new List<Curve>();
            updatePoints = new List<Point3d>();


            //get info from grasshopper
            if(!DA.GetDataList<Surface>(0, inputSurfaces)) {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "no input surface. will use groundplane instead");
                Plane plane = Plane.WorldXY;
                PlaneSurface surface = new PlaneSurface(plane, new Interval(-1000, 1000), new Interval(-1000, 1000));
                inputSurfaces.Add(surface);
            }
            if(!DA.GetDataList<Point3d>(1, inputPoints)) {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "no start points. will use (0,0,0)");
                inputPoints.Add(Point3d.Origin);
            }
            if(!DA.GetDataList<double>(2, inputAngles)) {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "no start angle. will use angle = 0");
                inputAngles.Add(0.0);
            }
            if(!DA.GetDataList<double>(3, inputLengths)) {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "no length specified. will use length = 100");
                inputLengths.Add(100.0);
            }


            //check for jagged arrays
            // angles.Length = lengths.Length = points.Length
            double[] angles = new double[inputPoints.Count];
            double[] lengths = new double[inputPoints.Count];
            for(int i = 0; i < inputPoints.Count; i++) {
                angles[i] = inputAngles[i%inputAngles.Count];
                lengths[i] = inputLengths[i % inputLengths.Count];
            }
            Surface[] surfaces = inputSurfaces.ToArray();




            //geodesic
            for(int i = 0; i < inputPoints.Count; i++) {
                Curve[] curves = geodesic(surfaces, inputPoints[i], angles[i], lengths[i]);
                outCurves.AddRange(curves);
            }


            //output
            DA.SetDataList(0, outCurves);
        }

        //empty lists
        List<Curve> updateCurves;
        List<Point3d> updatePoints;
        double uPrevious, vPrevious;
        Point3d pointPrevious;
        Brep allSurfaces;
        ComponentIndex ci;
        Vector3d normal;
        double maximumDistance = 100.0;
        int indexPrevious;
        Plane startFrame;
        Vector3d direction;
        Curve[] edges;

        Curve[] geodesic(Surface[] surfaces, Point3d point, double angle, double length) {

            //test all surfaces
            Brep[] brepSurfaces = new Brep[surfaces.Length];

            //combine all surfaces together
            for(int i = 0; i < brepSurfaces.Length; i++) { brepSurfaces[i] = surfaces[i].ToBrep(); }
            allSurfaces = Brep.MergeBreps(brepSurfaces, 0.001);
            edges = allSurfaces.DuplicateEdgeCurves(true);

            //point on surface
            allSurfaces.ClosestPoint(point, out pointPrevious, out ci, out uPrevious, out vPrevious, maximumDistance, out normal);
            indexPrevious = ci.Index;

            allSurfaces.Surfaces[indexPrevious].FrameAt(uPrevious, vPrevious, out startFrame);
            direction = startFrame.XAxis;
            direction.Rotate(Math.PI * ( angle + 90 ) / 180.0, startFrame.ZAxis);


            //get new points along the way
            for(int i = 0; i < length; ++i) {
                nextPoint();
            }
            return Curve.JoinCurves(updateCurves);
        }

        void nextPoint() {
            //empty list
            Point3d pointCurrent;
            ComponentIndex ciCurrent;
            int indexCurrent;
            double uCurrent, vCurrent;
            Vector3d normalCurrent;


            //make new point
            Point3d testCurrent = pointPrevious + direction;
            allSurfaces.ClosestPoint(testCurrent, out pointCurrent, out ciCurrent, out uCurrent, out vCurrent, 100.0, out normalCurrent);
            for(int j = 0; j < edges.Length; j++) {
                double t;
                bool onEdge = edges[j].ClosestPoint(pointCurrent, out t, 0.5);
                if(onEdge) {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "edge reached");
                    return;
                }
            }

            indexCurrent = ciCurrent.Index;

            //get normal
            Point3d normalPt = pointCurrent + normalCurrent;
            Plane osculatingPlane = new Plane(pointCurrent, pointPrevious, normalPt);

            //draw curve
            Curve currentPath;
            if(indexPrevious == indexCurrent) {
                //make a short path on the surface
                currentPath = allSurfaces.Surfaces[indexCurrent].ShortPath(new Point2d(uPrevious, vPrevious), new Point2d(uCurrent, vCurrent), 0.001);
                updateCurves.Add(currentPath);
            } else {


                //make two short paths, one on each surface
                double u1, v1, u2, v2;

                allSurfaces.Surfaces[indexPrevious].ClosestPoint(testCurrent, out u1, out v1);
                Point3d edgePoint = allSurfaces.Surfaces[indexPrevious].PointAt(u1, v1);
                allSurfaces.Surfaces[indexCurrent].ClosestPoint(edgePoint, out u2, out v2);


                Point2d pt0 = new Point2d(uPrevious, vPrevious);
                Point2d pt1 = new Point2d(u1, v1);
                Point2d pt2 = new Point2d(u2, v2);
                Point2d pt3 = new Point2d(uCurrent, vCurrent);


                Curve curvePrevious = allSurfaces.Surfaces[indexPrevious].ShortPath(pt0, pt1, 0.001);
                Curve curveCurrent = allSurfaces.Surfaces[indexCurrent].ShortPath(pt2, pt2, 0.001);

                updateCurves.Add(curvePrevious);
                updateCurves.Add(curveCurrent);

            }

            //update
            //updatePoints.Add(pointCurrent);
            direction = -osculatingPlane.XAxis;
            indexPrevious = indexCurrent;
            pointPrevious = pointCurrent;
            uPrevious = uCurrent;
            vPrevious = vCurrent;

            //TODO: reflect at trim curves
            //TODO insert curvature anticipation



            //allSurfaces.Surfaces[index].ClosestPoint(point, out uNew, out vNew);
            bool maxU = ( uCurrent >= allSurfaces.Surfaces[indexCurrent].Domain(0).Max );
            bool minU = ( uCurrent <= allSurfaces.Surfaces[indexCurrent].Domain(0).Min );
            bool maxV = ( vCurrent >= allSurfaces.Surfaces[indexCurrent].Domain(1).Max );
            bool minV = ( vCurrent <= allSurfaces.Surfaces[indexCurrent].Domain(1).Min );
            if(maxU || minU || maxV || minV) { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "domain min or max reached"); }



        }
    }
}