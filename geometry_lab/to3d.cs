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
public class Script_Instance27 : GH_ScriptInstance {
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
    private void RunScript(DataTree<Point3d> points, DataTree<Curve> curves, DataTree<Transform> reverseForms, ref object outCurves, ref object outPoints) {

        #region beginScript
        if (curves.DataCount < 1) { return; }
        double _unrollWidth = 0;
        double _unrollHeight = 0;

        //format from data tree
        Transform[][] reverseTransforms = new Transform[reverseForms.BranchCount][];
        for (int i = 0; i < reverseTransforms.Length; i++) {
            reverseTransforms[i] = new Transform[reverseForms.Branches[i].Count];
            for (int j = 0; j < reverseTransforms[i].Length; j++) {
                reverseTransforms[i][j] = reverseForms.Branches[i][j];
            }
        }

        //Transform[0][0] == overlap
        if (reverseTransforms.Length > 0) {
            _unrollWidth = reverseTransforms[0][0].M03;
            _unrollHeight = reverseTransforms[0][0].M13;
        }


        //work on curves
        for (int i = 0; i < curves.BranchCount; i++) {
            for (int j = 0; j < curves.Branches[i].Count; j++) {
                int index = 0;

                //find the index that matches this location
                Point3d pt = curves.Branches[i][j].PointAtStart;
                index = (int)Math.Floor((-pt.X - (_unrollWidth * 0.5)) / _unrollWidth);
                if (index < 0) { index = 0; }
                if (index > reverseForms.BranchCount - 1) { index = reverseForms.BranchCount - 1; }

                //move to 3d
                for (int k = 0; k < reverseTransforms[index].Length; k++) {
                    curves.Branches[i][j].Transform(reverseTransforms[index][k]);
                }
            }
        }



        //work on points
        List<Point3d> updatePoints = new List<Point3d>();
        for (int i = 0; i < points.BranchCount; i++) {
            for (int j = 0; j < points.Branches[i].Count; j++) {

                //find the index that matches this location
                Point3d pt = points.Branches[i][j];
                int index = (int)Math.Floor((-pt.X - (_unrollWidth * 0.5)) / _unrollWidth);
                if (index < 0) { index = 0; }
                if (index > reverseForms.BranchCount - 1) { index = reverseForms.BranchCount - 1; }


                //move to 3d
                for (int k = 0; k < reverseTransforms[index].Length; k++) {
                    pt.Transform(reverseTransforms[index][k]);
                }
                updatePoints.Add(pt);
            }
        }



        //output to grasshopper
        outPoints = updatePoints;
        outCurves = curves;
        #endregion





    }

    // <Custom additional code> 

    // </Custom additional code> 
}