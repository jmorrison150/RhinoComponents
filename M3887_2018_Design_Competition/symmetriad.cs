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
    private void RunScript(Point3d input, ref object A, ref object B) {

        #region runScript
 


        int objectNumber = 20000;

        double value00 = -1.4;
        double value01 = -value00;
        double value02 = -value00;
        double value10 = 0.7;
        double value11 = -value10;
        double value12 = value10;

        value00 = input.X;
        value10 = input.Y;

        int n;

        double bigX;
        double bigY;
        double bigZ;

        for (int i = 0; i < xyz.Length; i++) {
            n = (i + 1) % objectNumber;
            bigX = (Math.Sin(value00 * xyz[i].Y)) + (value10 * (Math.Cos(value00 * xyz[i].X)));
            bigY = (Math.Sin(value01 * xyz[i].X)) + (value11 * (Math.Cos(value01 * xyz[i].Y)));
            bigZ = (Math.Sin(value02 * xyz[i].Y)) + (value12 * (Math.Cos(value02 * xyz[i].Y)));

            xyz[n].X = bigX;
            xyz[n].Y = bigY;
            xyz[n].Z = bigZ;

        }

        PointCloud cloud = new PointCloud(xyz);
        Brep b = cloud.GetBoundingBox(false).ToBrep();
        
        Transform site = siteTransform();
        b.Transform(site);

        for (int i = 0; i < xyz.Length; i++) {
            xyz[i].Transform(site);
        }


        A = xyz;
        B = b;
        #endregion

    }

    // <Custom additional code> 




    #region customCode
    Point3d[] xyz = new Point3d[20000];
    Transform bbTransform(Box initial, Box final) {
        Vector3d initialBasisX, initialBasisY, initialBasisZ, finalBasisX, finalBasisY, finalBasisZ;
        initialBasisX = new Vector3d(initial.PointAt(0, 0, 0) + initial.PointAt(1, 0, 0));
        initialBasisY = new Vector3d(initial.PointAt(0, 0, 0) + initial.PointAt(0, 1, 0));
        initialBasisZ = new Vector3d(initial.PointAt(0, 0, 0) + initial.PointAt(0, 0, 1));
        finalBasisX = new Vector3d(final.PointAt(0, 0, 0) + final.PointAt(1, 0, 0));
        finalBasisY = new Vector3d(final.PointAt(0, 0, 0) + final.PointAt(0, 1, 0));
        finalBasisZ = new Vector3d(final.PointAt(0, 0, 0) + final.PointAt(0, 0, 1));
        return Transform.ChangeBasis(initialBasisX, initialBasisY, initialBasisZ, finalBasisX, finalBasisY, finalBasisZ);
    }

    Transform siteTransform() {
        Transform xform = Transform.Identity;
        double overallScale = 526;
        double rotateXY = 0;
        double rotateYZ = 188;
        double rotateZX = 42;
        double scaleNUX = 1.0;
        double scaleNUY = 1.0;
        double scaleNUZ = 0.221;
        double moveX = 2014;
        double moveY = 1328;
        double moveZ = 179;

        Transform scale = Transform.Scale(Point3d.Origin, overallScale);
        Transform XY = Transform.Rotation(rad(rotateXY), Vector3d.ZAxis,Point3d.Origin);
        Transform YZ = Transform.Rotation(rad(rotateYZ), Plane.WorldYZ.Normal, Plane.WorldYZ.Origin);
        Transform ZX = Transform.Rotation(rad(rotateZX), Plane.WorldZX.Normal, Plane.WorldZX.Origin);
        Transform scaleNU = Transform.Scale(Plane.WorldXY, scaleNUX, scaleNUY, scaleNUZ);
        Transform move = Transform.Translation(moveX, moveY, moveZ);

        xform *= move;
        xform *= scaleNU;
        xform *= ZX;
        xform *= YZ;
        xform *= XY;
        xform *= scale;

        return xform;
    }

    double rad(double degree) {

        return degree * Math.PI / 180.0;
    }
#endregion




    // </Custom additional code> 
}