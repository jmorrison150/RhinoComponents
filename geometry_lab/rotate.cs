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
    private void RunScript(Point3d basePt, double angle, double edge, int quantity, ref object A, ref object B) {



        #region beginScript
        Point3d p1 = basePt;
        Point3d p2;
        Double ang = ( Math.PI / 180 ) * angle;
        int n = quantity;


        double low1 = 0.0;
        double high1 = 90.0*0.0174532925;
        double low2 = 0.0;
        double high2 = 104.478*0.0174532925;

        Vector3d vec = new Vector3d(edge, 0, 0);
        List<Point3d> results = new List<Point3d>();
        List<Polyline> triangles = new List<Polyline>();

        for(int i = 0; i < n; i++) {
            p2 = p1 + vec;
            Transform rot = Transform.Rotation(ang, p1);
            p2.Transform(rot);
            results.Add(p2);
            vec = p2 - p1;
            p1 = p2;
        }

        for(int i = 0; i < results.Count - 1; i++) {
            Point3d a = results[i];
            Point3d b = results[i + 1];
            Vector3d ba = b - a;
            Point3d c = ( ( a + b ) / 2 ) + ( ( Math.Sqrt(3) ) / 2 ) * Vector3d.ZAxis * ( ba.Length );
            Point3d[] pts = new Point3d[4];
            pts[0] = a;
            pts[1] = b;
            pts[2] = c;
            pts[3] = a;
            Polyline pl = new Polyline(pts);


            Double rotAngle = computeAngle(( 180 - angle ), edge);
            double rotAngle2 = map(rotAngle, low1, high1, low2, high2);
            Transform rot = Transform.Rotation(rotAngle2, ba, ( a + b ) / 2);



            pl.Transform(rot);
            triangles.Add(pl);
        }

        A = results;
        B = triangles;
        #endregion


    }

    // <Custom additional code> 


    #region customCode
    public double map(double number, double low1, double high1, double low2, double high2) {
        return low2 + ( high2 - low2 ) * ( number - low1 ) / ( high1 - low1 );
    }
    double computeAngle(double angleDegree, double sideLength) {
        double angleRad = ( Math.PI / 180 ) * angleDegree;
        if(sideLength <= 0) { return 0; }


        double angleXZ = Math.Asin(( ( sideLength * 0.5 ) - ( 0.5 * sideLength * Math.Sin(angleRad * 0.5) ) ) / sideLength);
        return angleXZ;
    }
    #endregion



    // </Custom additional code> 
}