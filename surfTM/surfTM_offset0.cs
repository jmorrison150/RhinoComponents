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
    private void RunScript(List<Point3d> points, List<Curve> iThreads, List<Curve> jThreads, double offset, double thickness, bool unroll,
        ref object outCurves, ref object outUnrolled, ref object outSurfaces, ref object outPoints, ref object outRolled) {








        #region offsetCurves
        //inputs

        List<Brep> updateBreps = new List<Brep>();
        List<Point3d> updatePoints = new List<Point3d>();
        List<Curve> updateCurves = new List<Curve>();
        List<Surface> updateSurfaces = new List<Surface>();
        Surface[] iThreadSurfaces;
        Surface[] jThreadSurfaces;
        //List<Surface> iCuts;
        //List<Surface> jCuts;
        Curve[] slices;

        iThreadSurfaces = threadSurfaces(iThreads, -offset, offset);
        jThreadSurfaces = threadSurfaces(jThreads, -offset, offset);

        //Surface[] iRemove = threadSurfaces(iThreads, 0.0, offset * 1.2);
        //Curve[][] iCuts = intersect(jThreadSurfaces,iRemove);

            //iCuts = extrude(iThreadSurfaces.ToList(), 0.0, offset * 1.2);
           
            //jCuts = extrude(jThreadSurfaces.ToList(), -offset, offset);
        

        //List<Surface> jWithCuts = subtract(jThreadSurfaces.ToList(),iCuts);
        //List<Surface> jCuts = extrude(jWithCuts, thickness);
        //List<Surface> solids = extrude(iThreadSurfaces, thickness);






        //unroller

        bool spaced = true;
        List<Brep> unrolledBreps;
        List<Point3d> unrolledPoints;
        List<Curve> unrolledCurves;
        if (unroll) {

            List<Point3d> unrollPoints = points;

            //slices = intersectSurfaces(iThreadSurfaces, jThreadSurfaces);
            slices = new Curve[1];
            unrollSurfaces(iThreadSurfaces, unrollPoints, slices, out unrolledBreps, out unrolledPoints, out unrolledCurves);

            updateBreps.AddRange(unrolledBreps);
            updatePoints.AddRange(unrolledPoints);
            updateCurves.AddRange(unrolledCurves);



            //offset the unrolled surfaces
            if (spaced) {
                Point3d MaxY = Point3d.Origin;
                for (int j = 0; j < updateBreps.Count; ++j) {
                    BoundingBox bb = updateBreps[j].GetBoundingBox(false);
                    if (MaxY.Y < bb.Max.Y) {
                        MaxY = bb.Max;
                    }
                }


                for (int j = 0; j < updateBreps.Count; ++j) {
                    updateBreps[j].Translate(0, -MaxY.Y, 0);
                }
                for (int j = 0; j < updatePoints.Count; ++j) {
                    Point3d move = new Point3d(updatePoints[j].X, updatePoints[j].Y - MaxY.Y, updatePoints[j].Z);
                    updatePoints[j] = move;
                }
                for (int j = 0; j < updateCurves.Count; ++j) {
                    updateCurves[j].Translate(0, -MaxY.Y, 0);
                }


            }//end space out

            unrollSurfaces(jThreadSurfaces, unrollPoints, slices, out unrolledBreps, out unrolledPoints, out unrolledCurves);
            updateBreps.AddRange(unrolledBreps);
            updatePoints.AddRange(unrolledPoints);
            updateCurves.AddRange(unrolledCurves);




        }

            Curve[] iOutlines = getOutlines(iThreadSurfaces);
            Curve[] jOutlines = getOutlines(jThreadSurfaces);
            updateCurves.AddRange(iOutlines);
            updateCurves.AddRange(jOutlines);

        //updateSurfaces.AddRange(iCuts);
        //updateSurfaces.AddRange(jCuts);

        outPoints = updatePoints;
        outCurves = updateCurves;
        outUnrolled = updateBreps;
        outRolled = updateSurfaces;


        #endregion









    }

    // <Custom additional code> 


    #region customCode
    public Curve[][] makeWireframes(List<Curve> threads, double minOffset, double maxOffset, double thickness) {

        //check planar
        List<Curve> iThreads = new List<Curve>();
        for (int i = 0; i < threads.Count; ++i) {
            if (threads[i].IsPlanar()) {
                iThreads.Add(iThreads[i]);
            }
        }


        //each curve will have a group of 4 polylines that goes with it
        Curve[][] updatePolylines = new Curve[iThreads.Count][];
        for (int i = 0; i < updatePolylines[i].Length; ++i) {
            updatePolylines[i] = new Curve[4];
        }




        //work on each curve
        for (int i = 0; i < iThreads.Count; ++i) {
            Plane plane;
            try {
                plane = new Plane(iThreads[i].PointAtStart, iThreads[i].PointAtNormalizedLength(0.5), iThreads[i].PointAtEnd);
            } catch {
                plane = Plane.WorldXY;
                //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "some curves were not planar" +  e.Message);
                //Print("some curves were not planar");
            }


            //make 4 curves at the corners and 2 at the ends
            Curve[] offsetCurves = new Curve[4];
            Curve[] c;
            if (maxOffset == 0) { c = new Curve[1]; c[0] = iThreads[i]; } else {
                c = iThreads[i].Offset(plane, maxOffset, 0.001, CurveOffsetCornerStyle.Sharp);
            }
            if (c.Length == 1) {
                c[0].Translate(plane.ZAxis * -thickness / 2);
                updatePolylines[i][0] = c[0];  //bottom right
                c[0].Translate(plane.ZAxis * thickness);
                updatePolylines[i][1] = c[0];  //top right
            }

            if (maxOffset == 0) { c = new Curve[1]; c[0] = iThreads[i]; } else {
                c = iThreads[i].Offset(plane, minOffset, 0.001, CurveOffsetCornerStyle.Sharp);
            }
            if (c.Length == 1) {
                c[0].Translate(plane.ZAxis * thickness / 2);
                updatePolylines[i][2] = c[0];  //top left
                c[0].Translate(plane.ZAxis * -thickness);
                updatePolylines[i][3] = c[0];  //bottom left
            }







            //connect all 4 copies of the iThread
            //Brep[] wrappers = Brep.CreateFromLoft(offsetCurves, Point3d.Unset, Point3d.Unset, LoftType.Straight, true);



            ////end caps
            //Point3d[] startPoints0 = new Point3d[2];
            //Point3d[] startPoints1 = new Point3d[2];
            //Point3d[] endPoints0 = new Point3d[2];
            //Point3d[] endPoints1 = new Point3d[2];
            //PolylineCurve[] startLines = new PolylineCurve[2];
            //PolylineCurve[] endLines = new PolylineCurve[2];

            //startPoints0[0] = offsetCurves[0].PointAtStart;
            //startPoints0[1] = offsetCurves[3].PointAtStart;
            //startPoints1[0] = offsetCurves[1].PointAtStart;
            //startPoints1[1] = offsetCurves[2].PointAtStart;
            //endPoints0[0] = offsetCurves[0].PointAtStart;
            //endPoints0[1] = offsetCurves[3].PointAtStart;
            //endPoints1[0] = offsetCurves[1].PointAtStart;
            //endPoints1[1] = offsetCurves[2].PointAtStart;

            //startLines[0] = new PolylineCurve(startPoints0);
            //startLines[1] = new PolylineCurve(startPoints1);
            //endLines[0] = new PolylineCurve(endPoints0);
            //endLines[1] = new PolylineCurve(endPoints1);

            //Brep[] starts = Brep.CreateFromLoft(startLines, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);
            //Brep[] ends = Brep.CreateFromLoft(endLines, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);

            ////union
            //List<Brep> breps3 = new List<Brep>();
            //breps3.AddRange(wrappers);
            //breps3.AddRange(starts);
            //breps3.AddRange(ends);

            //Brep[] closedBrep = Brep.CreateBooleanUnion(breps3, RhinoMath.UnsetValue);
            //bool closed = closedBrep[0].IsSolid;
            //bool valid = closedBrep[0].IsValid;




            //Brep.CreateFromLoft
            //for (int k = 0; k < 4; ++k) {
            //    startPoints[k] = offsetCurves[k].PointAtStart;
            //    endPoints[k] = offsetCurves[k].PointAtEnd;
            //}
            //offsetCurves[4] = new PolylineCurve(startPoints);
            //offsetCurves[5] = new PolylineCurve(endPoints);





            //Curve[] offsetCurves = new Curve[2];

            ////make single plane based on start point, mid point, and end point
            ////default to ground plane
            //Plane plane;
            //try {
            //    plane = new Plane(iThreads[i].PointAtStart, iThreads[i].PointAtNormalizedLength(0.5), iThreads[i].PointAtEnd);
            //} catch { plane = Plane.WorldXY; }
            ////offset
            //if (offsetMax == 0) {
            //    offsetCurves[0] = iThreads[i];
            //} else {
            //    offsetCurves[0] = iThreads[i].Offset(plane, offsetMax, 0.001, CurveOffsetCornerStyle.Sharp)[0];
            //}
            ////negative offset
            //if (offsetMin == 0) {
            //    offsetCurves[1] = iThreads[i];
            //} else {
            //    offsetCurves[1] = iThreads[i].Offset(plane, offsetMin, 0.001, CurveOffsetCornerStyle.Sharp)[0];
            //}

            //Brep[] lofts = Brep.CreateFromLoft(offsetCurves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);
            //if (lofts.Length > 0 && lofts[0].Surfaces.Count > 0) {
            //    surfaces[i] = lofts[0].Surfaces[0];
            //}
        }
        return updatePolylines;
    }

    Brep[] makeSolids(Curve[][] wireframes) {
        List<Brep> solids = new List<Brep>();

        for (int i = 0; i < wireframes.Length; ++i) {
            List<Brep> solid = new List<Brep>();
            Brep[] sides = Brep.CreateFromLoft(wireframes[i], Point3d.Unset, Point3d.Unset, LoftType.Straight, true);
            Brep endFace = Brep.CreateFromCornerPoints(wireframes[i][0].PointAtEnd, wireframes[i][1].PointAtEnd, wireframes[i][2].PointAtEnd, wireframes[i][3].PointAtEnd, 0.001);
            Brep startFace = Brep.CreateFromCornerPoints(wireframes[i][0].PointAtStart, wireframes[i][1].PointAtStart, wireframes[i][2].PointAtStart, wireframes[i][3].PointAtStart, 0.001);

            solid.AddRange(sides);
            solid.Add(endFace);
            solid.Add(startFace);

            Brep.CreateSolid(solid, 0.001);
            //Brep.CreateBooleanUnion(solids, 0.001);

            solids.AddRange(solid);
        }



        return solids.ToArray();

    }


    Brep[] thickenThreadBrep(List<Curve> iThreads, double offsetMin, double offsetMax, double thickness) {
        Brep[] updateBreps = new Brep[iThreads.Count];
        Surface[] surfs = threadSurfaces(iThreads, offsetMin, offsetMax);
        for (int i = 0; i < surfs.Length; ++i) {
            surfs[i].Offset(offsetMin, 0.001);
            surfs[i].Offset(offsetMax, 0.001);

            Brep b = surfs[i].ToBrep();
            Curve[] surfEdges = b.DuplicateEdgeCurves(true);
            //surfEdges.Length == 4;
            //Print(surfEdges.Length.ToString());
            //Rhino.Geometry.Extrusion.CreateExtrusion
            //surfs[i].
        }

        return updateBreps;
    }
    public Mesh[] thickenThreadMesh(List<Curve> iThreads, double offsetMin, double offsetMax, double thickness) {
        Mesh[] updateMeshes = new Mesh[iThreads.Count];
        Surface[] surfaces = new Surface[iThreads.Count];
        //Brep[] breps = new Brep[iThreads.Count];

        for (int i = 0; i < iThreads.Count; ++i) {
            Curve[] offsetCurves = new Curve[2];

            //make single plane based on start point, mid point, and end point
            //default to ground plane
            Plane plane;
            try {
                plane = new Plane(iThreads[i].PointAtStart, iThreads[i].PointAtNormalizedLength(0.5), iThreads[i].PointAtEnd);
            } catch { plane = Plane.WorldXY; }
            //offset
            if (offsetMax == 0) {
                offsetCurves[0] = iThreads[i];
            } else {
                offsetCurves[0] = iThreads[i].Offset(plane, offsetMax, 0.001, CurveOffsetCornerStyle.Sharp)[0];
            }
            //negative offset
            if (offsetMin == 0) {
                offsetCurves[1] = iThreads[i];
            } else {
                offsetCurves[1] = iThreads[i].Offset(plane, offsetMin, 0.001, CurveOffsetCornerStyle.Sharp)[0];
            }

            //loft
            Brep[] lofts = Brep.CreateFromLoft(offsetCurves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);
            if (lofts.Length > 0 && lofts[0].Surfaces.Count > 0) {
                surfaces[i] = lofts[0].Surfaces[0];


                //make a closed polyline
                Polyline polyline0;
                Polyline polyline1;
                offsetCurves[0].TryGetPolyline(out polyline0);
                offsetCurves[1].Reverse();
                offsetCurves[1].TryGetPolyline(out polyline1);
                polyline0.AddRange(polyline1);
                polyline0.Add(polyline0[0]);


                //offset mesh
                try {
                    Mesh m = Mesh.CreateFromClosedPolyline(polyline0);
                    m.FaceNormals.ComputeFaceNormals();
                    m.Weld(0.001);
                    Mesh m0, m1;

                    if (thickness == 0) {
                        m0 = m;
                        m1 = m;
                    } else {
                        m0 = m.Offset(-thickness, true);
                        m1 = m.Offset(thickness, true);
                    }

                    m0.Append(m1);
                    updateMeshes[i] = m0;
                } catch { }
            }
        }//end each thread

        return updateMeshes;
    }
    public Surface[] threadSurfaces(List<Curve> jThreads, double offsetMin, double offsetMax) {
        //curves = new Curve[jThreads.Count];
        Surface[] surfaces = new Surface[jThreads.Count];
        //Brep[] breps = new Brep[jThreads.Count];

        for (int i = 0; i < jThreads.Count; ++i) {
            Curve[] offsetCurves = new Curve[2];

            //make single plane based on start point, mid point, and end point
            //default to ground plane
            Plane plane;
            try {
                plane = new Plane(jThreads[i].PointAtStart, jThreads[i].PointAtNormalizedLength(0.5), jThreads[i].PointAtEnd);
            } catch { plane = Plane.WorldXY; }
            //offset
            if (offsetMax == 0) {
                offsetCurves[0] = jThreads[i];
            } else {
                offsetCurves[0] = jThreads[i].Offset(plane, offsetMax, 0.001, CurveOffsetCornerStyle.Sharp)[0];
            }
            //negative offset
            if (offsetMin == 0) {
                offsetCurves[1] = jThreads[i];
            } else {
                offsetCurves[1] = jThreads[i].Offset(plane, offsetMin, 0.001, CurveOffsetCornerStyle.Sharp)[0];
            }

            Brep[] lofts = Brep.CreateFromLoft(offsetCurves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);
            if (lofts.Length > 0 && lofts[0].Surfaces.Count > 0) {
                surfaces[i] = lofts[0].Surfaces[0];
            }
        }
        return surfaces;
    }
    public void unrollSurfaces(Surface[] threadSurfaces, List<Point3d> points, Curve[] slices, out List<Brep> outUnrolled, out List<Point3d> outPoints, out List<Curve> outCurves) {
        //unroll///////////////////////////////////////////////////////////////////////////////////////////

        List<Brep> updateBreps = new List<Brep>();
        List<Point3d> updatePoints = new List<Point3d>();
        List<Curve> updateCurves = new List<Curve>();


            Surface[] surfaces = threadSurfaces;


            for (int i = 0; i < surfaces.Length; ++i) {
                Curve[] unrolledCurves;
                Point3d[] unrolledPoints;
                TextDot[] unrolledDots;

                Unroller un = new Unroller(surfaces[i]);
                un.ExplodeOutput = false;
                un.AddFollowingGeometry(points);
                //un.AddFollowingGeometry(curves);
                un.AddFollowingGeometry(slices);
                Brep[] unrolledBreps = un.PerformUnroll(out unrolledCurves, out unrolledPoints, out unrolledDots);


                updateBreps.AddRange(unrolledBreps);
                updatePoints.AddRange(unrolledPoints);
                updateCurves.AddRange(unrolledCurves);


                //spaced out
                    Point3d MaxX = Point3d.Origin;
                    for (int j = 0; j < unrolledBreps.Length; ++j) {
                        BoundingBox bb = unrolledBreps[j].GetBoundingBox(false);
                        if (MaxX.X < bb.Max.X) {
                            MaxX = bb.Max;
                        }
                    }


                    for (int j = 0; j < updateBreps.Count; ++j) {
                        updateBreps[j].Translate(-MaxX.X, 0, 0);
                    }
                    for (int j = 0; j < updatePoints.Count; ++j) {
                        Point3d maxX = new Point3d(updatePoints[j].X - MaxX.X, updatePoints[j].Y, updatePoints[j].Z);
                        updatePoints[j] = maxX;
                    }
                    for (int j = 0; j < updateCurves.Count; ++j) {
                        updateCurves[j].Translate(-MaxX.X, 0, 0);
                    }

            }//end each surface






        outUnrolled = updateBreps;
        outPoints = updatePoints;
        outCurves = updateCurves;
    }//end unrollSurfaces
    //public Surface[] subtract(Brep[] cuts, Brep[] jThreadSurfaces) {

    //Surface[] updateSubtract = new Surface[jThreadSurfaces.Length];
    //for (int j = 0; j < jThreadSurfaces.Length; ++j) {
    //    if (jThreadSurfaces[j].Surfaces.Count > 0) {
    //        updateSubtract[j] = jThreadSurfaces[j].Surfaces[0];
    //    }
    //}





    ////Brep[] updateSubtract = new Brep[jThreadSurfaces.Length];
    //Brep[] cutsBrep = new Brep[cuts.Length];
    //for (int i = 0; i < cuts.Length; ++i) {
    //    cutsBrep[i] = Brep.CreateFromMesh(cuts[i], true);
    //}


    //Brep[] updateSubtract = Brep.CreateBooleanIntersection(jThreadSurfaces,cutsBrep,0.001)


    //for (int j = 0; j < jThreadSurfaces.Length; ++j) {
    //    for (int i = 0; i < cutsBrep.Length; ++i) {
    //        Brep[] bs = Brep.CreateBooleanIntersection(
    //            //Rhino.Geometry.Brep.CreateBooleanDifference(cutsBrep[i], jThreadSurfaces[j], 0.001);
    //        if (bs.Length > 0) {
    //            updateSubtract[j] = bs[0];

    //        }
    //        Print(bs.Length.ToString());
    //    }
    //}






    //    return updateSubtract;
    //}
    List<Surface> extrude(List<Surface> surfaces, double thickness) {
        List<Surface> solids = new List<Surface>();

        //for (int i = 0; i < surfaces.Count; ++i) {
        //    Surface surf0 = surfaces[i].Offset(-thickness / 2.0, 0.001);
        //    Curve[] crvs = surf0.ToBrep().GetWireframe(0);

        //    Curve[] planarCurve = Curve.JoinCurves(crvs);
        //    if (planarCurve.Length > 1) {
        //        Surface solid = Rhino.Geometry.Extrusion.Create(planarCurve[0], thickness, true);
        //        if (solid.IsSolid) { solids.Add(solid); }
        //    }
        //}
        return solids;
    }
    List<Surface> extrude(List<Surface> surfaces, double minOffset, double maxOffset) {
        List<Surface> solids = new List<Surface>();

        for (int i = 0; i < surfaces.Count; ++i) {
            Surface surf1 = surfaces[i].Offset(maxOffset, 0.001);
            Surface surf0 = surfaces[i].Offset(minOffset, 0.001);


            //      Vector3d dir = surf1.NormalAt(surf1.Domain(0).Mid, surf1.Domain(1).Mid);
            //      dir.Unitize();
            //      dir *= maxOffset - minOffset;
            //      Curve curve0 = (Curve) null;
            //      GH_Convert.ToCurve((object) surfaces[i], ref curve0, GH_Conversion.Both);
            //      Surface solid;
            //      //Reflect(curve0);
            //      solid = Surface.CreateExtrusion(curve0, dir);
            //      if (solid.IsValid) {
            //        // Print(i.ToString());
            //        solids.Add(solid);
            solids.Add(surf0);
            solids.Add(surf1);
            //      }



        }
        return solids;
    }




    //for(int i=0;i<iThreads.Count;++i){
    //    Plane plane;
    //    try { plane = new Plane(iThreads[i].PointAtStart, iThreads[i].PointAtNormalizedLength(0.5), iThreads[i].PointAtEnd);
    //    } catch (Exception e){
    //        plane = Plane.WorldXY;
    //        //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "some curves were not planar" +  e.Message);
    //        //Print("some curves were not planar");
    //    }


    //    //make 4 curves at the corners and 2 at the ends
    //    Curve[] offsetCurves = new Curve[6];
    //    Curve[] offsets0;
    //    if (maxOffset == 0) { offsets0 = new Curve[1]; offsets0[0] = iThreads[i]; } else {
    //        offsets0 = iThreads[i].Offset(plane, maxOffset, 0.001, CurveOffsetCornerStyle.Sharp);
    //    }
    //    if (offsets0.Length == 1) {
    //        offsets0[0].Translate(plane.ZAxis * thickness / 2);
    //        offsetCurves[0] = offsets0[0];  //top right
    //        offsets0[0].Translate(plane.ZAxis * -thickness);
    //        offsetCurves[1] = offsets0[0];  //bottom right
    //    }
    //        //else {
    //        //    Print("problem offseting curves");
    //        //}
    //        //try { offsetCurves[0] = Curve.JoinCurves(offsets0, 0.001, true)[0]; } catch {
    //        //    //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "problem joining curves" +  e.Message);
    //        //    //Print("problem joining curves");
    //        //}
    //    Curve[] offsets2;
    //    if (maxOffset == 0) { offsets2 = new Curve[1]; offsets2[0] = iThreads[i]; } else {
    //        offsets2 = iThreads[i].Offset(plane, minOffset, 0.001, CurveOffsetCornerStyle.Sharp);
    //    }
    //    if (offsets2.Length == 1) {
    //        offsets2[0].Translate(plane.ZAxis * thickness / 2);
    //        offsetCurves[2] = offsets2[0];  //top left
    //        offsets2[0].Translate(plane.ZAxis * -thickness);
    //        offsetCurves[3] = offsets2[0];  //bottom left
    //    }
    //    Point3d[] startPoints = new Point3d[4];
    //    Point3d[] endPoints = new Point3d[4];
    //    for(int k=0;k<4;++k){
    //        startPoints[k] = offsetCurves[k].PointAtStart;
    //        endPoints[k] = offsetCurves[k].PointAtEnd;
    //    }
    //    offsetCurves[4] = new PolylineCurve(startPoints);
    //    offsetCurves[5] = new PolylineCurve(endPoints);


    //}


    //Brep[] breps = new Brep[surfaces.Length];
    //for (int i = 0; i < surfaces.Length;++i ) {
    //    //Brep.CreateFromSurface(surfaces[i]);
    //    Brep.

    //    //Brep b  = Brep.CreateFromOffsetFace(
    //    //surfaces[i]


    //    //breps[i] =
    //}

    //    return solids;
    //}

    List<Surface> subtract(List<Surface> keeps, List<Surface> removes) {
        List<Surface> surfaces = new List<Surface>();
        List<Brep> keepBreps = new List<Brep>();

        //convert
        Brep[] containers = new Brep[removes.Count];
        for (int i = 0; i < removes.Count; ++i) {
            containers[i] = removes[i].ToBrep();
        }


        //test
        for (int i = 0; i < keeps.Count; ++i) {
            for (int j = 0; j < containers.Length; ++j) {
                Brep[] tests = keeps[i].ToBrep().Split(removes[j].ToBrep(), 0.001);
                for (int k = 0; k < tests.Length; ++k) {
                    bool inside = isInside(tests[k], containers[j]);
                    if (!inside) { keepBreps.Add(tests[k]); }
                }
            }
        }

        for (int i = 0; i < keepBreps.Count; ++i) {
            if (keepBreps[i].Surfaces.Count > 1) {
                surfaces.Add(keepBreps[i].Surfaces[0]);
            }
        }


        return surfaces;
    }

    bool isInside(Brep keep, Brep removes) {
        Point3d[] points = keep.DuplicateVertices();
        for (int i = 0; i < points.Length; ++i) {
            Point3d point = points[i];
            if (!removes.IsPointInside(point, 0.001, false)) { return false; }
        }
        return true;
    }

    Curve[] getOutlines(Surface[] surfaces) {
        Curve[] outlines = new Curve[surfaces.Length];

        for (int i = 0; i < surfaces.Length; ++i) {
            Curve curve0 = (Curve)null;
            //GH_Convert.ToCurve_Primary((object)surfaces[i], ref curve0);
            GH_Convert.ToCurve((object)surfaces[i], ref curve0, GH_Conversion.Both);
            outlines[i] = curve0;
        }

        return outlines;

    }

    Curve[] intersectSurfaces(Surface[] iThreads, Surface[] jThreads) { 

    Curve[][] lines = new Curve[iThreads.Length][];
    for (int i = 0; i < lines.Length;++i ) { 
            lines[i] = new Curve[jThreads.Length];
            for (int j = 0; j < lines[i].Length;++j ) {
                Curve[] intersectionCurves;
                Point3d[] intersectionPoints;
                Rhino.Geometry.Intersect.Intersection.SurfaceSurface(iThreads[i], jThreads[j],0.001, out intersectionCurves, out intersectionPoints);
                if(intersectionCurves.Length>1){
                    lines[i][j] = intersectionCurves[0];
                }
            }
        }
        List<Curve> slices = new List<Curve>();
        for (int i = 0; i < lines.Length; ++i) {
            for (int j = 0; j < lines[i].Length; ++j) {
                slices.Add(lines[i][j]);
            }
        }

        return slices.ToArray();

    }

    #endregion







    // </Custom additional code> 
}