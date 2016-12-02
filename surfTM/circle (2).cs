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
    private void RunScript(object x, object y, ref object A)
  {
    int dimension = 3;
    BOOL bIsRational = TRUE;
    int order = 3;
    int cv_count = 9;
 
        ON_NurbsCurve
    //ON_NurbsCurve nc( dimension, bIsRational, order, cv_count );
    nc.SetCV(0, ON_4dPoint(1.0, 0.0, 0.0, 1.0));
    nc.SetCV(1, ON_4dPoint(0.707107, 0.707107, 0.0, 0.707107));
    nc.SetCV(2, ON_4dPoint(0.0, 1.0, 0.0, 1.0));
    nc.SetCV(3, ON_4dPoint(-0.707107, 0.707107, 0.0, 0.707107));
    nc.SetCV(4, ON_4dPoint(-1.0, 0.0, 0.0, 1.0));
    nc.SetCV(5, ON_4dPoint(-0.707107, -0.707107, 0.0, 0.707107));
    nc.SetCV(6, ON_4dPoint(0.0, -1.0, 0.0, 1.0));
    nc.SetCV(7, ON_4dPoint(0.707107, -0.707107, 0.0, 0.707107));
    nc.SetCV(8, ON_4dPoint(1.0, 0.0, 0.0, 1.0));
    nc.SetKnot(0, 0.0);
    nc.SetKnot(1, 0.0);
    nc.SetKnot(2, 0.5 * ON_PI);
    nc.SetKnot(3, 0.5 * ON_PI);
    nc.SetKnot(4, ON_PI);
    nc.SetKnot(5, ON_PI);
    nc.SetKnot(6, 1.5 * ON_PI);
    nc.SetKnot(7, 1.5 * ON_PI);
    nc.SetKnot(8, 2.0 * ON_PI);
    nc.SetKnot(9, 2.0 * ON_PI);
 
    if( nc.IsValid() )
    {
      context.m_doc.AddCurveObject(nc);
      context.m_doc.Redraw();
    }
  }

    // <Custom additional code> 

    // </Custom additional code> 
}