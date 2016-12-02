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
public class Script_Instance11 : GH_ScriptInstance {
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
    private void RunScript(List<Polyline> curves, string x, double offsetTop, 
        double offsetBottom, double varyTop, double varyBottom, 
        ref object surfaces, ref object top, ref object middle, ref object bottom) {



        //keep all input numbers at the top
        double varyWidth = varyBottom;

        //make empty list
        Polyline[] topLines = new Polyline[curves.Count];
        Polyline[] middleLines = new Polyline[curves.Count];
        Polyline[] bottomLines = new Polyline[curves.Count];
        Surface[] updateSurfaces = new Surface[curves.Count];

        //work on one curve at a time
        for(int i = 0; i < curves.Count; i++) {
            //normalize i
            double iNormalized = (double) i / (double) ( curves.Count - 1 );
            PolylineCurve[] cvs = new PolylineCurve[2];





            //copy points up
            Point3d[] ptsUp = new Point3d[curves[i].Count];
            for(int j = 0; j < curves[i].Count; j++) {
                //this is the key:  varyWidth * i
                //it can be changed to j to vary the other direction
                double distanceUp = ( offsetTop + ( varyTop * iNormalized ) );
                Vector3d moveUp = Vector3d.ZAxis * distanceUp;
                ptsUp[j] = curves[i][j] + moveUp;
            }
            topLines[i] = new Polyline(ptsUp);
            cvs[0] = new PolylineCurve(ptsUp);







            //copy points Down
            Point3d[] ptsDown = new Point3d[curves[i].Count];
            for(int j = 0; j < curves[i].Count; j++) {
                //this is the key:  varyWidth * i
                //it can be changed to j to vary the other direction
                double distanceDown = ( offsetBottom + ( varyBottom * iNormalized ) );
                Vector3d moveDown = Vector3d.ZAxis * distanceDown;
                ptsDown[j] = curves[i][j] - moveDown;
            }
            bottomLines[i] = new Polyline(ptsDown);
            cvs[1] = new PolylineCurve(ptsDown);



            Point3d[] midPts = new Point3d[topLines[i].Count];
            for(int j = 0; j < topLines[i].Count; j++) {
                midPts[j] = ( topLines[i][j] + bottomLines[i][j] ) / 2;
            }
            middleLines[i] = new Polyline(midPts);

            try {
                updateSurfaces[i] = Brep.CreateFromLoft(cvs, Point3d.Unset, Point3d.Unset, LoftType.Straight, false)[0].Surfaces[0];
            } catch { Print("loft " + i + " failed"); }
        }







        //output to grasshopper
        surfaces = updateSurfaces.ToList();
        top = topLines.ToList();
        middle = middleLines.ToList();
        bottom = bottomLines.ToList();



    }

    // <Custom additional code> 

    // </Custom additional code> 
}