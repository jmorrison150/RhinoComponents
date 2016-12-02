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
public class Script_Instance3 : GH_ScriptInstance {
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
    private void RunScript(DataTree<Point3d> points, List<Polyline> iThreads, 
        double unrollWidth_i, double unrollHeight_i, double offset_i_1, double offset_i_2, 
        Point3d centerPoint, double notch_rotation_i, double notch_width_i, double notch_height_i, 
        double textSize, string text_prefix_i, double midPoints_i, 
        double voids_width, double voids_height, double voids_angle, 
        bool bake, ref object offset_1, ref object offset_2, ref object notches, 
        ref object annotation, ref object indices, ref object voids) {


        #region makeXY
        //make empty lists
        Grasshopper.DataTree<Point3d> updatePoints = new Grasshopper.DataTree<Point3d>();
        Grasshopper.DataTree<Transform> updateTransforms = new Grasshopper.DataTree<Transform>();
        Grasshopper.DataTree<Transform> updateReverseTransforms = new Grasshopper.DataTree<Transform>();

        Transform[] transforms = new Transform[iThreads.Count];
        Transform[] transforms1 = new Transform[iThreads.Count];
        Transform[][] transforms2 = new Transform[iThreads.Count][];
        Transform[][] transforms3 = new Transform[iThreads.Count][];

        for (int i = 0; i < transforms2.Length; i++) {
            transforms2[i] = new Transform[4];
            transforms3[i] = new Transform[4];
        }
        BoundingBox[] boxes = new BoundingBox[iThreads.Count];
        Plane[] planes = new Plane[iThreads.Count];
        Rhino.Geometry.Box[] boxes1 = new Rhino.Geometry.Box[iThreads.Count];




        //work on each surface one at a time
        for (int i = 0; i < iThreads.Count; i++) {
            double _unrollWidth;
            double _unrollHeight;


            _unrollWidth = unrollWidth_i * -1.0;
            _unrollHeight = unrollHeight_i * (i + 1) * -1.0;

            //_unrollWidth = unrollWidth * (i + 1) * -1.0;
            //_unrollHeight = unrollHeight * -1.0;


            //TryGetPlane might reverse the surface depending on curvature
            //surfaces[i].TryGetPlane(out planes[i]);

            //Transform xy = Transform.PlaneToPlane(planes[i], Plane.WorldXY);
            //Transform xy1 = Transform.PlaneToPlane(Plane.WorldXY, planes[i]);

            ////put the surface on the ground
            //Surface surf = surfaces[i].ToNurbsSurface();
            //surf.Transform(xy);
            //boxes[i] = surf.GetBoundingBox(false);




            Point3d pointAtStart = iThreads[i].First;
            Point3d pointAtEnd = iThreads[i].Last;
            if (pointAtStart.DistanceTo(pointAtEnd)<0.001) {
                try {
                    pointAtEnd = iThreads[i][iThreads[i].Count - 2];
                } catch { 
                    Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,i.ToString() +"thread start point == end point");
                    }
            }

            pointAtStart.Z = 0;
            pointAtEnd.Z = 0;
            Vector3d startVector = (pointAtStart - pointAtEnd);
            Vector3d endVector = -Vector3d.XAxis;




            Transform faceFront = Transform.Rotation(startVector, endVector, pointAtStart);
            Transform faceFront3 = Transform.Rotation(endVector, startVector, pointAtStart);
            iThreads[i].Transform(faceFront);
            //faceFront = Transform.Identity;
            //faceFront3 = Transform.Identity;



            //pointAtMid.Z = 0;
            Transform moveToOrigin = Transform.Translation(Point3d.Origin - pointAtStart);
            Transform moveToOrigin3 = Transform.Translation(pointAtStart - Point3d.Origin);
            iThreads[i].Transform(moveToOrigin);
            //moveToOrigin = Transform.Identity;
            //moveToOrigin3 = Transform.Identity;

            Transform rotate = Transform.Rotation(Vector3d.ZAxis, Vector3d.YAxis, Point3d.Origin);
            Transform rotate3 = Transform.Rotation(Vector3d.YAxis, Vector3d.ZAxis, Point3d.Origin);
            //iThreads[i].Transform(rotate);
            rotate = Transform.Identity;
            rotate3 = Transform.Identity;



            Transform overlap = Transform.Translation(_unrollWidth, _unrollHeight, 0);
            Transform overlap3 = Transform.Translation(-_unrollWidth, -_unrollHeight, 0);
            iThreads[i].Transform(overlap);
            //overlap = Transform.Identity;
            //overlap3 = Transform.Identity;



            transforms2[i][0] = (faceFront);
            transforms2[i][1] = (moveToOrigin);
            transforms2[i][2] = (rotate);
            transforms2[i][3] = (overlap);

            transforms3[i][3] = (faceFront3);
            transforms3[i][2] = (moveToOrigin3);
            transforms3[i][1] = (rotate3);
            transforms3[i][0] = (overlap3);



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
       // A = iThreads;
       // B = updateTransforms;
       // C = updateReverseTransforms;

        #endregion

        #region toXY
        //for (int i = 0; i < curves.Count; i++) {
        //    for (int j = 0; j < xform.Branches[i].Count; j++) {
        //        curves[i].Transform(xform.Branches[i][j]);
        //    }
        //}
        //A = curves;
        #endregion

      

    }

    // <Custom additional code> 

    // </Custom additional code> 
}