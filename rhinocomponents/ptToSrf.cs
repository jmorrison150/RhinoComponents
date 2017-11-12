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
public class Script_Instance50 : GH_ScriptInstance
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
    private void RunScript(List<Point3d> points, ref object A)
    {


        #region beginScript

        //Point3d[] arrayName = points.ToArray();



        //Array.Sort(arrayName, delegate (Point3d pt1, Point3d pt2) { return pt1.Y.CompareTo(pt2.Y); });
        //Array.Sort(arrayName, delegate (Point3d pt1, Point3d pt2) { return pt1.X.CompareTo(pt2.X); });
        //Array.Sort(arrayName, delegate (Point3d pt1, Point3d pt2) { return pt1.Z.CompareTo(pt2.Z); });


        points.Sort(new PointXY());


        NurbsSurface ns = NurbsSurface.CreateFromPoints(points, 300, 200, 2, 2);



        A = ns;

#endregion



    }

    // <Custom additional code> 





    #region customCode


    public class PointXY : Comparer<Point3d>
    {
        // Compares by Length, Height, and Width.
        public override int Compare(Point3d pt1, Point3d pt2)
        {
            if (pt1.X.CompareTo(pt2.X) != 0)
            {
                return pt1.X.CompareTo(pt2.X);
            }
            else if (pt1.Y.CompareTo(pt2.Y) != 0)
            {
                return pt1.Y.CompareTo(pt2.Y);
            }
            //else if (pt1.Z.CompareTo(pt2.Z) != 0)
            //{
            //    return pt1.Z.CompareTo(pt2.Z);
            //}
            else
            {
                return 0;
            }
        }

    }

#endregion




    // </Custom additional code> 
}