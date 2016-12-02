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
    private void RunScript(List<Brep> breps3d, List<Point3d> points3d, List<Point3d> pointsFlat, ref object A, ref object B, ref object C) {



        #region beginScript

        List<Brep> updateBreps = new List<Brep>();
        List<Point3d> updatePoints = new List<Point3d>();
        List<Curve> updateCurves = new List<Curve>();


        //allows selection of multiple breps
        for (int j = 0; j < breps3d.Count; ++j) {


            int[] point3dIndex = new int[points3d.Count];
            int[] pointFlatIndex = new int[pointsFlat.Count];
            List<Point2d> paramPts = new List<Point2d>();
            List<Point3d> pointsOnSurface = new List<Point3d>();

            //make sure the points3d on the brep
            for (int i = 0; i < points3d.Count; ++i) {
                double u;
                double v;
                Point3d pt;
                Rhino.Geometry.ComponentIndex ci;
                Vector3d normal;
                bool success = breps3d[j].ClosestPoint(points3d[i], out pt, out ci, out u, out v, 0.001, out normal);
                if (success) {
                    point3dIndex[i] = ci.Index;
                    //Point3d closestPoint = breps3d[j].Surfaces[ci.Index].PointAt(u, v);
                }
            }




            //rolling... rolling... unrolling on the river
            Curve[] unrolledCurves;
            Point3d[] unrolledPoints;
            TextDot[] unrolledDots;
            Brep[] unrolledBreps;

            Unroller un = new Unroller(breps3d[j]);
            //unroll points
            un.AddFollowingGeometry(pointsOnSurface);
            un.ExplodeOutput = false;

            //this does the work
            unrolledBreps = un.PerformUnroll(out unrolledCurves, out unrolledPoints, out unrolledDots);
            //component output
            updateBreps.AddRange(unrolledBreps);


            //add user supplied pointsFlat
            for (int i = 0; i < pointsFlat.Count; ++i) {
                Point3d closestPoint;
                ComponentIndex ci;
                double u, v;
                Vector3d normal;
                //test each part of the brep
                for (int k = 0; k < breps3d[j].Surfaces.Count;++k ) {
                    bool success = unrolledBreps[k].ClosestPoint(pointsFlat[i], out closestPoint, out ci, out u, out v, 0.001, out normal);

                    if (success) {
                        Point3d point2dto3d = breps3d[j].Surfaces[k].PointAt(u, v);
                        updatePoints.Add(point2dto3d);
                        pointFlatIndex[i] = k;
                    }
                }

                //Point2d uvPoint = new Point2d(u, v);
                //paramPts.Add(uvPoint);
            }



            //geodesic
            for (int i = 1; i <= paramPts.Count; ++i) {
                //closed loop through all the points
                //Curve c = surface3d.ShortPath(paramPts[i - 1], paramPts[i % paramPts.Count], 0.001);
                //updateCurves.Add(c);
            }








            //Rhino.Geometry.Collections.BrepSurfaceList bsl = unrolledBreps[0].Surfaces[0].
        }

        //output
        A = updateBreps;
        B = updateCurves;
        C = updatePoints;
        //D = updateCurves;

        #endregion











    }

    // <Custom additional code> 

    // </Custom additional code> 
}