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
    private void RunScript(Brep roof, ref object A, ref object B, ref object C, ref object outWidth, ref object outLength) {


        #region beginScript

        double width = 100.000;
        double length = 50.000;

        double offsetMajor = 2.000;
        double offsetMinor = 1.000;

        int divideCount = 2;
        int divideCount4 = 4;

        Surface s = roof.Surfaces[0];
        s = s.Extend(IsoStatus.North, length * 2, true);
        s = s.Extend(IsoStatus.South, length, true);
        s = s.Extend(IsoStatus.East, width * 2, true);
        s = s.Extend(IsoStatus.West, width * 2, true);

        Brep brep = s.ToBrep();

        List<NurbsSurface> srf0 = diagrid(brep, width, length);

        for (int i = 0; i < srf0.Count; i++) {
            double factor = (width - offsetMajor) / width;
            Point3d anchor = srf0[i].PointAt(srf0[i].Domain(0).Mid, srf0[i].Domain(1).Mid);
            Transform scale = Transform.Scale(anchor, factor);
            srf0[i].Transform(scale);
        }

        List<NurbsSurface> srf2 = subDivide(srf0, divideCount);
        for (int i = 0; i < srf2.Count; i++) {

            Point3d anchor = srf2[i].PointAt(srf2[i].Domain(0).Mid, srf2[i].Domain(1).Mid);
            double scaleFactor = (((width - offsetMajor) / divideCount) - offsetMinor) / ((width - offsetMajor) / divideCount);
            Transform scale = Transform.Scale(anchor, scaleFactor);
            srf2[i].Transform(scale);
        }

        List<NurbsSurface> srf3 = subDivide(srf2, divideCount4);


        List<Point3d> centerPts = new List<Point3d>();

        for (int i = 0; i < srf3.Count; i++) {

            double u = srf3[i].Domain(0).Mid;
            double v = srf3[i].Domain(1).Mid;
            Point3d pt = srf3[i].PointAt(u, v);
            Point3d closestPoint;
            ComponentIndex ci;
            double t0, t1;
            Vector3d normal;
            roof.ClosestPoint(pt, out closestPoint, out ci, out t0, out t1, 0.001, out normal);
            centerPts.Add(closestPoint);

        }


        A = centerPts;


        C = srf0;

        outWidth = (((width - offsetMajor) / divideCount) - offsetMinor) / divideCount4; 
        outLength = (((length - offsetMajor) / divideCount) - offsetMinor) / divideCount4;
        #endregion








    }

    // <Custom additional code> 

    #region customCode

    List<NurbsSurface> diagrid(Brep brep, double width, double length) {


        List<NurbsSurface> srfs = new List<NurbsSurface>();
        Curve min1 = brep.Faces[0].TrimAwareIsoCurve(0, brep.Faces[0].Domain(1).Mid)[0];
        double[] ts1 = min1.DivideByLength(width, true);
        Curve min2 = brep.Faces[0].TrimAwareIsoCurve(1, brep.Faces[0].Domain(0).Mid)[0];
        double[] ts2 = min2.DivideByLength(length, true);

        for (int j = 1; j < ts1.Length - 1; j++) {
            for (int k = 1; k < ts2.Length - 1; k++) {
                if ((j % 2 == 0 && k % 2 == 0) || (j % 2 == 1 && k % 2 == 1)) {

                    Point3d pt0 = brep.Faces[0].PointAt(ts1[j - 1], ts2[k]);
                    Point3d pt1 = brep.Faces[0].PointAt(ts1[j], ts2[k + 1]);
                    Point3d pt2 = brep.Faces[0].PointAt(ts1[j + 1], ts2[k]);
                    Point3d pt3 = brep.Faces[0].PointAt(ts1[j], ts2[k - 1]);

                    NurbsSurface ns = NurbsSurface.CreateFromCorners(pt0, pt1, pt2, pt3);
                    srfs.Add(ns);
                }
            }
        }
        return srfs;
    }
    List<NurbsSurface> subDivide(List<Surface> srf1, double width, double length) {
        List<NurbsSurface> srf2 = new List<NurbsSurface>();

        for (int i = 0; i < srf1.Count; i++) {


            Curve min1 = srf1[i].IsoCurve(1, srf1[i].Domain(0).Mid);
            double[] ts1 = min1.DivideByLength(width, true);
            Curve min2 = srf1[i].IsoCurve(0, srf1[i].Domain(1).Mid);
            double[] ts2 = min2.DivideByLength(length, true);

            for (int j = 0; j < ts1.Length - 1; j++) {
                for (int k = 0; k < ts2.Length - 1; k++) {

                    Point3d pt0 = srf1[i].PointAt(ts1[j], ts2[k]);
                    Point3d pt1 = srf1[i].PointAt(ts1[j], ts2[k + 1]);
                    Point3d pt2 = srf1[i].PointAt(ts1[j + 1], ts2[k + 1]);
                    Point3d pt3 = srf1[i].PointAt(ts1[j + 1], ts2[k]);


                    NurbsSurface ns = NurbsSurface.CreateFromCorners(pt0, pt1, pt2, pt3);
                    srf2.Add(ns);
                }
            }






        }

        return srf2;

    }
    List<NurbsSurface> subDivide(List<NurbsSurface> srf1, int divideCount) {
        List<NurbsSurface> srf2 = new List<NurbsSurface>();

        for (int i = 0; i < srf1.Count; i++) {


            Curve min1 = srf1[i].IsoCurve(1, srf1[i].Domain(0).Mid);
            double[] ts1 = min1.DivideByCount(divideCount, true);
            Curve min2 = srf1[i].IsoCurve(0, srf1[i].Domain(1).Mid);
            double[] ts2 = min2.DivideByCount(divideCount, true);

            for (int j = 0; j < divideCount; j++) {
                for (int k = 0; k < divideCount; k++) {

                    Point3d pt0 = srf1[i].PointAt(ts1[j], ts2[k]);
                    Point3d pt1 = srf1[i].PointAt(ts1[j], ts2[k + 1]);
                    Point3d pt2 = srf1[i].PointAt(ts1[j + 1], ts2[k + 1]);
                    Point3d pt3 = srf1[i].PointAt(ts1[j + 1], ts2[k]);


                    NurbsSurface ns = NurbsSurface.CreateFromCorners(pt0, pt1, pt2, pt3);
                    srf2.Add(ns);
                }
            }






        }

        return srf2;

    }
    List<NurbsSurface> makeSurfaces(double width, double length, double offsetMajor, int copyX, int copyY) {
        List<NurbsSurface> srfs = new List<NurbsSurface>();
        List<NurbsSurface> updateSurfaces2 = new List<NurbsSurface>();

        Rectangle3d rect = new Rectangle3d(Plane.WorldXY, width, length);
        Point3d[] pts = new Point3d[4];
        for (int i = 0; i < pts.Length; i++) {
            pts[i] = rect.PointAt(i + 0.5);
        }
        NurbsSurface ns0 = NurbsSurface.CreateFromCorners(pts[0], pts[1], pts[2], pts[3]);

        Point3d anchor = ns0.PointAt(0.5, 0.5);
        double scaleFactor = (pts[1].DistanceTo(pts[0]) - offsetMajor) / pts[1].DistanceTo(pts[0]);
        Transform scale = Transform.Scale(anchor, scaleFactor);
        ns0.Transform(scale);
        srfs.Add(ns0);


        for (int i = 1; i < copyX; i++) {
            Transform xform = Transform.Translation((pts[1] - pts[0]) * i);
            NurbsSurface ns1 = (NurbsSurface)ns0.Duplicate();
            ns1.Transform(xform);
            srfs.Add(ns1);
        }

        for (int i = 0; i < srfs.Count; i++) {
            updateSurfaces2.Add(srfs[i]);
        };


        for (int i = 0; i < srfs.Count; i++) {
            for (int j = 1; j < copyY; j++) {
                Transform xform = Transform.Translation((pts[3] - pts[1]) * j);
                NurbsSurface ns2 = (NurbsSurface)srfs[i].Duplicate();
                ns2.Transform(xform);
                updateSurfaces2.Add(ns2);
            }
        }








        return updateSurfaces2;

    }
    #endregion




    // </Custom additional code> 
}