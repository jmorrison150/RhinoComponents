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
    private void RunScript(Brep brep, double width, double length, ref object A, ref object B) {


        #region beginScript

        List<Curve> updateCrvs = new List<Curve>();


        for (int i = 0; i < brep.Faces.Count; i++) {


            Curve min1 = brep.Faces[i].IsoCurve(1, brep.Faces[i].Domain(0).Min);
            //Curve[] min2 = brep.Faces[i].TrimAwareIsoCurve(1, brep.Faces[i].Domain(0).Min);
            double[] pts1 = min1.DivideByLength(length, true);
            Curve[][] crvs1 = new Curve[pts1.Length][];


            for (int j = 0; j < pts1.Length; j++) {
                crvs1[j] = brep.Faces[i].TrimAwareIsoCurve(0, pts1[j]);
                for (int k = 0; k < crvs1[j].Length; k++) {
                    updateCrvs.Add(crvs1[j][k]);
                }
            }




            //Width
            Curve min0 = brep.Faces[i].IsoCurve(0, brep.Faces[i].Domain(1).Min);
            double[] pts0 = min0.DivideByLength(width, true);
            Curve[][] crvs0 = new Curve[pts0.Length][];


            for (int j = 0; j < pts0.Length; j++) {
                crvs0[j] = brep.Faces[i].TrimAwareIsoCurve(1, pts0[j]);
                for (int k = 0; k < crvs0[j].Length; k++) {
                    updateCrvs.Add(crvs0[j][k]);
                }
            }

            A = updateCrvs;


        }
        #endregion







        // <Custom additional code> 

        // </Custom additional code> 
    }