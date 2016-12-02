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
public class Script_Instance : GH_ScriptInstance
{
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
    private void RunScript(List<Mesh> standardRooms, List<double> view_Areas, ref object A)
    {

        double relativeMax = 312.0;
        double relativeMin = 1.0;

        List<System.Drawing.Color> colors = new List<Color>();

        double maxView = view_Areas.Max();
        double minView = view_Areas.Min();

        for (int i = 0; i < standardRooms.Count; i++)
        {

            standardRooms[i].VertexColors.CreateMonotoneMesh(System.Drawing.Color.White);

            double r, g, b;
            r = ((1.0 - ((view_Areas[i] - relativeMin) / relativeMax))) * 255.0;
            g = (((view_Areas[i] - relativeMin) / relativeMax)) * 255.0;
            b = 0.0;

            if (r > 255) { r = 255; }
            if (g > 255) { g = 255; }
            if (b > 255) { b = 255; }
            if (r < 0) { r = 0; }
            if (g < 0) { g = 0; }
            if (b < 0) { b = 0; }

            System.Drawing.Color currentColor = System.Drawing.Color.FromArgb(255,
              (int)r, (int)g, (int)b);
            colors.Add(currentColor);
            for (int j = 0; j < standardRooms[i].VertexColors.Count; j++)
            {
                standardRooms[i].VertexColors[j] = currentColor;
            }



        }


        A = standardRooms;


    }

    // <Custom additional code> 


    // </Custom additional code> 
}