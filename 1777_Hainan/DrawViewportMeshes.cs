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
public class Script_Instance : GH_ScriptInstance
{
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
    private void RunScript(object x, object y, ref object A)
    {






        #region runScript
        mousePreview();
        #endregion






    }

    // <Custom additional code> 





    #region customCode

    Point3d mouseXYZ;

    void mousePreview()
    {

        // Obtain the view's world to screen transformation
        Rhino.Geometry.Transform world_To_Screen = RhinoDocument.Views.ActiveView.ActiveViewport.GetTransform(
          Rhino.DocObjects.CoordinateSystem.World,
          Rhino.DocObjects.CoordinateSystem.Screen);
        //Point3dList pts = new Point3dList(people.Length);
        //for (int i = 0; i < people.Length; ++i)
        //{
        //    pts.Add(people[i].point);
        //}

        ////get all the points into screen space
        //pts.Transform(world_To_Screen);

        //get mouse position
        System.Drawing.Point mouseXY = System.Windows.Forms.Cursor.Position;
        System.Drawing.Point screenOffset = RhinoDocument.Views.ActiveView.ScreenRectangle.Location;
        mouseXYZ = new Point3d(mouseXY.X - screenOffset.X, mouseXY.Y - screenOffset.Y, 0.0);
        Print(mouseXYZ.ToString());

        ////find nearest person
        //int closestIndex = pts.ClosestIndex(mouseXYZ);
        //selectedPerson = people[closestIndex];
    }



    public override void DrawViewportMeshes(IGH_PreviewArgs args)
    {     //Draw all lines
        //for (int i = 0; i < lines.Count; ++i)
        //{
        //    args.Display.DrawLine(lines[i], lineColors[i]);
        //}
        args.Display.DrawPoint(mouseXYZ, Rhino.Display.PointStyle.ActivePoint, 2, Color.FromArgb(0, 255, 0, 0));
        //args.Display.DrawPoints(community, Rhino.Display.PointStyle.ControlPoint, 8, Color.FromArgb(0, 0, 0, 255));
    }



    #endregion











    // </Custom additional code> 
}