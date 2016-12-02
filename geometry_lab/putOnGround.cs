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
public class Script_Instance21 : GH_ScriptInstance {
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
    private void RunScript(List<Surface> surfaces, DataTree<Point3d> points, double unrollWidth, double unrollHeight, ref object outSurfaces, ref object outPoints, ref object outTransforms, ref object reverseForms) {





        #region beginScript
        //make empty lists
        Grasshopper.DataTree<Point3d> updatePoints = new Grasshopper.DataTree<Point3d>();
        Grasshopper.DataTree<Transform> updateTransforms = new Grasshopper.DataTree<Transform>();
        Grasshopper.DataTree<Transform> updateReverseTransforms = new Grasshopper.DataTree<Transform>();

        Transform[] transforms = new Transform[surfaces.Count];
        Transform[] transforms1 = new Transform[surfaces.Count];
        Transform[][] transforms2 = new Transform[surfaces.Count][];
        Transform[][] transforms3 = new Transform[surfaces.Count][];

        for (int i = 0; i < transforms2.Length; i++) {
            transforms2[i] = new Transform[3];
            transforms3[i] = new Transform[3];
        }
        BoundingBox[] boxes = new BoundingBox[surfaces.Count];
        Plane[] planes = new Plane[surfaces.Count];
        Rhino.Geometry.Box[] boxes1 = new Rhino.Geometry.Box[surfaces.Count];

        //format points from data tree
        Point3d[][] pts = new Point3d[points.BranchCount][];
        for (int i = 0; i < pts.Length; i++) {
            pts[i] = new Point3d[points.Branches[i].Count];
            for (int j = 0; j < pts[i].Length; j++) {
                pts[i][j] = points.Branches[i][j];
            }
        }




        //work on each surface one at a time
        for (int i = 0; i < surfaces.Count; i++) {
            double _unrollWidth;
            double _unrollHeight;

            _unrollWidth = unrollWidth * (i + 1) * -1.0;
            _unrollHeight = unrollHeight * -1.0;


            //TryGetPlane might reverse the surface depending on curvature
            //surfaces[i].TryGetPlane(out planes[i]);

            //Transform xy = Transform.PlaneToPlane(planes[i], Plane.WorldXY);
            //Transform xy1 = Transform.PlaneToPlane(Plane.WorldXY, planes[i]);

            ////put the surface on the ground
            //Surface surf = surfaces[i].ToNurbsSurface();
            //surf.Transform(xy);
            //boxes[i] = surf.GetBoundingBox(false);




            Point3d pointAtStart = surfaces[i].PointAt(surfaces[i].Domain(0).Min, surfaces[i].Domain(1).Min);
            Point3d pointAtMid = surfaces[i].PointAt(surfaces[i].Domain(0).Mid, surfaces[i].Domain(1).Mid);
            Point3d pointAtEnd = surfaces[i].PointAt(surfaces[i].Domain(0).Max, surfaces[i].Domain(1).Max);
            pointAtStart.Z = 0;
            pointAtMid.Z = 0;
            pointAtEnd.Z = 0;
            Vector3d startVector = (pointAtStart - pointAtEnd);
            Vector3d endVector = -Vector3d.XAxis;



            Transform moveToOrigin = Transform.Translation(Point3d.Origin - pointAtMid);
            Transform moveToOrigin3 = Transform.Translation(pointAtMid - Point3d.Origin);
            //moveToOrigin3 = Transform.Identity;


            Transform faceFront = Transform.Rotation(startVector, endVector, pointAtStart);
            Transform faceFront3 = Transform.Rotation(endVector, startVector, pointAtStart);
            surfaces[i].Transform(moveToOrigin * faceFront);
            //faceFront3 = Transform.Identity;

            Transform rotate = Transform.Rotation(Vector3d.ZAxis, Vector3d.YAxis, Point3d.Origin);
            Transform rotate3 = Transform.Rotation(Vector3d.YAxis, Vector3d.ZAxis, Point3d.Origin);
            surfaces[i].Transform(rotate);
            //rotate3 = Transform.Identity;



            Transform overlap = Transform.Translation(_unrollWidth, _unrollHeight, 0);
            Transform overlap3 = Transform.Translation(-_unrollWidth, -_unrollHeight, 0);
            surfaces[i].Transform(overlap);
            //overlap3 = Transform.Identity;


            //move points
            for (int j = 0; j < pts[i].Length; j++) {

                //Print(pts[i][j].ToString());
                pts[i][j].Transform(moveToOrigin * faceFront);
                pts[i][j].Transform(rotate);
                pts[i][j].Transform(overlap);
                //Print(pts[i][j].ToString());
                //Print("---");

            }

            transforms2[i][0] = (moveToOrigin * faceFront);
            transforms2[i][1] = (rotate);
            transforms2[i][2] = (overlap);

            transforms3[i][2] = (faceFront3 * moveToOrigin3);
            transforms3[i][1] = (rotate3);
            transforms3[i][0] = (overlap3);

        }






        //format points back to data tree
        for (int m = 0; m < pts.Length; ++m) {
            Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
            for (int n = 0; n < pts[m].Length; ++n) {
                updatePoints.Insert(pts[m][n], path, n);
            }
        }


        //format back to data tree
        for (int m = 0; m < transforms2.Length; ++m) {
            Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
            for (int n = 0; n < transforms2[m].Length; ++n) {
                updateTransforms.Insert(transforms2[m][n], path, n);
            }
        }

        //format back to data tree
        for (int m = 0; m < transforms3.Length; ++m) {
            Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
            for (int n = 0; n < transforms3[m].Length; ++n) {
                updateReverseTransforms.Insert(transforms3[m][n], path, n);
            }
        }



        //output to Grasshopper
        outSurfaces = surfaces;
        outPoints = updatePoints;
        outTransforms = updateTransforms;
        reverseForms = updateReverseTransforms;

        #endregion











    }

    // <Custom additional code> 
    Transform addMatrix(Transform x, Transform y) {
        Transform transform = Transform.Identity;

        for (int i = 0; i < 4; i++) {
            for (int j = 0; j < 4; j++) {
                transform[i, j] = x[i, j] + y[i, j];
            }
        }


        return transform;

    }
    // </Custom additional code> 
}