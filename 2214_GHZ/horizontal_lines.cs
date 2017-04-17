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
public class Script_Instance4 : GH_ScriptInstance {
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
    private void RunScript(List<double> graph, List<Plane> planes, List<Curve> curves, List<Brep> breps, 
      double maxDist, double offsetDist, bool reset, ref object A, ref object B) {



        #region beginScript
        int resolution = 20;
        //double maxDist = 50.000;
        //double offsetDist = 0.250;
        double randomScale = 0;

        bool useAttractorCurves = true; //false uses attractor points

        List<Curve> crvs1 = new List<Curve>();
        List<Point3d> pts1 = new List<Point3d>();
        List<Brep> breps1 = new List<Brep>();
        Random random = new Random(0);

        //cache horizontal lines. This increases performance of the second run
        if(reset || intCrvs == null) { intCrvs = new Curve[planes.Count][]; }

        for(int k = 0; k < planes.Count; k++) {


            double localMax = maxDist - ( random.NextDouble() * maxDist * randomScale );
            Plane plane = planes[k];

            //limits attractor points to each line
            PointCloud pc = new PointCloud();
            if(!useAttractorCurves) {
                pc = getAttractorPoints(curves, plane);
                pts1.AddRange(pc.GetPoints());
            }

            //get lines for one floor level
            if(reset || intCrvs[k] == null) {
                intCrvs[k] = intersect(breps, plane);
                crvs1.AddRange(intCrvs[k]);
            }

            if(intCrvs[k] == null) { continue; }
            for(int i = 0; i < intCrvs[k].Length; i++) {


                //bool previous = false;
                Curve c = intCrvs[k][i];
                Point3d[] crvPts;
                double[] ds = intCrvs[k][i].DivideByCount(resolution, true, out crvPts);

                //this fixes disconnected curves
                if(intCrvs[k][i].IsClosed) {
                    List<Point3d> crvPts0 = crvPts.ToList();
                    crvPts0.Add(crvPts[0]);
                    crvPts = crvPts0.ToArray();
                }

                double[] distances = new double[crvPts.Length];
                double[] offsetDists = new double[crvPts.Length];
                Point3d[] offsetPts = new Point3d[crvPts.Length];
                Point3d[] offsetPts1 = new Point3d[crvPts.Length];
                bool[] previous = new bool[crvPts.Length];


                //calculate closest point
                if(useAttractorCurves) {
                    distances = closestPoint(crvPts, curves, localMax);
                } else {
                    for(int j = 0; j < crvPts.Length; j++) {
                        Point3d pt = pc[pc.ClosestPoint(crvPts[j])].Location;
                        distances[j] = crvPts[j].DistanceTo(pt);
                    }
                }



                //compute offset distances using graph
                for(int j = 0; j < offsetDists.Length; j++) {
                    if(distances[j] > localMax) {
                        offsetDists[j] = 0;
                    } else {
                        int index = (int) map(distances[j], 0, localMax, graph.Count - 1, 0);
                        if(index < 0) { index = 0; } else if(index > graph.Count - 1) { index = graph.Count - 1; }
                        offsetDists[j] = offsetDist * graph[index];
                    }

                }

                //previous
                for(int j = 0; j < distances.Length; j++) {
                    if(distances[j] < localMax) {
                        previous[j] = true;
                    } else {
                        previous[j] = false;
                    }
                }


                
                //offset points
                for(int j = 0; j < offsetPts.Length; j++) {
                    offsetPts[j] = getOffsetPt(crvPts[j], breps, offsetDists[j]);
                    offsetPts1[j] = getOffsetPt(crvPts[j], breps, -offsetDists[j]);
                }


                //make surfaces
                List<Brep> lofts = createLofts(offsetPts1, offsetPts, previous, intCrvs[k][i].IsClosed);
                breps1.AddRange(lofts);

            }


        }
        A = crvs1;
        B = breps1;
        #endregion




    }

    // <Custom additional code> 



    #region customCode
    Curve[][] intCrvs;
    double[] closestPoint(Point3d[] crvPts, List<Curve> crvs, double maximumDistance) {
        double[] distances = new double[crvPts.Length];
        for(int i = 0; i < distances.Length; i++) {
            double minDist = double.MaxValue;
            for(int j = 0; j < crvs.Count; j++) {
                double t;
                crvs[j].ClosestPoint(crvPts[i], out t, maximumDistance);
                Point3d pt = crvs[j].PointAt(t);
                double dist = pt.DistanceTo(crvPts[i]);
                if(dist < minDist) { minDist = dist; }
            }
            distances[i] = minDist;
        }
        return distances;
    }
    List<Brep> createLofts(Point3d[] pts0, Point3d[] pts1, bool[] previous, bool crvClosed) {
        if(pts0 == null || pts1 == null || previous == null) {
            Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "createLofts failed");
            return null;
        }

        List<Brep> lofts = new List<Brep>();
        int count = Math.Min(pts0.Length, pts1.Length);
        count = Math.Min(count, previous.Length);
        bool isClosed = false;
        if(crvClosed && !previous.Contains(false)) { isClosed = true; }
        if(isClosed) { count--; }
        int index = 0;




        while(index < count) {
            List<Curve> crvs = new List<Curve>();

            while(index < count && previous[index]) {
                //Print(previous[index].ToString());
                Point3d[] pts = new Point3d[2];
                pts[0] = pts0[index];
                pts[1] = pts1[index];
                Curve c = Curve.CreateControlPointCurve(pts);
                crvs.Add(c);
                index++;
            }

            Brep[] bs = Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Normal, isClosed);
            if(bs != null) {
                lofts.AddRange(bs);
            }
            index++;

        }
        return lofts;



    }
    Curve[] intersect(List<Brep> breps, Plane plane) {
        List<Curve> crvs = new List<Curve>();
        for(int i = 0; i < breps.Count; i++) {

            Curve[] intCrvs;
            Point3d[] intPts;
            Rhino.Geometry.Intersect.Intersection.BrepPlane(breps[i], plane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intCrvs, out intPts);
            if(intCrvs != null) {
                crvs.AddRange(intCrvs);
            }


        }
        return crvs.ToArray();
    }
    Point3d getOffsetPt(Point3d pt, List<Brep> breps, double dist) {
        if(dist == 0) { return pt; }

        double minDist = double.MaxValue;
        Vector3d normal = Vector3d.XAxis;
        int index = 0;
        for(int i = 0; i < breps.Count; i++) {
            Point3d closestPoint;
            ComponentIndex ci;
            double s, t;
            Vector3d tempNormal;
            breps[i].ClosestPoint(pt, out closestPoint, out ci, out s, out t, double.MaxValue, out tempNormal);
            double d = pt.DistanceTo(closestPoint);
            if(d < minDist) {
                if(ci.ComponentIndexType==ComponentIndexType.BrepEdge) {
                    int[] faceIndex = breps[i].Edges[ci.Index].AdjacentFaces();
                    if(faceIndex!=null) {
                        double u, v;
                        breps[i].Faces[faceIndex[0]].ClosestPoint(pt, out u, out v);
                        tempNormal = breps[i].Faces[faceIndex[0]].NormalAt(u, v);
                    }
                }
                minDist = d;
                normal = tempNormal;
                index = i;
            }
        }
        Vector3d xAxis, yAxis;
        xAxis = Vector3d.CrossProduct(normal, Vector3d.ZAxis);
        yAxis = Vector3d.CrossProduct(xAxis, normal);
        Plane plane = new Plane(pt, xAxis, yAxis);
        Point3d pt1 = pt + plane.YAxis * dist;
        Point3d pt2 = breps[index].ClosestPoint(pt1);


        return pt2;
    }
    PointCloud getAttractorPoints(List<Curve> curves, Plane plane) {
        List<Point3d> pts0 = new List<Point3d>();
        for(int k = 0; k < curves.Count; k++) {

            Rhino.Geometry.Intersect.CurveIntersections crvInts = Rhino.Geometry.Intersect.Intersection.CurvePlane(curves[k], plane, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            if(crvInts != null) {
                for(int i = 0; i < crvInts.Count; i++) {
                    if(crvInts[i].IsPoint) {
                        pts0.Add(crvInts[i].PointA);
                    }
                }
            }
        }

        return new PointCloud(pts0);
    }
    double map(double value1, double min1, double max1, double min2, double max2) {
        double value2 = min2 + ( value1 - min1 ) * ( max2 - min2 ) / ( max1 - min1 );
        return value2;
    }
    #endregion




    // </Custom additional code> 
}