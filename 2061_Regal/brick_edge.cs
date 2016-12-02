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
  private void RunScript(List<Curve> curves, object y, ref object A, ref object B0, ref object B1, ref object B2, ref object C)
  {

    List<Point3d> pts = new List<Point3d>();
    List<Line> lines = new List<Line>();
    List<Curve> outCurves = new List<Curve>();
    List<double> radius = new List<double>();
    List<Vector3d> normals = new List<Vector3d>();
    List<Circle> circles = new List<Circle>();
    List<double> outParameters = new List<double>();
    List<Box> boxes0 = new List<Box>();
    List<Box> boxes1 = new List<Box>();
    List<Box> boxes2 = new List<Box>();


    for (int i = 0; i < curves.Count; i++)
    {
      //start
      Line line0;
      line0 = lineAtStart(curves[i]);
      lines.Add(line0);


      Vector3d normal0 = Vector3d.CrossProduct(Vector3d.ZAxis, line0.To - line0.From);
      Plane plane0 = new Plane(line0.From, line0.To - line0.From, normal0);
      Interval xSize0 = new Interval(0, 0.215);
      Interval ySize0 = new Interval(0, 0.1025);
      Interval zSize0 = new Interval(0, 0.065);
      Box box0 = new Box(plane0, xSize0, ySize0, zSize0);
      boxes0.Add(box0);







      //mid
      Line line1;
      line1 = lineAtMid(curves[i]);
      lines.Add(line1);





      Curve c;
      c = curves[i];
      //c.Rebuild(25, 3, true);

      double t0;
      c.ClosestPoint(line1.From, out t0);
      double d0 = c.CurvatureAt(t0).Length;
      normals.Add(c.CurvatureAt(t0));
      pts.Add(c.PointAt(t0));
      radius.Add(d0);
      outParameters.Add(t0);
      outCurves.Add(c);


      double t2;
      c.ClosestPoint(line1.To, out t2);
      double d2 = c.CurvatureAt(t2).Length;
      radius.Add(d2);
      normals.Add(c.CurvatureAt(t2));
      pts.Add(c.PointAt(t2));
      outParameters.Add(t2);
      outCurves.Add(c);

      Vector3d normal1 = Vector3d.CrossProduct(Vector3d.ZAxis, line1.To - line1.From);
      Plane plane1 = new Plane(line1.From, line1.To - line1.From, normal1);
      Interval xSize1 = new Interval(0, 0.215);
      Interval ySize1 = new Interval(0, 0.1025);
      Interval zSize1 = new Interval(0, 0.065);
      Box box1 = new Box(plane1, xSize1, ySize1, zSize1);
      boxes1.Add(box1);








      //end
      Line line2;
      line2 = lineAtEnd(curves[i]);
      lines.Add(line2);
      Vector3d normal2 = Vector3d.CrossProduct(Vector3d.ZAxis, line2.To - line2.From);
      Plane plane2 = new Plane(line2.From, line2.To - line2.From, normal2);

      Interval xSize2 = new Interval(0, 0.215);
      Interval ySize2 = new Interval(0, -0.1025);
      Interval zSize2 = new Interval(0, 0.065);
      Box box2 = new Box(plane2, xSize2, ySize2, zSize2);
      boxes2.Add(box2);


    }

    A = outParameters;
    B0 = boxes0;
    B1 = boxes1;
    B2 = boxes2;
    C = outCurves;

  }

  // <Custom additional code> 

  Line lineAtStart(Curve c){
    Vector3d vec = c.TangentAtStart;
    vec.Unitize();
    vec *= 0.215;
    Line line = new Line(c.PointAtStart, vec);
    return line;
  }
  Line lineAtEnd(Curve c){
    Vector3d vec = c.TangentAtEnd;
    vec.Unitize();
    vec *= -1.0;
    vec *= 0.215;
    Line line = new Line(c.PointAtEnd, vec);
    return line;
  }
  Line lineAtMid(Curve c){
    double t;
    c.NormalizedLengthParameter(0.5, out t);
    Point3d midPt = c.PointAt(t);
    Vector3d vec = c.TangentAt(t);
    vec.Unitize();

    Vector3d vec0 = vec;
    vec0 *= (0.215 * -0.5);

    Vector3d vec1 = vec;
    vec1 *= (0.215 * 0.5);

    Point3d pt0 = vec0 + midPt;
    Point3d pt1 = vec1 + midPt;
    Line line = new Line(pt0, pt1);
    return line;
  }



















  List<Brick> bricks = new List<Brick>();
  public class Brick
  {
    //constructor
    public Brick(Point3d _center, Curve _centerLine, int _courseZ, int _courseX)
    {
      this.start = _center;
      this.centerLine = _centerLine;
      this.courseX = _courseX;
      this.courseZ = _courseZ;
      if (this.courseX == 0) { this.edge = 0; }
    }

    //properties
    public Curve centerLine;
    public Point3d start;

    public double sizeX = 0.215;
    public double sizeY = 0.1025;
    public double sizeZ = 0.065;

    public double localRotation = 0.0;
    public double extraSpaceAfter = 0.0;
    public int courseZ;
    public int courseX;
    public int edge = 1;
    public bool end = false;
    public double edgeLength = 0.0;

    //methods
    public Box draw()
    {

      double t;
      this.centerLine.ClosestPoint(this.start, out t, 10.000);
      Plane plane;
      Vector3d vecX = this.centerLine.TangentAt(t);
      Vector3d vecY = Vector3d.CrossProduct(vecX, Vector3d.ZAxis);
      plane = new Plane(this.start, vecX, vecY);

      Interval intervalX, intervalY, intervalZ;


      //check for end conditions
      double startX = 0.0;
      double endX = sizeX;
      if (this.edge == 0 && this.courseZ % 2 == 0) {
        startX = 0.0;
        endX = sizeX * 0.5; }
      if (this.end)
      {
        startX = this.edgeLength * -1.0;
        endX = 0.0;
      }


      intervalX = new Interval(startX, endX);
      intervalY = new Interval(sizeY * -0.5, sizeY * 0.5);
      intervalZ = new Interval(sizeZ * -0.5, sizeZ * 0.5);



      Box box = new Box(plane, intervalX, intervalY, intervalZ);



      Transform xform = Transform.Rotation((this.localRotation) * Math.PI / 180, plane.ZAxis, box.Center);
      box.Transform(xform);






      return box;

    }
    public void setRotation(double degrees)
    {
      this.localRotation = degrees;
      if (this.courseZ % 2 == 1)
      {
        this.localRotation *= -1.0;
      }
    }
    public void setRelief(List<Curve> crvs)
    {
      for (int i = 0; i < crvs.Count; i++)
      {

        double maxDist = this.sizeX;
        double t;
        if (!(crvs[i].ClosestPoint(this.start, out t, maxDist))) { return; }
        double dist = this.start.DistanceTo(crvs[i].PointAt(t));
        double t1;
        this.centerLine.ClosestPoint(this.start, out t1, maxDist + 1.000);

      }

    }



  }
  // </Custom additional code> 
}