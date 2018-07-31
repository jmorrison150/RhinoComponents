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
public class Script_Instance65 : GH_ScriptInstance {
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
    private void RunScript(string csv, double green, object x, ref object outLat, ref object outLng, ref object outColor, ref object A) {









        #region beginScript
        string[] stringRow = System.IO.File.ReadAllLines(csv);
        double[] latitudes = new double[stringRow.Length - 1];
        double[] longitudes = new double[stringRow.Length - 1];
        double[] averageDailyTraffic = new double[stringRow.Length - 1];
        System.Drawing.Color[] colors = new System.Drawing.Color[stringRow.Length - 1];
        string[] header = stringRow[0].Split(',');
        for (int i = 0; i < header.Length; i++) {
          //  Print(header[i]);
        }


        for (int i = 1; i < stringRow.Length - 0; i++) {
            string[] data = stringRow[i].Split(',');



            //Print(data[18]);
            double lat;
            double.TryParse(data[18], out lat);
            if (lat != 0) { latitudes[i - 1] = lat; }


            double lng;
            double.TryParse(data[17], out lng);
            if (lng != 0) {
                longitudes[i - 1] = lng;
            }



            double traffic;
            double.TryParse(data[2], out traffic);
            if (traffic != 0) {
                averageDailyTraffic[i - 1] = traffic;
            }
        }

        double maxTraffic = averageDailyTraffic.Max();
        for (int i = 0; i < averageDailyTraffic.Length; i++) {
            int colorR = (int)map(averageDailyTraffic[i], 0, maxTraffic, 0, 255);
            int colorG = (int)map(averageDailyTraffic[i], maxTraffic, 0, 0, green);


            colors[i] = System.Drawing.Color.FromArgb(colorR, colorG, 0);




        }

        outLat = latitudes;
        outLng = longitudes;
        outColor = colors;


        int[] reds = new int[colors.Length];
        for (int i = 0; i < reds.Length; i++) {
            reds[i] = colors[i].R;
        }


        #endregion











    }

    // <Custom additional code> 

    public double map(double number, double low1, double high1, double low2, double high2) {
        return low2 + (high2 - low2) * (number - low1) / (high1 - low1);
    }

    // </Custom additional code> 
}