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
public class Script_Instance62 : GH_ScriptInstance {
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
    private void RunScript(DataTree<Curve> x, DataTree<Color> y, ref object A) {

        #region beginScript
        pnts = new List<Point3d>();
        crvs = new List<Curve>();
        breps = new List<Brep>();
        colors = new List<Color>();

        for (int i = 0; i < x.BranchCount; i++) {
            for (int j = 0; j < x.Branch(i).Count; j++) {
                crvs.Add(x.Branch(i)[j]);
                colors.Add(y.Branch(i)[j]);


            }
        }


        #endregion




    }

    // <Custom additional code> 





    #region customCode
    List<Point3d> pnts;
    List<Curve> crvs;
    List<Brep> breps;
    List<Color> colors;
    //string NAME;
    //Point3d LOCATION;



    int THICKNESS = 2;
    //Color COL;

    //Draw all wires and points in this method.
    public override void DrawViewportWires(IGH_PreviewArgs args) {
        //foreach (Point3d p in pnts)
        //    args.Display.DrawPoint(p, Rhino.Display.PointStyle.ControlPoint, THICKNESS, COL);

        for (int i = 0; i < crvs.Count; i++) {
            args.Display.DrawCurve(crvs[i], colors[i], THICKNESS);
        }

        //foreach (PolyCurve c in crvs)
        //    args.Display.DrawCurve(c, COL, THICKNESS);

        //foreach (Brep b in breps)
        //    args.Display.DrawBrepShaded(b, new Rhino.Display.DisplayMaterial(COL));

        //args.Display.DrawPoint(LOCATION, Rhino.Display.PointStyle.ActivePoint, 3, Color.Black);
        //args.Display.Draw3dText(NAME, Color.Gray, new Plane(LOCATION, Vector3d.ZAxis), THICKNESS / 3, "Arial");
    }
    #endregion





    // </Custom additional code> 
}