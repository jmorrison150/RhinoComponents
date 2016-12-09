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
  private void RunScript(Curve curve0, Curve curve1, int count, int int1, ref object A) {



    #region beginScript

    if (int1 == 0) { return; }
    List<Line> updateLines = new List<Line>();
    List<Line> updateLines2 = new List<Line>();

    Point3d[] pts0;
    Point3d[] pts1;
    curve0.DivideByCount(count - 1, true, out pts0);
    curve1.DivideByCount(count - 1, true, out pts1);

    for (int i = 0; i < pts0.Length; i++) {
      //suspension cables
      Line l0 = new Line(pts0[i], pts1[i]);
      updateLines.Add(l0);
    }





    //truss
    int int2 = int1;
    for (int i = 0; i < pts0.Length; i += int2) {

      if (i < int2) {
        continue;
      }
      Point3d pt2 = (new Line(pts1[i - int2], pts1[i])).PointAt(0.5) + (Vector3d.ZAxis * (11 * 12));
      Line l2 = new Line(pts1[i - int2], pt2);
      Line l3 = new Line(pts1[i], pt2);
      updateLines2.Add(l2);
      updateLines2.Add(l3);

    }



    A = updateLines;
    B = updateLines2;
    #endregion









  }

  // <Custom additional code> 

  // </Custom additional code> 
}