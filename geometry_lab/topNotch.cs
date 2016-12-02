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
public class Script_Instance30 : GH_ScriptInstance {
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
    private void RunScript(List<Surface> surfaces, List<Polyline> polylines, double thickness, ref object curves) {


        #region beginScript
        //make sure the lists are the same length
        Surface[] surfaces0 = new Surface[polylines.Count];
        for(int i = 0; i < surfaces0.Length; i++) {
            surfaces0[i] = surfaces[i % surfaces.Count];
        }

        //empty list
        //Curve[][] curves1 = new Curve[polyLines.Count][];
        List<Curve> updateCurves = new List<Curve>();




        //intersect with cylinders of obscene height
        for(int i = 0; i < polylines.Count; i++) {
            for(int j = 0; j < polylines[i].Count; j++) {

                Circle baseCircle = new Circle(polylines[i][j], thickness * 0.5);
                double height = surfaces0[i].GetBoundingBox(false).Diagonal.Length;
                Rhino.Geometry.Extrusion ext = Rhino.Geometry.Extrusion.CreateCylinderExtrusion(new Cylinder(baseCircle, height), true, true);

                Brep b = new Cylinder(baseCircle, height).ToBrep(true, true);
                //Surface cylinder = new Cylinder(baseCircle).ToRevSurface();

                Curve[] intersectionCurves;
                Point3d[] intersectionPoints;
                Rhino.Geometry.Intersect.Intersection.BrepSurface(b, surfaces0[i], RhinoDocument.ModelAbsoluteTolerance, out intersectionCurves, out intersectionPoints);

                updateCurves.AddRange(intersectionCurves);
            }
        }


        curves = updateCurves;
        #endregion
    }

    // <Custom additional code> 

    // </Custom additional code> 
}