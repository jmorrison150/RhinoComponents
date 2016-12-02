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
    private void RunScript(ref object A, ref object B) {



        #region beginScript
        /////////////////////////////////////////////////////////////////////////////////

        double vRes = 8;
        int vMax = (int)((vRes) * 2 * Math.PI);
        int vMin = 0;
        int vDomain = vMax - vMin;

        int uMax = 20;
        int uMin = 0;
        int uDomain = uMax - uMin;

        double h = 10.0;
        double r = 5.0;

        double[][] x = new double[uDomain][];
        double[][] y = new double[uDomain][];
        double[][] z = new double[uDomain][];

        for (int i = 0; i < x.Length; ++i) { x[i] = new double[vDomain]; }
        for (int i = 0; i < y.Length; ++i) { y[i] = new double[vDomain]; }
        for (int i = 0; i < z.Length; ++i) { z[i] = new double[vDomain]; }

        List<Point3d> updatePoints = new List<Point3d>();
        List<Mesh> updateMeshes = new List<Mesh>();
        Point3d[][] pts = new Point3d[uDomain][];
        for (int i = 0; i < pts.Length; ++i) { pts[i] = new Point3d[vDomain]; }

        //antecedents///////////////////////////////////////////////////////////////////
        for (int u = 0; u < x.Length; ++u) {
            for (int v = 0; v < x[u].Length; ++v) {
                x[u][v] = (h - u) / h * r * Math.Cos(v / vRes);
                y[u][v] = (h - u) / h * r * Math.Sin(v / vRes);
                z[u][v] = u;
            }
        }

        //output////////////////////////////////////////////////////////////////////////
        for (int m = 0; m < pts.Length; ++m) {
            pts[m] = new Point3d[vDomain];
            for (int n = 0; n < pts[m].Length; ++n) {
                pts[m][n] = new Point3d(x[m][n], y[m][n], z[m][n]);
            }
        }







        //output

        for (int m = 0; m < pts.Length; ++m) {
            for (int n = 0; n < pts[m].Length; ++n) {
                updatePoints.Add(pts[m][n]);
            }
        }

        Mesh mesh = new Mesh();
        for (int n = 0; n < vDomain; ++n) {
            for (int m = 0; m < uDomain; ++m) {
                mesh.Vertices.Add(pts[m][n]);
            }
        }

        for (int n = 0; n < vDomain - 1; ++n) {
            for (int m = 0; m < uDomain - 1; ++m) {
                mesh.Faces.AddFace((n * uDomain) + m, (n * uDomain) + m + 1, ((n + 1) * uDomain) + m + 1, ((n + 1) * uDomain) + m);
            }
        }

        //mesh.UnifyNormals();
        mesh.FaceNormals.ComputeFaceNormals();
        updateMeshes.Add(mesh);

        A = updateMeshes;
        B = updatePoints;
        #endregion





    }

    // <Custom additional code> 

    // </Custom additional code> 
}