using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rhino;
using Rhino.Geometry;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Attributes;
using System.Drawing;
using Grasshopper.GUI.Canvas;


namespace gsd {
    public class offset : Grasshopper.Kernel.GH_Component {
        public int menu_startAngle;
        public offset() : base(".offset", ".offset", "surfTM polyline offset. input centerline, output developable surface", "Extra", "surfTM") {
            if (menu_startAngle == 0) { this.Message = "Curve"; 
            }else if (menu_startAngle == 1) { this.Message = "Surface"; 
            } else if (menu_startAngle == 2) { this.Message = "Vertical"; }
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("{0D03924C-221C-4F7D-9093-009739154911}");
            }
        }
        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                //return base.Internal_Icon_24x24;
                return gsd.Properties.Resources.surfTM_offset1;
            }
        }
        private void menu_curveNormal(object sender, EventArgs e) {
            RecordUndoEvent("startAngle");
            menu_startAngle = 0;
            this.Message = "Curve";
            this.ExpireSolution(true);
        }
        private void menu_surfaceNormal(object sender, EventArgs e) {
            RecordUndoEvent("startAngle");
            menu_startAngle = 1;
            this.Message = "Surface";
            this.ExpireSolution(true);
        }
        private void menu_vertical(object sender, EventArgs e) {
            RecordUndoEvent("startAngle");
            menu_startAngle = 2;
            this.Message = "Vertical";
            this.ExpireSolution(true);
        }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu) {
            base.AppendAdditionalMenuItems(menu);

            GH_DocumentObject.Menu_AppendItem(menu, "angle 0 = Curve Normal", new EventHandler(this.menu_curveNormal),true,(menu_startAngle==0));
            GH_DocumentObject.Menu_AppendItem(menu, "angle 0 = Surface Normal", new EventHandler(this.menu_surfaceNormal), true, (menu_startAngle == 1));
            GH_DocumentObject.Menu_AppendItem(menu, "angle 0 = Vertical", new EventHandler(this.menu_vertical), true, (menu_startAngle == 2));
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager) {
            pManager.AddCurveParameter("threads", "threads", "any polyline", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddNumberParameter("widths", "widths", "offset distances are perpendicular to the centerlines", Grasshopper.Kernel.GH_ParamAccess.list, 1.0);
            pManager.AddNumberParameter("angles", "angles", "angle in degrees at the Beginning of the polyline", Grasshopper.Kernel.GH_ParamAccess.list, 0.0);
            pManager.AddBooleanParameter("center", "center", "if center is true, then the thread is treated as the centerline of the offset", Grasshopper.Kernel.GH_ParamAccess.list, true);
            //pManager.AddIntegerParameter("type", "type", "0=planar;1=normal;2=vertical", Grasshopper.Kernel.GH_ParamAccess.list,0);
            pManager[0].Optional = true;
        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_SurfaceParam("surfaces", "surfaces", "offset as Surface", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_SurfaceParam("baseSurface", "baseSurface", "baseSurface");

        }
       
        
        
        
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {
            //empty list
            List<Curve> curves = new List<Curve>();
            List<double> distanceInputs = new List<double>();
            List<double> angleInputs = new List<double>();
            List<int> typeInputs = new List<int>();
            List<Surface> outSurfaces = new List<Surface>();
            List<bool> doubleSidedInputs = new List<bool>();

            //get input from grasshopper
            if (!DA.GetDataList<Curve>(0, curves)) {
                this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Remark, "add polylines");
                return;
            }
            DA.GetDataList<double>(1, distanceInputs);
            DA.GetDataList<double>(2, angleInputs);
            DA.GetDataList<bool>(3, doubleSidedInputs);


            //check for short lists
            for (int i = 0; i < curves.Count; i++) {
                if (curves[i].IsShort(0.001)) {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "curve " + i.ToString() + " is too short"); 
                    curves.RemoveAt(i); }
                else if (menu_startAngle==0 && curves[i].IsLinear()) { 
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "curve " + i.ToString() + " has no curvature"); 
                    curves.RemoveAt(i); }
            }
            DA.Util_RemoveNullRefs<Curve>(curves);
            //curves.TrimExcess();
            Polyline[] polylines = new Polyline[curves.Count];
            double[] distances = new double[curves.Count];
            double[] angles = new double[curves.Count];
            bool[] doubleSideds = new bool[curves.Count];
            Plane[] startPlanes = new Plane[curves.Count];
            Surface nurbsSurface = null;



            for (int i = 0; i < curves.Count; i++) {
                Polyline pl;
                if (!curves[i].TryGetPolyline(out pl)) {

                    double[] ts = curves[i].DivideByCount(99, true);
                    Point3d[] pts = new Point3d[ts.Length];
                    for (int j = 0; j < ts.Length; j++) {
                        pts[j] = curves[i].PointAt(ts[j]);
                    }
                    pl = new Polyline(pts);
                    this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Remark, "item " + i.ToString() + " has been resampled with " + pts.Length.ToString() + " points");
                }
                polylines[i] = pl;
                angles[i] = angleInputs[i % angleInputs.Count];
                doubleSideds[i]  = doubleSidedInputs[i%doubleSidedInputs.Count];
                double half = 1.0;
                if (doubleSideds[i]) {  half = 0.5;   }
                distances[i] = distanceInputs[i % distanceInputs.Count] * half;
            }

            Surface surfaceForNormals = null;
            if (menu_startAngle == 1) {
                if (polylines.Length <= 1) { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "needs at least two lines for surface normals"); return; }
                nurbsSurface = makeSurface(polylines,out surfaceForNormals);
                DA.SetData(1, surfaceForNormals);
            }
            for (int i = 0; i < polylines.Length; i++){
                Plane startPlane;
                findStartAngle(polylines[i], angles[i], nurbsSurface, out startPlane);
                startPlanes[i] = startPlane;
            }


            //offset
           for (int i = 0; i < polylines.Length; i++){
                List<Surface> surfs;
                offsetPolyline3D(polylines[i], distances[i], startPlanes[i], doubleSideds[i], 1.0, out surfs);
                outSurfaces.AddRange(surfs);
			}


            //offset
            //offsetPolylines(polylines, distances, startPlanes, doubleSideds, 1.0, out outSurfaces);


            //if (menu_startAngle == 1) {
            //    Point3d[] surfacePoints = new Point3d[polylines.Length * 2];
            //    for (int i = 0; i < polylines.Length; i++) {
            //        surfacePoints[i] = polylines[i][0];
            //        surfacePoints[polylines.Length + i] = polylines[i][1];
            //    }
            //    nurbsSurface = NurbsSurface.CreateThroughPoints(surfacePoints, polylines.Length, 2, 3, 3, false, false);
            //    Vector3d[] surfaceNormals = new Vector3d[polylines.Length];
            //    for (int i = 0; i < surfaceNormals.Length; i++) {
            //        surfaceNormals[i] = nurbsSurface.n
            //    }
            //}


            //if (outSurfaces.Length < 1) { return; }
            //Vector3d[] normals = new Vector3d[outSurfaces.Length];

            //normals[0] = outSurfaces[0].NormalAt(outSurfaces[0].Domain(0).Min, outSurfaces[0].Domain(1).Min);
            //for (int i = 1; i < outSurfaces.Length; i++) {
            //    normals[i] = outSurfaces[i].NormalAt(outSurfaces[i].Domain(0).Min, outSurfaces[i].Domain(1).Min);
                
            //    if (normals[i] * normals[i - 1] < 0) {
            //        //try {
            //        //    outSurfaces[i].Reverse(0, true);
            //        //} catch { }
            //        normals[i] = outSurfaces[i].NormalAt(outSurfaces[i].Domain(0).Min, outSurfaces[i].Domain(1).Min)*-1.0;
            //    }
            //}


            DA.SetDataList(0, outSurfaces);
        }
        Surface makeSurface(Polyline[] polylines,out Surface s) {
            NurbsSurface ns = NurbsSurface.Create(3,true,0,1,2,2);
            try            {
                List<Point3d> points = new List<Point3d>();
                for (int i = 0; i < polylines.Length; i++)
                {
                    points.Add(polylines[i][0]);
                    points.Add(polylines[i][1]);
                }
            ns = NurbsSurface.CreateThroughPoints(points, polylines.Length, 2, 3, 3, false, false);
            }
            catch { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "makeSurface failed"); }
            s = ns;
            return s;
        }
        private void offsetLine(Polyline polyLine, double distance, double angle, bool doubleSided, double positive, out List<Surface> outSurfaces) {
            List<Surface> updateSurfaces = new List<Surface>();
            try
            {


                if (polyLine.Count < 2)
                {
                    outSurfaces = updateSurfaces;
                    return;
                }

                PolylineCurve plCurve = new PolylineCurve(polyLine);

                polyLine.ReduceSegments(0.001);
                if (!plCurve.IsLinear())
                {
                    outSurfaces = updateSurfaces;
                    return;
                }

                Plane startPlane;

                //find the orientation of the startPlane
                int isVertical1 = (polyLine[1] - polyLine[0]).IsParallelTo(Vector3d.ZAxis);
                if (isVertical1 >= 1)
                {
                    Vector3d lineDir = polyLine[1] - polyLine[0];
                    startPlane = Plane.WorldXY;


                    //general case for non vertical lines
                    //vertical == 0;
                }
                else
                {
                    Plane plum = new Plane(polyLine[0], polyLine[1], new Point3d(polyLine[0].X, polyLine[0].Y, polyLine[0].Z - 1.0));
                    Plane startCap = new Plane(polyLine[0], new Vector3d(polyLine[1] - polyLine[0]));
                    Line intersectionLine;
                    Rhino.Geometry.Intersect.Intersection.PlanePlane(plum, startCap, out intersectionLine);
                    Plane plane3 = new Plane(polyLine[0], intersectionLine.Direction, polyLine[0] - polyLine[1]);
                    startPlane = new Plane(polyLine[0], intersectionLine.Direction, plane3.Normal);
                }


                //add rotation
                startPlane.Rotate(angle * System.Math.PI / 180.0, startPlane.ZAxis);


                //extrude
                Surface s;
                if (doubleSided)
                {
                    Transform xform = Transform.Translation(startPlane.XAxis * -distance);
                    plCurve.Transform(xform);
                    s = Surface.CreateExtrusion(plCurve, startPlane.XAxis * distance * 2.0);
                    updateSurfaces.Add(s);
                }
                else
                {
                    s = Surface.CreateExtrusion(plCurve, startPlane.XAxis * distance * positive);
                    updateSurfaces.Add(s);
                }
            }
            catch { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "offset line failed"); }
            outSurfaces = updateSurfaces;

        }
        private void findStartAngle(Polyline polyLine, double angle, Surface surface, out Plane startPlane) {


            startPlane = new Plane();
            for (int i = 0; i < polyLine.Length; i++) {

                int count = polyLine.Count - 1;

                int k;
                //Vertical 
                if (menu_startAngle == 2) {


                    //find first non vertical vertex
                    k = 1;
                    int isVertical = (polyLine[k] - polyLine[0]).IsParallelTo(Vector3d.ZAxis);
                    while (isVertical >= 1) {
                        if (k == polyLine.Count) {
                            return;
                        }
                        isVertical = (polyLine[k] - polyLine[0]).IsParallelTo(Vector3d.ZAxis);
                        k++;
                    }
                    //make vertial start plane
                    Plane plum = new Plane(polyLine[0], polyLine[k], new Point3d(polyLine[0].X, polyLine[0].Y, polyLine[0].Z - 1.0));
                    Plane startCap = new Plane(polyLine[0], new Vector3d(polyLine[k] - polyLine[0]));
                    Line intersectionLine;
                    Rhino.Geometry.Intersect.Intersection.PlanePlane(plum, startCap, out intersectionLine);
                    Vector3d normal = Vector3d.CrossProduct(intersectionLine.Direction, polyLine[k] - polyLine[0]);
                    startPlane = new Plane(polyLine[0], intersectionLine.Direction, normal);


                    //introduce rotation
                    startPlane.Rotate(angle * Math.PI / 180.0, startPlane.ZAxis);






                    //Suface Normal
                } else if (menu_startAngle == 1) {



                        double u, v;
                        if (!polyLine[0].IsValid) { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "polyline[0] is not valid"); continue; }
                        if (!polyLine[1].IsValid) { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "polyline[1] is not valid"); continue; }

                    try
                    {
                        surface.ClosestPoint(polyLine[0], out u, out v);
                        Vector3d normal = surface.NormalAt(u, v);
                        normal.Unitize();


                        startPlane = new Plane(polyLine[0], polyLine[1] - polyLine[0]);
                        //Plane frame;
                        //surface.FrameAt(u, v, out frame);
                        //startPlane = new Plane(polyLine[0], polyLine[0] + normal, polyLine[0] + frame.XAxis);






                        Plane plum = new Plane(polyLine[0], polyLine[0] + normal, polyLine[1]);
                        Plane startCap = new Plane(polyLine[0], new Vector3d(polyLine[1] - polyLine[0]));
                        Line intersectionLine;
                        Rhino.Geometry.Intersect.Intersection.PlanePlane(plum, startCap, out intersectionLine);
                        Vector3d normal0 = Vector3d.CrossProduct(intersectionLine.Direction, polyLine[1] - polyLine[0]);
                        startPlane = new Plane(polyLine[0], intersectionLine.Direction, normal0);



                        startPlane.Rotate((angle - 90.0) * Math.PI / 180.0, startPlane.ZAxis);


                    }
                    catch {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "findStartAngle failed. try selecting the threads in order");
                    }












                    //Curve normal
                } else if (menu_startAngle == 0) {

                    try
                    {
                        //find first non colinear vertex
                        k = 2;
                        Vector3d firstVec = polyLine[1] - polyLine[0];
                        int isCoLinear = firstVec.IsParallelTo(polyLine[k] - polyLine[1]);
                        while (isCoLinear >= 1)
                        {
                            if (k == polyLine.Count)
                            {
                                return;
                            }
                            isCoLinear = firstVec.IsParallelTo(polyLine[k] - polyLine[1]);
                            k++;
                        }



                        Point3d[] points = new Point3d[3];
                        points[0] = polyLine[0];
                        points[1] = polyLine[1];
                        points[2] = polyLine[k];

                        Plane curvePlane = new Plane(polyLine[0], polyLine[1], polyLine[k]);

                        startPlane = new Plane(polyLine[0], curvePlane.YAxis, curvePlane.ZAxis);

                        startPlane.Rotate(angle * Math.PI / 180.0, startPlane.ZAxis);

                    }
                    catch {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "startAngle failed");
                    }
                }

            }
        
        }
        private void offsetPolyline3D(Polyline polyLine, double distance, Plane startAngle, bool doubleSided, double positive, out List<Surface> outSurfaces) {
            Point3d corner0;
            Point3d corner1;
            Plane[] biPlanes = new Plane[polyLine.Count];
            Point3d[] offsetPoints0 = new Point3d[polyLine.Count];
            Point3d[] offsetPoints1 = new Point3d[polyLine.Count];
            List<Surface> updateSurfaces = new List<Surface>();

            biPlanes[0] = startAngle;
            //make the end stop at the beginning
            int count = polyLine.Count - 1;
            if (polyLine[0].DistanceTo(polyLine[count]) < 0.001) {
                biPlanes[count] = biPlanes[0];
            } else {
                biPlanes[count] = new Plane(polyLine[count], new Vector3d(polyLine[count] - polyLine[count - 1]));
            }

            double minOffset = -0.001;
            if (doubleSided) { minOffset = -distance * positive; }

            corner0 = biPlanes[0].PointAt(minOffset, 0);
            corner1 = biPlanes[0].PointAt(distance * positive, 0);


            //keep track of the corner point, use it for the beginning of the next line
            offsetPoints0[0] = corner0;
            offsetPoints1[0] = corner1;



            //work on one group of points at a time
            for (int j = 2; j < polyLine.Count; ++j) {
                Point3d pt0 = polyLine[j - 2];
                Point3d pt1 = polyLine[j - 1];
                Point3d pt2 = polyLine[j];
                Vector3d line0 = pt0 - pt1;
                Vector3d line1 = pt2 - pt1;

                line0.Unitize();
                line1.Unitize();

                //test for parallel
                double dot = Math.Abs(line0 * line1);
                if (dot > 0.999) {
                    biPlanes[j - 1] = new Plane(pt1, line0);
                } else {


                    //line0 *= distance;
                    //line1 *= distance;

                    Plane triangle = new Plane(pt1, pt0, pt2);
                    Point3d normalPoint = pt1 + triangle.Normal;

                    Vector3d bisector = -line1 + -line0;
                    Vector3d bisector2 = line1 + line0;

                    biPlanes[j - 1] = new Plane(pt1, bisector, triangle.Normal);

                }



                //make corner points
                Line ray0 = new Line(corner0, line0, line0.Length * 2);

                //offset right
                double lineParameter0;
                Rhino.Geometry.Intersect.Intersection.LinePlane(ray0, biPlanes[j - 1], out lineParameter0);
                Point3d intersectPoint0 = ray0.PointAt(lineParameter0);
                offsetPoints0[j - 1] = intersectPoint0;
                corner0 = intersectPoint0;

                //offset left
                Line ray1 = new Line(corner1, line0, line0.Length * 2);
                double lineParameter1;
                Rhino.Geometry.Intersect.Intersection.LinePlane(ray1, biPlanes[j - 1], out lineParameter1);
                Point3d intersectPoint1 = ray1.PointAt(lineParameter1);
                offsetPoints1[j - 1] = intersectPoint1;
                corner1 = intersectPoint1;



            }


            //make last line segment right
            double lineParameter2;
            Vector3d vec = polyLine[polyLine.Count - 1] - polyLine[polyLine.Count - 2];
            Line ray2 = new Line(corner0, vec, vec.Length * 2);
            Rhino.Geometry.Intersect.Intersection.LinePlane(ray2, biPlanes[polyLine.Count - 1], out lineParameter2);
            Point3d intersectPoint2 = ray2.PointAt(lineParameter2);
            offsetPoints0[polyLine.Count - 1] = intersectPoint2;
            corner0 = intersectPoint2;

            //make last line segment left
            double lineParameter3;
            Vector3d vec3 = polyLine[polyLine.Count - 1] - polyLine[polyLine.Count - 2];
            Line ray3 = new Line(corner1, vec3, vec3.Length * 2);
            Rhino.Geometry.Intersect.Intersection.LinePlane(ray3, biPlanes[polyLine.Count - 1], out lineParameter3);
            Point3d intersectPoint3 = ray3.PointAt(lineParameter3);
            offsetPoints1[polyLine.Count - 1] = intersectPoint3;
            corner1 = intersectPoint3;

            //make new outlines
            PolylineCurve[] pls = new PolylineCurve[2];
            pls[0] = new PolylineCurve(offsetPoints0);
            pls[1] = new PolylineCurve(offsetPoints1);

            //make surface
            Brep[] loft = Brep.CreateFromLoft(pls, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);


            // output
            for (int m = 0; m < loft.Length; m++) {
                for (int n = 0; n < loft[m].Surfaces.Count; n++) {
                    updateSurfaces.Add(loft[m].Surfaces[n]);
                }
            }

            outSurfaces = updateSurfaces;

        }
        public void offsetPolylines(Polyline[] polyLines, double[] distances, double[] angles, bool[] doubleSideds, double positive, out Surface[] outSurfaces) {




            List<Surface> updateSurfaces = new List<Surface>();
            List<Brep> updateBreps = new List<Brep>();
            List<Point3d> updatePoints = new List<Point3d>();
            List<Point3d> updateVectorPoints = new List<Point3d>();
            List<Vector3d> updateVectors = new List<Vector3d>();
            List<Plane> updatePlanes = new List<Plane>();
            List<PolylineCurve> updatePolylines = new List<PolylineCurve>();

            //work on one polyline at a time
            for (int i = 0; i < polyLines.Length; ++i) {
                PolylineCurve plCurve = new PolylineCurve(polyLines[i]);
                //double rotateAngle;
                //rotateAngle = angle;
                //rotateAngle = angle + Math.Sin((double) i / polyLines.Count) * 180.0;

                polyLines[i].ReduceSegments(0.001);



                bool planarCurve = plCurve.IsPlanar() && angles[i] == 0;





                if (polyLines[i].Count < 2) {
                    outSurfaces = updateSurfaces.ToArray();

                } else if (plCurve.IsLinear()) {
                    //the special case if the polyline points are colinear

                    Plane startPlane;

                    //find the orientation of the startPlane
                    int isVertical1 = (polyLines[i][1] - polyLines[i][0]).IsParallelTo(Vector3d.ZAxis);
                    if (isVertical1 >= 1) {
                        Vector3d lineDir = polyLines[i][1] - polyLines[i][0];
                        Vector3d nextLine;
                        if (i == 0) {
                            nextLine = polyLines[0][0] - polyLines[1][0];
                        } else {
                            nextLine = polyLines[i][0] - polyLines[i - 1][0];
                        }
                        Vector3d startVec = Vector3d.CrossProduct(lineDir, nextLine);
                        startPlane = new Plane(polyLines[i][0], startVec, nextLine);

                        //planes.Add(startPlane);//vertical
                        //general case for non vertical lines
                    } else {
                        Plane plum = new Plane(polyLines[i][0], polyLines[i][1], new Point3d(polyLines[i][0].X, polyLines[i][0].Y, polyLines[i][0].Z - 1.0));
                        Plane startCap = new Plane(polyLines[i][0], new Vector3d(polyLines[i][1] - polyLines[i][0]));
                        Line intersectionLine;
                        Rhino.Geometry.Intersect.Intersection.PlanePlane(plum, startCap, out intersectionLine);
                        Plane plane3 = new Plane(polyLines[i][0], intersectionLine.Direction, polyLines[i][0] - polyLines[i][1]);
                        startPlane = new Plane(polyLines[i][0], intersectionLine.Direction, plane3.Normal);

                        //planes.Add(startPlane);// corners
                    }


                    //angle zero is vertical
                    startPlane.Rotate(angles[i] * Math.PI / 180.0, startPlane.ZAxis);


                    //extrude
                    Surface s;
                    if (doubleSideds[i]) {
                        Transform xform = Transform.Translation(startPlane.XAxis * -distances[i]);
                        plCurve.Transform(xform);
                        s = Surface.CreateExtrusion(plCurve, startPlane.XAxis * distances[i] * 2.0);
                        updateSurfaces.Add(s);
                    } else {
                        s = Surface.CreateExtrusion(plCurve, startPlane.XAxis * distances[i] * positive);
                        updateSurfaces.Add(s);
                    }



                    //} else if (planarCurve) {
                    //    //the special case if the polyline points are coplanar and angle is equal to zero


                    //    Plane plum = new Plane(polyLines[i][0], polyLines[i][1], new Point3d(polyLines[i][0].X, polyLines[i][0].Y, polyLines[i][0].Z - 1.0));
                    //    Plane startCap = new Plane(polyLines[i][0], new Vector3d(polyLines[i][1] + polyLines[i][0]));
                    //    Line intersectionLine;
                    //    Rhino.Geometry.Intersect.Intersection.PlanePlane(plum, startCap, out intersectionLine);
                    //    Plane plane3 = new Plane(polyLines[i][0], intersectionLine.Direction, polyLines[i][0] - polyLines[i][1]);
                    //    Plane startPlane = new Plane(polyLines[i][0], intersectionLine.Direction, plane3.Normal);




                    //    Plane plane1;

                    //    plCurve.TryGetPlane(out plane1);
                    //    //planes.Add(startPlane);//planar midpoints
                    //    Curve[] c0;
                    //    Curve[] c1;
                    //    if (doubleSided) {
                    //        c1 = plCurve.Offset(plane1, distance, 0.001, CurveOffsetCornerStyle.Sharp);
                    //        c0 = plCurve.Offset(plane1, -distance, 0.001, CurveOffsetCornerStyle.Sharp);
                    //    } else {
                    //        Point3d pt = new Point3d(polyLines[i][0] + startPlane.XAxis * positive);

                    //        c1 = plCurve.Offset(pt, plane1.Normal, distance, 0.001, CurveOffsetCornerStyle.Sharp);

                    //        //          c1 = plCurve.Offset(plane1, distance * positive, 0.001, CurveOffsetCornerStyle.Sharp);
                    //        c0 = plCurve.Offset(plane1, -0.001, 0.001, CurveOffsetCornerStyle.Sharp);
                    //    }


                    //    if (c1.Length > 1 || c0.Length > 1) {





                    //        //Surface surface = (Surface)NurbsSurface.CreateRuledSurface(destination1, destination2);
                    //        //Curve[] booleanUnion = Curve.CreateBooleanUnion((IEnumerable<Curve>)C);

                    //        //List<Curve> cs = new List<Curve>();

                    //        ////make end lines
                    //        //for (int j = 0; j < Math.Min(c0.Length, c1.Length); j++) {
                    //        //    cs.Add(new LineCurve(c0[j].PointAtStart, c1[j].PointAtStart));
                    //        //    cs.Add(new LineCurve(c0[j].PointAtEnd, c1[j].PointAtEnd));
                    //        //}

                    //        ////brep gymnastics to get a single surface
                    //        //cs.AddRange(c0);
                    //        //cs.AddRange(c1);
                    //        //Brep[] bs = Brep.CreatePlanarBreps(cs);
                    //        //Brep[] union = Brep.CreateBooleanUnion(bs, 0.001);
                    //        //for (int j = 0; j < union.Length; j++) {

                    //        //    Curve[] edges = union[0].DuplicateNakedEdgeCurves(true, true);
                    //        //    Brep[] b = Brep.CreatePlanarBreps(edges);


                    //        //    for (int k = 0; k < b.Length; k++) {
                    //        //        if (b[k].Surfaces.Count >= 1) {
                    //        //            updateSurfaces.Add(b[k].Surfaces[0]);
                    //        //        }
                    //        //    }
                    //        //}



                    //    } else if (c1.Length == 1 && c0.Length == 1) {
                    //        //the special case for a well behaved planar offset
                    //        Curve[] offsetCurves = new Curve[2];
                    //        offsetCurves[0] = c0[0];
                    //        offsetCurves[1] = c1[0];
                    //        Brep[] lofts = Brep.CreateFromLoft(offsetCurves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);
                    //        if (lofts.Length > 0 && lofts[0].Surfaces.Count > 0) {
                    //            updateSurfaces.Add(lofts[0].Surfaces[0]);
                    //        }

                    //    }









                } else {
                    //the general case for any 3D space curve
                    //empty lists
                    Plane[] biPlanes = new Plane[polyLines[i].Count];
                    Point3d[] offsetPoints0 = new Point3d[polyLines[i].Count];
                    Point3d[] offsetPoints1 = new Point3d[polyLines[i].Count];
                    Point3d corner0;
                    Point3d corner1;

                    //start caps
                    int count = polyLines[i].Count - 1;


                    int k = 1;
                    int isVertical = (polyLines[i][k] - polyLines[i][0]).IsParallelTo(Vector3d.ZAxis);
                    while (isVertical >= 1) {
                        if (k == polyLines[i].Count) {
                            //make start plane from straight lines
                            Vector3d lineDir = polyLines[i][k] - polyLines[i][0];
                            Vector3d nextLine;
                            if (i == 0) {
                                nextLine = polyLines[0][0] - polyLines[k][0];
                            } else {
                                nextLine = polyLines[i][0] - polyLines[i - 1][0];
                            }
                            Vector3d startVec = Vector3d.CrossProduct(lineDir, nextLine);
                            Plane startPlane = new Plane(polyLines[i][0], startVec, nextLine);
                            startPlane.Rotate(angles[i] * Math.PI / 180.0, startPlane.ZAxis);

                            biPlanes[0] = startPlane;
                        }
                        isVertical = (polyLines[i][k] - polyLines[i][0]).IsParallelTo(Vector3d.ZAxis);
                        k++;
                    }



                    if (k < polyLines[i].Count) {
                        Plane plum = new Plane(polyLines[i][0], polyLines[i][1], new Point3d(polyLines[i][0].X, polyLines[i][0].Y, polyLines[i][0].Z - 1.0));
                        Plane startCap = new Plane(polyLines[i][0], new Vector3d(polyLines[i][1] - polyLines[i][0]));
                        Line intersectionLine;
                        Rhino.Geometry.Intersect.Intersection.PlanePlane(plum, startCap, out intersectionLine);
                        Vector3d normal = Vector3d.CrossProduct(intersectionLine.Direction, polyLines[i][1] - polyLines[i][0]);
                        biPlanes[0] = new Plane(polyLines[i][0], intersectionLine.Direction, normal);
                    }




                    if (plCurve.IsPlanar() && angles[i] == 0) {
                        Plane plane;
                        plCurve.TryGetPlane(out plane);
                        //rotation
                        if (isVertical >= 1) { }
                        Plane plum = new Plane(polyLines[i][0], polyLines[i][1], new Point3d(polyLines[i][0].X, polyLines[i][0].Y, polyLines[i][0].Z - 1.0));
                        Plane startCap = new Plane(polyLines[i][0], plane.YAxis, plane.ZAxis);


                        //startPlane.Rotate(angles[i] * Math.PI / 180.0, startPlane.ZAxis);






                        biPlanes[0] = new Plane(polyLines[i][0], plane.YAxis, plane.ZAxis);

                        //planes.Add(normal);
                        //planes.Add(plum);
                        //planes.Add(startCap);
                        //planes.Add(biPlanes[0]);
                        //planes.Add(plane);
                    }







                    if (polyLines[i][0].DistanceTo(polyLines[i][count]) < 0.001) {
                        biPlanes[count] = biPlanes[0];
                    } else {
                        biPlanes[count] = new Plane(polyLines[i][count], new Vector3d(polyLines[i][count] - polyLines[i][count - 1]));
                    }






                    //start point
                    biPlanes[0].Rotate(angles[i] * Math.PI / 180.0, biPlanes[0].ZAxis);

                    double minOffset = -0.001;
                    if (doubleSideds[i]) { minOffset = -distances[i] * positive; }

                    corner0 = biPlanes[0].PointAt(minOffset, 0);
                    corner1 = biPlanes[0].PointAt(distances[i] * positive, 0);


                    //keep track of the corner point, use it for the beginning of the next line
                    offsetPoints0[0] = corner0;
                    offsetPoints1[0] = corner1;



                    //work on one group of points at a time
                    for (int j = 2; j < polyLines[i].Count; ++j) {
                        Point3d pt0 = polyLines[i][j - 2];
                        Point3d pt1 = polyLines[i][j - 1];
                        Point3d pt2 = polyLines[i][j];
                        Vector3d line0 = pt0 - pt1;
                        Vector3d line1 = pt2 - pt1;

                        line0.Unitize();
                        line1.Unitize();

                        //test for parallel
                        double dot = Math.Abs(line0 * line1);
                        if (dot > 0.999) {
                            biPlanes[j - 1] = new Plane(pt1, line0);
                        } else {


                            //line0 *= distance;
                            //line1 *= distance;

                            Plane triangle = new Plane(pt1, pt0, pt2);
                            Point3d normalPoint = pt1 + triangle.Normal;

                            Vector3d bisector = -line1 + -line0;
                            Vector3d bisector2 = line1 + line0;

                            biPlanes[j - 1] = new Plane(pt1, bisector, triangle.Normal);

                            //updateVectorPoints.Add(pt1);
                            //updateVectors.Add(bisector);
                            //updateVectorPoints.Add(pt1);
                            //updateVectors.Add(bisector2);

                            //updateVectorPoints.Add(pt1);
                            //updateVectorPoints.Add(pt1);
                            //updateVectors.Add(line0);
                            //updateVectors.Add(line1);
                        }



                        //make corner points
                        Line ray0 = new Line(corner0, line0, line0.Length * 2);

                        //offset right
                        double lineParameter0;
                        Rhino.Geometry.Intersect.Intersection.LinePlane(ray0, biPlanes[j - 1], out lineParameter0);
                        Point3d intersectPoint0 = ray0.PointAt(lineParameter0);
                        offsetPoints0[j - 1] = intersectPoint0;
                        corner0 = intersectPoint0;

                        //offset left
                        Line ray1 = new Line(corner1, line0, line0.Length * 2);
                        double lineParameter1;
                        Rhino.Geometry.Intersect.Intersection.LinePlane(ray1, biPlanes[j - 1], out lineParameter1);
                        Point3d intersectPoint1 = ray1.PointAt(lineParameter1);
                        offsetPoints1[j - 1] = intersectPoint1;
                        corner1 = intersectPoint1;



                    }


                    //make last line segment right
                    double lineParameter2;
                    Vector3d vec = polyLines[i][polyLines[i].Count - 1] - polyLines[i][polyLines[i].Count - 2];
                    Line ray2 = new Line(corner0, vec, vec.Length * 2);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(ray2, biPlanes[polyLines[i].Count - 1], out lineParameter2);
                    Point3d intersectPoint2 = ray2.PointAt(lineParameter2);
                    offsetPoints0[polyLines[i].Count - 1] = intersectPoint2;
                    corner0 = intersectPoint2;

                    //make last line segment left
                    double lineParameter3;
                    Vector3d vec3 = polyLines[i][polyLines[i].Count - 1] - polyLines[i][polyLines[i].Count - 2];
                    Line ray3 = new Line(corner1, vec3, vec3.Length * 2);
                    Rhino.Geometry.Intersect.Intersection.LinePlane(ray3, biPlanes[polyLines[i].Count - 1], out lineParameter3);
                    Point3d intersectPoint3 = ray3.PointAt(lineParameter3);
                    offsetPoints1[polyLines[i].Count - 1] = intersectPoint3;
                    corner1 = intersectPoint3;

                    //make new outlines
                    PolylineCurve[] pls = new PolylineCurve[2];
                    pls[0] = new PolylineCurve(offsetPoints0);
                    pls[1] = new PolylineCurve(offsetPoints1);


                    //      if (min) {
                    //        pls[0] = new PolylineCurve(polyLines[i]);
                    //      }





                    //make surface

                    Brep[] loft = Brep.CreateFromLoft(pls, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);
                    if (loft.Length == 1) {
                        updateSurfaces.Add(loft[0].Surfaces[0]);
                    }
                }


            }



            if (updateSurfaces.Count < 1) { outSurfaces = new Surface[0]; return; }
            Vector3d[] normals = new Vector3d[updateSurfaces.Count];
            try {
                if (updateSurfaces.Count > 1) {
                    updateSurfaces[0].Reverse(0);
                    normals[0] = updateSurfaces[0].NormalAt(updateSurfaces[0].Domain(0).Min, updateSurfaces[0].Domain(1).Min);
                    for (int i = 1; i < updateSurfaces.Count; i++) {
                        normals[i] = updateSurfaces[i].NormalAt(updateSurfaces[i].Domain(0).Min, updateSurfaces[i].Domain(1).Min);
                        if (normals[i] * normals[i - 1] < 0) {
                            updateSurfaces[i].Reverse(0);
                            normals[i] = updateSurfaces[i].NormalAt(updateSurfaces[i].Domain(0).Min, updateSurfaces[i].Domain(1).Min);
                        }
                    }
                }
            } catch { this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Warning,"update Rhino. some curves might be reversed"); }


            outSurfaces = updateSurfaces.ToArray();

        }
        private void offsetPolylinePlanar() { }
        public override bool Write(GH_IWriter writer) {
            writer.SetInt32("menu_startAngle", menu_startAngle);
            writer.SetString("message", this.Message);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader) {
            menu_startAngle = reader.GetInt32("menu_startAngle");
            this.Message = reader.GetString("message");
            //if (!reader.TryGetInt32("menu_startAngle", ref menu_startAngle)) { menu_startAngle = 2; }
            return base.Read(reader);
        }
        
    }



    //public class Message : GH_ComponentAttributes {
    //    private Rectangle buttonBounds;
    //    public Message(GH_Component owner)
    //        : base(owner) {

    //    }
    //    //protected override void Layout() {
    //    //    base.Layout();



    //    //    Rectangle rectangle1 = GH_Convert.ToRectangle(this.Bounds);
    //    //    rectangle1.Height += 22;
    //    //    Rectangle rectangle2 = rectangle1;
    //    //    rectangle2.Y = rectangle2.Bottom - 22;
    //    //    rectangle2.Height = 22;
    //    //    rectangle2.Inflate(-2, -2);
    //    //    this.Bounds = (RectangleF)rectangle1;
    //    //    this.buttonBounds = rectangle2;
    //    //}


    //    protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel) {
    //        //base.Attributes.RenderToCanvas(canvas, channel);
    //        base.Render(canvas, graphics, channel);
    //        if (channel != GH_CanvasChannel.Objects)
    //            return;



    //        GH_Capsule ghCapsule = GH_Capsule.CreateTextCapsule(this.buttonBounds, this.buttonBounds, GH_Palette.Hidden, Owner.Message, 2, 0);
    //        ghCapsule.Render(graphics, this.Selected, false, false);
    //        ghCapsule.Dispose();
    //    }

    //}


}
