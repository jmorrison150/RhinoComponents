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
    private void RunScript(Curve rail, double scale, double hBeam, double taper, ref object A, ref object B) {















        #region beginScript

        List<Brep> updateBreps = new List<Brep>();
        List<NurbsCurve> updateCurves = new List<NurbsCurve>();

        //Curve rail = new Curve;
        Rectangle3d[] rectanglesAtStart = wideFlange(rail, 0.0, scale, taper);
        Rectangle3d[] rectanglesAtEnd = wideFlange(rail, 1.0, scale, taper);

        for (int i = 0; i < 3; i++) {
            NurbsCurve[] ns = new NurbsCurve[2];
            ns[0] = rectanglesAtStart[i].ToNurbsCurve();
            ns[1] = rectanglesAtEnd[i].ToNurbsCurve();
            updateCurves.AddRange(ns);

            Brep[] sweep = Brep.CreateFromSweep(rail, ns, rail.IsClosed, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            updateBreps.AddRange(sweep);

        }

        A = updateBreps;
        B = updateCurves;
        #endregion


















    }

    // <Custom additional code> 


    #region customCode

    Rectangle3d[] wideFlange(Curve rail, double normalizedLength, double scale, double taper) {
        double hBeam = 0.5;
        taper *= normalizedLength;
        //Point3d pt = rail.PointAtNormalizedLength(normalizedLength);
        double t;
        rail.NormalizedLengthParameter(normalizedLength, out t);
        //Vector3d vec = rail.TangentAt(t);
        Plane plane;
        rail.FrameAt(t, out plane);
        plane = new Plane(plane.Origin, plane.ZAxis, plane.YAxis);

        Rectangle3d[] rectangles = new Rectangle3d[3];
        hBeam *= 0.2;


        Interval interval0X = new Interval((-0.181 - hBeam) * scale, (0.181 + hBeam) * scale);
        Interval interval0Y = new Interval((0.500 + taper) * scale, ((0.460 - (hBeam * 0.9)) + taper) * scale);
        Interval interval1X = new Interval((-0.012 - (hBeam * 0.3)) * scale, (0.012 + (hBeam * 0.3)) * scale);
        Interval interval1Y = new Interval(((0.460 - (hBeam * 0.9)) + taper) * scale, ((-0.460 + (hBeam * 0.9)) - taper) * scale);
        Interval interval2X = new Interval((-0.181 - hBeam) * scale, (0.181 + hBeam) * scale);
        Interval interval2Y = new Interval((-0.500 - taper) * scale, ((-0.460 + (hBeam * 0.9)) - taper) * scale);

        rectangles[0] = new Rectangle3d(plane, interval0X, interval0Y);
        rectangles[1] = new Rectangle3d(plane, interval1X, interval1Y);
        rectangles[2] = new Rectangle3d(plane, interval2X, interval2Y);

        return rectangles;
    }

    #endregion


    // </Custom additional code> 
}