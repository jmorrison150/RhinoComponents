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
    private void RunScript(List<Curve> curves, List<Curve> porous, Curve rotation, Curve zRotation, double reliefDistance, double maxDist, ref object bricks0, ref object bricks1, ref object bricks2, ref object outCourses, ref object outPoints)
    {







        #region beginScript


        //scale = 1000.0;          //meters
        //scale = 1.0;             //millimeters // check in the Brick class too

        double sizeX = 0.2150;
        double sizeY = 0.1025;
        double sizeZ = 0.0650;

        double jointSize = 0.0100;
        double rotationAngle = 0;
        double spacingPercent = 0;
        //double reliefDistance = 50.0;
        //bool useBrep = false;

        //numbers below zero will lead to infinite loops

        if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0 || jointSize < 0) { Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "all sizes must be positive values"); return; }
        bricks.Clear();

        //assume all brick courses are horizontal
        //centerLines == contourLines == courses
        //BoundingBox bbox;
        //if (!useBrep) { bbox = meshes.GetBoundingBox(false); }
        //else { bbox = brep.GetBoundingBox(false); }
        //int courses = (int) ((bbox.Max.Z - bbox.Min.Z) / (sizeZ + jointSize));
        //List<Curve> centerLines = new List<Curve>();
        //List<Point3d> centerPoints = new List<Point3d>();

        //i == courseZ
        Point3d[] startPoints = new Point3d[curves.Count];
        for (int i = 0; i < curves.Count; i++)
			{
			 startPoints[i] = curves[i].PointAtStart;
			}
        BoundingBox bbox = new BoundingBox(startPoints);




        for (int i = 0; i < curves.Count; i++)
        //for (int i = 150; i < 154; i++)
        {

        int currentCourse = 0;
        currentCourse = (int)((curves[i].PointAtStart.Z - bbox.Min.Z) / (sizeZ + jointSize));


            // double currentZ = bbox.Min.Z + ((sizeZ + jointSize) * i);
            Point3d startPoint = curves[i].PointAtStart;
            Plane plane = new Plane(startPoint, Vector3d.ZAxis);
            List<Curve> intersectionCurves = new List<Curve>();

            intersectionCurves.Add(curves[i]);





            for (int j = 0; j < intersectionCurves.Count; j++)
            {



                //compute brick spacing
                double maxLength = intersectionCurves[j].GetLength(0.001);
                double currentLength = 0.0;

                double lengthLeft = maxLength - currentLength;
                double extraSpace = 0.0;
                List<double> courseXLengths = new List<double>();
                List<Point3d> courseXPoints = new List<Point3d>();


                //lay bricks
                while (currentLength <= maxLength - sizeX)
                {
                    //add this center point
                    Point3d pt = intersectionCurves[j].PointAtLength(currentLength);
                    courseXPoints.Add(pt);
                    courseXLengths.Add(currentLength);

                    //Is there room for another brick?
                    double offsetX = sizeX;
                    if (currentLength == 0 && i % 2 == 0) { offsetX *= 0.5; }
                    double rotationCurrent = rotationAngle * zGradient(pt, zRotation);
                    double rotationRad = (rotationCurrent * Math.PI) / 180;
                    double brickLength = (offsetX * Math.Cos(rotationRad)) + (sizeY * Math.Sin(rotationRad));
                    extraSpace = brickLength * (spacingPercent * 0.01) * calculatePorosity(pt, porous, maxDist);
                    currentLength += brickLength;
                    currentLength += jointSize;
                    currentLength += extraSpace;

                    lengthLeft = maxLength - currentLength;
                }
                courseXPoints.Add(intersectionCurves[j].PointAtEnd);

                //smooth
                Curve smoothCurve;
                if (courseXPoints.Count < 2) { smoothCurve = intersectionCurves[j]; }
                else { smoothCurve = Curve.CreateInterpolatedCurve(courseXPoints, 1); }

                //add brick
                for (int k = 0; k < courseXPoints.Count; k++)
                {
                    double rotationCurrent = rotationAngle * zGradient(courseXPoints[k], zRotation);
                    Brick brick = new Brick(courseXPoints[k], smoothCurve, currentCourse, k);
                    brick.setRotation(rotationCurrent);
                    bricks.Add(brick);
                }
                //ends
                try
                {
                    bricks[bricks.Count - 1].end = true;
                    bricks[bricks.Count - 1].edgeLength = lengthLeft;
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
            if (bricks[i].end) { boxes2.Add(bricks[i].draw()); }
            else if (bricks[i].edge == 0) { boxes0.Add(bricks[i].draw()); }
            else if (bricks[i].edge == 1) { boxes1.Add(bricks[i].draw()); }
        }



        bricks0 = boxes0;
        bricks1 = boxes1;
        bricks2 = boxes2;


        #endregion











    }

    // <Custom additional code> 




    #region customCode

    double zGradient(Point3d testPoint, Curve curve)
    {
        BoundingBox bb = curve.GetBoundingBox(false);
        double t1 = map(testPoint.Z, bb.Min.Z, bb.Max.Z, 1.0, 0.0);
        if (t1 < 0) { t1 = 0; } else if (t1 > 1.0) { t1 = 1.0; }
        //    Print(testPoint.Z.ToString());
        //Print(t1.ToString());
        return t1;
    }


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
                    if (!(crvs[i].ClosestPoint(bricks[j].start, out t, maxDist))) { continue; }

                    Vector3d normal;
                    ComponentIndex ci;
                    double s, t2;
                    Point3d closestPoint;

                    surfaces.ClosestPoint(bricks[j].start, out closestPoint, out ci, out s, out t2, 1.000, out normal);
                    normal.Z = 0.0;
                    normal.Unitize();

                    Transform move = Transform.Translation(normal * reliefDistance);
                    bricks[j].start.Transform(move);


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
            if (this.edge == 0 && this.courseZ % 2 == 0)
            {
                startX = 0.0;
                endX = sizeX * 0.5;
            }
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
    #endregion













    // </Custom additional code> 
}