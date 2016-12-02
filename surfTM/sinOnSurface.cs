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
public class Script_Instance35 : GH_ScriptInstance {
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
    private void RunScript(Surface surface, double assym, double frequency, double amplitude, Line axis, 
        ref object A, ref object B) {






        #region beginScript
        List<Curve> updateCurves = new List<Curve>();
        List<Point3d> updatePoints = new List<Point3d>();

        //angle *= Math.PI / 180;
        Point3d point = axis.From;
        Vector3d vector = axis.To - axis.From;
        int numberOfCurves = 3;
        int resolution = 50;


        NurbsSurface nSurface = surface.ToNurbsSurface();
        double range0 = ( nSurface.Domain(0).Max - nSurface.Domain(0).Min ) / (double) ( numberOfCurves );
        double range1 = ( nSurface.Domain(1).Max - nSurface.Domain(1).Min ) / (double) ( resolution - 1 );

        Curve[] isoCurves0 = new Curve[numberOfCurves];
        Curve[] isoCurves1 = new Curve[numberOfCurves];

        Point2d[][] pts = new Point2d[numberOfCurves][];
        for(int i = 0; i < pts.Length; i++) {
            pts[i] = new Point2d[resolution];
            double ptX = ( range0 * ( i ) ) + nSurface.Domain(0).Min;
            for(int j = 0; j < pts[i].Length; j++) {
                double ptY = ( range1 * j ) + nSurface.Domain(1).Min;
                double left = ptX + ( ( Math.Sin(j * frequency) ) * amplitude * assym ) + i * 0.02;
                pts[i][j] = new Point2d(left, ptY);

            }
            isoCurves0[i] = nSurface.InterpolatedCurveOnSurfaceUV(pts[i], 0.001);

            for(int j = 0; j < pts[i].Length; j++) {
                double ptY = ( range1 * j ) + nSurface.Domain(1).Min;
                double right = ptX - ( ( Math.Sin(j * frequency) ) * amplitude );
                pts[i][j] = new Point2d(right, ptY);
            }
            isoCurves1[i] = nSurface.InterpolatedCurveOnSurfaceUV(pts[i], 0.001);

        }
        //for (int i = 0; i < pts.Length; i++) {
        //isoCurves[i] = nSurface.InterpolatedCurveOnSurfaceUV(pts[i], 0.001);
        //updatePoints.Add(nSurface.PointAt(pts[i][5].X, pts[i][5].Y));
        //updateCurves.Add(nSurface.InterpolatedCurveOnSurfaceUV(pts[i], 0.001)) ;
        //}

        //for (int i = 0; i < curves.Count; i++) {
        //    curves[i].Rotate(angle * i, vector, point);
        //}

        A = isoCurves0;
        B = isoCurves1;

        #endregion







    }

    // <Custom additional code> 

    // </Custom additional code> 





}