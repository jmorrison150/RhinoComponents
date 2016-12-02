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
  private void RunScript(List<Polyline> polylines, int divisions, ref object A, ref object B, ref object C) {

    #region beginScript
    List<Point3d> outPoints = new List<Point3d>();
    List<Line> outLines = new List<Line>();
    List<NurbsSurface> outSurfaces = new List<NurbsSurface>();

    for (int i = 1; i < polylines.Count; i++) {


      Polyline pl0 = polylines[i - 1];
      Polyline pl1 = polylines[i];

      for (int j = 2; j < pl0.Count - 1; j++) {

        LineCurve l0 = new LineCurve(pl0[j - 2], pl1[j - 2]);
        LineCurve l1 = new LineCurve(pl0[j - 1], pl1[j - 1]);
        LineCurve l2 = new LineCurve(pl0[j - 0], pl1[j - 0]);

        Point3d[] pts0, pts1, pts2;

        l0.DivideByCount(divisions + 0, true, out pts0);
        l1.DivideByCount(divisions + 1, true, out pts1);
        l2.DivideByCount(divisions + 1, true, out pts2);

        outPoints.AddRange(pts0);
        outPoints.AddRange(pts1);
        outPoints.AddRange(pts2);


        for (int k = 1; k < pts1.Length - 1; k++) {
          Line l01 = new Line(pts0[k - 1], pts1[k]);
          outLines.Add(l01);

          Line l02 = new Line(pts0[k - 1], pts2[k]);
          outLines.Add(l02);

          Line l10 = new Line(pts1[k], pts0[k]);
          outLines.Add(l10);

          Line l20 = new Line(pts2[k], pts0[k]);
          outLines.Add(l20);

          Line l12 = new Line(pts1[k], pts2[k]);
          outLines.Add(l12);

        }


        for (int k = 2; k < pts1.Length; k += 2) {
          Line l001 = new Line(pts1[k - 1], pts2[k - 0]);
          outLines.Add(l001);
          //Line l002 = new Line(pts2[k - 1], pts1[k - 0]);
          //outLines.Add(l002);
        }

        for (int k = 1; k < pts1.Length; k += 2) {
          Line l010 = new Line(pts2[k - 1], pts1[k - 0]);
          outLines.Add(l010);
          //Line l020 = new Line(pts1[k - 1], pts2[k - 0]);
          //outLines.Add(l020);
        }

      }


    }



    Curve[] crvs = new Curve[polylines.Count];
    for (int i = 0; i < crvs.Length; i++) {

      crvs[i] = new PolylineCurve(polylines[i]);

    }
    Brep b = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Straight, false)[0];





    B = b;
    C = outLines;


    #endregion



  }

  // <Custom additional code> 

  // </Custom additional code> 
}