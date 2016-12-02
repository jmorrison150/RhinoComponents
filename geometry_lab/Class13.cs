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
public class Script_Instance5 : GH_ScriptInstance {
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
    private void RunScript(DataTree<Point3d> points, Curve curve, double phase, ref object angles, ref object A, ref object B) {



        #region beginScript
        DataTree<double> _angles = new DataTree<double>();
        List<Point3d> vecLocations = new List<Point3d>();
        List<Vector3d> vecs = new List<Vector3d>();


        for (int i = 0; i < points.BranchCount; i++) {
            Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(i);
            for (int j = 0; j < points.Branches[i].Count; j++) {
                Point3d pt = points.Branches[i][j];
                double t;
                curve.ClosestPoint(pt, out t);
                Point3d closestPoint = curve.PointAt(t);
                closestPoint.Z = 0;
                pt.Z = 0;



                Vector3d vec = closestPoint - points.Branches[i][j];
                vecs.Add(vec);
                vecLocations.Add(points.Branches[i][j]);


                //double _angle;
                //_angle = Vector3d.VectorAngle(vec, Vector3d.XAxis, Plane.WorldXY);


                //_angle = (_angle / Math.PI * 180.0) + phase;
                //_angles.Insert(_angle, path, j);

            }
        }
        angles = _angles;
        #endregion




        A = vecLocations;
        B = vecs;


    }

    // <Custom additional code> 

    // </Custom additional code> 
}