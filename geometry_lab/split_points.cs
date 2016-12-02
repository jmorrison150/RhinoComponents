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
public class Script_Instance26 : GH_ScriptInstance {
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
    private void RunScript(DataTree<Point3d> points, int index, ref object listA, ref object listB) {





        #region beginScript
        DataTree<Point3d> updatePoints0 = new DataTree<Point3d>();
        DataTree<Point3d> updatePoints1 = new DataTree<Point3d>();



        //format points from data tree
        Point3d[][] pts = new Point3d[points.BranchCount][];
        for (int i = 0; i < pts.Length; i++) {
            pts[i] = new Point3d[points.Branches[i].Count];
            for (int j = 0; j < pts[i].Length; j++) {
                pts[i][j] = points.Branches[i][j];
            }
        }





        Point3d[][] pts0 = new Point3d[index][];
        Point3d[][] pts1 = new Point3d[pts.Length-index][];

        for (int i = 0; i < pts0.Length; i++) {
            pts0[i] = pts[i];
        }
        for (int i = 0; i < pts1.Length; i++) {
            pts1[i] = pts[i+index];
        }







        //format points back to data tree
        for (int m = 0; m < pts0.Length; ++m) {
            Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
            for (int n = 0; n < pts0[m].Length; ++n) {
                updatePoints0.Insert(pts0[m][n], path, n);
            }
        }

        for (int m = 0; m < pts1.Length; ++m) {
            Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
            for (int n = 0; n < pts1[m].Length; ++n) {
                updatePoints1.Insert(pts1[m][n], path, n);
            }
        }





        listA = updatePoints0;
        listB = updatePoints1;
        #endregion





    }

    // <Custom additional code> 

    // </Custom additional code> 
}