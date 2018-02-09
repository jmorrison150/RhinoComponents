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
    private void RunScript(List<Curve> curves, double spacing, ref object A) {







        #region beginScript
        List<Brep> updateBreps = new List<Brep>();
        List<Curve> updateCurves = new List<Curve>();
        List<Polyline> updatePolylines = new List<Polyline>();


        BoundingBox bb = new BoundingBox();
        for (int i = 0; i < curves.Count; i++) {
            bb.Union(curves[i].GetBoundingBox(false));
        }

        Point3d pt0;
        Point3d pt1;
        pt0 = new Point3d(bb.Min.X, bb.Min.Y, bb.Min.Z);
        pt1 = new Point3d(bb.Max.X, bb.Min.Y, bb.Min.Z);
        LineCurve line = new LineCurve(pt0, pt1);
        Point3d[] frameBasePoint;
        line.DivideByLength(spacing, true, out frameBasePoint);




        for (int i = 0; i < frameBasePoint.Length; i++) {
            Plane plane = new Plane(frameBasePoint[i], Vector3d.XAxis);
            List<Point3d> intersectionPts = new List<Point3d>();

            for (int j = 0; j < curves.Count; j++) {

                Rhino.Geometry.Intersect.CurveIntersections intersections = Rhino.Geometry.Intersect.Intersection.CurvePlane(curves[j], plane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                try {
                    bool zero = intersections.Count < 1;
                } catch (Exception) {
                    continue;
                }
                for (int k = 0; k < intersections.Count; k++) {
                    if (intersections[k].IsPoint) {
                        intersectionPts.Add(intersections[k].PointA);
                    }
                }

            }

            if (intersectionPts.Count < 2) {
                continue;
            }

            
            intersectionPts.Sort(delegate (Point3d pt2, Point3d pt3) { return pt2.Y.CompareTo(pt3.Y); });
           // Point3d[] pts = intersectionPts.ToArray();

           // Array.Sort(pts, delegate (Point3d pt4, Point3d pt5) { return pt4.Y.CompareTo(pt5.Y); });

            //Polyline pl = new Polyline(intersectionPts);
            Curve curve = Curve.CreateControlPointCurve(intersectionPts, 1);
            //Brep[] squarePipe = squarePipe(curve);
            updateCurves.Add(curve);
        }
        A = updateCurves;

        #endregion





    }

    // <Custom additional code> 

    #region customCode
    Brep[] squarePipe(Curve curve) {



        double width = 1.0;
        double length = 1.0;
        int resolution = 200;
        Point3d[] pts;
        double[] ts = curve.DivideByCount(resolution, true, out pts);





        Polyline pl = new Polyline(pts);
        pl.ReduceSegments(Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
        Plane[] planes = new Plane[pl.Count];
        Curve[] profiles = new Curve[pl.Count];
        double[] polylineParameters = new double[pl.Count];
        for (int i = 0; i < pl.Count; i++) {
            double t;
            curve.ClosestPoint(pl[i], out t);
            polylineParameters[i] = t;
        }

        for (int i = 0; i < pl.Count; i++) {

            //curve.FrameAt(polylineParameters[i], out planes[i]);
            planes = curve.GetPerpendicularFrames(polylineParameters);
            //curve.PerpendicularFrameAt(polylineParameters[i], out planes[i]);

            Interval widthInterval = new Interval(-width * 0.5, width * 0.5);
            Interval widthLength = new Interval(-length * 0.5, length * 0.5);

            Rectangle3d rt = new Rectangle3d(planes[i], widthInterval, widthLength);





            profiles[i] = rt.ToNurbsCurve();
        }

        Brep[] lofts = Brep.CreateFromLoft(profiles, Point3d.Unset, Point3d.Unset, LoftType.Tight, false);
        for (int i = 0; i < lofts.Length; i++) {
            lofts[i].Flip();
        }

        return lofts;


    }
    #endregion

    // </Custom additional code> 
}