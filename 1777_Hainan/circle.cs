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
    private void RunScript(List<Line> floorLines, Curve startLine, Curve bridgeCenterLine, double constant1, double constant2, ref object A, ref object B, ref object C)
    {

        #region beginScript

        double length = 0.600;
        //double constant1 = 20.0;
        double bulgeA = constant1;
        double bulgeB = constant2;


        //make start points
        Point3d[] pts = new Point3d[floorLines.Count];
        Point3d[] pts2 = new Point3d[floorLines.Count];
        for (int i = 0; i < pts.Length; i++)
        {
            pts[i] = startLine.PointAtLength(i * length);
        }





        //make start lines
        Vector3d startVector = bridgeCenterLine.PointAtEnd - bridgeCenterLine.PointAtStart;
        double t;
        bridgeCenterLine.ClosestPoint(startLine.PointAtStart, out t);
        Vector3d xyStartVector = bridgeCenterLine.TangentAt(t);
        xyStartVector.Z = 0.0;
        Line[] lines = new Line[floorLines.Count];
        for (int i = 0; i < lines.Length; i++)
        {
            //find elevations
            Plane plane = new Plane(pts[i], xyStartVector, Vector3d.ZAxis);
            double lineParameter;
            Rhino.Geometry.Intersect.Intersection.LinePlane(floorLines[i], plane, out lineParameter);
            pts2[i] = floorLines[i].PointAt(lineParameter);






            //start lines flat on XY
            //lines[i] = new Line(pts[i], xyStartVector * (lines.Length - i) / lines.Length * constant1);

            //start lines with Z
            lines[i] = new Line(pts[i], pts2[i]);
            lines[i].Length *= 0.25;
            lines[i].Flip();
        }



        List<Curve> blendCurves = new List<Curve>();
        for (int i = 0; i < floorLines.Count; i++)
        {

            Curve[] cvs = new Curve[3];
            cvs[0] = new LineCurve(floorLines[i]);
            cvs[2] = new LineCurve(lines[i]);
            cvs[1] = Curve.CreateBlendCurve(cvs[0], cvs[2], BlendContinuity.Curvature, bulgeA, bulgeB);

            Curve[] join = Curve.JoinCurves(cvs);
            if (join.Length > 0)
            {
                join[0].Rebuild(16, 3, true);
                //blendCurves.Add(cvs[1]);
                blendCurves.Add(join[0]);
            }

        }


        Brep[] loft = Brep.CreateFromLoft(blendCurves, blendCurves[0].PointAtStart, blendCurves[blendCurves.Count-1].PointAtStart, LoftType.Normal, false);




        A = blendCurves;
        //B = loft;


        #endregion




    }

    // <Custom additional code> 

    // </Custom additional code> 
}