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
    private void RunScript(Curve curve, object y, ref object outMesh) {


        #region beginScript
        Interval width = new Interval(-5.0, 5.0);
        Interval height = new Interval(-5.0, 5.0);

        NurbsCurve ns = curve.ToNurbsCurve();

        Point3dList pts = ns.GrevillePoints();
        Rectangle3d[] rects = new Rectangle3d[ns.Points.Count];
        for (int i = 0; i < ns.Points.Count; i++) {

            Plane plane;
            double t = ns.GrevilleParameter(i);
            //ns.FrameAt(t, out plane);
            ns.PerpendicularFrameAt(t, out plane);

            rects[i] = new Rectangle3d(plane,width, height);

        }

        List<Mesh> updateMeshes = new List<Mesh>();


        for (int i = 0; i < 4; i++) {
            List<LineCurve> lcrvs = new List<LineCurve>();

            for (int j = 0; j < 1; j++) {

                LineCurve l0 = new LineCurve(rects[j].Corner(i), rects[j].Corner((i + 1) % 4));
                lcrvs.Add(l0);
            }


            for (int j = 1; j < rects.Length; j++) {
                LineCurve side = new LineCurve(rects[j - 1].Corner(i), rects[j].Corner(i));
                lcrvs.Add(side);
                LineCurve l0 = new LineCurve(rects[j].Corner(i), rects[j].Corner((i + 1) % 4));
                lcrvs.Add(l0);

                LineCurve side1 = new LineCurve(rects[j - 1].Corner((i + 1) % 4), rects[j].Corner((i + 1) % 4));
                lcrvs.Add(side1);
            }
            Curve[] lines = lcrvs.ToArray();

            Mesh m = Mesh.CreateFromLines(lines, 4, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            updateMeshes.Add(m);
            // outMesh = lcrvs;
        }


        Mesh meshJoin = new Mesh();
        for (int i = 0; i < updateMeshes.Count; i++) {
            meshJoin.Append(updateMeshes[i]);
        }

        outMesh = meshJoin;


        #endregion




    }

    // <Custom additional code> 

    // </Custom additional code> 
}