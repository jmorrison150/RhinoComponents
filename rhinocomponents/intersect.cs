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
public class Script_Instance15 : GH_ScriptInstance {
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
  private void RunScript(List<Plane> planes, List<Brep> roofs, List<Curve> curves, ref object A, ref object B) {


    #region beginScript
    //List<Curve> updateCurves = new List<Curve>();

    //for (int i = 0; i < planes.Count; i++) {
    //  for (int j = 0; j < breps.Count; j++) {


    //    Curve[] intCrvs;
    //    Point3d[] intPts;
    //    Rhino.Geometry.Intersect.Intersection.BrepPlane(breps[j], planes[i], Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intCrvs, out intPts);
    //if (intCrvs == null) { continue; }
    //    for (int k = 0; k < intCrvs.Length; k++) {
    //      updateCurves.Add(intCrvs[k]);
    //    }
    //  }
    //}


    //A = updateCurves;

    List<Curve> updateCurves = new List<Curve>();
    List<Line> updateLines = new List<Line>();

    for (int i = 0; i < planes.Count; i++) {
      for (int j = 0; j < curves.Count; j++) {


        //intersect base lines with (plumb) plane
        List<Point3d> pts = new List<Point3d>();
        Rhino.Geometry.Intersect.CurveIntersections crvInts = Rhino.Geometry.Intersect.Intersection.CurvePlane(curves[j], planes[i], Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
        if (crvInts == null) { continue; }
        for (int k = 0; k < crvInts.Count; k++) {
          if (crvInts[k].PointA.IsValid) {
            Point3d ptA = crvInts[k].PointA;
            pts.Add(ptA);
          }
        }


        //intersect roof(s) with (plumb) plane
        List<Curve> roofCrvs = new List<Curve>();
        for (int l = 0; l < roofs.Count; l++) {
          Curve[] intCrvs;
          Point3d[] intPts;
          Rhino.Geometry.Intersect.Intersection.BrepPlane(roofs[l], planes[i], Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intCrvs, out intPts);
          if (intCrvs == null) { continue; }
          for (int k = 0; k < intCrvs.Length; k++) {
            roofCrvs.Add(intCrvs[k]);
            updateCurves.Add(intCrvs[k]);
          }
        }


        //draw cables
        for (int m = 0; m < roofCrvs.Count; m++) {
          for (int n = 0; n < pts.Count; n++) {
            double t1;
            roofCrvs[m].ClosestPoint(pts[n], out t1);
            Point3d pt2 = roofCrvs[m].PointAt(t1);

            Line line = new Line(pts[n], pt2);
            updateLines.Add(line);
          }
        }


      }
    }



    A = updateLines;
    B = updateCurves;

    #endregion


  }

  // <Custom additional code> 

  // </Custom additional code> 
}