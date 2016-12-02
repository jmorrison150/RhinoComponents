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


using TSplines;
using tsImplementationDetails;
using ts4;



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
    private void RunScript(object x, object y, ref object A) {

        TSplines.BaseSurf baseSurf = new TSplines.BaseSurf();
        TSplines.Surf surf = new Surf();
        surf = Util.buildBox(plane, min, max_corner, x_spans, y_spans, z_spans, false, false, false);
        System.IntPtr pointer = surf.toMesh();
            Mesh m = pointer.ToPointer();
        new Mesh().



        TSplines.BaseUtil.thickenSurface(baseSurf, 1.000, BaseUtil.ThickenEdgeType.UNCREASED);
        baseSurf = baseSurf;



        TSplines.BBox bbox = new BBox(min, max);
        TSplines.Edge edge = new Edge(surf, index);
        TSplines.Face face = new Face(surf, index);
        TSplines.Grip grip = new Grip(surf, index);
        TSplines.Link link = new Link(surf, index);
        TSplines.Plane plane = new TSplines.Plane(plane_origin, plane_xaxis, plane_yaxis);
        TSplines.Point4d point4d = new TSplines.Point4d(xw, yw, zw, w);
        //TSplines.Selection selection;
        List<Edge> edges = TSplines.Selection.grow(edges);
        TSplines.Topo topo = new TSplines.Topo(surf, index, Topo.TopoType.FACE);
        BaseUtil.BridgeResult bridgeResult = Util.bridge(surf0, topos0, surf1, topos1, curve, angle, segments, rotation, flip, align_vert_0, align_vert_1);
        TSplines.UVNFrame uvn = new TSplines.UVNFrame();
        TSplines.Vert vert = new Vert(surf, index);
        TSplines.Xform xForm = new Xform();
        Util.GeometryToTSpline(pointer);
        
        
        
        
        
        
        
        A = 1;





    }

    // <Custom additional code> 

    // </Custom additional code> 
}