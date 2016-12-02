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
public class Script_Instance_Roof : GH_ScriptInstance {
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
    private void RunScript(Brep brickSurface, Surface refSurface, ref object bricks0, ref object outCourses, ref object outPoints) {




        #region beginScript

        bool uv = false;
        bool reverse = false;

        bricks.Clear();
        List<Surface> cuttingSurfaces0;
        List<Surface> cuttingSurfaces1;
        List<Curve> curves0;
        List<Curve> curves1;

        makeCutSurfaces(refSurface, uv, out cuttingSurfaces0, out cuttingSurfaces1);

        curves0 =   brickCourses(brickSurface, refSurface, cuttingSurfaces0);
        curves1 =   brickCourses(brickSurface, refSurface, cuttingSurfaces1);

        layBrick(curves0, 0, refSurface, reverse);
        layBrick(curves1, 1, refSurface, reverse);


        //output boxes
        List<Box> boxes0 = new List<Box>();
        for(int i = 0; i < bricks.Count; i++) { boxes0.Add(bricks[i].draw(reverse)); }
        bricks0 = boxes0;

        //output points
        Point3d[] updatePoints = new Point3d[bricks.Count];
        for(int i = 0; i < bricks.Count; i++) { updatePoints[i] = bricks[i].start; }
        outPoints = updatePoints;


        #endregion



    }

    // <Custom additional code> 




    #region customCode
    List<Brick> bricks = new List<Brick>();

    List<Curve> brickCourses(Brep brickSurface, Surface refSurface, List<Surface> cuttingSurfaces) {
        List<Curve> curves = new List<Curve>();
        for(int i = 0; i < cuttingSurfaces.Count; i++) {
            Curve[] intersectionCurves;
            Point3d[] intersectionPoints;
            Rhino.Geometry.Intersect.Intersection.BrepSurface(brickSurface, refSurface, 0.001, out intersectionCurves, out intersectionPoints);
            curves.AddRange(intersectionCurves);
        }
        return curves;
    }
    Point3d[] layBrick(List<Curve> curves, int course, Surface refSurface, bool reverse) {
        Point3d[] updatePoints = new Point3d[1];

        if(reverse) {
            for(int i = 0; i < curves.Count; i++) {
                Curve c = curves[i];
                c.Reverse();
                curves[i] = c;
            }
        }

        //units are meters here, in the Brick Class, and while laying bricks
        double sizeX = 0.2150;
        double sizeY = 0.1025;
        //double sizeZ = 0.0650;
        double jointSize = 0.0100;
        double spacingPercent = 0.0;
        double start = 0.0;

        for(int i = 0; i < curves.Count; i++) {
            List<Point3d> courseXPoints = new List<Point3d>();
            List<double> brickLengths = new List<double>();

            sizeX = 0.2150;
            //compute brick spacing
            double maxLength = curves[i].GetLength(0.001);
            double currentLength = start;
            double lengthLeft = maxLength - currentLength;
            double extraSpace = 0.0;


            if(sizeX > maxLength) { sizeX = maxLength; }


            //lay bricks
            while(currentLength < maxLength) {


                //add this center point
                Point3d pt = curves[i].PointAtLength(currentLength);
                courseXPoints.Add(pt);

                //Is there room for another brick?
                double offsetX = sizeX;

                //check for tight turns
                double r0 = curvature(curves[i], currentLength);
                double r1 = curvature(curves[i], currentLength + 0.1025);
                double r2 = curvature(curves[i], currentLength + 0.215);

                double radius = Math.Min(r0, r1);
                radius = Math.Min(radius, r2);



                double lowerThreshold = 0.700;
                if(radius > 3.500) { radius = 3.500; }
                if(radius < lowerThreshold) { radius = lowerThreshold; }
                //offsetX = radius * 0.0614285714285714;

                //offsetX = 0.050;
                brickLengths.Add(offsetX);






                if(currentLength == start && course % 2 == 0) { offsetX *= 0.5; }
                double rotationCurrent = 0.0;
                double rotationRad = ( rotationCurrent * Math.PI ) / 180;
                double brickLength = ( offsetX * Math.Cos(rotationRad) ) + ( sizeY * Math.Sin(rotationRad) );
                // extraSpace = (currentLength / 100);
                extraSpace = brickLength * spacingPercent;
                currentLength += brickLength;


                lengthLeft = maxLength - currentLength;


                currentLength += jointSize;
                currentLength += extraSpace;


            }

            //smooth
            Curve smoothCurve;
            if(courseXPoints.Count < 2) {
                Point3d[] pts = new Point3d[2];
                pts[0]  = curves[i].PointAtStart;
                pts[1] = curves[i].PointAtEnd;
                smoothCurve =  Curve.CreateInterpolatedCurve(pts, 1);
            } else { smoothCurve = Curve.CreateInterpolatedCurve(courseXPoints, 1); }

            //add brick
            for(int k = 0; k < courseXPoints.Count; k++) {
                Brick brick = new Brick(courseXPoints[k], curves[i], course, k, brickLengths[k], refSurface);
                bricks.Add(brick);
            }

            //ends
            if(lengthLeft < 0.0) {
                try {
                    bricks[bricks.Count - 1].end = true;
                    bricks[bricks.Count - 1].edgeLength = lengthLeft + sizeX;
                } catch { Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "brick.Count != courseXParameters.Count"); }
            }










        }
        return updatePoints;

    }
    void makeCutSurfaces(Surface surface, bool uv, out List<Surface> cuttingSurfaces0, out List<Surface> cuttingSurfaces1) {
        cuttingSurfaces0 = new List<Surface>();
        cuttingSurfaces1 = new List<Surface>();
        //List<Point3d> pts = new List<Point3d>();

        double distance = 1.000;
        int direction = 1;
        int otherDirection = 0;
        if(uv) { direction = 0; otherDirection = 1; }
        double u = surface.Domain(otherDirection).Mid;
        Curve c = surface.IsoCurve(otherDirection, u);
        double[] ts = c.DivideByLength(0.075, true);



        for(int i = 0; i < ts.Length; i++) {
            Curve c1 = surface.IsoCurve(direction, ts[i]);
            Curve c2 = c1.OffsetNormalToSurface(surface, -distance);
            Vector3d normal = surface.NormalAt(ts[i], u);
            normal.Unitize();
            normal *= ( distance * 2.0 );
            Surface s = Surface.CreateExtrusion(c2, normal);
            if(i%2==0) { cuttingSurfaces0.Add(s); } else { cuttingSurfaces1.Add(s); }
            //Point3d pt = surface.PointAt(ts[i], u);
            // pts.Add(pt);
        }




    }
    double curvature(Curve c, double currentLength) {
        double radius = 3.500;
        double t;
        c.LengthParameter(currentLength, out t);
        Vector3d vector3d1 = c.TangentAt(t);
        Vector3d vector3d2 = c.CurvatureAt(t);
        double length = vector3d2.Length;


        if(length > 1.490116119385E-08) {
            Vector3d vector3d3 = ( vector3d2 / ( length * length ) );
            radius = vector3d3.Length;
            //              Print(vector3d3.Length.ToString());
            //              Circle circle;
            //              circle = new Circle(pt, vector3d1, (pt + (vector3d3 * 2.0)));
        }
        return radius;
    }
    double zGradient(Point3d testPoint, Curve curve) {
        BoundingBox bb = curve.GetBoundingBox(false);
        double t1 = map(testPoint.Z, bb.Min.Z, bb.Max.Z, 1.0, 0.0);
        if(t1 < 0) { t1 = 0; } else if(t1 > 1.0) { t1 = 1.0; }
        //    Print(testPoint.Z.ToString());
        //Print(t1.ToString());
        return t1;
    }
    bool imageClip(Point3d testPoint, Surface surface, System.Drawing.Bitmap bitmap, double threshold) {

        int normalizedPixelX, normalizedPixelY;
        double u, v, normalizedU, normalizedV;

        surface.ClosestPoint(testPoint, out u, out v);
        normalizedU = surface.Domain(0).NormalizedParameterAt(u);
        normalizedV = surface.Domain(1).NormalizedParameterAt(v);




        normalizedPixelX = (int) map(normalizedU, 0.0, 1.0, 1.0, (double) bitmap.Width);
        normalizedPixelY = (int) map(normalizedV, 0.0, 1.0, 1.0, (double) bitmap.Height);


        System.Drawing.Color pixelColor = bitmap.GetPixel(normalizedPixelX, normalizedPixelY);
        if(pixelColor.R < threshold * 255) {
            return true;
        } else { return false; }
    }
    void setRelief(List<Curve> crvs, Brep surfaces, double reliefDistance) {

        try {
            for(int i = 0; i < crvs.Count; i++) {
                for(int j = 0; j < bricks.Count; j++) {



                    double maxDist = bricks[j].sizeX * 0.5;
                    double t;
                    if(!( crvs[i].ClosestPoint(bricks[j].start, out t, maxDist) )) { continue; }

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
        } catch { }
    }
    double calculatePorosity(Point3d testPoint, List<Curve> porosityCurves, double maxDist) {
        double max = 0.0;
        for(int i = 0; i < porosityCurves.Count; i++) {
            double t;
            porosityCurves[i].ClosestPoint(testPoint, out t, maxDist);
            Point3d testPoint2 = porosityCurves[i].PointAt(t);
            double dist = testPoint.DistanceTo(testPoint2);
            dist = 1.0 - ( dist / maxDist );
            if(max < dist) { max = dist; }
        }

        return max * max;
    }
    double calculateRotation(Point3d testPoint, Curve rotationCurve) {
        BoundingBox bb = rotationCurve.GetBoundingBox(false);
        double t1 = map(testPoint.Z, bb.Min.Z, bb.Max.Z, 1.0, 0.0);
        if(t1 < 0) { t1 = 0; } else if(t1 > 1.0) { t1 = 1.0; }
        return t1;
    }
    double map(double currentValue, double oldMin, double oldMax, double newMin, double newMax) {
        return ( newMin + ( newMax - newMin ) * ( ( currentValue - oldMin ) / ( oldMax - oldMin ) ) );
    }
    public class Brick {
        //constructor
        public Brick(Point3d _center, Curve _centerLine, int _courseZ, int _courseX, double _brickLength, Surface _refSurface) {
            this.start = _center;
            this.centerLine = _centerLine;
            this.courseX = _courseX;
            this.courseZ = _courseZ;
            this.sizeX = _brickLength;
            this.refSurface = _refSurface;

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
        public Surface refSurface;

        //methods
        public void setRotation(double degrees) {
            this.localRotation = degrees;
            if(this.courseZ % 2 == 1) {
                this.localRotation *= -1.0;
            }
        }
        public void setRelief(List<Curve> crvs) {
            for(int i = 0; i < crvs.Count; i++) {

                double maxDist = this.sizeX;
                double t;
                if(!( crvs[i].ClosestPoint(this.start, out t, maxDist) )) { return; }
                double dist = this.start.DistanceTo(crvs[i].PointAt(t));
                double t1;
                this.centerLine.ClosestPoint(this.start, out t1, maxDist + 1.000);

            }

        }
        public Box draw(bool reverse) {

            double t, s0, s1;
            Plane plane;
            Vector3d vecX, vecY, normal;
            Interval intervalX, intervalY, intervalZ;
            //Point3d closestPoint;
            //ComponentIndex ci;

            this.centerLine.ClosestPoint(this.start, out t, 10.000);
            //this.brep.ClosestPoint(this.start,out closestPoint,out ci, out s0, out s1, 10.000,out normal);
            this.refSurface.ClosestPoint(this.start, out s0, out s1);
            normal = this.refSurface.NormalAt(s0, s1);
            vecX = this.centerLine.TangentAt(t);
            vecY = normal;
            plane = new Plane(this.start, vecX, vecY);



            //check for end conditions
            double startX = 0.0;
            double endX = sizeX;
            if(this.edge == 0 && this.courseZ % 2 == 0) {
                startX = 0.0;
                endX = sizeX * 0.5;
            }
            if(this.end) {
                startX = 0.0;
                endX = ( this.edgeLength );
            }
            //if (reverse) { sizeY *= -1.0; }

            intervalX = new Interval(startX, endX);
            intervalY = new Interval(-sizeY*0.5, sizeY*0.5);
            intervalZ = new Interval(0, sizeZ);

            Box box = new Box(plane, intervalX, intervalY, intervalZ);
            Transform xform = Transform.Rotation(( this.localRotation ) * Math.PI / 180, plane.ZAxis, box.Center);
            box.Transform(xform);

            return box;

        }


    }
    #endregion






    // </Custom additional code> 
}