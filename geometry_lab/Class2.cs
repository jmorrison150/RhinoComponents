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
public class Script_Instance7 : GH_ScriptInstance {
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
    private void RunScript(Curve curve, List<Point3d> startPoints, double scale, double rotate, bool mirror, ref object outCurves) {
        BoundingBox box = curve.GetBoundingBox(false);
        Plane mirrorPlane = new Plane(box.Center, Vector3d.XAxis);

        Transform xformMirror = Transform.Mirror(mirrorPlane);
        Transform xformScale = Transform.Scale(new Plane(box.Center, Vector3d.ZAxis), scale, scale, scale);
        Transform xformRotate = Transform.Rotation(rotate * System.Math.PI / 180.0, Vector3d.ZAxis, box.Center);
        Transform xform = xformRotate * xformScale;
        if(mirror) { curve.Transform(xformMirror); }
        curve.Transform(xform);

        if(startPoints.Count<1) {
            startPoints = new List<Point3d>();
            startPoints.Add(Point3d.Origin);
        }
        Curve[] curves = new Curve[startPoints.Count];
        for(int i = 0; i < startPoints.Count; i++) {
            curves[i] = curve;
            Point3d curvePoint = curves[i].PointAtStart;
            Vector3d moveVector = startPoints[i] - curvePoint;
            Transform xformTranslate = Transform.Translation(moveVector);
            curve.Transform(xformTranslate);

        }


        outCurves = curves.ToList();
    }

    // <Custom additional code> 

    // </Custom additional code> 
}