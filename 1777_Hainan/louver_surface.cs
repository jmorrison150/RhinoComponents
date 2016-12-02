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
    private void RunScript(List<Curve> curves, Surface masterSurface, double angle, double max, double min, double frequency, ref object A)
    {


        #region beginScript


        int divideByCount = 100;
        bool useSurface = true;
        bool usePerpendicularFrames = true;



        //make empty lists that will hold our geometry
        List<Curve> updateCurves = new List<Curve>();
        List<Brep> updateBreps = new List<Brep>();
        List<Point3d> updatePoints = new List<Point3d>();
        List<Line> updateLines = new List<Line>();
        List<double> dists = new List<double>();
        List<Plane> updatePlanes = new List<Plane>();
        List<double> angles = new List<double>();
        for (int i = 0; i < curves.Count; i++)
        {
            angles.Add(angle);
        }

        double[][] distances = new double[curves.Count][];
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = new double[divideByCount];
            for (int j = 0; j < distances[i].Length; j++)
            {
                distances[i][j] = ((1.0 - (Math.Sin(((double)j / (divideByCount)) * frequency))) + min) * (max - min) * (0.5);
            }
        }




        //divide the curve
        //at each point, draw a ruling line based on the normal, binormal, or tangent
        //loft the lines



        //    i = Curve      j = point
        for (int i = 0; i < curves.Count; ++i)
        {





            //check for special cases
            bool closed = false;

            //initialize local variables
            //allPoints[i] = new Point3d[divideByCount + closedInt];
            //Curve[] rulingLines = new Curve[allPoints[i].Length];
            Curve[] rulingLines = new Curve[distances[i].Length];


            //divide the curve by count
            for (int j = 0; j < divideByCount; ++j)
            {

                double t = (curves[i].Domain.Length / divideByCount * j) + curves[i].Domain.Min;
                Point3d currentPoint = curves[i].PointAt(t);
                //allPoints[i][j] = currentPoint;


                //get the reference plane
                //this is the a convience function provided by rhino that
                //evaluates the curve at a point, and gives us a reference plane.
                //the axis of the plane correspond to normal, binormal, and tangent

                //the axis of the plane determines whether to use the tangent, normal, or binormal
                //plane.XAxis == Tangent
                //plane.YAxis == Normal
                //plane.ZAxis == Binormal

                Plane plane;
                if (useSurface)
                {







                    double u, v;
                    masterSurface.ClosestPoint(currentPoint, out u, out v);
                    Vector3d normal = masterSurface.NormalAt(u, v);
                    Vector3d binormal = Vector3d.CrossProduct(masterSurface.NormalAt(u, v), curves[i].TangentAt(t));

                    plane = new Plane(currentPoint, binormal, normal);












                }
                else if (usePerpendicularFrames)
                {
                    curves[i].PerpendicularFrameAt(t, out plane);
                }
                else
                {
                    curves[i].FrameAt(t, out plane);
                    plane = new Plane(plane.Origin, plane.ZAxis, plane.YAxis);
                }
                plane.Rotate((angles[i] * Math.PI / 180.0), plane.ZAxis);
                updatePlanes.Add(plane);

                //provides an option for variable width ruling lines
                //get curvature
                double cv = 1.0;






                //draw ruling lines
                Point3d[] pts = new Point3d[2];
                pts[0] = new Point3d(plane.Origin + (plane.XAxis * distances[i][j] * cv));
                pts[1] = new Point3d(plane.Origin + (plane.XAxis * distances[i][j] * cv * -1));
                rulingLines[j] = Curve.CreateControlPointCurve(pts, 1);

                if (j > 0 && !Curve.DoDirectionsMatch(rulingLines[j - 1], rulingLines[j]))
                {
                    rulingLines[j].Reverse();
                }
            }


            //loft
            Brep[] breps = Brep.CreateFromLoft(rulingLines, Point3d.Unset, Point3d.Unset, LoftType.Normal, closed);



            //check the loft to make sure they're all together
            for (int j = 1; j < breps.Length; ++j)
            {
                if (breps != null && breps.Length > 1)
                {
                    breps[0].Append(breps[j]);
                }
            }
            if (breps != null && breps.Length >= 1)
            {
                updateBreps.Add(breps[0]);
            }
        }


        A = updateBreps;
        #endregion



    }

    // <Custom additional code> 

    // </Custom additional code> 
}