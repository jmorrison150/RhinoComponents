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
  private void RunScript(List<Curve> obstacles, ref object outMesh) {



    #region beginScript
    double resolution = 100.000;
    double viewRadius = 3000.000;
    int accuracy = 20;

    BoundingBox bb = BoundingBox.Unset;
    for (int i = 0; i < obstacles.Count; i++) {
      bb.Union(obstacles[i].GetBoundingBox(false));
    }
    Vector3d offset = new Vector3d(viewRadius * 1.1, viewRadius * 1.1, 0.0);
    Plane plane = new Plane(bb.Min-offset, Vector3d.ZAxis);
    Rectangle3d rectangle = new Rectangle3d(plane, bb.Max.X - bb.Min.X+ (viewRadius*2.2), bb.Max.Y - bb.Min.Y+(viewRadius*2.2));


    Mesh mesh = meshPlane(rectangle, resolution);
    mesh = computeColors(mesh, obstacles, viewRadius, accuracy);


    //Point3d[] pts = mesh.Vertices.ToPoint3dArray();
    //A = pts;

    outMesh = mesh;
    #endregion




  }

  // <Custom additional code> 





  #region customCode
  Mesh meshPlane(Rectangle3d rectangle, double resolution) {
    int xCount = (int)(rectangle.Width / resolution);
    int yCount = (int)(rectangle.Height / resolution);
    Mesh mesh = Mesh.CreateFromPlane(rectangle.Plane, rectangle.X, rectangle.Y, xCount, yCount);
    return mesh;
  }

  Mesh computeColors(Mesh mesh, List<Curve> obstacles, double viewRadius, int accuracy) {


    Color[] colors = new Color[mesh.Vertices.Count];

    //check for divide by zero
    if (viewRadius <= 0) { viewRadius = 0.001; }
    if (accuracy < 1) { accuracy = 1; }




    //preCalculate for 3D
    Sphere sphere = new Sphere(Point3d.Origin, viewRadius);
    Mesh meshSphere = Mesh.CreateFromSphere(sphere, accuracy, 2);
    Point3d[] meshPts = meshSphere.Vertices.ToPoint3dArray();
    //preCalculate for 2D
    Circle circle = new Circle(Point3d.Origin, viewRadius);
    Point3d[] circlePts;
    circle.ToNurbsCurve().DivideByCount(accuracy, true, out circlePts);
    Vector3d[] viewDirections = new Vector3d[circlePts.Length];
    for (int j = 0; j < viewDirections.Length; j++) {
      Vector3d vec = new Vector3d(circlePts[j]);
      vec.Unitize();
      vec *= viewRadius;
      viewDirections[j] = vec;
    }




    System.Threading.Tasks.Parallel.For(0, mesh.Vertices.Count, p => {
      Point3d pt = mesh.Vertices[p];
      Point3d[] intersectionPts = new Point3d[viewDirections.Length];
      double currentMaxDistance = double.MaxValue;

      for (int j = 0; j < viewDirections.Length; j++) {
        Line line = new Line(pt, viewDirections[j]);
        intersectionPts[j] = line.To;
        currentMaxDistance = line.Length;



        for (int i = 0; i < obstacles.Count; i++) {
          //cheap bounding box test
          BoundingBox bbox = obstacles[i].GetBoundingBox(false);
          Point3d closestPt = bbox.ClosestPoint(pt);
          double dist = pt.DistanceTo(closestPt);
          if (dist > viewRadius) { continue; }

          //expensive curve intersection test
          LineCurve line1 = new LineCurve(line);
          Rhino.Geometry.Intersect.CurveIntersections crvIntersections = Rhino.Geometry.Intersect.Intersection.CurveCurve(line1, obstacles[i], 0.001, 0.001);
          for (int k = 0; k < crvIntersections.Count; k++) {
            double distance = pt.DistanceTo(crvIntersections[k].PointA);
            if (currentMaxDistance > distance) {
              intersectionPts[j] = crvIntersections[k].PointA;
              currentMaxDistance = distance;
            }
          }
        }
      }


      Polyline polyline = new Polyline(intersectionPts);
      polyline.Add(polyline[0]);
      PolylineCurve plCurve = new PolylineCurve(polyline);
      plCurve.MakeClosed(0.100);
      AreaMassProperties areaMass = AreaMassProperties.Compute(plCurve, 0.001);
      double area = areaMass.Area;
      Color color = Color.FromArgb((int)area);
      colors[p] = color;

    });//end Parallel.For


    mesh.VertexColors.CreateMonotoneMesh(Color.FromArgb(0));
    mesh.VertexColors.SetColors(colors);

    return mesh;
  }

  double map(double value1, double min1, double max1, double min2, double max2) {
    double value2 = min2 + (value1 - min1) * (max2 - min2) / (max1 - min1);
    return value2;
  }
  #endregion



  // </Custom additional code> 
}