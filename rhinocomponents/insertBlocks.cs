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
public class Script_Instance19 : GH_ScriptInstance {
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
  private void RunScript(List<Point3d> insPt, double dblRotation, double dblScale, string BlockName, bool blnAddBlock, ref object A) {


    foreach (Guid item in blockList) {
      Rhino.RhinoDoc.ActiveDoc.Objects.Delete(item, true);
    }
    blockList.Clear();

    if (blnAddBlock) {
      foreach (Point3d pt in insPt) {
        blockList.Add(InsertBlock(BlockName, pt, dblScale, dblRotation));
      }
    }





  }








  // <Custom additional code> 

  List<Guid> blockList = new List<Guid>();

  Guid InsertBlock(string BlockName, Point3d insPt, double dblScale, double dblRotation) {

    Rhino.DocObjects.Tables.InstanceDefinitionTable blockTable = Rhino.RhinoDoc.ActiveDoc.InstanceDefinitions;
    Rhino.DocObjects.InstanceDefinition block = blockTable.Find(BlockName, true);
    Transform move = Transform.Translation(new Vector3d(insPt));
    Transform scale = Transform.Scale(Point3d.Origin, dblScale);
    Transform rotate = Transform.Rotation(Math.PI * dblRotation / 180.0, Point3d.Origin);

    Transform xform = move * scale * rotate;
    Guid insertBlock = Rhino.RhinoDoc.ActiveDoc.Objects.AddInstanceObject(block.Index, xform);
    return insertBlock;
  }




  // </Custom additional code> 
}