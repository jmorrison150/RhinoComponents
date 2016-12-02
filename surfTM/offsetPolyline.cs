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
    private void RunScript(List<Curve> curves, double distance, ref object A, ref object B) {
        #region beginScript
        List<Curve> updateCurves = new List<Curve>();
        List<Brep> updateBreps = new List<Brep>();

        for (int i = 0; i < curves.Count; ++i) {
            Curve[] offsetCurves = new Curve[2];
            Plane plane;
            try {
                //make single plane based on start point, mid point, and end point
                plane = new Plane(curves[i].PointAtStart, curves[i].PointAtNormalizedLength(0.5), curves[i].PointAtEnd);
            } catch {
                plane = Plane.WorldXY;
            }
            //offset
            offsetCurves[0] = curves[i].Offset(plane, distance, 0.001, CurveOffsetCornerStyle.Sharp)[0];
            updateCurves.AddRange(offsetCurves);
            //negative offset
            offsetCurves[1] = curves[i].Offset(plane, -distance, 0.001, CurveOffsetCornerStyle.Sharp)[0];
            updateCurves.AddRange(offsetCurves);

            Brep[] breps = Brep.CreateFromLoft(offsetCurves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);
            updateBreps.AddRange(breps);
        }

        A = updateCurves;
        B = updateBreps;
        #endregion
    }

    // <Custom additional code> 

    // </Custom additional code> 
}