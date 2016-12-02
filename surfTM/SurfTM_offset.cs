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
     public class SurfTM_offset : Grasshopper.Kernel.GH_Component {
         public int menu_startAngle;
         public int counter;
         bool reverse0 = false;
         bool reverse1 = false;

        public SurfTM_offset() : base(".surfTM_offset", ".surfTM_offset", "offsets polylines,creates notches, unrolls, and bakes into groups", "Extra", "surfTM") {
            if (menu_startAngle == 0) {
                this.Message = "Curve";
            } else if (menu_startAngle == 1) {
                this.Message = "Surface";
            } else if (menu_startAngle == 2) { this.Message = "Vertical"; }
            counter = 0;
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("{8D77D2D9-E648-49D0-9D42-6B445C76706A}");
            }
        }
        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                //return base.Internal_Icon_24x24;
                return gsd.Properties.Resources.surfTM_offset2;
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

            GH_DocumentObject.Menu_AppendItem(menu, "angle 0 = Curve Normal", new EventHandler(this.menu_curveNormal), true, (menu_startAngle == 0));
            GH_DocumentObject.Menu_AppendItem(menu, "angle 0 = Surface Normal", new EventHandler(this.menu_surfaceNormal), true, (menu_startAngle == 1));
            GH_DocumentObject.Menu_AppendItem(menu, "angle 0 = Vertical", new EventHandler(this.menu_vertical), true, (menu_startAngle == 2));
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager) {
            //[0]
            pManager.AddCurveParameter("iThreads", "iThreads", "must be polylines, any polyline will do", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddNumberParameter("iWidths", "iWidths", "offset width of iThreads, can be a single number or a list", Grasshopper.Kernel.GH_ParamAccess.list,1.0);
            pManager.AddNumberParameter("iAngles", "iAngles", "0=planar if possible, if not, 0=vertical, 90=horizontal. angle is measured from the start of the polyline", Grasshopper.Kernel.GH_ParamAccess.list, 0);
            pManager.AddBooleanParameter("", "", "change notch side default is false", Grasshopper.Kernel.GH_ParamAccess.item, false);
            //[4]
            pManager.AddCurveParameter("jThreads", "jThreads", "must be polylines, any polyline will do", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddNumberParameter("jWidths", "jWidths", "offset width of jThreads, can be a single number or a list", Grasshopper.Kernel.GH_ParamAccess.list,1.0);
            pManager.AddNumberParameter("jAngles", "jAngles", "0=planar if possible, if not, 0=vertical, 90=horizontal. angle is measured from the start of the polyline", Grasshopper.Kernel.GH_ParamAccess.list, 0);
            pManager.AddBooleanParameter("", "", "change notch side default is false", Grasshopper.Kernel.GH_ParamAccess.item,false);
            //[8]
            pManager.AddNumberParameter("thickness", "thickness", "notch thickness", Grasshopper.Kernel.GH_ParamAccess.list, 0.250);
            pManager.AddBooleanParameter("unroll", "unroll", "this is slow. turn off to increase speed.", Grasshopper.Kernel.GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("bake", "bake", "creates rhino layers 'a_Score' and 'a_Cut1' - Control with Boolean Button", Grasshopper.Kernel.GH_ParamAccess.item, false);

            pManager[0].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[7].Optional = true;
        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_SurfaceParam("outSurfaces", "outSurfaces", "offsets surfaces with width, but no thickness", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_CurveParam("outCurves", "outCurves", "unrolled curves", Grasshopper.Kernel.GH_ParamAccess.list);
            //pManager.Register_PointParam("outPoints", "outPoints", "intersetion Points", Grasshopper.Kernel.GH_ParamAccess.list);
            //pManager.Register_BRepParam("outBreps", "outBreps", "solids", Grasshopper.Kernel.GH_ParamAccess.list);
        }
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {
           //empty lists
            List<Plane> planes = new List<Plane>();
           List<Point3d> points = new List<Point3d>();
           List<Curve> curves0 = new List<Curve>();
           List<Curve> curves1 = new List<Curve>();
           List<double> widthInputs0 = new List<double>();
           List<double> angleInputs0 = new List<double>();
           List<double> widthInputs1 = new List<double>();
           List<double> angleInputs1 = new List<double>();
           List<double> thicknessInputs = new List<double>();
           Surface[] iSurfaces;
           Surface[] jSurfaces;
           List<Surface> updateSurfaces = new List<Surface>();
           List<Curve> updateCurves = new List<Curve>();
           List<Point3d> updatePoints = new List<Point3d>();
           List<Brep> updateBreps = new List<Brep>();
           Surface[] iWholes;
           Surface[] jWholes;
           Surface[] iHalves;
           Surface[] jHalves;
           Curve[][] iNotches;
           Curve[][] jNotches;
           List<Surface> updateUnrollSurfaces = new List<Surface>();
           List<Curve> updateUnrollCurves = new List<Curve>();
           List<Point3d> updateUnrollPoints = new List<Point3d>();
           List<Surface> unSurfaces;
           List<Curve> unCurves;
           List<Point3d> unPoints;
   
          
            
            //set variables
           //to autoBake: comment out minY=0.0; set unrollBake =true; and right click animate a slider
           minY = 0.0;
           bool unroll = false;
           bool unrollBake = false;






           //get input from grasshopper
           DA.GetDataList<Curve>(0, curves0);
           DA.GetDataList<Curve>(4, curves1);
           DA.GetDataList<double>(1, widthInputs0);
           DA.GetDataList<double>(2, angleInputs0);
           DA.GetDataList<double>(5, widthInputs1);
           DA.GetDataList<double>(6, angleInputs1);
           DA.GetDataList<double>(8, thicknessInputs);
           DA.GetData<bool>(9, ref unroll);
           DA.GetData<bool>(10, ref unrollBake);
           DA.GetData<bool>(3, ref reverse0);
           DA.GetData<bool>(7, ref reverse1);

           //check for short lists
            DA.Util_RemoveNullRefs<Curve>(curves0);
            DA.Util_RemoveNullRefs<Curve>(curves1);


           Polyline[] iThreads = new Polyline[curves0.Count];
           Polyline[] jThreads = new Polyline[curves1.Count];
           double[] widths0 = new double[curves0.Count];
           double[] widths1 = new double[curves1.Count];
           double[] angles0 = new double[curves0.Count];
           double[] angles1 = new double[curves1.Count];
           int[] types0 = new int[curves0.Count];
           int[] types1 = new int[curves1.Count];
           bool[] doubleSideds0 = new bool[curves0.Count];
           bool[] doubleSideds1 = new bool[curves1.Count];
           bool[] singleSideds0 = new bool[curves0.Count];
           bool[] singleSideds1 = new bool[curves1.Count];
           double[] thicknesses = new double[2];

            //populate lists. loop if lengths don't match.
           for (int i = 0; i < curves0.Count; i++) {
               Polyline pl0;
               if (!curves0[i].TryGetPolyline(out pl0)) {

                   double[] ts = curves0[i].DivideByCount(99, true);
                   Point3d[] pts = new Point3d[ts.Length];
                   for (int j = 0; j < ts.Length; j++) {
                       pts[j] = curves0[i].PointAt(ts[j]);
                   }
                   pl0 = new Polyline(pts);
                   this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Remark, "item " + i.ToString() + " has been resampled with " + pts.Length.ToString() + " points");
               } 
               iThreads[i] = pl0;
               widths0[i] = widthInputs0[i % widthInputs0.Count]*0.5;
               angles0[i] = angleInputs0[i % angleInputs0.Count];
               doubleSideds0[i] = true;
               singleSideds0[i] = false;
               if (doubleSideds0[i]) {
                   thicknesses[0] = thicknessInputs[0] * 0.5;
               } else { 
                   thicknesses[0] = thicknessInputs[0]; 
               }
           }




           for (int i = 0; i < curves1.Count; i++) {
               Polyline pl1;
               if (!curves1[i].TryGetPolyline(out pl1)) {

                   double[] ts = curves1[i].DivideByCount(99, true);
                   Point3d[] pts = new Point3d[ts.Length];
                   for (int j = 0; j < ts.Length; j++) {
                       pts[j] = curves1[i].PointAt(ts[j]);
                   }
                   pl1 = new Polyline(pts);
                   this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Remark, "item " + i.ToString() + " has been resampled with " + pts.Length.ToString() + " points");
               } jThreads[i] = pl1;
               widths1[i] = widthInputs1[i % widthInputs1.Count]*0.5;
               angles1[i] = angleInputs1[i % angleInputs1.Count];
               doubleSideds1[i] = true;
               singleSideds1[i] = false;
               if (doubleSideds1[i]) {
                   thicknesses[1] = thicknessInputs[1 % thicknessInputs.Count] * 0.5;
               } else {
                   thicknesses[1] = thicknessInputs[1 % thicknessInputs.Count];
               }
           }



            //logic
            //offset//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            offsetPolylines(iThreads, widths0, angles0, doubleSideds0, 1.0, out iSurfaces);
            offsetPolylines(jThreads, widths1, angles1, doubleSideds1, 1.0, out jSurfaces);

            updateSurfaces.AddRange(iSurfaces);
            updateSurfaces.AddRange(jSurfaces);

            //intersect///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (unroll || unrollBake) {


                double[] notchWidths0 = new double[widths0.Length];
                double[] notchWidths1 = new double[widths1.Length];


                for (int i = 0; i < widths0.Length; i++) {                    notchWidths0[i] = widths0[i] * 2.1;                }
                for (int i = 0; i < widths1.Length; i++) {                    notchWidths1[i] = widths1[i] * 2.1;                }

                double side0 = 1.0; if (reverse0) { side0 = -1.0; }
                double side1 = 1.0; if (reverse1) { side1 = -1.0; }

                offsetPolylines(jThreads, notchWidths1, angles1, singleSideds1, side0, out jHalves);
                offsetPolylines(iThreads, notchWidths0, angles0, doubleSideds0, 1.0, out iWholes);

                offsetPolylines(iThreads, notchWidths0, angles0, singleSideds0, side1, out iHalves);
                offsetPolylines(jThreads, notchWidths1, angles1, doubleSideds1, 1.0, out jWholes);

                Brep[] jHalvesSolid = offsetSurfaces(jHalves, thicknesses[1]);
                Brep[] iHalvesSolid = offsetSurfaces(iHalves, thicknesses[0]);

//                Brep[] iNotched = trim(iWholes, jHalvesSolid, thicknesses[0]);



                //17.8


                intersect(iSurfaces, jHalvesSolid, out iNotches);
                intersect(jSurfaces, iHalvesSolid, out jNotches);


                //32.6



                
//                intersect(jSurfaces, iNotched, out jNotches);


                for (int i = 0; i < iNotches.Length; i++) {                    updateCurves.AddRange(iNotches[i]);                }
               for (int i = 0; i < jNotches.Length; i++) {                    updateCurves.AddRange(jNotches[i]);                }






            //unroll////////////////////////////////////////////////////////////////////////////////////////////////////////////


               unrollSrfs(iSurfaces.ToArray(), iThreads.ToArray(), iNotches, widths0, unrollBake, out unSurfaces, out unCurves, out unPoints);

               updateSurfaces.AddRange(unSurfaces);
               updateCurves.AddRange(unCurves);
               updatePoints.AddRange(unPoints);

               unSurfaces.Clear();
               unCurves.Clear();
               unPoints.Clear();

               unrollSrfs(jSurfaces.ToArray(), jThreads.ToArray(), jNotches, widths1, unrollBake, out unSurfaces, out unCurves, out unPoints);

               updateSurfaces.AddRange(unSurfaces);
               updateCurves.AddRange(unCurves);
               updatePoints.AddRange(unPoints);
            }



            //output
            DA.SetDataList(0, updateSurfaces);
            DA.SetDataList(1, updateCurves);


        }

     
        #region customCode
        Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
        double minY = 0.0;

        //public void offsetPolylines(Polyline[] polyLines, double[] distances, double[] angles, bool[] doubleSideds, double positive, out Surface[] outSurfaces) {




        //    List<Surface> updateSurfaces = new List<Surface>();
        //    List<Brep> updateBreps = new List<Brep>();
        //    List<Point3d> updatePoints = new List<Point3d>();
        //    List<Point3d> updateVectorPoints = new List<Point3d>();
        //    List<Vector3d> updateVectors = new List<Vector3d>();
        //    List<Plane> updatePlanes = new List<Plane>();
        //    List<PolylineCurve> updatePolylines = new List<PolylineCurve>();

        //    //work on one polyline at a time
        //    for (int i = 0; i < polyLines.Length; ++i) {
        //        PolylineCurve plCurve = new PolylineCurve(polyLines[i]);
        //        //double rotateAngle;
        //        //rotateAngle = angle;
        //        //rotateAngle = angle + Math.Sin((double) i / polyLines.Count) * 180.0;

        //        polyLines[i].ReduceSegments(0.001);



        //        bool planarCurve = plCurve.IsPlanar() && angles[i] == 0;





        //        if (polyLines[i].Count < 2) {
        //            outSurfaces = updateSurfaces.ToArray();

        //        } else if (plCurve.IsLinear()) {
        //            //the special case if the polyline points are colinear

        //            Plane startPlane;

        //            //find the orientation of the startPlane
        //            int isVertical1 = (polyLines[i][1] - polyLines[i][0]).IsParallelTo(Vector3d.ZAxis);
        //            if (isVertical1 >= 1) {
        //                Vector3d lineDir = polyLines[i][1] - polyLines[i][0];
        //                Vector3d nextLine;
        //                if (i == 0) {
        //                    nextLine = polyLines[0][0] - polyLines[1][0];
        //                } else {
        //                    nextLine = polyLines[i][0] - polyLines[i - 1][0];
        //                }
        //                Vector3d startVec = Vector3d.CrossProduct(lineDir, nextLine);
        //                startPlane = new Plane(polyLines[i][0], startVec, nextLine);

        //                //planes.Add(startPlane);//vertical
        //                //general case for non vertical lines
        //            } else {
        //                Plane plum = new Plane(polyLines[i][0], polyLines[i][1], new Point3d(polyLines[i][0].X, polyLines[i][0].Y, polyLines[i][0].Z - 1.0));
        //                Plane startCap = new Plane(polyLines[i][0], new Vector3d(polyLines[i][1] - polyLines[i][0]));
        //                Line intersectionLine;
        //                Rhino.Geometry.Intersect.Intersection.PlanePlane(plum, startCap, out intersectionLine);
        //                Plane plane3 = new Plane(polyLines[i][0], intersectionLine.Direction, polyLines[i][0] - polyLines[i][1]);
        //                startPlane = new Plane(polyLines[i][0], intersectionLine.Direction, plane3.Normal);

        //                //planes.Add(startPlane);// corners
        //            }


        //            //angle zero is vertical
        //            startPlane.Rotate(angles[i] * Math.PI / 180.0, startPlane.ZAxis);


        //            //extrude
        //            Surface s;
        //            if (doubleSideds[i]) {
        //                Transform xform = Transform.Translation(startPlane.XAxis * -distances[i]);
        //                plCurve.Transform(xform);
        //                s = Surface.CreateExtrusion(plCurve, startPlane.XAxis * distances[i] * 2.0);
        //                updateSurfaces.Add(s);
        //            } else {
        //                s = Surface.CreateExtrusion(plCurve, startPlane.XAxis * distances[i] * positive);
        //                updateSurfaces.Add(s);
        //            }



        //            //} else if (planarCurve) {
        //            //    //the special case if the polyline points are coplanar and angle is equal to zero


        //            //    Plane plum = new Plane(polyLines[i][0], polyLines[i][1], new Point3d(polyLines[i][0].X, polyLines[i][0].Y, polyLines[i][0].Z - 1.0));
        //            //    Plane startCap = new Plane(polyLines[i][0], new Vector3d(polyLines[i][1] + polyLines[i][0]));
        //            //    Line intersectionLine;
        //            //    Rhino.Geometry.Intersect.Intersection.PlanePlane(plum, startCap, out intersectionLine);
        //            //    Plane plane3 = new Plane(polyLines[i][0], intersectionLine.Direction, polyLines[i][0] - polyLines[i][1]);
        //            //    Plane startPlane = new Plane(polyLines[i][0], intersectionLine.Direction, plane3.Normal);




        //            //    Plane plane1;

        //            //    plCurve.TryGetPlane(out plane1);
        //            //    //planes.Add(startPlane);//planar midpoints
        //            //    Curve[] c0;
        //            //    Curve[] c1;
        //            //    if (doubleSided) {
        //            //        c1 = plCurve.Offset(plane1, distance, 0.001, CurveOffsetCornerStyle.Sharp);
        //            //        c0 = plCurve.Offset(plane1, -distance, 0.001, CurveOffsetCornerStyle.Sharp);
        //            //    } else {
        //            //        Point3d pt = new Point3d(polyLines[i][0] + startPlane.XAxis * positive);

        //            //        c1 = plCurve.Offset(pt, plane1.Normal, distance, 0.001, CurveOffsetCornerStyle.Sharp);

        //            //        //          c1 = plCurve.Offset(plane1, distance * positive, 0.001, CurveOffsetCornerStyle.Sharp);
        //            //        c0 = plCurve.Offset(plane1, -0.001, 0.001, CurveOffsetCornerStyle.Sharp);
        //            //    }


        //            //    if (c1.Length > 1 || c0.Length > 1) {





        //            //        //Surface surface = (Surface)NurbsSurface.CreateRuledSurface(destination1, destination2);
        //            //        //Curve[] booleanUnion = Curve.CreateBooleanUnion((IEnumerable<Curve>)C);

        //            //        //List<Curve> cs = new List<Curve>();

        //            //        ////make end lines
        //            //        //for (int j = 0; j < Math.Min(c0.Length, c1.Length); j++) {
        //            //        //    cs.Add(new LineCurve(c0[j].PointAtStart, c1[j].PointAtStart));
        //            //        //    cs.Add(new LineCurve(c0[j].PointAtEnd, c1[j].PointAtEnd));
        //            //        //}

        //            //        ////brep gymnastics to get a single surface
        //            //        //cs.AddRange(c0);
        //            //        //cs.AddRange(c1);
        //            //        //Brep[] bs = Brep.CreatePlanarBreps(cs);
        //            //        //Brep[] union = Brep.CreateBooleanUnion(bs, 0.001);
        //            //        //for (int j = 0; j < union.Length; j++) {

        //            //        //    Curve[] edges = union[0].DuplicateNakedEdgeCurves(true, true);
        //            //        //    Brep[] b = Brep.CreatePlanarBreps(edges);


        //            //        //    for (int k = 0; k < b.Length; k++) {
        //            //        //        if (b[k].Surfaces.Count >= 1) {
        //            //        //            updateSurfaces.Add(b[k].Surfaces[0]);
        //            //        //        }
        //            //        //    }
        //            //        //}



        //            //    } else if (c1.Length == 1 && c0.Length == 1) {
        //            //        //the special case for a well behaved planar offset
        //            //        Curve[] offsetCurves = new Curve[2];
        //            //        offsetCurves[0] = c0[0];
        //            //        offsetCurves[1] = c1[0];
        //            //        Brep[] lofts = Brep.CreateFromLoft(offsetCurves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);
        //            //        if (lofts.Length > 0 && lofts[0].Surfaces.Count > 0) {
        //            //            updateSurfaces.Add(lofts[0].Surfaces[0]);
        //            //        }

        //            //    }









        //        } else {
        //            //the general case for any 3D space curve
        //            //empty lists
        //            Plane[] biPlanes = new Plane[polyLines[i].Count];
        //            Point3d[] offsetPoints0 = new Point3d[polyLines[i].Count];
        //            Point3d[] offsetPoints1 = new Point3d[polyLines[i].Count];
        //            Point3d corner0;
        //            Point3d corner1;

        //            //start caps
        //            int count = polyLines[i].Count - 1;


        //            int k = 1;
        //            int isVertical = (polyLines[i][k] - polyLines[i][0]).IsParallelTo(Vector3d.ZAxis);
        //            while (isVertical >= 1) {
        //                if (k == polyLines[i].Count) {
        //                    //make start plane from straight lines
        //                    Vector3d lineDir = polyLines[i][k] - polyLines[i][0];
        //                    Vector3d nextLine;
        //                    if (i == 0) {
        //                        nextLine = polyLines[0][0] - polyLines[k][0];
        //                    } else {
        //                        nextLine = polyLines[i][0] - polyLines[i - 1][0];
        //                    }
        //                    Vector3d startVec = Vector3d.CrossProduct(lineDir, nextLine);
        //                    Plane startPlane = new Plane(polyLines[i][0], startVec, nextLine);
        //                    startPlane.Rotate(angles[i] * Math.PI / 180.0, startPlane.ZAxis);

        //                    biPlanes[0] = startPlane;
        //                }
        //                isVertical = (polyLines[i][k] - polyLines[i][0]).IsParallelTo(Vector3d.ZAxis);
        //                k++;
        //            }



        //            if (k < polyLines[i].Count) {
        //                Plane plum = new Plane(polyLines[i][0], polyLines[i][1], new Point3d(polyLines[i][0].X, polyLines[i][0].Y, polyLines[i][0].Z - 1.0));
        //                Plane startCap = new Plane(polyLines[i][0], new Vector3d(polyLines[i][1] - polyLines[i][0]));
        //                Line intersectionLine;
        //                Rhino.Geometry.Intersect.Intersection.PlanePlane(plum, startCap, out intersectionLine);
        //                Vector3d normal = Vector3d.CrossProduct(intersectionLine.Direction, polyLines[i][1] - polyLines[i][0]);
        //                biPlanes[0] = new Plane(polyLines[i][0], intersectionLine.Direction, normal);
        //            }




        //            if (plCurve.IsPlanar() && angles[i] == 0) {
        //                Plane plane;
        //                plCurve.TryGetPlane(out plane);
        //                //rotation
        //                if (isVertical >= 1) { }
        //                Plane plum = new Plane(polyLines[i][0], polyLines[i][1], new Point3d(polyLines[i][0].X, polyLines[i][0].Y, polyLines[i][0].Z - 1.0));
        //                Plane startCap = new Plane(polyLines[i][0], plane.YAxis, plane.ZAxis);


        //                //startPlane.Rotate(angles[i] * Math.PI / 180.0, startPlane.ZAxis);






        //                biPlanes[0] = new Plane(polyLines[i][0], plane.YAxis, plane.ZAxis);

        //                //planes.Add(normal);
        //                //planes.Add(plum);
        //                //planes.Add(startCap);
        //                //planes.Add(biPlanes[0]);
        //                //planes.Add(plane);
        //            }







        //            if (polyLines[i][0].DistanceTo(polyLines[i][count]) < 0.001) {
        //                biPlanes[count] = biPlanes[0];
        //            } else {
        //                biPlanes[count] = new Plane(polyLines[i][count], new Vector3d(polyLines[i][count] - polyLines[i][count - 1]));
        //            }






        //            //start point
        //            biPlanes[0].Rotate(angles[i] * Math.PI / 180.0, biPlanes[0].ZAxis);

        //            double minOffset = -0.001;
        //            if (doubleSideds[i]) { minOffset = -distances[i] * positive; }

        //            corner0 = biPlanes[0].PointAt(minOffset, 0);
        //            corner1 = biPlanes[0].PointAt(distances[i] * positive, 0);


        //            //keep track of the corner point, use it for the beginning of the next line
        //            offsetPoints0[0] = corner0;
        //            offsetPoints1[0] = corner1;



        //            //work on one group of points at a time
        //            for (int j = 2; j < polyLines[i].Count; ++j) {
        //                Point3d pt0 = polyLines[i][j - 2];
        //                Point3d pt1 = polyLines[i][j - 1];
        //                Point3d pt2 = polyLines[i][j];
        //                Vector3d line0 = pt0 - pt1;
        //                Vector3d line1 = pt2 - pt1;

        //                line0.Unitize();
        //                line1.Unitize();

        //                //test for parallel
        //                double dot = Math.Abs(line0 * line1);
        //                if (dot > 0.999) {
        //                    biPlanes[j - 1] = new Plane(pt1, line0);
        //                } else {


        //                    //line0 *= distance;
        //                    //line1 *= distance;

        //                    Plane triangle = new Plane(pt1, pt0, pt2);
        //                    Point3d normalPoint = pt1 + triangle.Normal;

        //                    Vector3d bisector = -line1 + -line0;
        //                    Vector3d bisector2 = line1 + line0;

        //                    biPlanes[j - 1] = new Plane(pt1, bisector, triangle.Normal);

        //                    //updateVectorPoints.Add(pt1);
        //                    //updateVectors.Add(bisector);
        //                    //updateVectorPoints.Add(pt1);
        //                    //updateVectors.Add(bisector2);

        //                    //updateVectorPoints.Add(pt1);
        //                    //updateVectorPoints.Add(pt1);
        //                    //updateVectors.Add(line0);
        //                    //updateVectors.Add(line1);
        //                }



        //                //make corner points
        //                Line ray0 = new Line(corner0, line0, line0.Length * 2);

        //                //offset right
        //                double lineParameter0;
        //                Rhino.Geometry.Intersect.Intersection.LinePlane(ray0, biPlanes[j - 1], out lineParameter0);
        //                Point3d intersectPoint0 = ray0.PointAt(lineParameter0);
        //                offsetPoints0[j - 1] = intersectPoint0;
        //                corner0 = intersectPoint0;

        //                //offset left
        //                Line ray1 = new Line(corner1, line0, line0.Length * 2);
        //                double lineParameter1;
        //                Rhino.Geometry.Intersect.Intersection.LinePlane(ray1, biPlanes[j - 1], out lineParameter1);
        //                Point3d intersectPoint1 = ray1.PointAt(lineParameter1);
        //                offsetPoints1[j - 1] = intersectPoint1;
        //                corner1 = intersectPoint1;



        //            }


        //            //make last line segment right
        //            double lineParameter2;
        //            Vector3d vec = polyLines[i][polyLines[i].Count - 1] - polyLines[i][polyLines[i].Count - 2];
        //            Line ray2 = new Line(corner0, vec, vec.Length * 2);
        //            Rhino.Geometry.Intersect.Intersection.LinePlane(ray2, biPlanes[polyLines[i].Count - 1], out lineParameter2);
        //            Point3d intersectPoint2 = ray2.PointAt(lineParameter2);
        //            offsetPoints0[polyLines[i].Count - 1] = intersectPoint2;
        //            corner0 = intersectPoint2;

        //            //make last line segment left
        //            double lineParameter3;
        //            Vector3d vec3 = polyLines[i][polyLines[i].Count - 1] - polyLines[i][polyLines[i].Count - 2];
        //            Line ray3 = new Line(corner1, vec3, vec3.Length * 2);
        //            Rhino.Geometry.Intersect.Intersection.LinePlane(ray3, biPlanes[polyLines[i].Count - 1], out lineParameter3);
        //            Point3d intersectPoint3 = ray3.PointAt(lineParameter3);
        //            offsetPoints1[polyLines[i].Count - 1] = intersectPoint3;
        //            corner1 = intersectPoint3;

        //            //make new outlines
        //            PolylineCurve[] pls = new PolylineCurve[2];
        //            pls[0] = new PolylineCurve(offsetPoints0);
        //            pls[1] = new PolylineCurve(offsetPoints1);


        //            //      if (min) {
        //            //        pls[0] = new PolylineCurve(polyLines[i]);
        //            //      }





        //            //make surface

        //            Brep[] loft = Brep.CreateFromLoft(pls, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);
        //            if (loft.Length == 1) {
        //                updateSurfaces.Add(loft[0].Surfaces[0]);
        //            }
        //        }


        //    }



        //    if (updateSurfaces.Count < 1) { outSurfaces = new Surface[0]; return; }
        //    Vector3d[] normals = new Vector3d[updateSurfaces.Count];
        //    try{
        //    if (updateSurfaces.Count > 1) {
        //        updateSurfaces[0].Reverse(0, true);
        //        normals[0] = updateSurfaces[0].NormalAt(updateSurfaces[0].Domain(0).Min, updateSurfaces[0].Domain(1).Min);
        //        for (int i = 1; i < updateSurfaces.Count; i++) {
        //            normals[i] = updateSurfaces[i].NormalAt(updateSurfaces[i].Domain(0).Min, updateSurfaces[i].Domain(1).Min);
        //            if (normals[i] * normals[i - 1] < 0) {
        //                updateSurfaces[i].Reverse(0, true);
        //                normals[i] = updateSurfaces[i].NormalAt(updateSurfaces[i].Domain(0).Min, updateSurfaces[i].Domain(1).Min);
        //            }
        //        }
        //    }
        //    } catch { this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Warning, "update Rhino. some curves might be reversed"); }


        //    outSurfaces = updateSurfaces.ToArray();

        //}
 
        private void intersect(Surface[] iSurfaces, Brep[] jHalves, out Curve[][] outNotches) {




            Curve[][] notches = new Curve[iSurfaces.Length][];
            for (int i = 0; i < iSurfaces.Length; ++i) {
                List<Curve> updateCurves1 = new List<Curve>();


                //if startpoint.DistaneTo(endpoint)<thickness
                //split intersectionCurves using startplane


                for (int j = 0; j < jHalves.Length; ++j) {
                    List<Curve> intCurves = new List<Curve>();


                        Curve[] intersectionCurves;
                        Point3d[] intersectionPoints;

                        //intersect
                        Rhino.Geometry.Intersect.Intersection.BrepSurface(jHalves[j], iSurfaces[i], 0.001, out intersectionCurves, out intersectionPoints);
                        updateCurves1.AddRange(intersectionCurves);

                    
                }
                notches[i] = updateCurves1.ToArray();
            }

            outNotches = notches;



        }
        private void unrollSrfs(Surface[] surfaces, Polyline[] iThreads, Curve[][] curves, double[] widths, bool unrollBake, out List<Surface> updateSurfaces, out List<Curve> updateCurves, out List<Point3d> updatePoints) {
            //Point3d[][] points = null;
            updateSurfaces = new List<Surface>();
            updateCurves = new List<Curve>();
            updatePoints = new List<Point3d>();
            double maxX = 0.0;
            double minX = 0.0;
            double maxY = 0.0;



            //work on one strip at a time
            for (int i = 0; i < surfaces.Length; i++) {


                int groupIndex = -1;
                groupIndex = doc.Groups.Add();
                //Bake(obj, att, groupIndex);




                Point3d[] pts = new Point3d[iThreads[i].Count];
                for (int j = 0; j < iThreads[i].Count; j++) {
                    pts[j] = iThreads[i][j];
                }


                //List<Curve> unrolledOutline = new List<Curve>();
                Curve[] unrolledCurves;
                Point3d[] unrolledPoints;
                TextDot[] unrolledDots;
                Curve[] unrolledText; 
                Point3d textPoint = Point3d.Origin;
                List<Curve> allText = new List<Curve>();



                //make text in 3d
                string text = i.ToString();
                Plane drawingPlane;
                surfaces[i].FrameAt(surfaces[i].Domain(0).Min,surfaces[i].Domain(1).Min,out drawingPlane);
                
                double width;
                try {                    width = widths[i];
                } catch {                    width = 1.0;                }

                drawingPlane.Translate(drawingPlane.YAxis * width );
                PolylineCurve[] text3d;
                drawingPlane.Flip();
                drawingPlane.YAxis *= -1;
                
                singleLineFont(text, drawingPlane, width * 0.75, out text3d);
                updateCurves.AddRange(text3d);


                //Plane flat = Plane.WorldXY;
                //flat.Translate(flat.YAxis * width);
                //for (int j = 0; j < text3d.Length; j++) {
                //    text3d[j].Transform(Transform.PlaneToPlane(drawingPlane, flat));
                //}



                //unroll
                Unroller unText = new Unroller(surfaces[i]);
                unText.ExplodeOutput = false;
                unText.ExplodeSpacing = 1.0;

                unText.AddFollowingGeometry(text3d);
                Brep[] unrollText = unText.PerformUnroll(out unrolledText, out unrolledPoints, out unrolledDots);

                allText.AddRange(unrolledText);



                Unroller un = new Unroller(surfaces[i]);

                un.AddFollowingGeometry(curves[i]);
                un.AddFollowingGeometry(pts);

                Brep[] unrolledBreps = un.PerformUnroll(out unrolledCurves, out unrolledPoints, out unrolledDots);
                
                //unroll


                //label text flat

                if (unrolledBreps[0].Vertices[0] != null) {
                    textPoint = unrolledBreps[0].Vertices[0].Location;
                }










                ////convert to surfaces wireframe
                //for (int j = 0; j < unrolledBreps.Length; j++) {

                //    //Curve[] curves1 = unrolledBreps[j].DuplicateNakedEdgeCurves(true, true);
                //    //unrolledOutline.AddRange(curves1);

                //    Mesh[] m = Mesh.CreateFromBrep(unrolledBreps[j], MeshingParameters.Smooth);
                //    for (int k = 0; k < m.Length; k++) {
                //        Polyline[] curves5 = m[k].GetOutlines(Plane.WorldXY);
                //        for (int l = 0; l < curves5.Length; l++) {
                //            PolylineCurve curves6 = new PolylineCurve(curves5[l]);
                //            unrolledOutline.Add(curves6);
                //        }
                //    }
                //}


                //find BoundingBox.Max.X
                double bBox = 0.0;
                for (int j = 0; j < unrolledBreps.Length; j++) {
                    BoundingBox bb = unrolledBreps[j].GetBoundingBox(false);
                    if (bBox < bb.Max.X) {
                        bBox = bb.Max.X;
                    }
                    if (maxY < bb.Max.Y) {
                        maxY = bb.Max.Y;
                    }
                }
                if (maxX < bBox) {
                    maxX = bBox;
                }

                //move
                for (int j = 0; j < unrolledBreps.Length; j++) {
                    unrolledBreps[j].Translate(-maxX + minX, 0, 0);
                }

                //for (int j = 0; j < unrolledOutline.Count; j++) {
                //    unrolledOutline[j].Translate(-maxX + minX, 0, 0);
                //}
                for (int j = 0; j < unrolledCurves.Length; j++) {
                    unrolledCurves[j].Translate(-maxX + minX, 0, 0);
                }
                for (int j = 0; j < unrolledPoints.Length; j++) {
                    unrolledPoints[j].X += (-maxX + minX);
                }
                for (int j = 0; j < unrolledDots.Length; j++) {
                    unrolledDots[j].Translate(-maxX + minX, 0, 0);
                }
                for (int j = 0; j < unrolledText.Length; j++) {
                    unrolledText[j].Translate(-maxX + minX,0,0);
                }
                textPoint.X += (-maxX + minX);

                //update global box
                minX -= maxX;
                maxX = 0.0;


                //move Y

                for (int j = 0; j < unrolledBreps.Length; j++) {
                    unrolledBreps[j].Translate(0, -maxY + minY, 0);
                }
                //for (int j = 0; j < unrolledOutline.Count; j++) {
                //    unrolledOutline[j].Translate(0, -maxY + minY, 0);
                //}
                for (int j = 0; j < unrolledCurves.Length; j++) {
                    unrolledCurves[j].Translate(0, -maxY + minY, 0);
                }
                for (int j = 0; j < unrolledPoints.Length; j++) {
                    unrolledPoints[j].Y += (-maxY + minY);
                }
                for (int j = 0; j < unrolledDots.Length; j++) {
                    unrolledDots[j].Translate(0, -maxX + minX, 0);
                }
                for (int j = 0; j < unrolledText.Length; j++) {
                    unrolledText[j].Translate(0, -maxY + minY, 0);
                }
                textPoint.Y += (-maxY + minY);


                //backup labels
                if (unrolledText.Length < 1) {

                    textPoint.X += width * 0.25;
                    textPoint.Y += width * 2.0;
                    PolylineCurve[] singleLineText;

                    singleLineFont(text, textPoint, width * 0.75, out singleLineText);
                    allText.AddRange(singleLineText);
                }


                //remove unrolls across the seam
                List<Curve> curves7 = new List<Curve>();
                for (int j = 0; j < unrolledCurves.Length; j++) {
                    if (unrolledCurves[j].GetLength() < width * 4.0) {
                        curves7.Add(unrolledCurves[j]);
                    }
                }


                List<Surface> unSurf = new List<Surface>();
                for (int j = 0; j < unrolledBreps.Length; j++) {
                    unSurf.Add(unrolledBreps[j].Surfaces[0]);
                }



                System.Drawing.Color green = System.Drawing.Color.FromArgb(0, 255, 0);

                bakery(allText, "a_Score", green, groupIndex, unrollBake);
                bakery(curves7, "a_Cut1", System.Drawing.Color.Blue, groupIndex, unrollBake);
                bakery(unSurf, "a_Cut2", System.Drawing.Color.Magenta, groupIndex, unrollBake);



                updateCurves.AddRange(allText);
                updateCurves.AddRange(curves7);
                updateSurfaces.AddRange(unSurf);


            }
            minY -= maxY;
            maxY = 0.0;





        }
        private Brep[] offsetSurfaces(Surface[] jHalves, double thickness) {

            //Surface[][] jHalvesOffset = new Surface[jHalves.Count][];
            Brep[] jHalvesSolid = new Brep[jHalves.Length];

            for (int i = 0; i < jHalves.Length; i++) {

                Brep solid = Brep.CreateFromOffsetFace(jHalves[i].ToBrep().Faces[0], thickness, 0.001, true, true);
                jHalvesSolid[i] = solid;
                //jHalvesOffset[i] = solid.Surfaces.ToArray();

            }
            return jHalvesSolid;

        }
        private void singleLineFont(string text, Point3d location, double height, out PolylineCurve[] singleLineText) {

            //makes a font instance copy
            string font = "Machine Tool SanSerif";
            double precision = 50;
            Plane plane = new Plane(location, Vector3d.ZAxis);
            System.Drawing.Font localFont;
            try {
                localFont = new System.Drawing.Font(font, (float)height);
            } catch {
                //Print("Cannot Find Machine Tool SanSerif");
                font = "Arial";
                localFont = new System.Drawing.Font(font, (float)height);
            }

            //Makes a graphics path object instance
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddString(text, localFont.FontFamily, (int)localFont.Style, localFont.Size, new System.Drawing.PointF(0, 0), new System.Drawing.StringFormat());

            //This is a transformation matrix.
            System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
            matrix.Reset();
            //this basically turns the path into a polyline that approximates the path
            path.Flatten(matrix, (float)(height / precision));

            //extracts the points from the path
            System.Drawing.PointF[] pts = path.PathPoints;
            byte[] tps = path.PathTypes;

            //empty list of polylines
            List<Polyline> strokes = new List<Polyline>();
            Polyline stroke = null;

            //finds start point
            byte typStart = System.Convert.ToByte(System.Drawing.Drawing2D.PathPointType.Start);

            int i = -1;
            while (true) {
                i++;


                //when the stroke has i number of points in it, add it to the list and exit the while loop
                if (i >= pts.Length) {
                    if (stroke != null && stroke.Count > 1) {
                        strokes.Add(stroke);
                    }
                    break;
                }



                //if this is the start, then add the line and start a new one
                if (tps[i] == typStart) {
                    if (stroke != null && stroke.Count > 1) {
                        strokes.Add(stroke);
                    }
                    stroke = new Polyline();
                    stroke.Add(pts[i].X, -pts[i].Y, 0);
                } else {
                    stroke.Add(pts[i].X, -pts[i].Y, 0);
                }


            }

            for (int j = 0; j < strokes.Count; j++) {
                strokes[j].Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));
            }


            PolylineCurve[] strokesCurves = new PolylineCurve[strokes.Count];
            for (int j = 0; j < strokes.Count; j++) {
                strokesCurves[j] = new PolylineCurve(strokes[j]);
            }

            singleLineText = strokesCurves;


        }
        private void singleLineFont(string text, Plane location, double height, out PolylineCurve[] singleLineText) {



            //makes a font instance copy
            string font = "Machine Tool SanSerif";
            double precision = 50;
            Plane plane = location;
            System.Drawing.Font localFont;
            try {
                localFont = new System.Drawing.Font(font, (float)height);
            } catch {
                //Print("Cannot Find Machine Tool SanSerif");
                font = "Arial";
                localFont = new System.Drawing.Font(font, (float)height);
            }

            //Makes a graphics path object instance
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddString(text, localFont.FontFamily, (int)localFont.Style, localFont.Size, new System.Drawing.PointF(0, 0), new System.Drawing.StringFormat());

            //This is a transformation matrix.
            System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
            matrix.Reset();
            //this basically turns the path into a polyline that approximates the path
            path.Flatten(matrix, (float)(height / precision));

            //extracts the points from the path
            System.Drawing.PointF[] pts = path.PathPoints;
            byte[] tps = path.PathTypes;

            //empty list of polylines
            List<Polyline> strokes = new List<Polyline>();
            Polyline stroke = null;

            //finds start point
            byte typStart = System.Convert.ToByte(System.Drawing.Drawing2D.PathPointType.Start);

            int i = -1;
            while (true) {
                i++;


                //when the stroke has i number of points in it, add it to the list and exit the while loop
                if (i >= pts.Length) {
                    if (stroke != null && stroke.Count > 1) {
                        strokes.Add(stroke);
                    }
                    break;
                }



                //if this is the start, then add the line and start a new one
                if (tps[i] == typStart) {
                    if (stroke != null && stroke.Count > 1) {
                        strokes.Add(stroke);
                    }
                    stroke = new Polyline();
                    
                    //this is negative if the plane.YAxis is not flipped
                    stroke.Add(pts[i].X, -pts[i].Y, 0);
                } else {
                    //this is negative if the plane.YAxis is not flipped
                    stroke.Add(pts[i].X, -pts[i].Y, 0);
                }


            }

            for (int j = 0; j < strokes.Count; j++) {
                strokes[j].Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));
            }


            PolylineCurve[] strokesCurves = new PolylineCurve[strokes.Count];
            for (int j = 0; j < strokes.Count; j++) {
                strokesCurves[j] = new PolylineCurve(strokes[j]);
            }

            singleLineText = strokesCurves;


        }

         void bakery(object obj, string layer, System.Drawing.Color color, int groupIndex, bool activate) {

            if (!activate) { return; }
            if (obj == null) { return; }

            //Guid id;


            //Make new attribute
            Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();

            ////Set object name
            //if (name != null && Rhino.DocObjects.Layer.IsValidName(name) == true) {
            //    att.Name = name;
            //}

            //Set layer
            if (layer != null && Rhino.DocObjects.Layer.IsValidName(layer) == true) {
                //Get the current layer index

                Rhino.DocObjects.Tables.LayerTable layerTable = doc.Layers;
                int layerIndex = layerTable.Find(layer, true);
                if (layerIndex == -1) { //This layer does not exist, we add it
                    //make new layer
                    Rhino.DocObjects.Layer myLayer = new Rhino.DocObjects.Layer();
                    myLayer.Name = layer;
                    myLayer.Color = color;
                    myLayer.PlotWeight = 0.0;
                    //Add the layer to the layer table
                    layerIndex = layerTable.Add(myLayer);
                }

                if (layerIndex > -1) { //We manged to add layer!
                    att.LayerIndex = layerIndex;
                }


            }

            //Set color
            if (color != null) {
                att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromLayer;
                att.ObjectColor = color;
                att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromLayer;
                att.PlotColor = color;
                att.PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromLayer;
                att.WireDensity = -1;
            }



            bake(obj, att, groupIndex);


        }
        void bake(object obj, Rhino.DocObjects.ObjectAttributes att, int groupIndex) {
            if (obj == null)
                return;

            Guid id;

            //Bake to the right type of object
            if (obj is GeometryBase) {
                GeometryBase geomObj = obj as GeometryBase;

                switch (geomObj.ObjectType) {
                    case Rhino.DocObjects.ObjectType.Brep:
                        id = doc.Objects.AddBrep(obj as Brep, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Curve:
                        id = doc.Objects.AddCurve(obj as Curve, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Point:
                        id = doc.Objects.AddPoint((obj as Rhino.Geometry.Point).Location, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Surface:
                        id = doc.Objects.AddSurface(obj as Surface, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Mesh:
                        id = doc.Objects.AddMesh(obj as Mesh, att);
                        break;
                    case (Rhino.DocObjects.ObjectType)1073741824://Rhino.DocObjects.ObjectType.Extrusion:
                        id = (Guid)typeof(Rhino.DocObjects.Tables.ObjectTable).InvokeMember("AddExtrusion", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, doc.Objects, new object[] { obj, att });
                        break;
                    case Rhino.DocObjects.ObjectType.PointSet:
                        id = doc.Objects.AddPointCloud(obj as Rhino.Geometry.PointCloud, att); //This is a speculative entry
                        break;
                    default:
                        //Print("The script does not know how to handle this type of geometry: " + obj.GetType().FullName);
                        return;
                }
            } else {
                Type objectType = obj.GetType();

                if (objectType == typeof(Arc)) {
                    id = doc.Objects.AddArc((Arc)obj, att);
                } else if (objectType == typeof(Box)) {
                    id = doc.Objects.AddBrep(((Box)obj).ToBrep(), att);
                } else if (objectType == typeof(Circle)) {
                    id = doc.Objects.AddCircle((Circle)obj, att);
                } else if (objectType == typeof(Ellipse)) {
                    id = doc.Objects.AddEllipse((Ellipse)obj, att);
                } else if (objectType == typeof(Polyline)) {
                    id = doc.Objects.AddPolyline((Polyline)obj, att);
                } else if (objectType == typeof(Sphere)) {
                    id = doc.Objects.AddSphere((Sphere)obj, att);
                } else if (objectType == typeof(Point3d)) {
                    id = doc.Objects.AddPoint((Point3d)obj, att);
                } else if (objectType == typeof(Line)) {
                    id = doc.Objects.AddLine((Line)obj, att);
                } else if (objectType == typeof(Vector3d)) {
                    //Print("Impossible to bake vectors");
                    return;
                } else if (obj is System.Collections.IEnumerable) {
                    int newGroupIndex;
                    if (groupIndex == -1)
                        newGroupIndex = doc.Groups.Add();
                    else
                        newGroupIndex = groupIndex;
                    foreach (object o in obj as System.Collections.IEnumerable) {
                        bake(o, att, newGroupIndex);
                    }
                    return;
                } else {
                    //Print("Unhandled type: " + objectType.FullName);
                    return;
                }
            }

            if (groupIndex != -1) {
                doc.Groups.AddToGroup(groupIndex, id);
                //Print("Added " + obj.GetType().Name + " to group number " + groupIndex);
            } else {
                //Print("Added " + obj.GetType().Name);
            }
        }
        Plane[] getStartPlanes(Polyline[] iThreads) {
            Plane[] updatePlanes = new Plane[iThreads.Length];
            for (int i = 0; i < iThreads.Length; i++) {
                if (iThreads[i].Count >= 2) {
                    updatePlanes[i] = new Plane(iThreads[i][0], iThreads[i][1] - iThreads[i][0]);
                }
            }
            return updatePlanes;
        }
        private Brep[] surfacesToBrep(List<Surface>iSurfaces) {
        Brep[] breps = new Brep[iSurfaces.Count];
        for (int i = 0; i < iSurfaces.Count; i++) {
            breps[i] = iSurfaces[i].ToBrep();
        }
        return breps;
        }
        private bool isInside(Brep query, Brep notch) {
            for (int i = 0; i < query.Vertices.Count; i++) {
                Point3d pt = query.Vertices[i].Location;
                if (!notch.IsPointInside(pt,0.001,false)) {
                    return false;
                }
            }
            return true;
        }
        private Brep[] trim(Surface[] keep, Brep[] delete,double thickness) {

            //Brep[] keeps = new Brep[keep.Length];
            List<Brep> keeps = new List<Brep>();
            List<Brep> trims = new List<Brep>();
            
            //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "keep length");
            //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, keep.Length.ToString());
            
            for (int i = 0; i < keep.Length; i++) {
                Brep b = keep[i].ToBrep();
                for (int j = 0; j < b.Faces.Count; j++) {
                Brep solid = Brep.CreateFromOffsetFace(b.Faces[j],thickness, 0.0,true, true);
                keeps.Add(solid);
                    
                }
            }

            for (int i = 0; i < keeps.Count; i++) {
                Brep[] bs = new Brep[1];
                bs[0] = keeps[i];
    
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "bs" + i.ToString() + " " + bs[0].ToString());
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, " " + i.ToString() + " " + delete.Length.ToString());
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, " " + i.ToString() + " " + delete.ToString());


                Brep[] diff = Brep.CreateBooleanDifference(bs, delete, 0.001);
                //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, " " + i.ToString() + " " + diff.Length.ToString());
                //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, " " + i.ToString() + " " + diff.Length.ToString());



                trims.AddRange(diff);
            }
            return trims.ToArray();
        }
        //private List<Brep> trim(Brep keep, Brep delete) {
        //    List<Brep> updateBreps = new List<Brep>();


        //    Brep[] trims = Brep.CreateBooleanDifference(keep, delete, 0.001);

        //    for (int i = 0; i < trims.Length; i++) {
        //        if (!isInside(trims[i],delete)) {
        //            updateBreps.Add(trims[i]);
        //        }
        //    }

        //    return updateBreps;

                
            
        //}
        Surface makeSurface(Polyline[] polylines) {
            List<Point3d> points = new List<Point3d>();
            for (int i = 0; i < polylines.Length; i++) {
                points.Add(polylines[i][0]);
                points.Add(polylines[i][1]);
            }

            NurbsSurface ns = NurbsSurface.CreateThroughPoints(points, polylines.Length, 2, 3, 3, false, false);
            return ns;
        }
        private void offsetLine(Polyline polyLine, double distance, double angle, bool doubleSided, double positive, out List<Surface> outSurfaces) {

            List<Surface> updateSurfaces = new List<Surface>();


            if (polyLine.Count < 2) {
                outSurfaces = updateSurfaces;
                return;
            }

            PolylineCurve plCurve = new PolylineCurve(polyLine);

            polyLine.ReduceSegments(0.001);
            if (!plCurve.IsLinear()) {
                outSurfaces = updateSurfaces;
                return;
            }

            Plane startPlane;

            //find the orientation of the startPlane
            int isVertical1 = (polyLine[1] - polyLine[0]).IsParallelTo(Vector3d.ZAxis);
            if (isVertical1 >= 1) {
                Vector3d lineDir = polyLine[1] - polyLine[0];
                startPlane = Plane.WorldXY;


                //general case for non vertical lines
                //vertical == 0;
            } else {
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
            if (doubleSided) {
                Transform xform = Transform.Translation(startPlane.XAxis * -distance);
                plCurve.Transform(xform);
                s = Surface.CreateExtrusion(plCurve, startPlane.XAxis * distance * 2.0);
                updateSurfaces.Add(s);
            } else {
                s = Surface.CreateExtrusion(plCurve, startPlane.XAxis * distance * positive);
                updateSurfaces.Add(s);
            }

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






                    startPlane.Rotate(angle * Math.PI / 180.0, startPlane.ZAxis);















                    //Curve normal
                } else if (menu_startAngle == 0) {
                    //find first non colinear vertex
                    k = 2;
                    Vector3d firstVec = polyLine[1] - polyLine[0];
                    int isCoLinear = firstVec.IsParallelTo(polyLine[k] - polyLine[1]);
                    while (isCoLinear >= 1) {
                        if (k == polyLine.Count) {
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
            } catch { this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Warning, "update Rhino. some curves might be reversed"); }


            outSurfaces = updateSurfaces.ToArray();

        }

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
        #endregion
    }
}
