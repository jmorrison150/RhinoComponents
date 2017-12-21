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
    private void RunScript(List<string> blockNames, List<string> values, List<Point3d> centerPoints, List<Mesh> image, ref object A, ref object B) {




        #region beginScript

        Vector3d sunAngle = Vector3d.ZAxis;


        string[] sortedNames = new string[centerPoints.Count];
        double[] aperture = new double[values.Count];

        for (int i = 0; i < aperture.Length; i++) {
            aperture[i] = Double.Parse(values[i]);
        }
        double max = aperture.Max();
        double min = aperture.Min();


        for (int i = 0; i < aperture.Length; i++) {
            aperture[i] = map(aperture[i], min, max, 0.0, 1.0);

        }



        Dictionary<string, double> blockData = new Dictionary<string, double>(blockNames.Count);
        for (int i = 0; i < blockNames.Count; i++) {
            blockData.Add(blockNames[i], aperture[i]);
        }
        //var sortedDict = from data in blockData orderby data.Value ascending select data;



        for (int j = 0; j < centerPoints.Count; j++) {
            Point3d[] centerPoint = new Point3d[1];
            centerPoint[0] = centerPoints[j];

            Point3d[] intersectPts = Rhino.Geometry.Intersect.Intersection.ProjectPointsToMeshes(image, centerPoint, sunAngle, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            for (int i = 0; i < intersectPts.Length; i++) {

                MeshPoint mp = image[0].ClosestMeshPoint(intersectPts[i], 0.0);
                double[] baryCentricCoordinates = mp.T;
                int faceIndex = mp.FaceIndex;
                int vertexIndexA = image[0].Faces[faceIndex].A;
                int vertexIndexB = image[0].Faces[faceIndex].B;
                int vertexIndexC = image[0].Faces[faceIndex].C;
                int vertexIndexD = image[0].Faces[faceIndex].D;

                Color colorA = image[0].VertexColors[vertexIndexA];
                Color colorB = image[0].VertexColors[vertexIndexB];
                Color colorC = image[0].VertexColors[vertexIndexC];
                Color colorD = image[0].VertexColors[vertexIndexD];

                double brightness = (colorA.GetBrightness() * baryCentricCoordinates[0])
                  + (colorB.GetBrightness() * baryCentricCoordinates[1])
                  + (colorC.GetBrightness() * baryCentricCoordinates[2])
                  + (colorD.GetBrightness() * baryCentricCoordinates[3]);



                string blockName = getValue(blockData, brightness);
                sortedNames[j] = blockName;
            }
        }


        A = sortedNames.ToList();
        B = centerPoints;


        #endregion




    }

    // <Custom additional code> 



    #region customCode
    string getValue(Dictionary<string, double> blockData, double brightness) {

        double diff = 0;
        double cVal = Double.MaxValue;
        string cID = string.Empty;


        foreach (KeyValuePair<string, double> item in blockData) {
            if (item.Value == brightness) {
                //Console.WriteLine(item.ID);
                cID = item.Key;
                return cID;
            }
            diff = Math.Abs(item.Value - brightness);
            if (diff < cVal) {
                cVal = diff;
                cID = item.Key;
            }
        }
        return cID;

    }
    public double map(double number, double low1, double high1, double low2, double high2) {
        return low2 + (high2 - low2) * (number - low1) / (high1 - low1);
    }

    #endregion



    // </Custom additional code> 
}