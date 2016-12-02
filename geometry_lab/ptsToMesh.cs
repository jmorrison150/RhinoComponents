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
public class Script_Instance6 : GH_ScriptInstance {
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
    private void RunScript(DataTree<Point3d> points, ref object A) {




        #region beginScript
        DataTree<Point3d> updatePoints = new DataTree<Point3d>();
        List<Mesh> updateMeshes = new List<Mesh>();




        //format points from data tree
        Point3d[][] pts = new Point3d[points.BranchCount][];
        for (int i = 0; i < pts.Length; i++) {
            pts[i] = new Point3d[points.Branches[i].Count];
            for (int j = 0; j < pts[i].Length; j++) {
                pts[i][j] = points.Branches[i][j];
            }
        }








        //mesh
        Mesh mesh = new Mesh();

        for (int n = 0; n < pts.Length; ++n) {
            for (int m = 0; m < pts[n].Length; ++m) {
                mesh.Vertices.Add(pts[m][n]);
            }
        }

        for (int n = 0; n < pts.Length - 1; ++n) {
            for (int m = 0; m < pts[n].Length - 1; ++m) {
                int columns = pts[n].Length;
                MeshFace face = new MeshFace((n * columns) + m, (n * columns) + m + 1, ((n + 1) * columns) + m + 1, ((n + 1) * columns) + m);
                mesh.Faces.AddFace(face);
            }
        }

        //mesh.UnifyNormals();
        mesh.FaceNormals.ComputeFaceNormals();
        updateMeshes.Add(mesh);






        //format points back to data tree
        for (int m = 0; m < pts.Length; ++m) {
            Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
            for (int n = 0; n < pts[m].Length; ++n) {
                updatePoints.Insert(pts[m][n], path, n);
            }
        }


        A = mesh;
        #endregion





    }

    // <Custom additional code> 

    // </Custom additional code> 
}