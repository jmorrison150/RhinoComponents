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
  private void RunScript(Curve arch, GeometryBase floor, ref object A) {

    #region beginScript;

    double spacingZ = 8 * 12;
    double spacingXY = 3 * 12;

    List<Curve> mullions = new List<Curve>();

    //floor
    Point3d floorPt = Point3d.Origin;
    try {
    floorPt = floor.GetBoundingBox(false).Min;
    } catch (Exception e) {
      Print(e.ToString());
    floorPt = arch.GetBoundingBox(false).Min;
    }

    //glazingSurface
    Plane floorPlane = new Plane(floorPt, Vector3d.ZAxis);
    Curve[] glazingCurves = new Curve[2];
    glazingCurves[0] = arch;
    glazingCurves[1] = Curve.ProjectToPlane(arch, floorPlane);
    Brep[] loft = Brep.CreateFromLoft(glazingCurves, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);

    //mullionsXY
    BoundingBox bb = loft[0].GetBoundingBox(false);
    for (double i = floorPt.Z; i <= bb.Max.Z; i += spacingZ) {
      Plane plane = new Plane(new Point3d(0, 0, i), Vector3d.ZAxis);
      Curve[] intCrvs;
      Point3d[] intPts;
      Rhino.Geometry.Intersect.Intersection.BrepPlane(loft[0], plane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intCrvs, out intPts);
      if (intCrvs.Length > 0) { mullions.AddRange(intCrvs); }
    }

    //mullionsZ
    Point3d[] mullionsZPts = glazingCurves[1].DivideEquidistant(spacingXY);
    for (int i = 0; i < mullionsZPts.Length; i++) {
      Point3d pt = mullionsZPts[i];
      LineCurve l = new LineCurve(pt, new Point3d(pt.X, pt.Y, bb.Max.Z));
      Rhino.Geometry.Intersect.CurveIntersections intCvs = Rhino.Geometry.Intersect.Intersection.CurveCurve(glazingCurves[0], l, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
      for (int j = 0; j < intCvs.Count; j++) {
        if (intCvs[j].IsPoint) {
          Point3d intPt = intCvs[j].PointA;
          LineCurve mullionZ = new LineCurve(pt, intPt);
          mullions.Add(mullionZ);
        }
      }
    }

    A = mullions;
    #endregion;

  }

  // <Custom additional code> 

  // </Custom additional code> 
}