﻿using Rhino;
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
public class Script_Instance13 : GH_ScriptInstance {
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
  private void RunScript(Mesh mesh, double amplitude, ref object A) {




    #region runScript
    List<Circle> updateCircles = new List<Circle>();


    Vector3d moveWhite = Vector3d.YAxis * amplitude;
    Vector3d moveBlack = Vector3d.YAxis * amplitude * -1.0;
    double threshold = 25;

    mesh.Normals.ComputeNormals();

    for (int i = 0; i < mesh.VertexColors.Count; i++) {
      double radius = mesh.VertexColors[i].GetBrightness();
      if (radius<=threshold) { continue; }
      radius /= 255;
      radius *= amplitude;
      Point3d point = new Point3d(mesh.Vertices[i]);


      Vector3d normal = mesh.Normals[i];
      Plane plane = new Plane(point, normal);
      Circle circle = new Circle(plane, radius);

      updateCircles.Add(circle);


      //  //test for white or black (0 to 1.0)
      //  if (mesh.VertexColors[i].GetBrightness() > 0.5) {

      //    //white
      //    Point3d pt = new Point3d(mesh.Vertices[i]);
      //    pt = pt + moveWhite;
      //    mesh.Vertices[i] = new Point3f((float)pt.X, (float)pt.Y, (float)pt.Z);
      //  } else {
      //    //Black
      //    Point3d pt = new Point3d(mesh.Vertices[i]);
      //    pt = pt + moveBlack;
      //    mesh.Vertices[i] = new Point3f((float)pt.X, (float)pt.Y, (float)pt.Z);
      //  }
      //}

      A = updateCircles;


    }
    #endregion


  }
    // <Custom additional code> 

    // </Custom additional code> 
  }