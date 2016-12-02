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
public class Script_Instance24 : GH_ScriptInstance {
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
    private void RunScript(DataTree<Point3d> center_2D, DataTree<Point3d> points_2D, double angle, DataTree<Transform> transforms, ref object angles) {









        #region beginScript

        if ((center_2D.BranchCount != points_2D.BranchCount) || (center_2D.BranchCount != transforms.BranchCount)) { Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "number of points must match number of transformation matrices"); return; }

        //format from data tree
        Transform[][] xforms = new Transform[transforms.BranchCount][];
        for (int i = 0; i < xforms.Length; i++) {
            xforms[i] = new Transform[transforms.Branches[i].Count];
            for (int j = 0; j < xforms[i].Length; j++) {
                xforms[i][j] = transforms.Branches[i][j];
            }
        }

        //format points from data tree
        Point3d[][] pts = new Point3d[points_2D.BranchCount][];
        for (int i = 0; i < pts.Length; i++) {
            pts[i] = new Point3d[points_2D.Branches[i].Count];
            for (int j = 0; j < pts[i].Length; j++) {
                pts[i][j] = points_2D.Branches[i][j];
            }
        }

        //format points from data tree
        Point3d[][] centers = new Point3d[center_2D.BranchCount][];
        for (int i = 0; i < centers.Length; i++) {
            centers[i] = new Point3d[center_2D.Branches[i].Count];
            for (int j = 0; j < centers[i].Length; j++) {
                centers[i][j] = center_2D.Branches[i][j];
            }
        }



        //make empty list
        double[][] updateAngles = new double[pts.Length][];
        for (int i = 0; i < updateAngles.Length; i++) {
            updateAngles[i] = new double[pts[i].Length];
        }

        //compute angle
        for (int i = 0; i < pts.Length; i++) {
            for (int j = 0; j < pts[i].Length; j++) {
                Vector3d toCenter = centers[i][0] - pts[i][j];

                double _angle;
                _angle = Vector3d.VectorAngle(toCenter, Vector3d.YAxis, Plane.WorldXY);
                //_angle = Vector3d.VectorAngle(toCenter, Vector3d.XAxis);


                updateAngles[i][j] = (_angle / Math.PI * 180.0) + angle;



                //        for (int k = 0; k < xforms[i].Length; k++) {
                //          toCenter.Transform(xforms[i][k]);
                //        }



            }
        }


        //format points back to data tree
        DataTree<double> angles1 = new DataTree<double>();
        for (int m = 0; m < updateAngles.Length; ++m) {
            Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
            for (int n = 0; n < updateAngles[m].Length; ++n) {
                angles1.Insert(updateAngles[m][n], path, n);
            }
        }

        angles = angles1;

        #endregion










    }

    // <Custom additional code> 

    // </Custom additional code> 
}