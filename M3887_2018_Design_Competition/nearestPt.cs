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
    private void RunScript(List<Point3d> pts, ref object A) {










        #region beginScript
        //Line[] lines = new Line[pts.Count];
        List<Line> updateLines = new List<Line>();

        //PointCloud cloud = new PointCloud(pts);


        for (int i = 0; i < pts.Count; i++) {



            //int index = cloud.ClosestPoint(pts[i]);
            //Point3d pt = cloud[index].Location;


            double dist0 = double.MaxValue;
            double dist1 = double.MaxValue;
            double dist2 = double.MaxValue;

            int index0 = -1;
            int index1 = -1;
            int index2 = -1;

            for (int j = 0; j < pts.Count; j++) {
                if (i == j) continue;
                double d = pts[i].DistanceToSquared(pts[j]);


                if (d < dist0) {
                    if (dist0 < dist1) {
                        if (dist1 < dist2) {
                            dist2 = dist1; index2 = index1;
                        }
                        dist1 = dist0; index1 = index0;
                    }
                    dist0 = d; index0 = j; continue;


                } else if (d < dist1) {
                    if (dist1 < dist2) {
                        dist2 = dist1; index2 = index1;
                    }
                    dist1 = d; index1 = j; continue;
                } else if (d < dist2) {
                    dist2 = d; index2 = j; continue;
                }

            }

            if (index0>=0) {

            Line l = new Line(pts[i], pts[index0]);
                updateLines.Add(l);
            }
            if (index1 >= 0) {

                Line l = new Line(pts[i], pts[index1]);
                updateLines.Add(l);
            }
            if (index2 >= 0) {

                Line l = new Line(pts[i], pts[index2]);
                updateLines.Add(l);
            }




        }


        A = updateLines;

        #endregion










    }

    // <Custom additional code> 

    // </Custom additional code> 
}