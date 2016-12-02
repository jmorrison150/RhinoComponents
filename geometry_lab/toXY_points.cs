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
public class Script_Instance31 : GH_ScriptInstance {
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
    private void RunScript(DataTree<Point3d> points, DataTree<Transform> xforms, ref object A) {




        if (xforms.BranchCount < points.BranchCount || xforms.BranchCount < 1) { Print("error"); return; }
        DataTree<Point3d> updatePoints = new DataTree<Point3d>();


        //format points from data tree
        Point3d[][] pts = new Point3d[points.BranchCount][];
        for (int i = 0; i < pts.Length; i++) {
            pts[i] = new Point3d[points.Branches[i].Count];
            for (int j = 0; j < pts[i].Length; j++) {
                pts[i][j] = points.Branches[i][j];
            }
        }


        for (int i = 0; i < pts.Length; i++) {
            for (int j = 0; j < pts[i].Length; j++) {
                for (int k = 0; k < xforms.Branches[0].Count; k++) {
                    pts[i][j].Transform(xforms.Branches[i][k]);
                }
            }
        }






        //format points back to data tree
        for (int m = 0; m < pts.Length; ++m) {
            Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
            for (int n = 0; n < pts[m].Length; ++n) {
                updatePoints.Insert(pts[m][n], path, n);
            }
        }

        A = updatePoints;




    }

    // <Custom additional code> 

    // </Custom additional code> 
}