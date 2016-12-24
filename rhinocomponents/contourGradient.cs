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
public class Script_Instance10 : GH_ScriptInstance {
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
  private void RunScript(List<Surface> surfaces, List<double> graphMapper, ref object A, ref object B, ref object C) {






    #region runScript
    Brep[] breps = new Brep[surfaces.Count];
    BoundingBox bb;
    Point3d[] points = new Point3d[graphMapper.Count];
    Plane[] planes = new Plane[graphMapper.Count];
    List<Curve> updateCurves = new List<Curve>();


    bb = surfaces[0].GetBoundingBox(true);
    for (int i = 0; i < surfaces.Count; i++) {
      bb.Union(surfaces[i].GetBoundingBox(true));
      breps[i] = surfaces[i].ToBrep();
    }

    double zDepth = bb.Max.Z - bb.Min.Z;

    for (int i = 0; i < graphMapper.Count; i++) {

      double z = map(graphMapper[i], 0.0, 1.0, bb.Min.Z, bb.Max.Z);
      Point3d pt = new Point3d(bb.Min.X, bb.Min.Y, z);
      points[i] = pt;
      Plane plane = new Plane(pt, Vector3d.ZAxis);
      planes[i] = plane;

      for (int j = 0; j < breps.Length; j++) {
        Curve[] intCurves;
        Point3d[] intPoints;
        Rhino.Geometry.Intersect.Intersection.BrepPlane(breps[j], planes[i], Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intCurves, out intPoints);
        if (intCurves !=null) {
        for (int k = 0; k < intCurves.Length; k++) {
          updateCurves.Add(intCurves[k]);
        }
        }
      }
    }

    A = points;
    B = planes;
    C = updateCurves;

    #endregion








  }

  // <Custom additional code> 

  public double map(double number, double low1, double high1, double low2, double high2) {
    return low2 + (high2 - low2) * (number - low1) / (high1 - low1);
  }
  // </Custom additional code> 
}