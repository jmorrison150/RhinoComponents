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
public class Script_Instance4 : GH_ScriptInstance {
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
    private void RunScript(List<Curve> curves, double offset, Brep boundary, double angle, int flag, ref object A, ref object B) {



        double degree = angle;

        List<Brep> breps0 = new List<Brep>();
        List<Brep> breps1 = new List<Brep>();

        Surface[] srfs0 = new Surface[curves.Count];
        Surface[] srfs1 = new Surface[curves.Count];

        for(int i = 0; i < curves.Count; i++) {
            Line l = new Line(curves[i].PointAtStart, curves[i].PointAtEnd);
            if(isVertical(l)) {
                string warning = string.Format("Cannot find the offset plane; curve {0} is Vertical", i);
                Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
                continue;
            }

            //Plane plane = new Plane(curves[i].PointAtStart, curves[i].PointAtEnd, curves[i].PointAtStart+Vector3d.ZAxis);
            Transform rotate0 = Transform.Rotation(rad(degree), curves[i].PointAtEnd - curves[i].PointAtStart, curves[i].PointAtStart);
            Transform rotate1 = Transform.Rotation(rad(-degree), curves[i].PointAtEnd - curves[i].PointAtStart, curves[i].PointAtStart);
            Vector3d extrudeDirection0 = Vector3d.ZAxis * 1.000;
            Vector3d extrudeDirection1 = Vector3d.ZAxis * 1.000;
            extrudeDirection0.Transform(rotate0);
            extrudeDirection1.Transform(rotate1);
            Surface s0 = Extrusion.CreateExtrusion(curves[i], extrudeDirection0);
            Surface s1 = Extrusion.CreateExtrusion(curves[i], extrudeDirection1);
            srfs0[i] = s0;
            srfs1[i] = s1;

            Brep b0 = s0.ToBrep();
            Brep b1 = s1.ToBrep();
            Brep[] trims0 = b0.Trim(boundary, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            Brep[] trims1 = b1.Trim(boundary, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            breps0.AddRange(trims0);
            breps1.AddRange(trims1);
        }

        A = breps0;
        B = breps1;





    }

    // <Custom additional code> 
    #region customCode

    double rad(double degree) {
        return degree * 0.0174532925;
    }
    bool isVertical(Line l) {
        Vector3d dir = l.Direction;
        dir.Unitize();
        double dot = dir * Vector3d.ZAxis;
        if(Math.Abs(dot) >= 1.0) {
            return true;

        } else { return false; }

    }
    List<Brep> extrude(List<Line> lines, Brep fromMaya, Point3d ceiling, double offset) {

        Plane cutter = new Plane(ceiling, Vector3d.ZAxis);
        List<Brep> breps = new List<Brep>();
        Transform offsetZ = Transform.Translation(Vector3d.ZAxis * offset);

        for(int i = 0; i < lines.Count; i++) {
            Point3d pt0 = lines[i].From;
            Point3d pt1 = lines[i].To;


            LineCurve l = new LineCurve(lines[i]);
            Transform moveZ = Transform.Translation(Vector3d.ZAxis * -1000);
            l.Transform(moveZ);
            //Surface surfaceExtrusion = Surface.CreateExtrusion(l, Vector3d.ZAxis* 2000);
            Surface extrusion = Extrusion.CreateExtrusion(l, Vector3d.ZAxis * 2000);
            Curve[] intCrvs;

            Point3d[] intPts;
            Rhino.Geometry.Intersect.Intersection.BrepSurface(fromMaya, extrusion, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intCrvs, out intPts);




            for(int j = 0; j < intCrvs.Length; j++) {
                intCrvs[j].Transform(offsetZ);
                Brep b = Extrusion.CreateExtrusion(intCrvs[j], Vector3d.ZAxis * 10).ToBrep();
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