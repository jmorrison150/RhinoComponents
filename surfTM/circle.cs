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
    bool bIsRational = true;
    int order = 3;
    int cv_count = 9;
 
        Point4d[] setCV = new Point4d[9];
        setCV[0] = new Point4d(1.0, 0.0, 0.0, 1.0);
        setCV[1] = new Point4d(0.707107, 0.707107, 0.0, 0.707107);
        setCV[2] = new Point4d(0.0, 1.0, 0.0, 1.0);
        setCV[3] = new Point4d(-0.707107, 0.707107, 0.0, 0.707107);
        setCV[4] = new Point4d(-1.0, 0.0, 0.0, 1.0);
        setCV[5] = new Point4d(-0.707107, -0.707107, 0.0, 0.707107);
        setCV[6] = new Point4d(0.0, -1.0, 0.0, 1.0);
        setCV[7] = new Point4d(0.707107, -0.707107, 0.0, 0.707107);
        setCV[8] = new Point4d(1.0, 0.0, 0.0, 1.0);


        Point3d[] cvPoints = new Point3d[9];
        setCV[0] = new Point3d(1.0, 0.0, 0.0, 1.0);
        setCV[1] = new Point3d(0.707107, 0.707107, 0.0, 0.707107);
        setCV[2] = new Point3d(0.0, 1.0, 0.0, 1.0);
        setCV[3] = new Point3d(-0.707107, 0.707107, 0.0, 0.707107);
        setCV[4] = new Point3d(-1.0, 0.0, 0.0, 1.0);
        setCV[5] = new Point3d(-0.707107, -0.707107, 0.0, 0.707107);
        setCV[6] = new Point3d(0.0, -1.0, 0.0, 1.0);
        setCV[7] = new Point3d(0.707107, -0.707107, 0.0, 0.707107);
        setCV[8] = new Point3d(1.0, 0.0, 0.0, 1.0);

        Point2d[] setKnot = new Point2d[10];
        setKnot[0] = new Point2d(  0, 0.0  );
        setKnot[0] = new Point2d(  1, 0.0  );
        setKnot[0] = new Point2d(  2, 0.5 * Math.PI  );
        setKnot[0] = new Point2d(  3, 0.5 * Math.PI  );
        setKnot[0] = new Point2d(  4, Math.PI  );
        setKnot[0] = new Point2d(  5, Math.PI  );
        setKnot[0] = new Point2d(  6, 1.5 * Math.PI  );
        setKnot[0] = new Point2d(  7, 1.5 * Math.PI );
        setKnot[0] = new Point2d(  8, 2.0 * Math.PI  );
        setKnot[0] = new Point2d(  9, 2.0 * Math.PI  );

        NurbsCurve nc = NurbsCurve.Create(true, 2, setCV);
        NurbsCurve.CreateControlPointCurve(setCV, 2);
        NurbsCurve.CreateInterpolatedCurve(
    //ON_NurbsCurve nc( dimension, bIsRational, order, cv_count );


    // <Custom additional code> 

    // </Custom additional code> 
}