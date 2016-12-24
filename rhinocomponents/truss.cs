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
  private void RunScript(Brep brep, double baySpacing, double trussWidth, List<double> trussTriangles, double rotate, double scaleXY, double spanRatio, ref object A, ref object B, ref object C, ref object D) 
    {




    #region runScript

    int trussCount = trussTriangles.Count - 1;
    List<Curve> curves = new List<Curve>();
    Vector3d axis = Vector3d.XAxis;
    BoundingBox b = brep.GetBoundingBox(false);
    LineCurve lineCurve;
    lineCurve = new LineCurve(new Point3d(b.Min.X, b.Min.Y, b.Min.Z) + (axis * trussWidth), new Point3d(b.Max.X, b.Min.Y, b.Min.Z));
    Point3d[] points;
    Plane[][] planes;
    Curve[][] cvs;
    List<Line> lines = new List<Line>();
    List<Line> roofLines = new List<Line>();


    lineCurve.DivideByLength(baySpacing, false, out points);
    planes = new Plane[points.Length][];
    cvs = new Curve[planes.Length][];


    for (int j = 0; j < planes.Length; j++) {
      cvs[j] = new Curve[3];
      planes[j] = new Plane[2];
      planes[j][0] = new Plane(points[j], axis);
      planes[j][1] = new Plane(points[j] - (axis * trussWidth), axis);

      //rotate planes
      double angleRadians = rotate * Math.PI / 180.0;
      planes[j][0].Rotate(angleRadians, Vector3d.YAxis, points[j]);
      planes[j][1].Rotate(-angleRadians, Vector3d.YAxis, points[j]);




      Curve[] intCurves;
      Point3d[] intPts;
      Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, planes[j][0], Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intCurves, out intPts);
      for (int k = 0; k < 1; k++) {
        cvs[j][0] = intCurves[k];
      }

      Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, planes[j][1], Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intCurves, out intPts);
      for (int k = 0; k < 1; k++) {
        cvs[j][1] = intCurves[k];
      }
    }

  


    for (int i = 0; i < cvs.Length; i++) {
      //measure
      BoundingBox bb = cvs[i][0].GetBoundingBox(true);
      BoundingBox bb1 = cvs[i][1].GetBoundingBox(true);
      bb.Union(bb1);
      Point3d mid = new Point3d((bb.Max.X + bb.Min.X) * 0.5, (bb.Max.Y + bb.Min.Y) * 0.5, bb.Min.Z);
      Plane plane = new Plane(mid, Vector3d.ZAxis);
      cvs[i][2] = Curve.CreateMeanCurve(cvs[i][0], cvs[i][1]);

      //make truss
      double curveLength;
      try { curveLength = cvs[i][2].GetLength(); } catch { curveLength = 1.0; }
      double depth = curveLength / spanRatio;
      Transform scaleHorizontal = Transform.Scale(plane, scaleXY, scaleXY, 1.0);
      cvs[i][2].Transform(scaleHorizontal);

      Curve x0 = cvs[i][0];
      Curve x1 = cvs[i][1];
      Curve i2 = cvs[i][2];

      Point3d[] points0;
      Point3d[] points1;
      Point3d[] points2;
      x0.DivideByCount(trussCount, true, out points0);
      x1.DivideByCount(trussCount, true, out points1);
      i2.DivideByCount(trussCount, true, out points2);


      for (int j = 1; j < points2.Length; j++) {
        Transform move = Transform.Translation(new Vector3d(0, 0, (-depth * trussTriangles[j])));
        points2[j].Transform(move);
      }



      for (int j = 2; j < points2.Length; j += 2) {
        Line l0 = new Line(points0[j], points2[j - 1]);
        lines.Add(l0);
        Line l1 = new Line(points0[j - 2], points2[j - 1]);
        lines.Add(l1);
        Line l2 = new Line(points1[j], points2[j - 1]);
        lines.Add(l2);
        Line l3 = new Line(points1[j - 2], points2[j - 1]);
        lines.Add(l3);
        Line l4 = new Line(points0[j], points1[j]);
        lines.Add(l4);

        if (j > 3) {
          Line l5 = new Line(points2[j - 1], points2[j - 3]);
          lines.Add(l5);
        }


      }




      for (int j = 0; j < 2; j++) {
        curves.Add(cvs[i][j]);
      }



    }




    //roof
    for (int i = 1; i < cvs.Length; i++) {
      Curve crv0 = cvs[i - 1][0];
      Curve crv1 = cvs[i][1];
      Point3d[] points0, points1;
      crv0.DivideByCount(trussCount, true, out points0);
      crv1.DivideByCount(trussCount, true, out points1);
      for (int j = 2; j < points0.Length; j += 2) {
        Line l6 = new Line(points0[j], points1[j]);
        roofLines.Add(l6);
      }
    }


    A = curves;
    B = lines;
    C = roofLines;
    #endregion





  }

  // <Custom additional code> 

  // </Custom additional code> 
}