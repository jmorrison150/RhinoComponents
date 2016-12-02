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
    private void RunScript(Curve curves, List<Surface> roof, Surface ground, ref object outCount, ref object outBoxes, ref object outPoints)
    {










        #region beginScript



        //set room size
        double roomX = 6.400;
        double roomY = 10.000;
        double roomZ = 3.500;







        //point on ground
        Point3d curveStart = curves.PointAtStart;
        Point3d pointOnGround;
        LineCurve line = new LineCurve(new Point3d(curveStart.X, curveStart.Y, -10000.0), new Point3d(curveStart.X, curveStart.Y, 10000.0));
        Rhino.Geometry.Intersect.CurveIntersections pt2 = Rhino.Geometry.Intersect.Intersection.CurveSurface(line, ground, 0.001, 0.001);
        pointOnGround = pt2[0].PointA;
        //pointOnGround.Z -= roomZ;


        //arrayCrv
        Point3d[] pts;
        double[] ts = curves.DivideByLength(roomX, true, out pts);


        List<Box> boxes = new List<Box>();
        //make rooms
        for (int i = 1; i < pts.Length; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                Point3d origin = new Point3d(pts[i].X, pts[i].Y, (roomZ * j) + pointOnGround.Z);
                Vector3d vectorX = pts[i - 1] - pts[i];
                Vector3d vectorY = Vector3d.CrossProduct(Vector3d.ZAxis, vectorX);
                Plane basePlane = new Plane(origin, vectorX, vectorY);
                Box box = new Box(basePlane, new Interval(0, roomX), new Interval(roomY * -0.5, roomY * 0.5), new Interval(0, roomZ));
                LineCurve line1 = new LineCurve(new Point3d(box.Center.X, box.Center.Y, (box.Center.Z + 1.800)), new Point3d(box.Center.X, box.Center.Y, (box.Center.Z + 1000000.0)));


                double maxZ = 0.0;




                    for (int l = 0; l < roof.Count; l++)
                    {

                        Rhino.Geometry.Intersect.CurveIntersections intersectionPoints = Rhino.Geometry.Intersect.Intersection.CurveSurface(line1, roof[l], 0.001, 0.001);
                        for (int m = 0; m < intersectionPoints.Count; m++)
                        {

                            if (intersectionPoints[m].IsPoint)
                            {
                                if (intersectionPoints[m].PointA.Z>maxZ)
                                {
                                    maxZ = intersectionPoints[m].PointA.Z;
                                }

                            }
                        }
                    }

                    if (box.Center.Z+ 1.800<maxZ)
                    {
                        boxes.Add(box);
                    }





            }

        }



        //center point
        Point3d[] centerPoints = new Point3d[boxes.Count];
        for (int i = 0; i < centerPoints.Length; i++)
        {
            centerPoints[i] = new Point3d(boxes[i].Center);
        }



        outBoxes = boxes;
        outCount = boxes.Count;
        outPoints = centerPoints;
        #endregion












    }

    // <Custom additional code> 



    // </Custom additional code> 
}