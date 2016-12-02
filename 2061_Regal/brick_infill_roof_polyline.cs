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
public class Script_Instance1 : GH_ScriptInstance {
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

        curves0 = brickCourses(brickSurface, cuttingSurfaces0);
        curves1 = brickCourses(brickSurface, cuttingSurfaces1);

        layBrick(curves0, 0, refSurface, reverse);
        layBrick(curves1, 1, refSurface, reverse);


        //output boxes
        List<Brep> boxes0 = new List<Brep>();
        for(int i = 0; i < bricks.Count; i++) { boxes0.Add(bricks[i].draw()); }
        bricks0 = boxes0;

        //output points
        List<Point3d> updatePoints = new List<Point3d>();
        for(int i = 0; i < bricks.Count; i++) {
            updatePoints.Add(bricks[i].start);
        }
        outPoints = updatePoints;


        #endregion




    }

    // <Custom additional code> 






    #region customCode
    List<Brick> bricks = new List<Brick>();

    List<Curve> brickCourses(Brep brickSurface, List<Surface> cuttingSurfaces) {
        List<Curve> curves = new List<Curve>();
        for(int i = 0; i < cuttingSurfaces.Count; i++) {
            Curve[] intersectionCurves;
            Point3d[] intersectionPoints;
            Rhino.Geometry.Intersect.Intersection.BrepSurface(brickSurface, cuttingSurfaces[i], 0.001, out intersectionCurves, out intersectionPoints);
            curves.AddRange(intersectionCurves);
        }
        return curves;
    }
    void layBrick(List<Curve> curves, int course, Surface refSurface, bool reverse) {

        for(int i = 0; i < curves.Count; i++) {
            if(reverse) { curves[i].Reverse(); }

            //units are meters here, in the Brick Class, and while laying bricks
            //double sizeZ = 0.0650;
            double sizeX = 0.2150;
            //double sizeY = 0.1025;
            double jointSize = 0.0100;
            //double spacingPercent = 0.0;

            //compute brick spacing
            double size = sizeX;
            double maxLength = curves[i].GetLength(0.001);
            double currentLength = 0.0;
            double extraSpace = 0.0;
            double lengthLeft;
            Point3d currentPoint;
            Point3d nextPoint;
            Vector3d previousVector;
            Circle circle;
            Brick brick;

            //last brick
            if(maxLength > ( ( size * 2.0 ) + jointSize )) {
                currentPoint = curves[i].PointAtLength(maxLength - size);
                nextPoint = curves[i].PointAtLength(maxLength);
                Vector3d tangent = curves[i].TangentAt(maxLength - ( ( size * 2.0 ) + jointSize ));
                circle = new Circle(currentPoint, tangent, nextPoint);
                brick = new Brick(currentPoint, nextPoint, circle.Radius, tangent, refSurface);
                bricks.Add(brick);

                maxLength -= sizeX;
            }



            //first brick////////////////////
            currentPoint = curves[i].PointAtStart;
            lengthLeft = maxLength - currentLength;
            //compute spacing
            size = 0.215;
            size += jointSize;
            size += extraSpace;
            if(size > lengthLeft) { size = lengthLeft; }
            //if(course%2==1) { size*=0.5; }//just for first brick
            currentLength += size;
            //endPoint
            nextPoint = curves[i].PointAtLength(currentLength);
            circle = new Circle(currentPoint, curves[i].TangentAtStart, nextPoint);
            brick = new Brick(currentPoint, nextPoint, circle.Radius, curves[i].TangentAtStart, refSurface);
            bricks.Add(brick);
            //reset
            previousVector = nextPoint - currentPoint;



            //second brick////////////////////
            if(lengthLeft > 0.250) {
                currentPoint = curves[i].PointAtLength(currentLength);
                lengthLeft = maxLength - currentLength;
                //compute spacing
                size = 0.215;
                size += jointSize;
                size += extraSpace;
                if(size > lengthLeft) { size = lengthLeft; }

                if(course % 2 == 1) { size *= 0.5; }//just for first brick
                currentLength += size;
                //endPoint
                nextPoint = curves[i].PointAtLength(currentLength - jointSize);
                circle = new Circle(currentPoint, previousVector, nextPoint);
                brick = new Brick(currentPoint, nextPoint, circle.Radius, previousVector, refSurface);
                bricks.Add(brick);
                //reset
                previousVector = nextPoint - currentPoint;
            }


            //lay bricks
            while(currentLength < maxLength) {

                //startPoint
                currentPoint = curves[i].PointAtLength(currentLength);
                lengthLeft = maxLength - currentLength;
                //compute spacing
                size = 0.215;
                size += jointSize;
                size += extraSpace;
                if(size > lengthLeft) { size = lengthLeft; }
                currentLength += size;
                //endPoint
                nextPoint = curves[i].PointAtLength(currentLength);
                circle = new Circle(currentPoint, previousVector, nextPoint);
                brick = new Brick(currentPoint, nextPoint, circle.Radius, previousVector, refSurface);
                bricks.Add(brick);
                //reset
                previousVector = nextPoint - currentPoint;

            }

            ////end brick
            //currentPoint = curves[i].PointAtLength(currentLength);
            //nextPoint = curves[i].PointAtEnd;
            //circle = new Circle(currentPoint, previousVector, nextPoint);
            //brick = new Brick(currentPoint, nextPoint, circle.Radius, refSurface);
            //bricks.Add(brick);




            ////smooth
            //Curve smoothCurve;
            //if(courseXPoints.Count < 2) {
            //    Point3d[] pts = new Point3d[2];
            //    pts[0]  = curves[i].PointAtStart;
            //    pts[1] = curves[i].PointAtEnd;
            //    smoothCurve =  Curve.CreateInterpolatedCurve(pts, 1);
            //} else { smoothCurve = Curve.CreateInterpolatedCurve(courseXPoints, 1); }

            ////add brick
            //for(int k = 0; k < courseXPoints.Count; k++) {
            //    Brick brick = new Brick(courseXPoints[k], brickLengths[k], curves[i], course, k, refSurface);
            //    bricks.Add(brick);
            //}

            ////ends
            //if(lengthLeft < 0.0) {
            //    try {
            //        bricks[bricks.Count - 1].end = true;
            //        bricks[bricks.Count - 1].edgeLength = lengthLeft + sizeX;
            //    } catch { Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "brick.Count != courseXParameters.Count"); }
            //}


        }
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
            if(i % 2 == 0) { cuttingSurfaces0.Add(s); } else { cuttingSurfaces1.Add(s); }
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
        public Brick(Point3d _start, Point3d _next, double _radius, Vector3d _tangent, Surface _refSurface) {
            //(Point3d _center, double _brickLength, Curve _centerLine, int _courseZ, int _courseX, Surface _refSurface) {
            this.start = _start;
            double length = ( _next - _start ).Length;
            if(sizeX > length) { sizeX = length; }

            //make plane
            double s0, s1;
            Vector3d vecX, vecY, normal;
            _refSurface.ClosestPoint(this.start, out s0, out s1);
            normal = _refSurface.NormalAt(s0, s1);
            vecX = _next - _start;
            vecY = normal;
            this.plane = new Plane(this.start, vecX, vecY);

            this.radius = _radius;
            this.tangent = _tangent;
            this.endPt = _next;
            this.refNormal = normal;
        }

        //properties
        public Point3d start;
        public Point3d endPt;

        public double sizeX = 0.215;
        public double sizeY = 0.1025;
        public double sizeZ = 0.065;

        public double localRotation = 0.0;
        public double extraSpaceAfter = 0.0;
        public int courseZ;
        public int courseX;
        public int edge = 1;
        public bool end = false;
        public double length;
        public Plane plane;
        public double radius;
        public Vector3d tangent;
        public Vector3d refNormal;

        //methods
        public void setRotation(double degrees) {
            this.localRotation = degrees;
            if(this.courseZ % 2 == 1) {
                this.localRotation *= -1.0;
            }
        }
        public Brep draw() {

            if(this.radius < 3.500 && this.radius > 0.1025) {
                Curve[] curves = new Curve[2];
                curves[0] = new ArcCurve(new Arc(this.start, this.tangent, this.endPt)); ;
                curves[1] = new ArcCurve(new Arc(this.start, this.tangent, this.endPt)); ;
                Transform translate0 = Transform.Translation(this.refNormal*this.sizeY*0.5);
                Transform translate1 = Transform.Translation(this.refNormal*this.sizeY*-0.5);
                curves[0].Transform(translate0);
                curves[1].Transform(translate1);
                Brep b;
                b = Brep.CreateFromLoft(curves, Point3d.Unset, Point3d.Unset, LoftType.Straight, false)[0];
                b = Brep.CreateFromOffsetFace(b.Faces[0], 0.065, 0.001, false, true);


                return b;

            } else {





                Interval intervalX, intervalY, intervalZ;
                intervalX = new Interval(0.0, this.sizeX);
                intervalY = new Interval(-this.sizeY * 0.5, this.sizeY * 0.5);
                intervalZ = new Interval(0, this.sizeZ);

                Box box = new Box(this.plane, intervalX, intervalY, intervalZ);
                Transform xform = Transform.Rotation(( this.localRotation ) * Math.PI / 180, plane.ZAxis, box.Center);
                box.Transform(xform);

                return Brep.CreateFromBox(box);
            }
        }




    }
    #endregion












    // </Custom additional code> 
}