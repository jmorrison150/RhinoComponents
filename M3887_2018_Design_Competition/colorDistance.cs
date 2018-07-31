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
public class Script_Instance60 : GH_ScriptInstance {
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
    private void RunScript(DataTree<double> list, ref object A) {

        DataTree<Color> colorTree = new DataTree<Color>();


        double max = double.MinValue;
        double min = double.MaxValue;


        for (int j = 0; j < list.BranchCount; j++) {
            double currentMax= list.Branch(j).Max();
            double currentMin = list.Branch(j).Min();

            if (currentMax>max) {
                max = currentMax;
            }
            if (currentMin<min) {
                min = currentMin;
            }

        }



        for (int j = 0; j < list.BranchCount; j++) {

            GH_Path path = list.Path(j);
            int[] values = new int[list.Branch(j).Count];
            System.Drawing.Color[] colors = new System.Drawing.Color[list.Branch(j).Count];

            for (int i = 0; i < values.Length; i++) {
                values[i] = (int)map(list.Branch(j)[i], min, max, 0, 255);

                colors[i] = System.Drawing.Color.FromArgb(values[i], 255 - values[i], 0);

                colorTree.Add(colors[i], path);
            }


        }

        A = colorTree;

        //A = colors.ToList();

    }

    // <Custom additional code> 
    public double map(double number, double low1, double high1, double low2, double high2) {
        return low2 + (high2 - low2) * (number - low1) / (high1 - low1);
    }
    double rad(double degree) {

        return degree * Math.PI / 180.0;
    }
    // </Custom additional code> 
}