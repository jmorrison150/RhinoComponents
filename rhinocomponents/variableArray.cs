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
  private void RunScript(Curve arrayLine, Brep beamPrototype, List<Point3d> pointAttractors, List<Curve> lineAttractors, double min, double max, List<double> graph, double rotate,ref object A) {



    #region beginScript
    List<Brep> updateBreps = new List<Brep>();
    List<Point3d> pts = new List<Point3d>();
    List<Plane> updatePlanes = new List<Plane>();
    double currentLength = 0.0;
    double maxLength = arrayLine.GetLength();



    //varies
    while (currentLength < maxLength) {


      //planes
      double t0;
      arrayLine.LengthParameter(currentLength, out t0);
      Point3d pt = arrayLine.PointAt(t0);
      Vector3d a0 = arrayLine.TangentAt(t0);
      a0.Z = 0.0;
      double angleRadians = rotate * 0.0174533;
      Transform rotation = Transform.Rotation(angleRadians, Vector3d.ZAxis, pt);
      Transform rotation1 = Transform.Rotation(-angleRadians, Vector3d.ZAxis, pt);
      Plane plane0 = new Plane(pt, a0);
      Plane plane1 = new Plane(pt, a0);
      plane0.Transform(rotation);
      plane1.Transform(rotation1);
      updatePlanes.Add(plane0);
      updatePlanes.Add(plane1);

      double dist = double.MaxValue;



      //calculate next point
      ////line attractors
      for (int i = 0; i < lineAttractors.Count; i++) {
        double t;
        lineAttractors[i].ClosestPoint(pt, out t);
        Point3d linePt = lineAttractors[i].PointAt(t);
        double d = pt.DistanceTo(linePt);
        if (d < dist) { dist = d; }
      }

      ////point attractors
      for (int i = 0; i < pointAttractors.Count; i++) {
        double d = pt.DistanceTo(pointAttractors[i]);
        if (d < dist) { dist = d; }
      }

      dist *= 0.0001;
      dist *= max;
      if (dist < min) { dist = min; }
      if (dist > max) { dist = max; }
      currentLength += dist;
    }





    //array
    for (int i = 1; i < pts.Count; i++) {
      Transform move = Transform.Translation(pts[i] - pts[0]);
      Brep copy = beamPrototype.DuplicateBrep();
      copy.Transform(move);
      updateBreps.Add(copy);
    }



    //graph


    //output


    A = updatePlanes;

    #endregion



   
  }

  // <Custom additional code> 
  public double map(double number, double low1, double high1, double low2, double high2) {
    return low2 + (high2 - low2) * (number - low1) / (high1 - low1);
  }
  // </Custom additional code> 
}