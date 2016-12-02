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
public class geodesic : GH_ScriptInstance {
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
    private void RunScript(Curve curve0, Curve curve1, Surface surface, List<double> divisions, int offset, int flag, ref object A, ref object B) {
        //sbyte flag = 0;




        List<Curve> crvs0 = new List<Curve>();
        List<Curve> crvs1 = new List<Curve>();
        List<Line> lines0 = new List<Line>();
        List<Line> lines1 = new List<Line>();




        Point3d[] pts0 = new Point3d[divisions.Count];
        for(int i = 0; i < divisions.Count; i++) {
            pts0[i] = curve0.PointAtNormalizedLength(divisions[i]);
        }



        Point3d[] pts1 = new Point3d[divisions.Count];
        for(int i = 0; i < divisions.Count; i++) {
            pts1[i] = curve1.PointAtNormalizedLength(divisions[i]);
        }



        int min = Math.Min(pts0.Length, pts1.Length);
        for(int i = offset; i < min; i++) {


            if(flag == 0) {
                Line l;
                l = new Line(pts0[i], pts1[i - offset]);
                lines0.Add(l);
                l = new Line(pts1[i], pts0[i - offset]);
                lines1.Add(l);



            }
        }




        if(flag == 0) {
            A = lines0;
            B = lines1;
        }



    }

    // <Custom additional code> 
     void connectLines(Curve curve0, Curve curve1, Surface surface, List<double> divisions, int offset, ref object A, ref object B) {


        //make points
        Point3d[] pts0 = new Point3d[divisions.Count];
        for(int i = 0; i < divisions.Count; i++) {
            pts0[i] = curve0.PointAtNormalizedLength(divisions[i]);
        }

        Point3d[] pts1 = new Point3d[divisions.Count];
        for(int i = 0; i < divisions.Count; i++) {
            pts1[i] = curve1.PointAtNormalizedLength(divisions[i]);
        }





        //draw lines
        int min = Math.Min(pts0.Length, pts1.Length);
        Line[] lines0 = new Line[min-offset];
        Line[] lines1 = new Line[min-offset];
        for(int i = offset; i < min; i++) {
            lines0[i-offset] = new Line(pts0[i], pts1[i - offset]);
            lines1[i-offset] =  new Line(pts1[i], pts0[i - offset]);
        }


        A = lines0;
        B = lines1;




    }

    // </Custom additional code> 
}