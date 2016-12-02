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
public class Script_Instance32 : GH_ScriptInstance {
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
    private void RunScript(List<Point3d> pts, ref object A) {














        #region beginScript
 

        List<Point3d> outPts = new List<Point3d>();

        for(int i = 1; i < pts.Count - 1; i++) {

            Line line0 = new Line(pts[i - 1], pts[i]);
            Line line1 = new Line(pts[i], pts[i + 1]);

            double angle = Vector3d.VectorAngle(line0.Direction, line1.Direction);
            double edgeLength = line0.Length;


            Point3d origin = line1.From;
            Vector3d zAxis = Vector3d.CrossProduct(line0.Direction, line1.Direction);
            Vector3d yAxis = ( ( line0.From + line1.To ) / 2 ) - line1.From;
            Plane bisector = new Plane(origin, yAxis, zAxis);


            bisector.Translate(bisector.Normal * edgeLength * 0.5);

            Point3d midPt = line1.PointAt(0.5);
            Plane midPlane = new Plane(midPt, line1.Direction);
            Circle cir = new Circle(midPlane, line1.Length);



            Point3d topPt = midPt;
            Rhino.Geometry.Intersect.CurveIntersections ints = Rhino.Geometry.Intersect.Intersection.CurvePlane(cir.ToNurbsCurve(), bisector, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            for(int j = 0; j < ints.Count; j++) {
                if(ints[j].IsPoint) {
                    if(ints[j].PointA.Z>topPt.Z) {
                    topPt = ints[j].PointA;
                        
                    }
                }
            }
            

            outPts.Add(topPt);

        }

        for(int i = 1; i < outPts.Count; i++) {

            Print(outPts[i - 1].DistanceTo(outPts[i]).ToString());
        }

        A = outPts;


        #endregion










    }

    // <Custom additional code> 

    // </Custom additional code> 
}