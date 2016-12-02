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
public class Script_Instance1 : GH_ScriptInstance {
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
    private void RunScript(Point3d pt, Curve circle, Curve template, ref object A, ref object B, ref object C) {





        #region beginScript
        
        //sbyte reverse = -1;
        sbyte reverse = 1;

        if(reverse<0) { template.Reverse(); }
        Point3d tangentPoint;
        double min = circle.Domain.Min;
        double max = circle.Domain.Max;

        getTangent(pt, circle, min, max, out min, out max, out tangentPoint);
        getTangent(pt, circle, min, max, out min, out max, out tangentPoint);
        getTangent(pt, circle, min, max, out min, out max, out tangentPoint);
        getTangent(pt, circle, min, max, out min, out max, out tangentPoint);
        getTangent(pt, circle, min, max, out min, out max, out tangentPoint);


        Plane tangentPlane = new Plane(pt, tangentPoint-pt, Vector3d.CrossProduct(tangentPoint-pt,Vector3d.ZAxis*reverse) );
        Plane templatePlane = new Plane(template.PointAtStart, template.PointAtEnd - template.PointAtStart, -template.TangentAtStart);

        Curve template1 = template.DuplicateCurve();
        Transform xform = Transform.PlaneToPlane(templatePlane, tangentPlane);
        template1.Transform(xform);

        double xScaleFactor = ( ( tangentPoint - pt ).Length ) / ( ( template.PointAtEnd - template.PointAtStart ).Length );
        Transform scale1D = Transform.Scale(tangentPlane, xScaleFactor, 1.0, 1.0);
        template1.Transform(scale1D);



        NurbsCurve nc = template1.ToNurbsCurve();
        Point3d pt2 = nc.Points[nc.Points.Count - 2].Location;
        Point3d tangentPoint2;
        getTangent(pt2, circle, circle.Domain.Min, circle.Domain.Max, out min, out max, out tangentPoint2);
        getTangent(pt2, circle, min, max, out min, out max, out tangentPoint2);
        getTangent(pt2, circle, min, max, out min, out max, out tangentPoint2);
        getTangent(pt2, circle, min, max, out min, out max, out tangentPoint2);
        getTangent(pt2, circle, min, max, out min, out max, out tangentPoint2);

        ControlPoint cp = new ControlPoint(tangentPoint2);
        nc.Points[nc.Points.Count - 1] = cp;

        //    A = tangentPlane;
        //  B = templatePlane;
        C = nc;

        #endregion



    }

    // <Custom additional code> 


    #region customCode
    void getTangent(Point3d testPoint, Curve c, double min, double max, out double outMin, out double outMax, out Point3d tangentPoint) {

        double closestDist = double.MaxValue;
        double step = ( max - min ) / 10.0;
        outMin = min;
        outMax = max;
        tangentPoint = c.PointAt(min);

        for(double i = min; i < max; i += step) {
            Point3d point2 = c.PointAt(i);
            Vector3d vec = c.TangentAt(i);
            Line l = new Line(point2, vec, 1000);
            LineCurve lc = new LineCurve(l);
            double lineCurveT;
            lc.ClosestPoint(testPoint, out lineCurveT, l.Length);
            Point3d lineCurvePoint = lc.PointAt(lineCurveT);
            double d = testPoint.DistanceTo(lineCurvePoint);
            if(d < closestDist) {
                closestDist = d;
                if(i > min) { outMin = i - step; }
                outMax = i + step;
                tangentPoint = point2;
            }
        }
    }
    double map(double value1, double min1, double max1, double min2, double max2) {
        double value2 = min2 + ( value1 - min1 ) * ( max2 - min2 ) / ( max1 - min1 );
        return value2;
    }
    #endregion




    // </Custom additional code> 
}