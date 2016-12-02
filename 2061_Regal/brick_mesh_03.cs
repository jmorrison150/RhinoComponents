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
    private void RunScript(Mesh meshes, List<Curve> porous, Curve rotation, List<Curve> reliefs, double reliefDistance, double maxDist, ref object bricks0, ref object bricks1, ref object bricks2, ref object outCourses)
    {









        #region beginScript

        //millimeters
        double sizeX = 215.0;
        double sizeY = 102.5;
        double sizeZ = 65.0;

        double jointSize = 10.0;
        double rotationAngle = 0.0;
        double spacingPercent = 70.0;
        //double reliefDistance = 50.0;


        //numbers below zero will lead to infinite loops

        if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0 || jointSize < 0)
        {
            Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "all sizes must be positive values");
            return;
        }


        bricks.Clear();
        //assume all brick courses are horizontal
        //centerLines == contourLines == courses
        BoundingBox bbox = meshes.GetBoundingBox(false);
        int courses = (int)((bbox.Max.Z - bbox.Min.Z) / (sizeZ + jointSize));
        List<Curve> centerLines = new List<Curve>();
        List<Point3d> centerPoints = new List<Point3d>();

        //i == courseZ
        for (int i = 0; i < courses; i++)
        //for (int i = 0; i < 160; i++)
        {
            double currentZ = bbox.Min.Z + ((sizeZ + jointSize) * i);
            Point3d startPoint = new Point3d(bbox.Min.X, bbox.Min.Y, currentZ);
            Plane plane = new Plane(startPoint, Vector3d.ZAxis);
            List<Curve> intersectionCurves = new List<Curve>();
            Polyline[] pls = Rhino.Geometry.Intersect.Intersection.MeshPlane(meshes, plane);

            try
            {
                for (int j = 0; j < pls.Length; j++)
                {

                    //take out sharp turns
                    //this does not work right now
                    Curve c = Curve.CreateControlPointCurve(pls[j], 1);
                    c.RemoveShortSegments(215.0);
                    double length = c.GetLength(1.0);
                    int cpoints = (int)(length / 215.0);
                    if (cpoints < 2) { cpoints = 2; }
                    c.Rebuild(cpoints, 3, false);
                    intersectionCurves.Add(c);
                }
            }
            catch { }
            //just for visualization
            centerLines.AddRange(intersectionCurves);


            for (int j = 0; j < intersectionCurves.Count; j++)
            {



                //compute brick spacing
                //TODO: revise///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                double maxLength = intersectionCurves[j].GetLength(1.0);
                double currentLength = 0;
                double lengthLeft = maxLength - currentLength;
                currentLength = sizeX * 0.5 * (i % 2);
                List<double> courseXParameters = new List<double>();
                Point3d previousPoint = startPoint;
                double brickAtLength = 0.0;


                //lay bricks
                while (currentLength <= maxLength)
                {
                    double rotationCurrent = rotationAngle * calculateRotation(previousPoint, rotation);
                    double rotationRad = (rotationCurrent * Math.PI) / 180;
                    brickAtLength = (sizeX * Math.Cos(rotationRad)) + (sizeY * Math.Sin(rotationRad));
                    double extraSpace = brickAtLength * (spacingPercent * 0.01 * calculatePorosity(previousPoint, porous, maxDist));

                    brickAtLength += extraSpace;
                    brickAtLength += jointSize;
                    courseXParameters.Add(brickAtLength);

                    Point3d pt = intersectionCurves[j].PointAtLength(currentLength);
                    Brick brick = new Brick(pt, intersectionCurves[j], i, courseXParameters.Count - 1);
                    brick.setRotation(rotationCurrent);
                    if (courseXParameters.Count == 1 && (i % 2 == 0)) { brick.edge = 0; }
                    bricks.Add(brick);




                    currentLength += brickAtLength;
                    lengthLeft = maxLength - currentLength;
                    previousPoint = pt;
                }

                try
                {
                    bricks[bricks.Count - 1].edge = 2;
                    bricks[bricks.Count - 1].end = true;
                    bricks[bricks.Count - 1].edgeLength = lengthLeft + (brickAtLength * 0.5);

                }
                catch { Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "brick.Count != courseXParameters.Count"); }


            }
        }

        // setRelief(reliefs, surfaces, reliefDistance);




        //output
        List<Box> boxes0 = new List<Box>();
        List<Box> boxes1 = new List<Box>();
        List<Box> boxes2 = new List<Box>();
        for (int i = 0; i < bricks.Count; i++)
        {

            if (bricks[i].edge == 0) { boxes0.Add(bricks[i].draw()); }
            else if (bricks[i].edge == 1) { boxes1.Add(bricks[i].draw()); }
            else if (bricks[i].edge == 2) { boxes2.Add(bricks[i].draw()); }
        }



        bricks0 = boxes0;
        bricks1 = boxes1;
        bricks2 = boxes2;

        outCourses = centerLines;

        #endregion












    }

    // <Custom additional code> 




    #region customCode
    bool imageClip(Point3d testPoint, Surface surface, System.Drawing.Bitmap bitmap, double threshold)
    {

        int normalizedPixelX, normalizedPixelY;
        double u, v, normalizedU, normalizedV;

        surface.ClosestPoint(testPoint, out u, out v);
        normalizedU = surface.Domain(0).NormalizedParameterAt(u);
        normalizedV = surface.Domain(1).NormalizedParameterAt(v);




        normalizedPixelX = (int)map(normalizedU, 0.0, 1.0, 1.0, (double)bitmap.Width);
        normalizedPixelY = (int)map(normalizedV, 0.0, 1.0, 1.0, (double)bitmap.Height);


        System.Drawing.Color pixelColor = bitmap.GetPixel(normalizedPixelX, normalizedPixelY);
        if (pixelColor.R < threshold * 255)
        {
            return true;
        }
        else { return false; }
    }
    void setRelief(List<Curve> crvs, Brep surfaces, double reliefDistance)
    {

        try
        {
            for (int i = 0; i < crvs.Count; i++)
            {
                for (int j = 0; j < bricks.Count; j++)
                {



                    double maxDist = bricks[j].sizeX * 0.5;
                    double t;
                    if (!(crvs[i].ClosestPoint(bricks[j].center, out t, maxDist))) { continue; }

                    Vector3d normal;
                    ComponentIndex ci;
                    double s, t2;
                    Point3d closestPoint;

                    surfaces.ClosestPoint(bricks[j].center, out closestPoint, out ci, out s, out t2, 1000.0, out normal);
                    normal.Z = 0.0;
                    normal.Unitize();

                    Transform move = Transform.Translation(normal * reliefDistance);
                    bricks[j].center.Transform(move);


                }
            }
        }
        catch { }
    }
    double calculatePorosity(Point3d testPoint, List<Curve> porosityCurves, double maxDist)
    {
        double max = 0.0;
        for (int i = 0; i < porosityCurves.Count; i++)
        {
            double t;
            porosityCurves[i].ClosestPoint(testPoint, out t, maxDist);
            Point3d testPoint2 = porosityCurves[i].PointAt(t);
            double dist = testPoint.DistanceTo(testPoint2);
            dist = 1.0 - (dist / maxDist);
            if (max < dist) { max = dist; }
        }

        return max * max;
    }
    double calculateRotation(Point3d testPoint, Curve rotationCurve)
    {
        BoundingBox bb = rotationCurve.GetBoundingBox(false);
        double t1 = map(testPoint.Z, bb.Min.Z, bb.Max.Z, 1.0, 0.0);
        if (t1 < 0) { t1 = 0; } else if (t1 > 1.0) { t1 = 1.0; }
        return t1;
    }
    double map(double currentValue, double oldMin, double oldMax, double newMin, double newMax)
    {
        return (newMin + (newMax - newMin) * ((currentValue - oldMin) / (oldMax - oldMin)));
    }
    List<Brick> bricks = new List<Brick>();
    public class Brick
    {
        //constructor
        public Brick(Point3d _center, Curve _centerLine, int _courseZ, int _courseX)
        {
            this.center = _center;
            this.centerLine = _centerLine;
            this.courseX = _courseX;
            this.courseZ = _courseZ;

            //if (this.courseX == 0 && this.courseZ % 2 == 0) { this.edge = 0; }
        }

        //properties
        public Curve centerLine;
        public Point3d center;
        public double sizeX = 215.0;
        public double sizeY = 102.5;
        public double sizeZ = 65.0;

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
            this.centerLine.ClosestPoint(this.center, out t, 1000.0);
            Plane plane;
            Vector3d vecX = this.centerLine.TangentAt(t);
            Vector3d vecY = Vector3d.CrossProduct(vecX, Vector3d.ZAxis);
            plane = new Plane(this.center, vecX, vecY);
            Transform xform = Transform.Rotation((this.localRotation) * Math.PI / 180, plane.ZAxis, plane.Origin);
            plane.Transform(xform);


            Interval intervalX, intervalY, intervalZ;
            if (this.edge == 0)
            {
                if (this.end) { intervalX = new Interval(0, (sizeX * 0.5) + this.edgeLength); }
                intervalX = new Interval(0, (sizeX * 0.5));
            }
            else
            {
                if (this.end) { intervalX = new Interval((sizeX * -0.5), (sizeX * 0.5) + this.edgeLength); }
                intervalX = new Interval(sizeX * -0.5, (sizeX * 0.5) + this.edgeLength);
            }
            intervalY = new Interval(sizeY * -0.5, sizeY * 0.5);
            intervalZ = new Interval(sizeZ * -0.5, sizeZ * 0.5);

            Box box = new Box(plane, intervalX, intervalY, intervalZ);
            return box;

        }
        public void setRotation(double degrees) { this.localRotation = degrees; }
        public void setRelief(List<Curve> crvs)
        {
            for (int i = 0; i < crvs.Count; i++)
            {

                double maxDist = this.sizeX;
                double t;
                //crvs[i].ClosestPoint(this.center, out t, maxDist);
                if (!(crvs[i].ClosestPoint(this.center, out t, maxDist))) { return; }
                double dist = this.center.DistanceTo(crvs[i].PointAt(t));
                double t1;
                this.centerLine.ClosestPoint(this.center, out t1, 1000.0);

            }

        }



    }
    #endregion








    // </Custom additional code> 
}