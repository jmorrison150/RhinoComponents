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
public class Script_Instance15 : GH_ScriptInstance {
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
    private void RunScript(List<Surface> surfaceI, List<Surface> surfaceJ, ref object linesI, ref object linesJ) {





        #region beginScript
        //empty lists
        DataTree<Curve> updateLinesI = new DataTree<Curve>();
        DataTree<Curve> updateLinesJ = new DataTree<Curve>();
        List<Curve>[] linesII = new List<Curve>[surfaceI.Count];
        List<Curve>[] linesJJ = new List<Curve>[surfaceJ.Count];

        for (int i = 0; i < linesII.Length; i++) {
            linesII[i] = new List<Curve>();
        }
        for (int j = 0; j < linesJJ.Length; j++) {
            linesJJ[j] = new List<Curve>();
        }


        //nested for loop
        for (int i = 0; i < surfaceI.Count; i++) {
            for (int j = 0; j < surfaceJ.Count; j++) {

                Curve[] intersectionCurves;
                Point3d[] intersectionPoints;
                Rhino.Geometry.Intersect.Intersection.SurfaceSurface(surfaceI[i], surfaceJ[j], 0.001, out intersectionCurves, out intersectionPoints);

                linesII[i].AddRange(intersectionCurves);
                linesJJ[j].AddRange(intersectionCurves);
            }
        }



        //format for grasshopper
        for (int i = 0; i < linesII.Length; i++) {
            GH_Path path = new GH_Path(i);
            for (int j = 0; j < linesII[i].Count; j++) {
                updateLinesI.Add(linesII[i][j], path);
            }
        }
        for (int j = 0; j < linesJJ.Length; j++) {
            GH_Path path = new GH_Path(j);
            for (int i = 0; i < linesJJ[j].Count; i++) {
                updateLinesJ.Add(linesJJ[j][i], path);
            }
        }


        //output to grasshopper
        linesI = updateLinesI;
        linesJ = updateLinesJ;
    
#endregion
    





    }

    // <Custom additional code> 

    // </Custom additional code> 
}