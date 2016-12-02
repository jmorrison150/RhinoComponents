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



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance {
    #region Utility functions
    /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
    /// <param name="text">String to print.</param>
    private void Print(string text) { /* Implementation hidden. */ }
    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
    /// <param name="format">String format.</param>
    /// <param name="args">Formatting parameters.</param>
    private void Print(string format, params object[] args) { /* Implementation hidden. */ }
    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj) { /* Implementation hidden. */ }
    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
    #endregion

    #region Members
    /// <summary>Gets the current Rhino document.</summary>
    private readonly RhinoDoc RhinoDocument;
    /// <summary>Gets the Grasshopper document that owns this script.</summary>
    private readonly GH_Document GrasshopperDocument;
    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
    private readonly IGH_Component Component;
    /// <summary>
    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
    /// Any subsequent call within the same solution will increment the Iteration count.
    /// </summary>
    private readonly int Iteration;
    #endregion

    /// <summary>
    /// This procedure contains the user code. Input parameters are provided as regular arguments,
    /// Output parameters as ref arguments. You don't have to assign output parameters,
    /// they will have a default value.
    /// </summary>
    private void RunScript(List<Polyline> iThreads, List<Polyline> jThreads, double width, double thickness, double angle, bool unroll, bool unrollBake, ref object outCurves, ref object outSurfaces, ref object unrolledCurves, ref object unrolledSurfaces, ref object unrolledText, ref object unrolledPoints, ref object outPlanes) {





        #region unroll
        //to autoBake: comment out minY=0.0; set unrollBake =true; and right click animate a slider
        minY = 0.0;

        //empty lists
        List<Surface> iHalves;
        List<Surface> jHalves;
        List<Surface> iSurfaces;
        List<Surface> jSurfaces;

        //offset
        offsetPolyline(iThreads, width, angle, true, 1.0, out iSurfaces);
        offsetPolyline(jThreads, width, angle, true, 1.0, out jSurfaces);

        //output
        List<Surface> updateSurfaces = new List<Surface>();
        List<Curve> updateCurves = new List<Curve>();

        updateSurfaces.AddRange(iSurfaces);
        updateSurfaces.AddRange(jSurfaces);


        if (unroll || unrollBake) {
            offsetPolyline(iThreads, width * 2.1, angle, false, 1.0, out iHalves);
            offsetPolyline(jThreads, width * 2.1, angle, false, -1.0, out jHalves);

            //updateSurfaces.AddRange(iHalves);
            //updateSurfaces.AddRange(jHalves);

            Surface[][] jHalvesOffsets = offsetSurfaces(jHalves, thickness);
            Surface[][] iHalvesOffsets = offsetSurfaces(iHalves, thickness);

            Curve[][] iNotches;
            Curve[][] jNotches;


            intersect(iSurfaces.ToArray(), jHalvesOffsets, out iNotches);
            intersect(jSurfaces.ToArray(), iHalvesOffsets, out jNotches);


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
            unrollSrfs(iSurfaces.ToArray(), iThreads.ToArray(), iNotches, out unSurfaces, out unCurves, out unPoints);
            updateUnrollCurves.AddRange(unCurves);
            updateUnrollSurfaces.AddRange(unSurfaces);
            updateUnrollPoints.AddRange(unPoints);


            unSurfaces.Clear();
            unCurves.Clear();
            unPoints.Clear();
            unrollSrfs(jSurfaces.ToArray(), jThreads.ToArray(), jNotches, out unSurfaces, out unCurves, out unPoints);
            updateUnrollCurves.AddRange(unCurves);
            updateUnrollSurfaces.AddRange(unSurfaces);
            updateUnrollPoints.AddRange(unPoints);


            unrolledCurves = updateUnrollCurves;
            unrolledSurfaces = updateUnrollSurfaces;
            unrolledPoints = updateUnrollPoints;

            //List<string> text = new List<string>();
            //List<Point3d> locations = new List<Point3d>();
            List<Curve> allText = new List<Curve>();

            for (int i = 0; i < updateUnrollSurfaces.Count; i++) {
                string text = i.ToString();
                Surface srf = updateUnrollSurfaces[i];
                Point3d pt = srf.PointAt(srf.Domain(0).Min, srf.Domain(1).Min);
                pt.X += width * 0.25;
                pt.Y += width * 2.0;
                PolylineCurve[] singleLineText;

                singleLineFont(text, pt, width * 0.5, out singleLineText);
                allText.AddRange(singleLineText);
            }


            unrolledText = allText;







            Color green = Color.FromArgb(0, 255, 0);
            //bakery(text,locations,thickness*0.5,"a_Score",green,unrollBake);
            bakery(allText, "a_Score", green, unrollBake);
            bakery(updateUnrollCurves, "a_Cut1", Color.Blue, unrollBake);
            bakery(updateUnrollSurfaces, "a_Cut2", Color.Magenta, unrollBake);
        }//end unroll










        outSurfaces = updateSurfaces;
        outCurves = updateCurves;

        outPlanes = planes;

        #endregion





    }

    // <Custom additional code> 


    #region customCode

    List<Plane> planes = new List<Plane>();
    double minY = 0.0;


    private void offsetPolyline(List<Polyline> polyLines, double distance, double angle, bool doubleSided, double positive, out List<Surface> outSurfaces) {




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
            double rotateAngle;
            rotateAngle = angle;
            //rotateAngle = angle + Math.Sin((double) i / polyLines.Count) * 180.0;

            //polyLines[i].ReduceSegments(0.001);

            if (polyLines[i].Count < 2) {
                outSurfaces = updateSurfaces;

            }

            if (plCurve.IsLinear()) {
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
                    //general case for non vertical lines
                } else {
                    Plane plum = new Plane(polyLines[i][0], polyLines[i][1], new Point3d(polyLines[i][0].X, polyLines[i][0].Y, polyLines[i][0].Z - 1.0));
                    Plane startCap = new Plane(polyLines[i][0], new Vector3d(polyLines[i][1] + polyLines[i][0]));
                    Line intersectionLine;
                    Rhino.Geometry.Intersect.Intersection.PlanePlane(plum, startCap, out intersectionLine);
                    Plane plane3 = new Plane(polyLines[i][0], intersectionLine.Direction, polyLines[i][0] - polyLines[i][1]);
                    startPlane = new Plane(polyLines[i][0], intersectionLine.Direction, plane3.Normal);
                }


                //angle zero is vertical
                startPlane.Rotate(angle * Math.PI / 180.0, startPlane.ZAxis);


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



            } else if (plCurve.IsPlanar() && angle == 0) {
                //the special case if the polyline points are coplanar and angle is equal to zero
                Plane plane1;
                plCurve.TryGetPlane(out plane1);
                Curve[] c0;
                Curve[] c1;
                if (doubleSided) {
                    c1 = plCurve.Offset(plane1, distance, 0.001, CurveOffsetCornerStyle.Sharp);
                    c0 = plCurve.Offset(plane1, -distance, 0.001, CurveOffsetCornerStyle.Sharp);
                } else {
                    c1 = plCurve.Offset(plane1, distance * positive, 0.001, CurveOffsetCornerStyle.Sharp);
                    c0 = plCurve.Offset(plane1, -0.001, 0.001, CurveOffsetCornerStyle.Sharp);
                }


                if (c1.Length > 1 || c0.Length>1) {

                    //Surface surface = (Surface)NurbsSurface.CreateRuledSurface(destination1, destination2);
                    //Curve[] booleanUnion = Curve.CreateBooleanUnion((IEnumerable<Curve>)C);
                    
                    List<Curve> cs = new List<Curve>();

                    //make end lines
                    for (int j = 0; j < Math.Min(c0.Length, c1.Length); j++) {
                        cs.Add(new LineCurve(c0[j].PointAtStart, c1[j].PointAtStart));
                        cs.Add(new LineCurve(c0[j].PointAtEnd, c1[j].PointAtEnd));
                    }

                    //brep gymnastics to get a single surface
                    cs.AddRange(c0);
                    cs.AddRange(c1);
                    Brep[] bs = Brep.CreatePlanarBreps(cs);
                    Brep[] union = Brep.CreateBooleanUnion(bs, 0.001);
                    for (int j = 0; j < union.Length; j++) {
                        
                    Curve[] edges = union[0].DuplicateNakedEdgeCurves(true, true);
                    Brep[] b = Brep.CreatePlanarBreps(edges);


                    for (int k = 0; k < b.Length; k++) {
                        if (b[k].Surfaces.Count >= 1) {
                            updateSurfaces.Add(b[k].Surfaces[0]);
                        }
                    }
                    }



                } else if (c1.Length == 1 && c0.Length == 1) {
                    //the special case for a well behaved planar offset
                    Curve[] offsetCurves = new Curve[2];
                    offsetCurves[0] = c0[0];
                    offsetCurves[1] = c1[0];
                    Brep[] lofts = Brep.CreateFromLoft(offsetCurves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);
                    if (lofts.Length > 0 && lofts[0].Surfaces.Count > 0) {
                        updateSurfaces.Add(lofts[0].Surfaces[0]);
                    }

                }









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
                    startPlane.Rotate(angle * Math.PI / 180.0, startPlane.ZAxis);

                    biPlanes[0] = startPlane;


                } else {
                    Plane plum = new Plane(polyLines[i][0], polyLines[i][1], new Point3d(polyLines[i][0].X, polyLines[i][0].Y, polyLines[i][0].Z - 1.0));
                    Plane startCap = new Plane(polyLines[i][0], new Vector3d(polyLines[i][1] - polyLines[i][0]));
                    Line intersectionLine;
                    Rhino.Geometry.Intersect.Intersection.PlanePlane(plum, startCap, out intersectionLine);
                    Vector3d normal = Vector3d.CrossProduct(intersectionLine.Direction, polyLines[i][1] - polyLines[i][0]);
                    biPlanes[0] = new Plane(polyLines[i][0], intersectionLine.Direction, normal);

                }

                if (polyLines[i][0].DistanceTo(polyLines[i][count]) < 0.001) {
                    biPlanes[count] = biPlanes[0];
                } else {
                    biPlanes[count] = new Plane(polyLines[i][count], new Vector3d(polyLines[i][count] - polyLines[i][count - 1]));
                }


                //start point
                biPlanes[0].Rotate(rotateAngle * Math.PI / 180.0, biPlanes[0].ZAxis);
                planes.Add(biPlanes[0]);
                double minOffset = -0.001;
                if (doubleSided) { minOffset = -distance; }

                corner0 = biPlanes[0].PointAt(minOffset, 0);
                corner1 = biPlanes[0].PointAt(distance * positive, 0);


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
    private void intersect(Surface[] iSurfaces, Surface[][] jHalves, out Curve[][] outNotches) {





        Curve[][] notches = new Curve[iSurfaces.Length][];
        for (int i = 0; i < iSurfaces.Length; ++i) {
            List<Curve> updateCurves1 = new List<Curve>();

            //each strip gets its own
            for (int j = 0; j < jHalves.Length; ++j) {
                List<Curve> intCurves = new List<Curve>();

                //do this in pairs
                for (int k = 0; k < jHalves[j].Length; k++) {
                    Curve[] intersectionCurves;
                    Point3d[] intersectionPoints;

                    //intersect
                    Rhino.Geometry.Intersect.Intersection.SurfaceSurface(iSurfaces[i], jHalves[j][k], 0.001,
                      out intersectionCurves, out intersectionPoints);
                    updateCurves1.AddRange(intersectionCurves);
                    intCurves.AddRange(intersectionCurves);
                }

                //make notch into a parallelogram
                //exclude the ends and any weird cases
                if (intCurves.Count == 2 && j != 0 && j != jHalves.Length - 1) {
                    for (int k = 1; k < intCurves.Count; k++) {
                        LineCurve line0 = new LineCurve(intCurves[k - 1].PointAtStart, intCurves[k].PointAtStart);
                        LineCurve line1 = new LineCurve(intCurves[k - 1].PointAtEnd, intCurves[k].PointAtEnd);
                        updateCurves1.Add(line0);
                        updateCurves1.Add(line1);
                    }
                }




            }
            notches[i] = updateCurves1.ToArray();
        }

        outNotches = notches;



    }
    private void unrollSrfs(Surface[] surfaces, Polyline[] iThreads, Curve[][] curves, out List<Surface> updateSurfaces, out List<Curve> updateCurves, out List<Point3d> updatePoints) {
        //Point3d[][] points = null;
        updateSurfaces = new List<Surface>();
        updateCurves = new List<Curve>();
        updatePoints = new List<Point3d>();
        double maxX = 0.0;
        double minX = 0.0;
        double maxY = 0.0;



        //work on one strip at a time
        for (int i = 0; i < surfaces.Length; i++) {

            Point3d[] pts = new Point3d[iThreads[i].Count];
            for (int j = 0; j < iThreads[i].Count; j++) {
                pts[j] = iThreads[i][j];
            }




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
            for (int j = 0; j < unrolledCurves.Length; j++) {
                unrolledCurves[j].Translate(-maxX + minX, 0, 0);
            }
            for (int j = 0; j < unrolledPoints.Length; j++) {
                unrolledPoints[j].X += (-maxX + minX);
            }
            for (int j = 0; j < unrolledDots.Length; j++) {
                unrolledDots[j].Translate(-maxX + minX, 0, 0);
            }

            //update global box
            minX -= maxX;
            maxX = 0.0;




            //convert to surfaces
            for (int j = 0; j < unrolledBreps.Length; j++) {
                updateSurfaces.AddRange(unrolledBreps[j].Surfaces);
            }


            updateCurves.AddRange(unrolledCurves);
            updatePoints.AddRange(unrolledPoints);
        }

        //move
        for (int j = 0; j < updateSurfaces.Count; j++) {
            updateSurfaces[j].Translate(0, -maxY + minY, 0);
        }
        for (int j = 0; j < updateCurves.Count; j++) {
            updateCurves[j].Translate(0, -maxY + minY, 0);
        }
        for (int j = 0; j < updatePoints.Count; j++) {
            updatePoints[j] = new Point3d(updatePoints[j].X, updatePoints[j].Y + (-maxY + minY), updatePoints[j].Z);
        }
        //for (int j = 0; j < updateDots.Count; j++) {
        //    updateDots[j].Translate(0,-maxX + minX, 0);
        //}

        minY -= maxY;
        maxY = 0.0;




    }
    private Surface[][] offsetSurfaces(List<Surface> jHalves, double thickness) {

        Surface[][] jHalvesOffset = new Surface[jHalves.Count][];

        for (int i = 0; i < jHalves.Count; i++) {
            Surface[] surfaces = new Surface[2];

            surfaces[0] = jHalves[i].Offset(thickness / 2.0, 0.001);
            surfaces[1] = jHalves[i].Offset(-thickness / 2.0, 0.001);

            jHalvesOffset[i] = surfaces;
        }
        return jHalvesOffset;

    }
    void bakery(List<Brep> objs, string layer, System.Drawing.Color color, bool activate) {
        //Inserts geometry into the Rhino document, with custom attributes
        //Written by Giulio Piacentino - rewritten by [uto] for GH 07
        //Version written 2010 09 27 - for Grasshopper 0.7.0055

        //obj is geometry
        if (objs == null) return;
        if (activate) {
            for (int i = 0; i < objs.Count; ++i) {
                Brep obj = objs[i];
                //Make new attribute
                Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
                Guid o_id = default(Guid);

                ////Set object name
                //if (name != null && Rhino.DocObjects.Layer.IsValidName(name) == true) {
                //    att.Name = name;
                //}

                //Set layer
                if (layer != null && Rhino.DocObjects.Layer.IsValidName(layer) == true) {
                    //Get the current layer index

                    Rhino.DocObjects.Tables.LayerTable layerTable = RhinoDocument.Layers;
                    int layerIndex = layerTable.Find(layer, true);
                    if (layerIndex == -1) { //This layer does not exist, we add it
                        //make new layer
                        Rhino.DocObjects.Layer myLayer = new Rhino.DocObjects.Layer();
                        myLayer.Name = layer;
                        myLayer.Color = color;
                        //Add the layer to the layer table
                        layerIndex = layerTable.Add(myLayer);
                    }

                    if (layerIndex > -1) { //We manged to add layer!
                        att.LayerIndex = layerIndex;
                        //__out.Add("Added new layer to the document at position " + layerIndex + " named " + layer + ". ");
                    }
                    //else {
                    //att.LayerIndex = layerIndex;
                    ////__out.Add("Added to existing layer " + layerIndex + " named " + layer + ". ");
                    //}

                }

                //Set color
                if (color != null) {
                    att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromLayer;
                    att.ObjectColor = color;
                    att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromLayer;
                    att.PlotColor = color;
                }

                o_id = RhinoDocument.Objects.AddBrep(obj, att);

            }
        }
    }
    void bakery(List<Surface> objs, string layer, System.Drawing.Color color, bool activate) {
        //Inserts geometry into the Rhino document, with custom attributes
        //Written by Giulio Piacentino - rewritten by [uto] for GH 07
        //Version written 2010 09 27 - for Grasshopper 0.7.0055

        //obj is geometry
        if (objs == null) return;
        if (activate) {
            for (int i = 0; i < objs.Count; ++i) {
                Surface obj = objs[i];
                //Make new attribute
                Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
                Guid o_id = default(Guid);

                ////Set object name
                //if (name != null && Rhino.DocObjects.Layer.IsValidName(name) == true) {
                //    att.Name = name;
                //}

                //Set layer
                if (layer != null && Rhino.DocObjects.Layer.IsValidName(layer) == true) {
                    //Get the current layer index

                    Rhino.DocObjects.Tables.LayerTable layerTable = RhinoDocument.Layers;
                    int layerIndex = layerTable.Find(layer, true);
                    if (layerIndex == -1) { //This layer does not exist, we add it
                        //make new layer
                        Rhino.DocObjects.Layer myLayer = new Rhino.DocObjects.Layer();
                        myLayer.Name = layer;
                        myLayer.Color = color;
                        //Add the layer to the layer table
                        layerIndex = layerTable.Add(myLayer);
                    }

                    if (layerIndex > -1) { //We manged to add layer!
                        att.LayerIndex = layerIndex;
                        //__out.Add("Added new layer to the document at position " + layerIndex + " named " + layer + ". ");
                    }
                    //else {
                    //att.LayerIndex = layerIndex;
                    ////__out.Add("Added to existing layer " + layerIndex + " named " + layer + ". ");
                    //}

                }

                //Set color
                if (color != null) {
                    att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromLayer;
                    att.ObjectColor = color;
                    att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromLayer;
                    att.PlotColor = color;
                    att.WireDensity = -1;
                }

                o_id = RhinoDocument.Objects.AddSurface(obj, att);

            }
        }
    }
    void bakery(List<Curve> objs, string layer, System.Drawing.Color color, bool activate) {
        //Inserts geometry into the Rhino document, with custom attributes
        //Written by Giulio Piacentino - rewritten by [uto] for GH 07
        //Version written 2010 09 27 - for Grasshopper 0.7.0055

        //obj is geometry
        if (objs == null) return;
        if (activate) {
            for (int i = 0; i < objs.Count; ++i) {
                Curve obj = objs[i];
                //Make new attribute
                Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
                Guid o_id = default(Guid);

                ////Set object name
                //if (name != null && Rhino.DocObjects.Layer.IsValidName(name) == true) {
                //    att.Name = name;
                //}

                //Set layer
                if (layer != null && Rhino.DocObjects.Layer.IsValidName(layer) == true) {
                    //Get the current layer index

                    Rhino.DocObjects.Tables.LayerTable layerTable = RhinoDocument.Layers;
                    int layerIndex = layerTable.Find(layer, true);
                    if (layerIndex == -1) { //This layer does not exist, we add it
                        //make new layer
                        Rhino.DocObjects.Layer myLayer = new Rhino.DocObjects.Layer();
                        myLayer.Name = layer;
                        myLayer.Color = color;
                        //Add the layer to the layer table
                        layerIndex = layerTable.Add(myLayer);
                    }

                    if (layerIndex > -1) { //We manged to add layer!
                        att.LayerIndex = layerIndex;
                        //__out.Add("Added new layer to the document at position " + layerIndex + " named " + layer + ". ");
                    }
                    //else {
                    //att.LayerIndex = layerIndex;
                    ////__out.Add("Added to existing layer " + layerIndex + " named " + layer + ". ");
                    //}

                }

                //Set color
                if (color != null) {
                    att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromLayer;
                    att.ObjectColor = color;
                    att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromLayer;
                    att.PlotColor = color;
                }

                o_id = RhinoDocument.Objects.AddCurve((Curve)obj, att);

            }
        }
    }
    void bakery(List<string> objs, List<Point3d> locations, double height, string layer, System.Drawing.Color color, bool activate) {
        //Inserts geometry into the Rhino document, with custom attributes
        //Written by Giulio Piacentino - rewritten by [uto] for GH 07
        //Version written 2010 09 27 - for Grasshopper 0.7.0055

        //obj is geometry
        if (objs == null) return;
        if (activate) {
            for (int i = 0; i < objs.Count; ++i) {
                Plane plane = new Plane(locations[i], Vector3d.ZAxis);

                Rhino.Display.Text3d obj = new Rhino.Display.Text3d(objs[i], plane, height);
                //obj.FontFace = "Arial";
                //obj.FontFace = "Machine Tool SanSerif";
                obj.FontFace = "1CAMBam_Stick_9";
                //Make new attribute
                Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
                Guid o_id = default(Guid);

                //Set object name
                if (objs[i] != null && Rhino.DocObjects.Layer.IsValidName(objs[i]) == true) {
                    att.Name = objs[i];
                }



                //int currentLayerIndex = RhinoDocument.Layers.CurrentLayerIndex;
                //att.LayerIndex = currentLayerIndex;
                //Set layer
                if (layer != null && Rhino.DocObjects.Layer.IsValidName(layer) == true) {
                    //Get the current layer index

                    Rhino.DocObjects.Tables.LayerTable layerTable = RhinoDocument.Layers;
                    int layerIndex = layerTable.Find(layer, true);
                    if (layerIndex == -1) { //This layer does not exist, we add it
                        //make new layer
                        Rhino.DocObjects.Layer myLayer = new Rhino.DocObjects.Layer();
                        myLayer.Name = layer;
                        myLayer.Color = color;
                        //Add the layer to the layer table
                        layerIndex = layerTable.Add(myLayer);

                        if (layerIndex > -1) { //We manged to add layer!
                            att.LayerIndex = layerIndex;
                            //__out.Add("Added new layer to the document at position " + layerIndex + " named " + layer + ". ");
                        } else {
                            //__out.Add("Layer did not add. Try cleaning up your layers."); //This never happened to me.
                        }
                    } else {
                        att.LayerIndex = layerIndex;
                        //__out.Add("Added to existing layer " + layerIndex + " named " + layer + ". ");
                    }
                }










                //Set color
                if (color != null) {
                    att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromLayer;
                    att.ObjectColor = color;
                    att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromLayer;
                    att.PlotColor = color;

                }

                o_id = RhinoDocument.Objects.AddText(obj, att);

            }
        }
    }
    //  private void group(string group_name, ref object A)
    //  {
    //    List<GeometryBase> geo_out = new List<GeometryBase>();
    //    Rhino.DocObjects.Tables.GroupTable g;
    //    RhinoDoc RhinoDocument;
    //    int index = RhinoDocument.Groups.Find(group_name, true);
    //    List<RhinoObject> objs = new List<RhinoObject>();
    //    objs.AddRange(RhinoDocument.Groups.GroupMembers(index));
    //    foreach (RhinoObject obj in objs) {
    //      geo_out.Add(obj.Geometry);
    //    }
    //    A = geo_out;
    //
    //  }
    private void singleLineFont(string text, Point3d location, double height, out PolylineCurve[] singleLineText) {

        //makes a font instance copy
        string font = "Machine Tool SanSerif";
        double precision = 50;
        Plane plane = new Plane(location, Vector3d.ZAxis);
        System.Drawing.Font localFont;
        try {
            localFont = new System.Drawing.Font(font, (float)height);
        } catch {
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



    #endregion





    // </Custom additional code> 
}