using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;




namespace gsd {
    public class unroll : Grasshopper.Kernel.GH_Component {
        public unroll() : base(".unroll", ".unroll", "unroll surfaces", "Extra", "surfTM") { }
        public override Guid ComponentGuid {
            get {
                return new Guid("{D2F0305C-8CA7-4525-8B2E-11F3A46CAFC2}");
            }
        }
        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                return gsd.Properties.Resources.gsd11;
            }
        }
        public override Grasshopper.Kernel.GH_Exposure Exposure {
            get {
                return Grasshopper.Kernel.GH_Exposure.primary;
            }
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager) {
            pManager.AddBrepParameter("breps", "breps", "breps to unroll", Grasshopper.Kernel.GH_ParamAccess.list);
            //pManager.AddBrepParameter("brep", "brep", "3d brep", Grasshopper.Kernel.GH_ParamAccess.item);
            //pManager.AddSurfaceParameter("surface", "surface", "surface", Grasshopper.Kernel.GH_ParamAccess.item);
            pManager.AddCurveParameter("curves", "curves", "curves on surface", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddPointParameter("points", "points", "points on surface", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddTextParameter("prefix", "prefix", "insert a panel. text will be appended with the index", Grasshopper.Kernel.GH_ParamAccess.item, "");
            pManager.AddNumberParameter("text size", "text size", "number for the size of index labels", Grasshopper.Kernel.GH_ParamAccess.item, 1.0);


            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;




        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_BRepParam("breps", "breps", "unrolled surfaces", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_CurveParam("curves", "curves", "unrolled curves", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_PointParam("points", "points", "unrolled points", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_CurveParam("text", "text", "index of each piece as text curves", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_GenericParam("", "", "");
        }

        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {

            //empty lists;
            //List<Grasshopper.Kernel.Types.IGH_GeometricGoo> geometryInput = new List<Grasshopper.Kernel.Types.IGH_GeometricGoo>();
            //List<Surface> inputSurfaces = new List<Surface>();
            List<Brep> inputBreps = new List<Brep>();
            List<Curve> inputCurves = new List<Curve>();
            List<Point3d> inputPoints = new List<Point3d>();


            //Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> outSurfaces = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
            //Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> outCurves = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
            //Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> outPoints = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
            //Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> outDots = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
            //Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> outText = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();

            List<Brep> outBreps = new List<Brep>();
            List<Surface> outSurfaces = new List<Surface>();
            List<Curve> outCurves = new List<Curve>();
            List<Point3d> outPoints = new List<Point3d>();
            List<TextDot> outDots = new List<TextDot>();
            List<Curve> outText = new List<Curve>();




            string appendText = null;
            double maxX = 0.0;
            double minX = 0.0;
            double maxY = 0.0;
            double minY = 0.0;
            double textSize = 1.0;







            //get input from grasshopper
            //if(!DA.GetDataList<Grasshopper.Kernel.Types.IGH_GeometricGoo>(0, geometryInput)) { return; }
            //DA.Util_RemoveNullRefs<Grasshopper.Kernel.Types.IGH_GeometricGoo>(geometryInput);
            DA.GetDataList<Brep>(0, inputBreps);
            DA.GetDataList<Curve>(1,  inputCurves);
            DA.GetDataList<Point3d>(2, inputPoints);
            DA.GetData<string>(3, ref appendText);
            DA.GetData<double>(4, ref textSize);
            //DA.GetDataList<TextDot>(3, dots);

            //geometryInput.RemoveAll(item => item == null);
            //for(int i = 0; i < geometryInput.Count; i++) {
            //    Brep brep;
            //    if(geometryInput[i].CastTo<Brep>(out brep)) {
            //        inputBreps.Add(brep);
            //    }
            //}
            if(textSize <= 0) { textSize = 0.001; }


            #region surfaceRegion
            ////logic
            ////surface

            //for(int i = 0; i < inputSurfaces.Count; i++) {



            //    //empty lists
            //    Unroller un;
            //    Curve[] unrolledCurves;
            //    Point3d[] unrolledPoints;
            //    TextDot[] unrolledDots;
            //    Brep[] unrolledBreps;
            //    Curve[] unrolledText;






            //    //make text in 3d
            //    string text = appendText + i.ToString();
            //    Plane drawingPlane;
            //    inputSurfaces[i].FrameAt(inputSurfaces[i].Domain(0).Min, inputSurfaces[i].Domain(1).Min, out drawingPlane);

            //    drawingPlane.Translate(drawingPlane.YAxis * textSize);
            //    PolylineCurve[] text3d;
            //    drawingPlane.Flip();
            //    drawingPlane.YAxis *= -1;

            //    singleLineFont(text, drawingPlane, textSize * 0.75, out text3d);
            //    outText.AddRange(text3d);

            //    //unroll text
            //    Unroller unText = new Unroller(inputSurfaces[i]);
            //    unText.ExplodeOutput = false;
            //    unText.ExplodeSpacing = 1.0;
            //    unText.AddFollowingGeometry(text3d);
            //    Brep[] unrollText = unText.PerformUnroll(out unrolledText, out unrolledPoints, out unrolledDots);




            //    //unroll geometry
            //    un = new Unroller(inputSurfaces[i]);

            //    un.AddFollowingGeometry(inputCurves);
            //    un.AddFollowingGeometry(inputPoints);
            //    unrolledBreps = un.PerformUnroll(out unrolledCurves, out unrolledPoints, out unrolledDots);

            //    //spacing
            //    double bBox = 0.0;
            //    for(int j = 0; j < unrolledBreps.Length; j++) {
            //        BoundingBox bb = unrolledBreps[j].GetBoundingBox(false);
            //        if(bBox < bb.Max.X) {
            //            maxX = bb.Max.X;
            //        }
            //        if(maxY < bb.Max.Y) {
            //            maxY = bb.Max.Y;
            //        }
            //    }


            //    //move X
            //    for(int j = 0; j < unrolledBreps.Length; j++) {
            //        unrolledBreps[j].Translate(-maxX + minX, 0, 0);
            //    }
            //    for(int j = 0; j < unrolledCurves.Length; j++) {
            //        unrolledCurves[j].Translate(-maxX + minX, 0, 0);
            //    }
            //    for(int j = 0; j < unrolledPoints.Length; j++) {
            //        unrolledPoints[j].X += ( -maxX + minX );
            //    }
            //    for(int j = 0; j < unrolledDots.Length; j++) {
            //        unrolledDots[j].Translate(-maxX + minX, 0, 0);
            //    }
            //    for(int j = 0; j < unrolledText.Length; j++) {
            //        unrolledText[j].Translate(-maxX + minX, 0, 0);
            //    }
            //    //update global box
            //    minX -= maxX;
            //    maxX = 0.0;


            //    //move Y
            //    for(int j = 0; j < unrolledBreps.Length; j++) {
            //        unrolledBreps[j].Translate(0, -maxY + minY, 0);
            //    }
            //    for(int j = 0; j < unrolledCurves.Length; j++) {
            //        unrolledCurves[j].Translate(0, -maxY + minY, 0);
            //    }
            //    for(int j = 0; j < unrolledPoints.Length; j++) {
            //        unrolledPoints[j].Y += ( -maxY + minY );
            //    }
            //    for(int j = 0; j < unrolledDots.Length; j++) {
            //        unrolledDots[j].Translate(0, -maxX + minX, 0);
            //    }
            //    for(int j = 0; j < unrolledText.Length; j++) {
            //        unrolledText[j].Translate(0, -maxY + minY, 0);
            //    }



            //    //label text flat
            //    Point3d textPoint;
            //    if(( unrolledText.Length < 1 )) {
            //        this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Remark, "index label " + i.ToString() + " did not unroll");
            //        if(unrolledBreps[0].Vertices[0] != null) {
            //            textPoint = unrolledBreps[0].Vertices[0].Location;

            //            textPoint.X += textSize * 0.25;
            //            textPoint.Y += textSize * 2.0;
            //            PolylineCurve[] singleLineText;

            //            singleLineFont(text, textPoint, textSize * 0.75, out singleLineText);
            //            outText.AddRange(singleLineText);
            //        } else {
            //            this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Remark, "index " + i.ToString() + " vertex[0]==null");

            //        }
            //    }


            //    //TODO: grouped


            //    //output
            //    for(int j = 0; j < unrolledBreps.Length; j++) {
            //        for(int k = 0; k < unrolledBreps[j].Surfaces.Count; k++) {
            //            outSurfaces.Add(unrolledBreps[j].Surfaces[k]);
            //        }
            //    }
            //    outCurves.AddRange(unrolledCurves);
            //    outPoints.AddRange(unrolledPoints);
            //    outText.AddRange(unrolledText);
            //    outDots.AddRange(unrolledDots);
            //}
            //minX = 0.0;
            //maxX = 0.0;
            //minY -= maxY;
            //maxY = 0.0;
            #endregion
            #region brepRegion
            //brep
            for(int i = 0; i < inputBreps.Count; i++) {

                //empty lists
                Unroller un;
                Curve[] unrolledCurves;
                Point3d[] unrolledPoints;
                TextDot[] unrolledDots;
                Brep[] unrolledBreps;
                Curve[] unrolledText;






                //make text in 3d
                string text = i.ToString();
                Plane drawingPlane;
                inputBreps[i].Surfaces[0].FrameAt(inputBreps[i].Surfaces[0].Domain(0).Min, inputBreps[i].Surfaces[0].Domain(1).Min, out drawingPlane);

                drawingPlane.Translate(drawingPlane.YAxis * textSize);
                PolylineCurve[] text3d;
                drawingPlane.Flip();
                drawingPlane.YAxis *= -1;

                singleLineFont(text, drawingPlane, textSize * 0.75, out text3d);
                outText.AddRange(text3d);

                //unroll text
                Unroller unText = new Unroller(inputBreps[i]);
                unText.ExplodeOutput = false;
                unText.ExplodeSpacing = 1.0;
                unText.AddFollowingGeometry(text3d);
                Brep[] unrollText = unText.PerformUnroll(out unrolledText, out unrolledPoints, out unrolledDots);


                //unroll geometry
                un = new Unroller(inputBreps[i]);

                un.AddFollowingGeometry(inputCurves);
                un.AddFollowingGeometry(inputPoints);
                unrolledBreps = un.PerformUnroll(out unrolledCurves, out unrolledPoints, out unrolledDots);


                //find BoundingBox.Max.X
                double bBox = 0.0;
                for(int j = 0; j < unrolledBreps.Length; j++) {
                    BoundingBox bb = unrolledBreps[j].GetBoundingBox(false);
                    if(bBox < bb.Max.X) {
                        maxX = bb.Max.X;
                    }
                    if(maxY < bb.Max.Y) {
                        maxY = bb.Max.Y;
                    }
                }


                //move X
                for(int j = 0; j < unrolledBreps.Length; j++) {
                    unrolledBreps[j].Translate(-maxX + minX, 0, 0);
                }
                for(int j = 0; j < unrolledCurves.Length; j++) {
                    unrolledCurves[j].Translate(-maxX + minX, 0, 0);
                }
                for(int j = 0; j < unrolledPoints.Length; j++) {
                    unrolledPoints[j].X += ( -maxX + minX );
                }
                for(int j = 0; j < unrolledDots.Length; j++) {
                    unrolledDots[j].Translate(-maxX + minX, 0, 0);
                }
                for(int j = 0; j < unrolledText.Length; j++) {
                    unrolledText[j].Translate(-maxX + minX, 0, 0);
                }
                //update global box
                minX -= maxX;
                maxX = 0.0;


                //move Y
                for(int j = 0; j < unrolledBreps.Length; j++) {
                    unrolledBreps[j].Translate(0, -maxY + minY, 0);
                }
                for(int j = 0; j < unrolledCurves.Length; j++) {
                    unrolledCurves[j].Translate(0, -maxY + minY, 0);
                }
                for(int j = 0; j < unrolledPoints.Length; j++) {
                    unrolledPoints[j].Y += ( -maxY + minY );
                }
                for(int j = 0; j < unrolledDots.Length; j++) {
                    unrolledDots[j].Translate(0, -maxX + minX, 0);
                }
                for(int j = 0; j < unrolledText.Length; j++) {
                    unrolledText[j].Translate(0, -maxY + minY, 0);
                }



                //label text flat
                Point3d textPoint;
                if(unrolledText.Length < 1) {
                    this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Remark, "index label " + i.ToString() + " did not unroll");
                    if(unrolledBreps[0].Vertices[0] != null) {
                        textPoint = unrolledBreps[0].Vertices[0].Location;

                        textPoint.X += textSize * 0.25;
                        textPoint.Y += textSize * 2.0;
                        PolylineCurve[] singleLineText;

                        singleLineFont(text, textPoint, textSize * 0.75, out singleLineText);
                        outText.AddRange(singleLineText);
                    } else {
                        this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Remark, "index " + i.ToString() + " vertex[0]==null");
                    }
                }





                //TODO: grouped


                //output
                if(unrolledBreps != null) { outBreps.AddRange(unrolledBreps); }
                if(unrolledCurves != null) { outCurves.AddRange(unrolledCurves); }
                if(unrolledPoints != null) { outPoints.AddRange(unrolledPoints); }
                if(unrolledText != null) { outText.AddRange(unrolledText); }
                if(unrolledDots != null) { outDots.AddRange(unrolledDots); }
            }
            minY -= maxY;
            maxY = 0.0;
            #endregion


            ////output to Grasshopper
            //DA.SetDataTree(0, outSurfaces);
            //DA.SetDataTree(1, outCurves);
            //DA.SetDataTree(2, outPoints);
            //DA.SetDataTree(3, outText);
            ////DA.SetDataList(3, updateDots);

            DA.SetDataList(0, outBreps);
            DA.SetDataList(1, outCurves);
            DA.SetDataList(2, outPoints);
            DA.SetDataList(3, outText);



        }

        private void singleLineFont(string text, Point3d location, double height, out PolylineCurve[] singleLineText) {
            singleLineText = null;

            Plane plane = new Plane(location, Vector3d.ZAxis);

            singleLineFont(text, plane, height, out singleLineText);
        }
        private void singleLineFont(string text, Plane location, double height, out PolylineCurve[] singleLineText) {
            singleLineText = null;



            //makes a font instance copy
            string font = "Machine Tool SanSerif";
            double precision = 50;
            Plane plane = location;
            System.Drawing.Font localFont;
            try {
                localFont = new System.Drawing.Font(font, (float) height);
            } catch {
                this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Warning, "Install the font 'Machine Tool San Serif' www.jmorrison.co/font/");
                try {
                    font = "Arial";
                    localFont = new System.Drawing.Font(font, (float) height);
                } catch {
                    this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error, "font error");
                    return;
                }
            }

            //Makes a graphics path object instance
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddString(text, localFont.FontFamily, (int) localFont.Style, localFont.Size, new System.Drawing.PointF(0, 0), new System.Drawing.StringFormat());

            //This is a transformation matrix.
            System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
            matrix.Reset();
            //this basically turns the path into a polyline that approximates the path
            path.Flatten(matrix, (float) ( height / precision ));

            //extracts the points from the path
            System.Drawing.PointF[] pts = path.PathPoints;
            byte[] tps = path.PathTypes;

            //empty list of polylines
            List<Polyline> strokes = new List<Polyline>();
            Polyline stroke = null;

            //finds start point
            byte typStart = System.Convert.ToByte(System.Drawing.Drawing2D.PathPointType.Start);

            int i = -1;
            while(true) {
                i++;


                //when the stroke has i number of points in it, add it to the list and exit the while loop
                if(i >= pts.Length) {
                    if(stroke != null && stroke.Count > 1) {
                        strokes.Add(stroke);
                    }
                    break;
                }



                //if this is the start, then add the line and start a new one
                if(tps[i] == typStart) {
                    if(stroke != null && stroke.Count > 1) {
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

            for(int j = 0; j < strokes.Count; j++) {
                strokes[j].Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));
            }


            PolylineCurve[] strokesCurves = new PolylineCurve[strokes.Count];
            for(int j = 0; j < strokes.Count; j++) {
                strokesCurves[j] = new PolylineCurve(strokes[j]);
            }

            singleLineText = strokesCurves;


        }

    }
}
