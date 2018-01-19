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
    private void RunScript(List<Curve> curves, ref object outLocation, ref object outText, ref object outSize, ref object outColour) {

        #region beginScript

        List<Point3d> points = new List<Point3d>();
        for (int i = 0; i < curves.Count; i++) {
            Point3d[] pts;
            curves[i].DivideByLength(500, true, out pts);
            if (pts != null) {

                //Print(pts.Length.ToString());
                for (int j = 0; j < pts.Length; j++) {
                    points.Add(pts[j]);
                }
            }
        }

        points.Add(Point3d.Origin);

        //A = String.Format("{0:0.0}",x[i].Z);


        List<Color> colors = new List<Color>();
        List<string> text = new List<string>();

        double max = double.MinValue;
        double min = double.MaxValue;
        for (int i = 0; i < points.Count; i++) {
            if (max < points[i].Z) {
                max = points[i].Z;
            }
            if (min > points[i].Z) {
                min = points[i].Z;
            }
        }
        //Print(min.ToString());
        //Print(max.ToString());

        for (int i = 0; i < points.Count; i++) {
            int r = (int)map(points[i].Z, min, max, 0, 255);
            if (r > 255) { r = 255; } else if (r < 0) { r = 0; }

            string txt = String.Format("{0:0.0}", points[i].Z);
            text.Add(txt);
            //int b = (int)map(x[i].Z, min, max, 255, 0);


            colors.Add(Color.FromArgb(r, r, r));
        }
        outLocation = points;
        outText = text;
        outSize = 10.0;
        outColour = colors;

        #endregion

    }

    // <Custom additional code> 
    public double map(double number, double low1, double high1, double low2, double high2) {
        return low2 + (high2 - low2) * (number - low1) / (high1 - low1);
    }
    // </Custom additional code> 
}