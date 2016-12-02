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
    private void RunScript(List<Curve> curves, List<int> skip, 
        double bend, double inflection, ref object A, ref object B) {




        List<double> divisions = divide(20);
        int offset = 2;


        Line[] lines0;
        Line[] lines1;
        connectLines(curves, skip, divisions, offset, out lines0, out lines1);


        Curve[] curves0 = bendLines(lines0, bend, inflection);
        Curve[] curves1 = bendLines(lines1, bend, inflection);

        A = curves0;
        B = curves1;





    }

    // <Custom additional code> 


    #region customCode
    Curve[] bendLines(Line[] lines, double bend, double inflection) {
        Curve[] crvs = new Curve[lines.Length];
        for(int i = 0; i < crvs.Length; i++) {

            Vector3d offset = Vector3d.CrossProduct(lines[i].Direction, Vector3d.ZAxis);
            offset.Unitize();
            offset *= bend;
            Point3d mid = new LineCurve(lines[i]).PointAtNormalizedLength(inflection);
            mid += offset;
            List<Point3d> pts = new List<Point3d>();
            pts.Add(lines[i].From);
            pts.Add(mid);
            pts.Add(lines[i].To);

            Curve c = Curve.CreateControlPointCurve(pts);
            crvs[i] = c;
        }

        return crvs;

    }
    void connectLines(List<Curve> curves, List<int> skip, List<double> divisions, int offset, out  Line[] A, out  Line[] B) {
        List<Line> lines0 = new List<Line>();
        List<Line> lines1 = new List<Line>();


        for(int j = 1; j < curves.Count; j++) {
            if(skip.Contains(j)) { continue; }


            Curve curve0 = curves[j - 1];
            Curve curve1 = curves[j];

            //make points
            Point3d[] pts0 = new Point3d[divisions.Count];
            for(int i = 0; i < divisions.Count; i++) {
                pts0[i] = curve0.PointAtNormalizedLength(divisions[i]);
            }

            Point3d[] pts1 = new Point3d[divisions.Count];
            for(int i = 0; i < divisions.Count; i++) {
                pts1[i] = curve1.PointAtNormalizedLength(divisions[i]);
            }

            //draw lines
            int min = Math.Min(pts0.Length, pts1.Length);
            for(int i = offset; i < min; i++) {
                Line l0 = new Line(pts0[i], pts1[i - offset]);
                Line l1 = new Line(pts1[i], pts0[i - offset]);
                lines0.Add(l0);
                lines1.Add(l1);
            }
        }



        A = lines0.ToArray();
        B = lines1.ToArray();
    }

    void tween(List<Curve> curves, List<int> skip, double spacing, ref object A, ref object B) {
        List<Curve> curves0 = new List<Curve>();
        List<Curve> curves1 = new List<Curve>();

        for(int i = 1; i < curves.Count; i++) {
            if(skip.Contains(i)) { continue; }


            Curve curve0 = curves[i - 1];
            Curve curve1 = curves[i];
            Point3d pt0, pt1;
            pt0 = curve0.PointAtNormalizedLength(0.5);
            pt1 = curve1.PointAtNormalizedLength(0.5);
            double[] ds = new double[3];
            ds[0] = curve0.PointAtStart.DistanceTo(curve1.PointAtStart);
            ds[1] = pt0.DistanceTo(pt1);
            ds[2] = curve0.PointAtEnd.DistanceTo(curve1.PointAtEnd);
            double ptDist = ds.Max();

            int count = (int) ( ptDist / spacing );
            int samples = (int) ( curve0.GetLength() / 0.300 );

            Curve[] cvs;
            // cvs = Curve.CreateTweenCurvesWithMatching(curve0, curve1, count);
            cvs = Curve.CreateTweenCurvesWithSampling(curve0, curve1, count, samples);
            curves0.AddRange(cvs);
        }
        A = curves0;
        B = curves1;
    }
    List<double> divide(int count) {
        List<double> ds = new List<double>();
        for(int i = 0; i < count; i++) {
            double d = (double) i / (double) count;
            ds.Add(d);
        }

        return ds;
    }
    #endregion






    // </Custom additional code> 
}