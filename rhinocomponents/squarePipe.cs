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
  private void RunScript(Curve curve, double width, double length, ref object A) {



    #region script

    int resolution = 200;
    Point3d[] pts;
    double[] ts = curve.DivideByCount(resolution, true, out pts);





    Polyline pl = new Polyline(pts);
    pl.ReduceSegments(Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
    Plane[] planes = new Plane[pl.Count];
    Curve[] profiles = new Curve[pl.Count];
    double[] polylineParameters = new double[pl.Count];
    for (int i = 0; i < pl.Count; i++) {
      double t;
      curve.ClosestPoint(pl[i], out t);
      polylineParameters[i] = t;
    }

    for (int i = 0; i < pl.Count; i++) {

      //curve.FrameAt(polylineParameters[i], out planes[i]);
      planes = curve.GetPerpendicularFrames(polylineParameters);
      //curve.PerpendicularFrameAt(polylineParameters[i], out planes[i]);

      Interval widthInterval = new Interval(-width * 0.5, width * 0.5);
      Interval widthLength = new Interval(-length * 0.5, length * 0.5);

      Rectangle3d rt = new Rectangle3d(planes[i], widthInterval, widthLength);





      profiles[i] = rt.ToNurbsCurve();
    }

    Brep[] lofts = Brep.CreateFromLoft(profiles, Point3d.Unset, Point3d.Unset, LoftType.Tight, false);
    for (int i = 0; i < lofts.Length; i++) {
      lofts[i].Flip();
    }

    A = lofts;

    #endregion



  }

  // <Custom additional code> 

  // </Custom additional code> 
}