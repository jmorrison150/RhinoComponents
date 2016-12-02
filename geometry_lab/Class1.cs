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
public class Script_Instance2 : GH_ScriptInstance {
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
    private void RunScript(List<Polyline> polylines, double spacing, bool showGrid, int group, ref object A) {




        #region beginScript
        double space = 100;
        space = spacing;


        List<Polyline> updatePolylines = new List<Polyline>();


        for(int i = 0; i < polylines.Count; i++) {



            Point3d pt0 = polylines[i][0];
            pt0.Z = 0.0;
            Point3d pt1 = polylines[i].Find((pt) => { return ( pt.X != polylines[i][0].X ) || ( pt.Y != polylines[i][0].Y ); });
            pt1.Z = pt0.Z;
            Point3d pt2 = pt0 + Vector3d.ZAxis;

            Plane plane0 = new Plane(pt0, pt1, pt2);
            Point3d origin = new Point3d(( -space * ( polylines.Count - 1 ) ) + ( space * i ), 0, 0);
            Plane plane1 = new Plane(origin, Vector3d.XAxis, Vector3d.YAxis);
            Transform xform0 = Transform.PlaneToPlane(plane0, plane1);
            polylines[i].Transform(xform0);
            updatePolylines.Add(polylines[i]);


            Rectangle3d rt = new Rectangle3d(plane1, -space, space);
            updatePolylines.Add(rt.ToPolyline());
            //Plane plane;
            //surface.TryGetPlane(out plane);
            //Transform xform = Transform.PlaneToPlane(plane, Plane.WorldXY);
            //surface.Transform(xform);
            ////A = surface;


        }


        A = updatePolylines;
        #endregion





    }

    // <Custom additional code> 

    // </Custom additional code> 
}