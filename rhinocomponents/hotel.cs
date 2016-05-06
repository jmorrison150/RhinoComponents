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
  private void RunScript(List<Curve> floors, Point3d axis, double rotation, ref object A) {




    #region script
    List<Curve> curves = new List<Curve>();
    BoundingBox bBox = new BoundingBox();
    for (int i = 0; i < floors.Count; i++) {
      BoundingBox bb = floors[i].GetBoundingBox(false);
      bBox.Union(bb);
    }
    Line l = new Line(bBox.Min, bBox.Max);

    for (int i = 0; i < floors.Count; i++) {
      double fade = map(floors[i].PointAtStart.Z, bBox.Min.Z, bBox.Max.Z, 1.0, 0.0);
      double rad = rotation * fade * Math.PI / 180.0;
      Transform rot = Transform.Rotation(rad, Vector3d.ZAxis, axis);
      floors[i].Transform(rot);
      curves.Add(floors[i]);
    }


    List<Surface> surfaces = new List<Surface>();
    for (int i = 0; i < curves.Count; i++) {
      Surface eee = Extrusion.CreateExtrusion(curves[i], Vector3d.ZAxis * 10 * 12);
      surfaces.Add(eee);
    }
    A = surfaces;

    #endregion


  }

  // <Custom additional code> 
  public double map(double number, double low1, double high1, double low2, double high2) {
    return low2 + (high2 - low2) * (number - low1) / (high1 - low1);
  }
  // </Custom additional code> 
}