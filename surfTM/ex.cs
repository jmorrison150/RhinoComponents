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
    private void RunScript(List<Point3d> points, List<Curve> iThreads, List<Curve> jThreads, double offset, double thickness, bool solid, bool unroll, bool spaced, ref object outSurfaces, ref object outMeshes, ref object outUnrolled, ref object outPoints, ref object outCurves) {






        #region offsetCurves
        bool intersect = false;
        if (offset == 0) { offset = 0.001; }
        List<Brep> updateBreps = new List<Brep>();
        List<Point3d> updatePoints = new List<Point3d>();
        List<Curve> updateCurves = new List<Curve>();
        List<Mesh> updateMeshes = new List<Mesh>();
        Mesh[] meshCuts;
        Mesh[] iThicken = new Mesh[iThreads.Count];
        Mesh[] jThicken = new Mesh[jThreads.Count];

        if (solid) {
            //make surfaces
            meshCuts = threadThickMesh(iThreads, -offset * 1.2, 0.0, thickness);
            iThicken = threadThickMesh(iThreads, -offset, offset, thickness);
            jThicken = threadThickMesh(jThreads, -offset, offset, thickness);
        }

        //Brep[] brepCuts = threadThickBrep(iThreads, 0.0, offset * 1.2, thickness);
        Surface[] iThreadSurfaces = threadSurfaces(iThreads, -offset, offset);
        Surface[] jThreadSurfaces = threadSurfaces(jThreads, -offset, offset);

        //Surface[] subtracts = subtract(cuts, jThreadSurfaces);
        //outSurfaces = subtracts.ToList();
        
        List<Brep> unrolledBreps;
        List<Point3d> unrolledPoints;
        List<Curve> unrolledCurves;
        unrollSurfaces(iThreadSurfaces, points, unroll, spaced, intersect, out unrolledBreps, out unrolledPoints, out unrolledCurves);
        updateBreps.AddRange(unrolledBreps);
        updatePoints.AddRange(unrolledPoints);
        updateCurves.AddRange(unrolledCurves);

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

        unrollSurfaces(jThreadSurfaces, points, unroll, spaced, intersect, out unrolledBreps, out unrolledPoints, out unrolledCurves);
        updateBreps.AddRange(unrolledBreps);
        updatePoints.AddRange(unrolledPoints);
        updateCurves.AddRange(unrolledCurves);



        //output
        updateMeshes.AddRange(iThicken);
        updateMeshes.AddRange(jThicken);
        Surface[] updateSurfaces = new Surface[iThreadSurfaces.Length + jThreadSurfaces.Length];
        for (int i = 0; i < iThreadSurfaces.Length; ++i) {
            updateSurfaces[i] = iThreadSurfaces[i];
        }
        for (int j = 0; j < jThreadSurfaces.Length; ++j) {
            updateSurfaces[iThreadSurfaces.Length + j] = jThreadSurfaces[j];
        }
        outSurfaces = updateSurfaces;


        outMeshes = updateMeshes;
        outUnrolled = updateBreps;
        outPoints = updatePoints;
        outCurves = updateCurves;


        #endregion









    }

    // <Custom additional code> 



    #region customCode

    Brep[] threadThickBrep(List<Curve> iThreads, double offsetMin, double offsetMax, double thickness) {
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
    public Mesh[] threadThickMesh(List<Curve> iThreads, double offsetMin, double offsetMax, double thickness) {
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
    public void unrollSurfaces(Surface[] threadSurfaces, List<Point3d> points, bool unroll, bool spaced, bool intersect, out List<Brep> outUnrolled, out List<Point3d> outPoints, out List<Curve> outCurves) {
        //unroll///////////////////////////////////////////////////////////////////////////////////////////

        List<Brep> updateBreps = new List<Brep>();
        List<Point3d> updatePoints = new List<Point3d>();
        List<Curve> updateCurves = new List<Curve>();

        if (unroll) {



            Surface[] surfaces = threadSurfaces;

            //curve intersections
            List<Curve> curveIntersections = new List<Curve>();
            if (intersect) {
                for (int i = 0; i < surfaces.Length; ++i) {
                    for (int j = i + 1; j < surfaces.Length; ++j) {
                        Curve[] intersectionCurves;
                        Point3d[] intersectionPoints;
                        Rhino.Geometry.Intersect.Intersection.SurfaceSurface(surfaces[i], surfaces[j], 0.001, out intersectionCurves, out intersectionPoints);
                        curveIntersections.AddRange(intersectionCurves);
                    }
                }
            }




            for (int i = 0; i < surfaces.Length; ++i) {
                Curve[] unrolledCurves;
                Point3d[] unrolledPoints;
                TextDot[] unrolledDots;

                Unroller un = new Unroller(surfaces[i]);
                un.ExplodeOutput = false;
                un.AddFollowingGeometry(points);
                //un.AddFollowingGeometry(curves);
                un.AddFollowingGeometry(curveIntersections);
                Brep[] unrolledBreps = un.PerformUnroll(out unrolledCurves, out unrolledPoints, out unrolledDots);


                updateBreps.AddRange(unrolledBreps);
                updatePoints.AddRange(unrolledPoints);
                updateCurves.AddRange(unrolledCurves);


                if (spaced) {
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
                }//end space out
            }//end each surface

        }//end if unroll
        outUnrolled = updateBreps;
        outPoints = updatePoints;
        outCurves = updateCurves;
    }//end unrollSurfaces
    public Surface[] subtract(Brep[] cuts, Brep[] jThreadSurfaces) {

        Surface[] updateSubtract = new Surface[jThreadSurfaces.Length];
        for (int j = 0; j < jThreadSurfaces.Length; ++j) {
            if (jThreadSurfaces[j].Surfaces.Count > 0) {
                updateSubtract[j] = jThreadSurfaces[j].Surfaces[0];
            }
        }





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






        return updateSubtract;
    }
    #endregion







    // </Custom additional code> 
}