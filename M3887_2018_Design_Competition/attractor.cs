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
    private void RunScript(Curve  movingCurve, List<Curve> curves, double distance, ref object A) {


        double squared = distance * distance;
        NurbsCurve ns = movingCurve.ToNurbsCurve();

        for (int i = 0; i < ns.Points.Count; i++) {
            Point3d point = ns.Points[i].Location;

            for (int j = 0; j < curves.Count; j++) {
                double t;
                curves[j].ClosestPoint(point, out t);
                Point3d crvPt = curves[j].PointAt(t);
                double ptDist = point.DistanceToSquared(crvPt);
                if (ptDist < squared) {

                    Vector3d direction = crvPt - point;
                    direction *= (1 - (ptDist / squared));
                    point += direction;

                }

            }
            ControlPoint cpoint = new ControlPoint(point, ns.Points[i].Weight);
            ns.Points[i] = cpoint;
        }


        A = ns;
    }

    // <Custom additional code> 


    List<Point3d> attractor(List<Point3d> movingPoints,List<Curve> curves,double distance ) {
        double squared = distance * distance;

        for (int i = 0; i < movingPoints.Count; i++) {
            for (int j = 0; j < curves.Count; j++) {
                double t;
                curves[j].ClosestPoint(movingPoints[i], out t);
                Point3d crvPt = curves[j].PointAt(t);
                double ptDist = movingPoints[i].DistanceToSquared(crvPt);
                if (ptDist < squared) {
                    Vector3d direction = crvPt - movingPoints[i];
                    direction *= (1 - (ptDist / squared));
                    movingPoints[i] += direction;
                }

            }
        }


        return movingPoints;

    }
    NurbsCurve attractor(Curve movingCurve, List<Curve> curves, double distance) {
        double squared = distance * distance;
        NurbsCurve ns = movingCurve.ToNurbsCurve();

        for (int i = 0; i < ns.Points.Count; i++) {
            Point3d point = ns.Points[i].Location;

            for (int j = 0; j < curves.Count; j++) {
                double t;
                curves[j].ClosestPoint(point, out t);
                Point3d crvPt = curves[j].PointAt(t);
                double ptDist = point.DistanceToSquared(crvPt);
                if (ptDist < squared) {

                    Vector3d direction = crvPt - point;
                    direction *= (1 - (ptDist / squared));
                    point += direction;

                }

            }
            ControlPoint cpoint = new ControlPoint(point, ns.Points[i].Weight);
            ns.Points[i] = cpoint;
        }


        return ns;

    }
    Brep attractor(Brep movingBrep, List<Curve> curves, double distance) {

        double squared = distance * distance;


        for (int i = 0; i < movingBrep.Vertices.Count; i++) {
            Point3d point = movingBrep.Vertices[i].Location;

            for (int j = 0; j < curves.Count; j++) {
                double t;
                curves[j].ClosestPoint(point, out t);
                Point3d crvPt = curves[j].PointAt(t);
                double ptDist = point.DistanceToSquared(crvPt);
                if (ptDist < squared) {

                    Vector3d direction = crvPt - point;
                    direction *= (1 - (ptDist / squared));
                    point += direction;

                }

            }
            movingBrep.Vertices[i].Location = point;
        }


        return movingBrep;

    }
    Mesh attractor(Mesh movingMesh, List<Curve> curves, double distance) {

        double squared = distance * distance;

        for (int i = 0; i < movingMesh.Vertices.Count; i++) {

            Point3d point = new Point3d(movingMesh.Vertices[i].X, movingMesh.Vertices[i].Y, movingMesh.Vertices[i].Z);


            for (int j = 0; j < curves.Count; j++) {
                double t;
                curves[j].ClosestPoint(point, out t);
                Point3d crvPt = curves[j].PointAt(t);
                double ptDist = point.DistanceToSquared(crvPt);
                if (ptDist < squared) {
                    Vector3d direction = crvPt - point;
                    direction *= (1 - (ptDist / squared));
                    point += direction;

                }

            }
            Point3f meshPt = new Point3f((float)point.X, (float)point.Y, (float)point.Z);
            movingMesh.Vertices[i] = meshPt;

        }


        return movingMesh;

    }




    // </Custom additional code> 
}