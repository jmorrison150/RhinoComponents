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
    private void RunScript(string txt, ref object A) {

        #region beginScript




        //Transform xform0 = RhinoDocument.EarthAnchorPoint.GetModelToEarthTransform(RhinoDocument.ModelUnitSystem);
        //   Transform latLng;
        //   xform0.TryGetInverse(out latLng);


        string[] stringRow = System.IO.File.ReadAllLines(txt);
        Point4d[] points = new Point4d[stringRow.Length];
        for (int i = 0; i < stringRow.Length; i++) {
            string[] xyz = stringRow[i].Split(' ');
            if (xyz.Length != 4) {
                continue;
            }
            double x = double.Parse(xyz[0], System.Globalization.NumberStyles.Float);
            double y = double.Parse(xyz[1], System.Globalization.NumberStyles.Float);
            double z = double.Parse(xyz[2], System.Globalization.NumberStyles.Float);
            double a = double.Parse(xyz[3], System.Globalization.NumberStyles.Float);


            Point4d pt = new Point4d(x, y, z, a);
            //pt.Transform(latLng);
            points[i] = pt;
        }



        //   points.Sort(delegate (Point3d pt1, Point3d pt2) { return pt2.X.CompareTo(pt1.X); });
        //    points.Sort(delegate (Point3d pt1, Point3d pt2) { return pt2.Y.CompareTo(pt1.Y); });
        //







        //convert point3d to node2
        //grasshopper requres that nodes are saved within a Node2List for Delaunay
        var nodes = new Grasshopper.Kernel.Geometry.Node2List();
        for (int i = 0; i < points.Length; i++) {
            //notice how we only read in the X and Y coordinates
            //  this is why points should be mapped onto the XY plane
            nodes.Append(new Grasshopper.Kernel.Geometry.Node2(points[i].X, points[i].Y));
        }

        //solve Delaunay
        Mesh delMesh = new Mesh();
        List<Grasshopper.Kernel.Geometry.Delaunay.Face> faces = new List<Grasshopper.Kernel.Geometry.Delaunay.Face>();
        faces = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Faces(nodes, 1);

        //output
        delMesh = Grasshopper.Kernel.Geometry.Delaunay.Solver.Solve_Mesh(nodes, 1, ref faces);

        for (int i = 0; i < delMesh.Vertices.Count; i++) {
            delMesh.Vertices[i] = new Point3f(delMesh.Vertices[i].X, delMesh.Vertices[i].Y, (float)points[i].Z);
        }

        delMesh.VertexColors.CreateMonotoneMesh(System.Drawing.Color.White);
        for (int i = 0; i < delMesh.VertexColors.Count; i++) {
            delMesh.VertexColors[i] = Color.FromArgb(255, (int)points[i].W, (int)points[i].W, (int)points[i].W);
        }



        //for (int i = 0; i < points.Count; i++) {
        //    Ray3d ray = new Ray3d(new Point3d(points[i].X, points[i].Y, double.MinValue),Vector3d.ZAxis);
        //    int[] index;
        //    Rhino.Geometry.Intersect.Intersection.MeshRay(delMesh, ray, out index);
        //    delMesh.Vertices[index[0]] = new Point3f((float)points[i].X, (float)points[i].Y, (float)points[i].Z);
        //}


        A = delMesh;
        #endregion



    }

    // <Custom additional code> 

    // </Custom additional code> 
}