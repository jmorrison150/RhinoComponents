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
using System.Collections.Generic;
using System.Runtime.InteropServices;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance53 : GH_ScriptInstance {
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
    private void RunScript(object pointCloud, List<Curve> curves, ref object A) {



        #region beginScript
        List<Surface> updateSurfaces = new List<Surface>();

        PointCloud cloud = (PointCloud)pointCloud;
        Point3d[] cloudPts = cloud.GetPoints();


        for (int i = 0; i < curves.Count; i++) {
            AreaMassProperties areaMass = AreaMassProperties.Compute(curves[i]);
            Point3d midPoint = areaMass.Centroid;

            bool inside = false;
            int count = 1;
            Point3d closestPoint = Point3d.Unset;



            //Surface extrusion = Extrusion.Create()


            while (!inside || count < 100) {

                double radius = count * 0.1;
                Circle baseCircle = new Circle(midPoint, radius);
                Cylinder cylinder = new Cylinder(baseCircle, 10000000);
                Brep b = cylinder.ToBrep(true, true);


                for (int j = 0; j < cloudPts.Length; j++) {
                    inside = b.IsPointInside(cloudPts[j], RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, false);
                    if (inside) {
                        closestPoint = cloudPts[j];
                        break;
                    }
                }

                count++;
            }

            double buildingHeight = closestPoint.Z - midPoint.Z;

            Surface s = Extrusion.CreateExtrusion(curves[i], Vector3d.ZAxis * buildingHeight);
            updateSurfaces.Add(s);



        }

        A = updateSurfaces;
        #endregion


    }

    // <Custom additional code> 

    // </Custom additional code> 
}