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
public class script1 : GH_ScriptInstance {
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
    private void RunScript(List<Line> lines, Brep fromMaya, Point3d ceiling, ref object A) {




        A = extrude(lines, fromMaya, ceiling);





    }

    // <Custom additional code> 


    #region customCode
    List<Brep> extrude(List<Line> lines, Brep fromMaya, Point3d ceiling) {

        Plane cutter = new Plane(ceiling, Vector3d.ZAxis);
        List<Brep> breps = new List<Brep>();

        for(int i = 0; i < lines.Count; i++) {
            Point3d pt0 = lines[i].From;
            Point3d pt1 =  lines[i].To;


            LineCurve l = new LineCurve(lines[i]);
            Transform moveZ = Transform.Translation(Vector3d.ZAxis*1000);
            l.Transform(moveZ);
            //Surface surfaceExtrusion = Surface.CreateExtrusion(l, Vector3d.ZAxis* 2000);
            Surface extrusion = Extrusion.CreateExtrusion(l, Vector3d.ZAxis * 2000);
            Curve[] intCrvs;
            Point3d[] intPts;
            Rhino.Geometry.Intersect.Intersection.BrepSurface(fromMaya, extrusion, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intCrvs, out intPts);

            for(int j = 0; j < intCrvs.Length; j++) {
                Brep b  =Extrusion.CreateExtrusion(intCrvs[j], Vector3d.ZAxis*10).ToBrep();
                Brep[] brs = b.Trim(cutter, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                breps.AddRange(brs);
            }


        }

        return breps;

    }

    double map(double value1, double min1, double max1, double min2, double max2) {
        double value2 = min2 + ( value1 - min1 ) * ( max2 - min2 ) / ( max1 - min1 );
        return value2;
    }
    #endregion




    // </Custom additional code> 
}