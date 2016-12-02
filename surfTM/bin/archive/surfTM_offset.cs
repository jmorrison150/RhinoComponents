using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;

using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;



namespace gsd {
    class SurfTM_offset0 {
        Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
        private void offset(List<Polyline> iThreads, List<double> width0, List<double> angle0, object lll, List<Polyline> jThreads, List<double> width1, List<double> angle1, object ll, double thickness, bool unroll, bool unrollBake, ref object outSurfaces, ref object outPoints, ref object outCurves) {


            

            #region unroll
            planes = new List<Plane>();
            points = new List<Point3d>();

            //empty lists
            double[] widths0 = new double[iThreads.Count];
            double[] widths1 = new double[iThreads.Count];
            double[] angles0 = new double[jThreads.Count];
            double[] angles1 = new double[jThreads.Count];

            //populate lists. Loop if lengths don't match
            for (int i = 0; i < widths0.Length; i++) {
                widths0[i] = width0[i % width0.Count];
            }
            for (int i = 0; i < widths1.Length; i++) {
                widths1[i] = width1[i % width1.Count];
            }
            for (int i = 0; i < angles0.Length; i++) {
                angles0[i] = angle0[i % angle0.Count];
            }
            for (int i = 0; i < angles1.Length; i++) {
                angles1[i] = angle1[i % angle1.Count];
            }







            //to autoBake: comment out minY=0.0; set unrollBake =true; and right click animate a slider
            minY = 0.0;

            //empty lists

            List<Surface> iSurfaces;
            List<Surface> jSurfaces;

            //offset
            offsetPolyline(iThreads, widths0, angles0, true, 1.0, out iSurfaces);
            offsetPolyline(jThreads, widths1, angles1, true, 1.0, out jSurfaces);

            //output
            List<Surface> updateSurfaces = new List<Surface>();
            List<Curve> updateCurves = new List<Curve>();
            List<Point3d> updatePoints = new List<Point3d>();

            updateSurfaces.AddRange(iSurfaces);
            updateSurfaces.AddRange(jSurfaces);

            //intersect///////////////////////////////////
            if (unroll || unrollBake) {
                //empty lists
                List<Surface> iHalves;
                List<Surface> jHalves;

                double[] notchWidths0 = new double[widths0.Length];
                double[] notchWidths1 = new double[widths1.Length];

                for (int i = 0; i < widths0.Length; i++) {
                    notchWidths0[i] = widths0[i] * 2.1;
                }
                for (int i = 0; i < widths1.Length; i++) {
                    notchWidths1[i] = widths1[i] * 2.1;
                }



                offsetPolyline(iThreads, notchWidths0, angles0, false, -1.0, out iHalves);
                offsetPolyline(jThreads, notchWidths1, angles1, false, 1.0, out jHalves);

                //updateSurfaces.AddRange(iHalves);
                //updateSurfaces.AddRange(jHalves);

                Surface[][] jHalvesOffsets = offsetSurfaces(jHalves, thickness);
                Surface[][] iHalvesOffsets = offsetSurfaces(iHalves, thickness);

                Curve[][] iNotches;
                Curve[][] jNotches;

                Plane[] iPlanes = getStartPlanes(iThreads);
                Plane[] jPlanes = getStartPlanes(jThreads);



                intersect(iSurfaces.ToArray(), jHalvesOffsets, out iNotches, iPlanes);
                intersect(jSurfaces.ToArray(), iHalvesOffsets, out jNotches, jPlanes);


                for (int i = 0; i < iNotches.Length; i++) {
                    updateCurves.AddRange(iNotches[i]);
                }
                for (int i = 0; i < jNotches.Length; i++) {
                    updateCurves.AddRange(jNotches[i]);
                }






                //unroll
                List<Surface> updateUnrollSurfaces = new List<Surface>();
                List<Curve> updateUnrollCurves = new List<Curve>();
                List<Point3d> updateUnrollPoints = new List<Point3d>();


                List<Surface> unSurfaces;
                List<Curve> unCurves;
                List<Point3d> unPoints;

                unrollSrfs(iSurfaces.ToArray(), iThreads.ToArray(), iNotches, widths0, unrollBake, out unSurfaces, out unCurves, out unPoints);

                updateCurves.AddRange(unCurves);
                updatePoints.AddRange(unPoints);
                //updateUnrollSurfaces.AddRange(unSurfaces);


                unSurfaces.Clear();
                unCurves.Clear();
                unPoints.Clear();
                unrollSrfs(jSurfaces.ToArray(), jThreads.ToArray(), jNotches, widths1, unrollBake, out unSurfaces, out unCurves, out unPoints);
                updateCurves.AddRange(unCurves);
                updatePoints.AddRange(unPoints);
                //updateUnrollSurfaces.AddRange(unSurfaces);


                //unrolledCurves = updateUnrollCurves;
                //unrolledSurfaces = updateUnrollSurfaces;
                //unrolledPoints = updateUnrollPoints;

                //List<string> text = new List<string>();
                //List<Point3d> locations = new List<Point3d>();





            }//end unroll










            outPoints = updatePoints;

            outSurfaces = updateSurfaces;
            outCurves = updateCurves;


            //outPlanes = planes;
            #endregion


        }

        // <Custom additional code> 






        #region customCode

        List<Plane> planes;
        List<Point3d> points;
        double minY = 0.0;


        private void offsetPolyline(List<Polyline> polyLines, double[] distances, double[] angles, bool doubleSided, double positive, out List<Surface> outSurfaces) {




            List<Surface> updateSurfaces = new List<Surface>();
            List<Brep> updateBreps = new List<Brep>();
            List<Point3d> updatePoints = new List<Point3d>();
            List<Point3d> updateVectorPoints = new List<Point3d>();
            List<Vector3d> updateVectors = new List<Vector3d>();
            List<Plane> updatePlanes = new List<Plane>();
            List<PolylineCurve> updatePolylines = new List<PolylineCurve>();

            //work on one polyline at a time
            for (int i = 0; i < polyLines.Count; ++i) {
                PolylineCurve plCurve = new PolylineCurve(polyLines[i]);
                //double rotateAngle;
                //rotateAngle = angle;
                //rotateAngle = angle + Math.Sin((double) i / polyLines.Count) * 180.0;

                polyLines[i].ReduceSegments(0.001);



                bool planarCurve = plCurve.IsPlanar() && angles[i] == 0;





                if (polyLines[i].Count < 2) {
                    outSurfaces = updateSurfaces;

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
                    if (doubleSided) {
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


                    int isVertical = (polyLines[i][1] - polyLines[i][0]).IsParallelTo(Vector3d.ZAxis);
                    if (isVertical >= 1) {
                        //make start plane from straight lines
                        Vector3d lineDir = polyLines[i][1] - polyLines[i][0];
                        Vector3d nextLine;
                        if (i == 0) {
                            nextLine = polyLines[0][0] - polyLines[1][0];
                        } else {
                            nextLine = polyLines[i][0] - polyLines[i - 1][0];
                        }
                        Vector3d startVec = Vector3d.CrossProduct(lineDir, nextLine);
                        Plane startPlane = new Plane(polyLines[i][0], startVec, nextLine);
                        startPlane.Rotate(angles[i] * Math.PI / 180.0, startPlane.ZAxis);

                        biPlanes[0] = startPlane;


                    } else {
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
                    if (doubleSided) { minOffset = -distances[i] * positive; }

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

            outSurfaces = updateSurfaces;

        }
        private void intersect(Surface[] iSurfaces, Surface[][] jHalves, out Curve[][] outNotches, Plane[] iPlanes) {




            Curve[][] notches = new Curve[iSurfaces.Length][];
            for (int i = 0; i < iSurfaces.Length; ++i) {
                List<Curve> updateCurves1 = new List<Curve>();


                //if startpoint.DistaneTo(endpoint)<thickness
                //split intersectionCurves using startplane


                for (int j = 0; j < jHalves.Length; ++j) {
                    List<Curve> intCurves = new List<Curve>();


                    for (int k = 0; k < jHalves[j].Length; k++) {
                        Curve[] intersectionCurves;
                        Point3d[] intersectionPoints;

                        //intersect
                        Rhino.Geometry.Intersect.Intersection.SurfaceSurface(iSurfaces[i], jHalves[j][k], 0.001, out intersectionCurves, out intersectionPoints);


                        ////if (k == 0 || k == jHalves[j].Length - 1) {
                        //for (int l = 0; l < intersectionCurves.Length; l++) {
                        //    Rhino.Geometry.Intersect.CurveIntersections splitPts = Rhino.Geometry.Intersect.Intersection.CurvePlane(intersectionCurves[l], iPlanes[i], 0.001);
                        //    if (splitPts.Count == 0) {
                        //        updateCurves1.Add(intersectionCurves[l]);
                        //    } else {
                        //        for (int m = 0; m < splitPts.Count; m++) {
                        //            if (splitPts[m].IsPoint) {
                        //                Curve[] splits = intersectionCurves[l].Split(splitPts[m].ParameterA);
                        //                updateCurves1.AddRange(splits);
                        //            } else {
                        //                updateCurves1.Add(intersectionCurves[l]);
                        //            }
                        //        }
                        //    }
                        //}
                        ////}

                        updateCurves1.AddRange(intersectionCurves);





                        //          Curve[] cvs = iSurfacaes[i].ToBrep().DuplicateNakedEdgeCurves(true, false);
                        //          for (int l = 0; l < cvs.Length; l++) {
                        //            for (int m = 0; m < intersectionCurves.Length; m++) {
                        //              Rhino.Geometry.Intersect.CurveIntersections inter = Rhino.Geometry.Intersect.Intersection.CurveCurve(intersectionCurves[m], cvs[l], 0.001, 0.001);
                        //              for (int n = 0; n < inter.Count; n++) {
                        //                if (inter[n].IsPoint) {
                        //
                        //                  Curve[] splits = intersectionCurves[m].Split(inter[n].ParameterA);
                        //                  if (splits != null) {
                        //                    updateCurves1.AddRange(splits);
                        //
                        //                  }
                        //                }
                        //              }
                        //            }
                        //
                        //          }
                    }
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


                List<Curve> unrolledOutline = new List<Curve>();
                Curve[] unrolledCurves;
                Point3d[] unrolledPoints;
                TextDot[] unrolledDots;









                Unroller un = new Unroller(surfaces[i]);

                un.ExplodeOutput = false;
                un.ExplodeSpacing = 1.0;
                un.AddFollowingGeometry(curves[i]);
                un.AddFollowingGeometry(pts);

                //unroll
                Brep[] unrolledBreps = un.PerformUnroll(out unrolledCurves, out unrolledPoints, out unrolledDots);


                //label text
                Point3d textPoint = Point3d.Origin;
                List<Curve> allText = new List<Curve>();
                if (unrolledBreps[0].Vertices[0] != null) {
                    textPoint = unrolledBreps[0].Vertices[0].Location;
                }










                //convert to surfaces wireframe
                for (int j = 0; j < unrolledBreps.Length; j++) {



                    //Curve[] curves1 = unrolledBreps[j].DuplicateNakedEdgeCurves(true, true);
                    //unrolledOutline.AddRange(curves1);


                    Mesh[] m = Mesh.CreateFromBrep(unrolledBreps[j], MeshingParameters.Smooth);
                    for (int k = 0; k < m.Length; k++) {
                        Polyline[] curves5 = m[k].GetOutlines(Plane.WorldXY);
                        for (int l = 0; l < curves5.Length; l++) {
                            PolylineCurve curves6 = new PolylineCurve(curves5[l]);
                            unrolledOutline.Add(curves6);
                        }
                    }




                }

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
                //for (int j = 0; j < unrolledBreps.Length; j++) {
                //    unrolledBreps[j].Translate(-maxX + minX, 0, 0);
                //}

                for (int j = 0; j < unrolledOutline.Count; j++) {
                    unrolledOutline[j].Translate(-maxX + minX, 0, 0);
                }
                for (int j = 0; j < unrolledCurves.Length; j++) {
                    unrolledCurves[j].Translate(-maxX + minX, 0, 0);
                }
                for (int j = 0; j < unrolledPoints.Length; j++) {
                    unrolledPoints[j].X += (-maxX + minX);
                }
                for (int j = 0; j < unrolledDots.Length; j++) {
                    unrolledDots[j].Translate(-maxX + minX, 0, 0);
                }
                textPoint.X += (-maxX + minX);

                //update global box
                minX -= maxX;
                maxX = 0.0;


                //move Y
                //for (int j = 0; j < updateSurfaces.Count; j++) {
                //    updateSurfaces[j].Translate(0, -maxY + minY, 0);
                //}
                for (int j = 0; j < unrolledOutline.Count; j++) {
                    unrolledOutline[j].Translate(0, -maxY + minY, 0);
                }
                for (int j = 0; j < unrolledCurves.Length; j++) {
                    unrolledCurves[j].Translate(0, -maxY + minY, 0);
                }
                for (int j = 0; j < unrolledPoints.Length; j++) {
                    unrolledPoints[j].Y += (-maxY + minY);
                }
                for (int j = 0; j < unrolledDots.Length; j++) {
                    unrolledDots[j].Translate(0, -maxX + minX, 0);
                }
                textPoint.Y += (-maxY + minY);




                //make text curves
                string text = i.ToString();

                double width;
                try {
                    width = widths[i];
                } catch {
                    width = 1.0;
                }

                textPoint.X += width * 0.25;
                textPoint.Y += width * 2.0;
                PolylineCurve[] singleLineText;

                singleLineFont(text, textPoint, width * 0.75, out singleLineText);
                allText.AddRange(singleLineText);


                List<Curve> curves7 = new List<Curve>();
                for (int j = 0; j < unrolledCurves.Length; j++) {
                    if (unrolledCurves[j].GetLength() < width * 4.0) {
                        curves7.Add(unrolledCurves[j]);
                    }
                }





                Color green = Color.FromArgb(0, 255, 0);

                bakery(allText, "a_Score", green, groupIndex, unrollBake);
                bakery(curves7, "a_Cut1", Color.Blue, groupIndex, unrollBake);
                bakery(unrolledOutline, "a_Cut2", Color.Magenta, groupIndex, unrollBake);



                updateCurves.AddRange(allText);
                updateCurves.AddRange(curves7);
                updateCurves.AddRange(unrolledOutline);


            }
            minY -= maxY;
            maxY = 0.0;





        }
        private Surface[][] offsetSurfaces(List<Surface> jHalves, double thickness) {

            Surface[][] jHalvesOffset = new Surface[jHalves.Count][];

            for (int i = 0; i < jHalves.Count; i++) {

                Brep solid = Brep.CreateFromOffsetFace(jHalves[i].ToBrep().Faces[0], thickness, 0.001, true, true);
                jHalvesOffset[i] = solid.Surfaces.ToArray();

            }
            return jHalvesOffset;

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
            path.AddString(text, localFont.FontFamily, (int)localFont.Style, localFont.Size, new System.Drawing.PointF(0, 0), new StringFormat());

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
                        id = (Guid)typeof(Rhino.DocObjects.Tables.ObjectTable).InvokeMember("AddExtrusion", BindingFlags.Instance | BindingFlags.InvokeMethod, null, doc.Objects, new object[] { obj, att });
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
                } else if (obj is IEnumerable) {
                    int newGroupIndex;
                    if (groupIndex == -1)
                        newGroupIndex = doc.Groups.Add();
                    else
                        newGroupIndex = groupIndex;
                    foreach (object o in obj as IEnumerable) {
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
        Plane[] getStartPlanes(List<Polyline> iThreads) {
            Plane[] updatePlanes = new Plane[iThreads.Count];
            for (int i = 0; i < iThreads.Count; i++) {
                if (iThreads[i].Count >= 2) {
                    updatePlanes[i] = new Plane(iThreads[i][0], iThreads[i][1] - iThreads[i][0]);
                }
            }
            return updatePlanes;
        }


        #endregion








        // </Custom additional code> 
    }
}