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
public class MeshDisplay : GH_ScriptInstance {
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
    private void RunScript(Mesh mesh, int max,int min, ref object outMesh) {
        Mesh updateMesh = mesh;
        
        
        //color check
        if(max<0) { max=0; } 
        if(max>255) { max=255; }
        if(min<0) { min=0; } 
        if(min>255) { min=255; }
        
        //find max value
        int colorMax = 0;
        for(int i = 0; i < updateMesh.VertexColors.Count; i++) {
            int colorInt = updateMesh.VertexColors[i].ToArgb();
            if(colorMax<colorInt) { colorMax = colorInt; }
        }

        //assign colors
        for(int i = 0; i < updateMesh.VertexColors.Count; i++) {
            double value2 = map(updateMesh.VertexColors[i].ToArgb(),0,colorMax,0,255);
            if(value2<0) { value2=0; }
            if(value2>255) { value2=255; }
            updateMesh.VertexColors[i] = System.Drawing.Color.FromArgb(0, 0, (int) value2);
        }

        //output
        outMesh = updateMesh;

     
    }

    // <Custom additional code> 
    double map(double value1, double min1, double max1, double min2, double max2) {
        double value2 = min2 + ( value1-min1 )*( max2-min2 )/( max1-min1 );
        return value2;
    }
    // </Custom additional code> 
}