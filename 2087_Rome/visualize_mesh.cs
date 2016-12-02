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
public class visualizeMesh : GH_ScriptInstance {
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
    private void RunScript(Mesh mesh, double max, double min, ref object A) {


        //mesh.VertexColors.CreateMonotoneMesh(Color.FromArgb(0));


        System.Drawing.Color[] colors = new Color[mesh.Vertices.Count];




        for(int i = 0; i < mesh.Vertices.Count; i++) {

            double r, g, b;
            //      r = ((1.0 - ((areas[i] - min) / max))) * 255.0;
            //      g = (((areas[i] - min) / max)) * 255.0;
            //      b = 0.0;


            //Print(mesh.VertexColors[i].ToArgb().ToString());

            r = 0;
            g = 0;
            //b = ( ( ((mesh.VertexColors[i].ToArgb() - min) / max))) * 255.0;

            b = map(mesh.VertexColors[i].ToArgb(), min, max, 0, 255);
            if(b > 255) { b = 255; } else if(b < 0) { b = 0; }


            r = Math.Min(Math.Max(r, 0), 255);
            g = Math.Min(Math.Max(g, 0), 255);
            // b = Math.Min(Math.Max(g, 0), 255);

            System.Drawing.Color currentColor = System.Drawing.Color.FromArgb(255,
              (int) r, (int) g, (int) b);
            colors[i] = currentColor;


            //
            //      System.Drawing.Color currentColor = System.Drawing.Color.FromArgb((int) areas[i]);
            //      colors[i] = currentColor;


        }

        mesh.VertexColors.SetColors(colors);
        A = mesh;


    }

    // <Custom additional code> 
    double map(double value1, double min1, double max1, double min2, double max2) {
        double value2 = min2 + ( value1 - min1 ) * ( max2 - min2 ) / ( max1 - min1 );
        return value2;
    }
    // </Custom additional code> 
}