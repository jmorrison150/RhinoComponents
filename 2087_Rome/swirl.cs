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
public class Script_Instance : GH_ScriptInstance
{
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
    private void RunScript(Brep surface, Brep surface1, int uCount, int vCount, double curvy, ref object A, ref object B)
    {
        int kCount = 3;

        Curve[] crvs = trimAwareCurves(surface, uCount);
        //Point3d[][] pts = divide(crvs, vCount);
        Point3d[][] pts = divideSurface(surface, uCount, vCount);
        pts = makeCurvy(pts, curvy, kCount);
        pts = offset(pts, surface, surface1, kCount);
        NurbsSurface ns = nurbsSurfaceByPoints(pts);


        A = ns;
        B = toArray(pts);
    }

    // <Custom additional code> 
    Point3d[][] offset(Point3d[][] points, Brep surface, Brep surface1, int kCount)
    {
        Point3d[][] pts = points;
        for (int i = 0; i < pts.Length; i++)
        {

            for (int j = kCount; j < pts[i].Length - kCount; j++)
            {
                if ((
                  (i % 2 == 0) && (j % (kCount * 2) == 0)
                  ) || (
                  (i % 2 == 1) && (j % (kCount * 2) == kCount)
                ))
                {

                    if (surface1 == null)
                    {
                        //            pts[i][j - kCount + 1] = project(pts[i][j - kCount + 1], surface, surface);
                        //            pts[i][j + kCount - 1] = project(pts[i][j + kCount - 1], surface, surface);

                    }
                    else
                    {

                        pts[i][j - kCount + 1] = project(pts[i][j - kCount + 1], surface, surface1);
                        pts[i][j + kCount - 1] = project(pts[i][j + kCount - 1], surface, surface1);
                        if (i > 0)
                        {
                            pts[i - 1][j - kCount + 1] = project(pts[i - 1][j - kCount + 1], surface, surface1);
                            pts[i - 1][j + kCount - 1] = project(pts[i - 1][j + kCount - 1], surface, surface1);
                        }

                        //pts[i][j - kCount+1] += Vector3d.ZAxis * amplitude;
                        //pts[i][j + kCount-1] += Vector3d.ZAxis * amplitude;
                    }

                }
            }
        }
        return pts;

    }
    Point3d[][] makeCurvy(Point3d[][] points, double curvy, int kCount)
    {
        Point3d[][] pts = points;
        for (int i = 0; i < pts.Length; i++)
        {

            for (int j = kCount; j < pts[i].Length - kCount; j++)
            {
                if ((
                  (i % 2 == 0) && (j % (kCount * 2) == 0)
                  ) || (
                  (i % 2 == 1) && (j % (kCount * 2) == kCount)
                ))
                {

                    //curvy
                    Transform scale = Transform.Scale(pts[i][j], 1.0 - curvy);
                    for (int k = 1; k < kCount; k++)
                    {
                        pts[i][j - k].Transform(scale);
                        pts[i][j + k].Transform(scale);
                    }
                }
            }

        }
        return pts;
    }
    Point3d project(Point3d pt, Brep surface, Brep surface1)
    {

        //normal
        Point3d pt0 = pt;
        Point3d brepPt0;
        ComponentIndex ci0;
        Vector3d normal0;
        double s0, t0;
        surface.ClosestPoint(pt0, out brepPt0, out ci0, out s0, out t0, double.MaxValue, out normal0);

        Ray3d ray0;
        ray0 = new Ray3d(pt0, normal0);
        List<GeometryBase> geometry = new List<GeometryBase>();
        geometry.Add(surface1);
        int reflections = 1;
        Point3d[] rayPts = Rhino.Geometry.Intersect.Intersection.RayShoot(ray0, geometry, reflections);
        if (rayPts != null && rayPts.Length >= 1)
        {
            return rayPts[0];
        }
        else
        {
            return pt;
        }

    }
    NurbsSurface nurbsSurfaceByPoints(Point3d[][] points)
    {
        int uCount, vCount, uDegree, vDegree;
        uDegree = 2;
        vDegree = 2;
        uCount = points.Length;
        vCount = points[0].Length;
        Point3d[] pts = new Point3d[uCount * vCount];

        for (int i = 0; i < points.Length; i++)
        {
            for (int j = 0; j < points[i].Length; j++)
            {
                pts[(i * points[0].Length) + j] = points[i][j];
            }
        }

        NurbsSurface ns;
        ns = NurbsSurface.CreateFromPoints(pts, uCount, vCount, uDegree, vDegree);
        //ns = NurbsSurface.CreateThroughPoints(pts, uCount, vCount, uDegree, vDegree, false, false);
        return ns;

    }
    Point3d[][] divideSurface(Brep b, int uCount, int vCount)
    {
        Point3d[][] pts = new Point3d[uCount][];
        for (int i = 0; i < pts.Length; i++)
        {
            pts[i] = new Point3d[vCount];
        }

        for (int i = 0; i < b.Faces.Count; i++)
        {

            double minU, maxU, minV, maxV;
            minU = b.Faces[i].Domain(0).Min;
            maxU = b.Faces[i].Domain(0).Max;
            minV = b.Faces[i].Domain(1).Min;
            maxV = b.Faces[i].Domain(1).Max;
            for (int u = 0; u < uCount; u++)
            {
                double paramU = map((double)u, 0, (double)uCount - 1, minU, maxU);
                for (int v = 0; v < vCount; v++)
                {
                    double paramV = map((double)v, 0, (double)vCount - 1, minV, maxV);
                    //Point2d pt = new Point2d(paramU, paramV);
                    pts[u][v] = b.Faces[i].PointAt(paramU, paramV);
                }
            }

        }
        return pts;
    }
    Point3d[][] divide(Curve[] curves, int vCount)
    {

        Point3d[][] outPoints = new Point3d[curves.Length][];
        for (int i = 0; i < curves.Length; i++)
        {
            curves[i].DivideByCount(vCount, true, out outPoints[i]);
        }

        return outPoints;

    }
    Curve[] trimAwareCurves(Brep b, int uCount)
    {
        List<Curve> curves = new List<Curve>();
        int currentDir = 1;

        for (int i = 0; i < b.Faces.Count; i++)
        {


            //isoCurve
            for (int j = 0; j < uCount; j++)
            {
                double min, max;
                min = b.Faces[i].Domain((currentDir + 1) % 2).Min;
                max = b.Faces[i].Domain((currentDir + 1) % 2).Max;
                double param = map((double)j, 0, (double)uCount - 1, min, max);
                Curve[] crvs;
                crvs = b.Faces[i].TrimAwareIsoCurve((currentDir + 0) % 2, param);
                if (crvs != null)
                {
                    curves.AddRange(crvs);
                }
            }
        }
        return curves.ToArray();
    }
    double map(double value1, double min1, double max1, double min2, double max2)
    {
        double value2 = min2 + (value1 - min1) * (max2 - min2) / (max1 - min1);
        return value2;
    }
    List<Point3d> toArray(Point3d[][] pts)
    {

        List<Point3d> outPoints = new List<Point3d>();
        for (int i = 0; i < pts.Length; i++)
        {
            outPoints.AddRange(pts[i]);
        }

        return outPoints;
    }
    // </Custom additional code> 
}