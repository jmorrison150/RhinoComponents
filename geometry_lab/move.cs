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
public class Script_Instance19 : GH_ScriptInstance {
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
    private void RunScript(
        DataTree<Surface> surfaces, DataTree<Curve> curves, 
        DataTree<Point3d> points, DataTree<Curve> text, 
        ref object outSurfaces, ref object outCurves, ref object outPoints, ref object outText) {
        
        
        #region beginScript


        double distanceX = -10.0;
        double distanceY = 100.0;
        double distanceZ = 0.0;





        for (int i = 0; i < surfaces.Branches.Count; i++) {
            Vector3d motion = new Vector3d(distanceX, distanceY * i, distanceZ);
            Transform xForm = Transform.Translation(motion);
            for (int j = 0; j < surfaces.Branches[i].Count; j++) {
                
            surfaces.Branches[i][j].Transform(xForm);
            }
        }

        for (int i = 0; i < curves.Branches.Count; i++) {
            Vector3d motion = new Vector3d(distanceX, distanceY * i, distanceZ);
            Transform xForm = Transform.Translation(motion);
            for (int j = 0; j < curves.Branches[i].Count; j++) {
            curves.Branches[i][j].Transform(xForm);
            }
        }




        for (int i = 0; i < text.Branches.Count; i++) {
            Vector3d motion = new Vector3d(distanceX, distanceY * i, distanceZ);
            Transform xForm = Transform.Translation(motion);
            for (int j = 0; j < text.Branches[i].Count; j++) {
            text.Branches[i][j].Transform(xForm);
            }
        }


        for (int i = 0; i < points.Branches.Count; i++) {
             Vector3d motion = new Vector3d(distanceX, distanceY * i, distanceZ);
            Transform xForm = Transform.Translation(motion);
            xForm.TransformList(points.Branches[i]);
        }


        outSurfaces = surfaces;
        outCurves = curves;
        outPoints = points;
        outText = text;




#endregion



    }

    // <Custom additional code> 

    // </Custom additional code> 
}