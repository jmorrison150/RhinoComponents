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
    private void RunScript(List<Point3d> points, List<Curve> iThreads, List<Curve> jThreads, double offset, double thickness, bool unroll, bool spaced, bool intersect, ref object outSurfaces, ref object outMeshes, ref object outUnrolled, ref object outPoints, ref object outCurves) {




        #region offsetCurves
        if (offset == 0) { offset = 0.001; }
        List<Brep> updateBreps1 = new List<Brep>();
        List<Mesh> updateMeshes = new List<Mesh>();
        Curve[] curves = new Curve[iThreads.Count + jThreads.Count];
        for (int i = 0; i < iThreads.Count; ++i) {
            curves[i] = iThreads[i];
        }
        for (int i = 0; i < jThreads.Count; ++i) {
            curves[i + iThreads.Count] = jThreads[i];
        }



        for (int i = 0; i < curves.Length; ++i) {
            Curve[] offsetCurves = new Curve[2];
            Plane plane;
            try {
                //make single plane based on start point, mid point, and end point
                plane = new Plane(curves[i].PointAtStart, curves[i].PointAtNormalizedLength(0.5), curves[i].PointAtEnd);
            } catch {
                //default to ground plane
                plane = Plane.WorldXY;
            }
            //offset
            offsetCurves[0] = curves[i].Offset(plane, offset, 0.001, CurveOffsetCornerStyle.Sharp)[0];
            //negative offset
            offsetCurves[1] = curves[i].Offset(plane, -offset, 0.001, CurveOffsetCornerStyle.Sharp)[0];

            Brep[] breps = Brep.CreateFromLoft(offsetCurves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);
            updateBreps1.AddRange(breps);



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
                //Mesh m = Mesh.CreateFromPlanarBoundary(polyline0, MeshingParameters.Smooth);
                m.FaceNormals.ComputeFaceNormals();
                m.Weld(0.001);

                Mesh m0 = m.Offset(thickness, true);
                Mesh m1 = m.Offset(thickness, true);
                m0.Append(m1);
                updateMeshes.Add(m0);
                //Print(m0.IsClosed.ToString());
            } catch { }
        }




        //unroll///////////////////////////////////////////////////////////////////////////////////////////
        Surface[] surfaces = new Surface[updateBreps1.Count];
        for (int i = 0; i < surfaces.Length; ++i) {
            surfaces[i] = updateBreps1[i].Surfaces[0];
        }




        List<Brep> updateBreps = new List<Brep>();
        List<Point3d> updatePoints = new List<Point3d>();
        List<Curve> updateCurves = new List<Curve>();

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



        if (unroll) {
            for (int i = 0; i < surfaces.Length; ++i) {
                Curve[] unrolledCurves;
                Point3d[] unrolledPoints;
                TextDot[] unrolledDots;

                Unroller un = new Unroller(surfaces[i]);
                un.ExplodeOutput = false;
                un.AddFollowingGeometry(points);
                un.AddFollowingGeometry(curves);
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
        }






        outSurfaces = updateBreps1;
        outMeshes = updateMeshes;
        outUnrolled = updateBreps;
        outPoints = updatePoints;
        outCurves = updateCurves;
        #endregion






    }

    // <Custom additional code> 

    // </Custom additional code> 
}