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
public class Script_Instance29 : GH_ScriptInstance {
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
    private void RunScript(DataTree<Curve> curves2D, DataTree<Surface> surface2D, DataTree<Surface> surface3D, double unrollHeight, ref object curves3D) {


        #region beginScript
        //List<Curve> updateCurves = new List<Curve>();
        DataTree<Curve> updateCurves = new DataTree<Curve>();

        for (int k = 0; k < curves2D.BranchCount; k++) {
            for (int l = 0; l < curves2D.Branches[k].Count; l++) {
                int index;
                Point3d testPoint = curves2D.Branches[k][l].PointAtStart;
                index = (int)((testPoint.Y/unrollHeight)-1);

                if (curves2D.Branches[k][l].IsPolyline()) {
                    try {
                        Polyline pl;
                        curves2D.Branches[k][l].TryGetPolyline(out pl);

                        List<Point2d> pts = new List<Point2d>();
                        double extend = 1000.0;
                        Surface s = surface2D.Branches[index][0].Extend(IsoStatus.North, extend, true);
                        s = s.Extend(IsoStatus.South, extend, true);
                        s = s.Extend(IsoStatus.East, extend, true);
                        s = s.Extend(IsoStatus.West, extend, true);


                        for (int i = 0; i < pl.Count; i++) {
                            double u, v;
                            s.ClosestPoint(pl[i], out u, out v);
                            Point2d pt = new Point2d(u, v);
                            pts.Add(pt);

                        }

                        for (int i = 1; i < pts.Count; i++) {
                            //                Curve c = surface3D.ShortPath(pts[i - 1], pts[i], 0.001);
                            Point2d[] points = new Point2d[2];
                            points[0] = pts[i - 1];
                            points[1] = pts[i];
                            Curve c = surface3D.Branches[index][0].InterpolatedCurveOnSurfaceUV(points, 0.001);

                            GH_Path path = new GH_Path(index);
                            updateCurves.Add(c, path);

                            
                        }

                    } catch { }
                }
            }
        }


        curves3D = updateCurves;
        #endregion


    }

    // <Custom additional code> 

    // </Custom additional code> 
}