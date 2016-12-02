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
    private void RunScript(List<Surface> surfaces, object points, double unrollWidth, double unrollHeight, ref object outSurfaces, ref object outPoints, ref object outPlanes, ref object outTransforms) {







        #region beginScript
        Transform[] transforms = new Transform[surfaces.Count];
        BoundingBox[] boxes = new BoundingBox[surfaces.Count];
        Plane[] planes = new Plane[surfaces.Count];
        Rhino.Geometry.Box[] boxes1 = new Rhino.Geometry.Box[surfaces.Count];
        for (int i = 0; i < surfaces.Count; i++) {
            double _unrollWidth = unrollWidth * (i + 1) * -1.0;
            double _unrollHeight = unrollHeight;


            //Plane plane;
            surfaces[i].TryGetPlane(out planes[i]);

            Transform xy = Transform.PlaneToPlane(planes[i], Plane.WorldXY);
            Transform xy1 = Transform.PlaneToPlane(Plane.WorldXY, planes[i]);
            Surface surf = surfaces[i];
            surf.Transform(xy);
            boxes[i] = surf.GetBoundingBox(false);
            Transform origin = Transform.Translation(Point3d.Origin - boxes[i].Min);
            Transform origin1 = Transform.Translation(boxes[i].Min - Point3d.Origin);
            Transform overlap = Transform.Translation(_unrollWidth, _unrollHeight, 0);
            Transform overlap1 = Transform.Translation(-_unrollWidth, -_unrollHeight, 0);
            Transform flatten = origin * overlap;




            surfaces[i].Transform(flatten);

            Transform to3D = (xy1 * origin1 * overlap1);
            transforms[i] = to3D;
            //boxes[i] = surfaces[i].GetBoundingBox(xy);
            //boxes[i].Transform(yx);



            //BoundingBox box = surfaces[i].GetBoundingBox(planes[i]);
            //boxes[i] = box;
            //surfaces[i].GetBoundingBox(planes[i], out boxes1[i]);
        }

        outSurfaces = surfaces;
        outPlanes = planes;
        outTransforms = transforms;
        //outTransforms1 = boxes;
        //outTransforms1 = boxes1;

        #endregion





    }

    // <Custom additional code> 

    // </Custom additional code> 
}