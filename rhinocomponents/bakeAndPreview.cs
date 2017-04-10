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
public class Script_Instance18 : GH_ScriptInstance {
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
  private void RunScript(bool bake, List<GeometryBase> G, Point3d L, Color C) {
    COL = C;
    LOCATION = L;
    NAME = "My block instance";
    pnts.Clear(); crvs.Clear(); breps.Clear();

    foreach (GeometryBase geom in G) {
      switch (geom.GetType().Name) {
        case "Point":
          pnts.Add(((Rhino.Geometry.Point)geom).Location);
          break;
        case "Curve":
          //create a new geometry list for display
          break;
        case "PolyCurve":
          crvs.Add((PolyCurve)geom);
          break;
        case "Brep":
          breps.Add((Brep)geom);
          break;
        default:
          Print("Add a new case for this type: " + geom.GetType().Name);
          break;
      }
    }

    if (bake) {
      Rhino.DocObjects.InstanceDefinition I = doc.InstanceDefinitions.Find(NAME, false);

      if (I != null)
        doc.InstanceDefinitions.Delete(I.Index, true, true);

      int index = doc.InstanceDefinitions.Add(NAME, "description", Point3d.Origin, G);
      doc.Objects.AddInstanceObject(index, Transform.Scale(L, 1));
    }
  }

  // <Custom additional code> 
  //GEOMETRY Lists to display

  List<Point3d> pnts = new List<Point3d>();
  List<PolyCurve> crvs = new List<PolyCurve>();
  List<Brep> breps = new List<Brep>();

  string NAME;
  Point3d LOCATION;
  int THICKNESS = 2;
  Color COL;

  //Return a BoundingBox that contains all the geometry you are about to draw.
  public override BoundingBox ClippingBox {
    get {
      return BoundingBox.Empty;
    }
  }
  //Draw all meshes in this method.
  public override void DrawViewportMeshes(IGH_PreviewArgs args) {

  }

  //Draw all wires and points in this method.
  public override void DrawViewportWires(IGH_PreviewArgs args) {
    foreach (Point3d p in pnts)
      args.Display.DrawPoint(p, Rhino.Display.PointStyle.ControlPoint, THICKNESS, COL);

    foreach (PolyCurve c in crvs)
      args.Display.DrawCurve(c, COL, THICKNESS);

    foreach (Brep b in breps)
      args.Display.DrawBrepShaded(b, new Rhino.Display.DisplayMaterial(COL));

    args.Display.DrawPoint(LOCATION, Rhino.Display.PointStyle.ActivePoint, 3, Color.Black);
    args.Display.Draw3dText(NAME, Color.Gray, new Plane(LOCATION, Vector3d.ZAxis), THICKNESS / 3, "Arial");
  }

  // </Custom additional code> 
}