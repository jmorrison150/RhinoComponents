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
public class Script_Instance41 : GH_ScriptInstance {
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
    private void RunScript(DataTree<Transform> transforms, double textSize, string prefix, ref object text, ref object bounds) {



        #region beginScript

        //format from data tree
        Transform[][] xforms = new Transform[transforms.BranchCount][];
        for (int i = 0; i < xforms.Length; i++) {
            xforms[i] = new Transform[transforms.Branches[i].Count];
            for (int j = 0; j < xforms[i].Length; j++) {
                xforms[i][j] = transforms.Branches[i][j];
            }
        }



        PolylineCurve[][] text2 = new PolylineCurve[xforms.Length][];
        List<Rectangle3d> rects = new List<Rectangle3d>();



        for (int i = 0; i < xforms.Length; i++) {
            double _unrollWidth;
            double _unrollHeight;
            double width;
            double height;

            _unrollWidth = xforms[i][3].M03;
            _unrollHeight = xforms[i][3].M13;
            width = xforms[0][3].M03;
            height = xforms[0][3].M13;


            Plane location = new Plane(new Point3d(_unrollWidth, _unrollHeight, 0), Vector3d.ZAxis);

            //label indices
            string text1;
            text1 = prefix + i.ToString();
            singleLineFont(text1, location, textSize, out text2[i]);


            //make bounds
            Plane location1 = new Plane(new Point3d(_unrollWidth - (width * 0.5), _unrollHeight - (height * 0.5), 0), Vector3d.ZAxis);
            Rectangle3d rt = new Rectangle3d(location1, width, height);
            
            //move text to lower left corner
            BoundingBox bb = BoundingBox.Unset;
            for (int j = 0; j < text2[i].Length; j++) {
                BoundingBox bb2 = text2[i][j].GetBoundingBox(false);
                bb.Union(bb2);
            }
            Point3d pt = new Point3d(bb.Min.X, bb.Min.Y, 0.0);
            Point3d pt1 = new Point3d(rt.BoundingBox.Min.X, rt.BoundingBox.Min.Y, 0.0);
            rects.Add(rt);


            Transform tr = Transform.Translation(pt1 - pt);
            for (int j = 0; j < text2[i].Length; j++) {
                text2[i][j].Transform(tr);
            }
        }







        //format points back to data tree
        DataTree<Curve> text3 = new DataTree<Curve>();
        for (int m = 0; m < text2.Length; ++m) {
            Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
            for (int n = 0; n < text2[m].Length; ++n) {
                text3.Insert(text2[m][n], path, n);
            }
        }






        //output
        text = text3;
        bounds = rects;




        #endregion




    }

    // <Custom additional code> 

    private void singleLineFont(string text, Plane location, double height, out PolylineCurve[] singleLineText) {



        //makes a font instance copy
        string font = "Machine Tool SanSerif";
        double precision = 50;
        Plane plane = location;
        System.Drawing.Font localFont;
        try {
            localFont = new System.Drawing.Font(font, (float)height);
        } catch {
            //Print("Cannot Find Machine Tool SanSerif");
            font = "Arial";
            localFont = new System.Drawing.Font(font, (float)height);
        }

        //Makes a graphics path object instance
        System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddString(text, localFont.FontFamily, (int)localFont.Style, localFont.Size, new System.Drawing.PointF(0, 0), new System.Drawing.StringFormat());

        //This is a transformation matrix.
        System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
        matrix.Reset();
        //this basically turns the path into a polyline that approximates the path
        path.Flatten(matrix, (float)(height / precision));

        //extracts the points from the path
        System.Drawing.PointF[] pts = path.PathPoints;
        byte[] tps = path.PathTypes;

        //empty list of polylines
        List<Polyline> strokes = new List<Polyline>();
        Polyline stroke = null;

        //finds start point
        byte typStart = System.Convert.ToByte(System.Drawing.Drawing2D.PathPointType.Start);

        int i = -1;
        while (true) {
            i++;


            //when the stroke has i number of points in it, add it to the list and exit the while loop
            if (i >= pts.Length) {
                if (stroke != null && stroke.Count > 1) {
                    strokes.Add(stroke);
                }
                break;
            }



            //if this is the start, then add the line and start a new one
            if (tps[i] == typStart) {
                if (stroke != null && stroke.Count > 1) {
                    strokes.Add(stroke);
                }
                stroke = new Polyline();

                //this is negative if the plane.YAxis is not flipped
                stroke.Add(pts[i].X, -pts[i].Y, 0);
            } else {
                //this is negative if the plane.YAxis is not flipped
                stroke.Add(pts[i].X, -pts[i].Y, 0);
            }


        }

        for (int j = 0; j < strokes.Count; j++) {
            strokes[j].Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));
        }


        PolylineCurve[] strokesCurves = new PolylineCurve[strokes.Count];
        for (int j = 0; j < strokes.Count; j++) {
            strokesCurves[j] = new PolylineCurve(strokes[j]);
        }

        singleLineText = strokesCurves;


    }




    // </Custom additional code> 
}