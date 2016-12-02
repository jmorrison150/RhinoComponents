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
  private void RunScript(List<Surface> surfaces, List<double> width, List<double> rotation, 
    ref object outMesh, ref object A, ref object B, ref object C) {








    #region beginScript


    List<Mesh> updateMeshes = new List<Mesh>();
    List<Curve> updateCurves = new List<Curve>();
    List<Point3d> updatePoints = new List<Point3d>();
    List<Plane> updatePlanes = new List<Plane>();


    Surface[] ss = surfaces.ToArray();
    int u = 0;
    int v = 1;


    for (int i = 0; i < ss.Length; i++) {
      Curve pivotLine = (ss[i].IsoCurve(u, ss[i].Domain(v).Max));
      Point3d[] pivotPts = pivotLine.DivideEquidistant(width[0]);
      for (int j = 0; j < pivotPts.Length; j++) {
        double param;
        pivotLine.ClosestPoint(pivotPts[j], out param);
        Plane plane;
        pivotLine.FrameAt(param, out plane);


        //Vector3d normal = ss[i].NormalAt(        ss[i].NormalAt(ss[i].Domain(v).Mid), param)
        //Plane plane = new Plane()
        //  Vector3d.CrossProduct(Vector3d.ZAxis,normal );


      }

      updatePoints.AddRange(pivotPts);

    }


    outMesh = updateMeshes;
    A = updateCurves;
    B = updatePoints;
    C = updatePlanes;

    #endregion




  }

  // <Custom additional code> 

  // </Custom additional code> 
}