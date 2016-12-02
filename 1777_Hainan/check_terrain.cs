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
    private void RunScript(Rectangle3d rect, Surface surface, Mesh mesh, ref object A)
    {



        //make points
        int countU = (int)(rect.Width / 50.0);
        int countV = (int)(rect.Height / 100.0);
        double spacingU = rect.Width / ((double)countU);
        double spacingV = rect.Height / ((double)countV);
        Point3d startPoint = rect.BoundingBox.Min;
        Point3d[] pts = new Point3d[(countU + 1) * (countV + 1)];

        for (int v = 0; v <= countV; v++)
        {
            for (int u = 0; u <= countU; u++)
            {

                Point3d pt = new Point3d(startPoint.X + (spacingU * u), startPoint.Y + spacingV * v, 0.0);
                pts[(v * (countU + 1)) + u] = pt;


            }
        }
        A = pts;

        //NurbsSurface ns = NurbsSurface.CreateThroughPoints(pts, (countU + 1), (countV + 1), 3, 3, false, false);


        //check Z height
        ///////////////////////////////////////////////////////////





        for (int i = 0; i < pts.Length; i++)
        {


            Rhino.Geometry.Line line = new Line(new Point3d(pts[i].X, pts[i].Y, -1000.0), new Point3d(pts[i].X, pts[i].Y, 1000.0));

            int[] faceIds;
            Point3d[] meshPts = Rhino.Geometry.Intersect.Intersection.MeshLine(mesh, line, out faceIds);


            if (meshPts.Length > 0)
            {
                pts[i] = meshPts[0];
            }
            else
            {


                LineCurve lineCurve = new LineCurve(new Point3d(pts[i].X, pts[i].Y, -1000.0), new Point3d(pts[i].X, pts[i].Y, 1000.0));
                Rhino.Geometry.Intersect.CurveIntersections pt2 = Rhino.Geometry.Intersect.Intersection.CurveSurface(lineCurve, surface, 0.001, 0.001);

                if (pt2[0].IsPoint)
                {
                    pts[i] = pt2[0].PointA;
                }

            }


        }


        //NurbsSurface ns = NurbsSurface.CreateThroughPoints(pts, countU, countV, 3, 3, false, false);
        NurbsSurface ns = NurbsSurface.CreateThroughPoints(pts, (countU + 1), (countV + 1), 3, 3, false, false);
        A = ns;


    }

    // <Custom additional code> 

    // </Custom additional code> 
}