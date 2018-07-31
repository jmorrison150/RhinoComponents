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
    private void RunScript(Brep trimSurface, double xSpacing, double minHole, bool uvToggle, bool diagrid, ref object B) {






        #region beginScript


        //set variables
        double rotate = 0.0;
        //        int maxPerfCount = 40;
        double maxHoleSize = 0.145833;
        double extraSpace = 0.0;
        //bool diagrid = true;
        if (diagrid) {
            rotate = 45.0;
        }

        //empty list
        List<Curve> updateCurves = new List<Curve>();
        Brep brep = trimSurface;

        double length, width;
        length = xSpacing;
        width = xSpacing;

        double panelMin = maxHoleSize + extraSpace;
        if (length < panelMin) { length = panelMin; }
        length = -1;

        int toggleU = 0;
        int toggleV = 1;
        if (uvToggle) {
            toggleU = 1;
            toggleV = 0;
        }

        //subdivide untrimmed surface
        //watch out for [0]
        Surface surface = trimSurface.Surfaces[0];
        Interval domain = surface.Domain(toggleV);
        Curve mid = surface.IsoCurve(toggleV, domain.Mid);
        double[] parameters = mid.DivideByLength(width, true);
        for (int i = 1; i < parameters.Length; i++) {
            Curve c = surface.IsoCurve(toggleU, parameters[i]);
            updateCurves.Add(c);

            if (length > 0) {
                double panelLength = 0;
                while (panelLength < c.GetLength()) {
                    double panelParam;
                    c.LengthParameter(panelLength, out panelParam);
                    Point2d[] points = new Point2d[2];
                    points[0] = new Point2d(panelParam, parameters[i]);
                    points[1] = new Point2d(panelParam, parameters[i - 1]);
                    if (uvToggle) {
                        points[0] = new Point2d(parameters[i], panelParam);
                        points[1] = new Point2d(parameters[i - 1], panelParam);
                    }
                    Curve cc = surface.InterpolatedCurveOnSurfaceUV(points, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                    updateCurves.Add(cc);

                    panelLength += panelMin;
                }
            }

        }


        //make perforations
        List<Curve> updatePerf = new List<Curve>();
        List<Point3d> updatePts = new List<Point3d>();

        double maxLength = 0;
        for (int i = 0; i < updateCurves.Count; i++) {
            double curveLength = updateCurves[i].GetLength();
            if (curveLength > maxLength) {
                maxLength = curveLength;
            }
        }


        int maxPerfCount = (int)(maxLength / xSpacing);
        double holeSpacing = maxLength / maxPerfCount;


        double[] multiplierNormalized = new double[maxPerfCount + 1];
        for (int i = 0; i < multiplierNormalized.Length; i++) {
            multiplierNormalized[i] = map(i, 0, multiplierNormalized.Length, 1.0, minHole);
        }


        for (int i = 0; i < updateCurves.Count; i++) {


            double currentY = extraSpace;
            if (diagrid) {
                currentY += (i % 2) * (holeSpacing * 0.5);
            }


            int currentCount = 0;
            int holeCount = 0;
            while (currentY < updateCurves[i].GetLength()) {





                Point3d pt = updateCurves[i].PointAtLength(currentY);
                updatePts.Add(pt);
                double holeRadius = maxHoleSize * 0.5 * multiplierNormalized[currentCount];
                Point3d closestPt;
                ComponentIndex ci;
                double s;
                double t;
                Vector3d normal;
                bool test = brep.ClosestPoint(pt, out closestPt, out ci, out s, out t, holeRadius, out normal);

                if (test) {
                    holeCount++;
                    if (holeCount >= 1) {

                        Plane plane = new Plane(pt, normal);


                        //define perforation shape
                        Circle c = new Circle(plane, holeRadius);
                        Rectangle3d rect = new Rectangle3d(plane, new Interval(-holeRadius, holeRadius), new Interval(-holeRadius, holeRadius));
                        Transform rotation = Transform.Rotation(rad(rotate), normal, pt);
                        rect.Transform(rotation);

                        NurbsCurve ns;
                        //ns = c.ToNurbsCurve();
                        ns = rect.ToNurbsCurve();
                        updatePerf.Add(ns);
                    }
                }


                currentY += holeSpacing;
                currentCount++;
            }

        }


        // A = updatePts;
        B = updatePerf;
        #endregion




    }

    // <Custom additional code> 

    public double map(double number, double low1, double high1, double low2, double high2) {
        return low2 + (high2 - low2) * (number - low1) / (high1 - low1);
    }
    double rad(double degree) {

        return degree * Math.PI / 180.0;
    }
    // </Custom additional code> 
}