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
public class visualPermeability : GH_ScriptInstance {
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
    private void RunScript(List<Rectangle3d> rectangles, 
      List<Curve> obstacles, List<Surface> axis, 
      ref object outMesh, ref object outLines) {


        #region beginScript
        double resolution = 0.100;
        double viewRadius = 20.000;

        double scalar = 100.0;
        bool useCustomAxis = true;
        List<Mesh> meshes = new List<Mesh>();
        Line[][][] lines3 = new Line[rectangles.Count][][];
        for(int i = 0; i < rectangles.Count; i++) {
            Mesh mesh = meshPlane(rectangles[i], resolution);
            if(useCustomAxis) {
                mesh = computeColors(mesh, obstacles, viewRadius, axis, scalar, out lines3[i]);
            } else {
                int accuracy = 10;
                mesh = computeColors(mesh, obstacles, viewRadius, accuracy, out lines3[i]);
            }
            meshes.Add(mesh);
            //bake(mesh);

        }



        outMesh = meshes;
        //outLines = lines3;
        #endregion
        //    for (int i = 0; i < points.Count; i++)
        //    {
        //      V = viewDirections(points[i], axis0, axis1, axis2, axis3, axis4);
        //    }


        //    if(save) {
        //      for(int i = 0; i < x.Count; i++) {      bake(x[i]);
        //
        //
        //        string path = RhinoDocument.Path;
        //        path = @"C:\Users\JM\Desktop\01.3dm";
        //        Rhino.FileIO.FileWriteOptions options = new Rhino.FileIO.FileWriteOptions();
        //        options.SuppressDialogBoxes = true;
        //        options.WriteGeometryOnly = true;
        //        bool saved = RhinoDocument.WriteFile(path, options);
        //        Print(saved.ToString());
        //      }
        //    }
    }

    // <Custom additional code> 


    #region customCode
    Mesh computeColors(Mesh mesh, List<Curve> obstacles, double viewRadius, List<Surface> axis, double scalar, out Line[][] outLines) {

        int accuracy = axis.Count * 2;
        Line[][] updateLines = new Line[mesh.Vertices.Count][];
        for(int i = 0; i < updateLines.Length; i++) {
            updateLines[i] = new Line[accuracy];
        }
        Color[] colors = new Color[mesh.Vertices.Count];

        //check for divide by zero
        if(viewRadius <= 0) { viewRadius = 0.001; }
        if(accuracy < 1) { accuracy = 1; }






        for(int p = 0; p < mesh.Vertices.Count; p++) {



            Point3d pt = mesh.Vertices[p];

            Vector3d[] viewDirections = getViewDirections(pt, axis, viewRadius);
            Point3d[] intersectionPts = new Point3d[viewDirections.Length];
            double currentMaxDistance = double.MaxValue;

            for(int j = 0; j < viewDirections.Length; j++) {
                Line line = new Line(pt, viewDirections[j]);
                intersectionPts[j] = line.To;
                currentMaxDistance = line.Length;



                for(int i = 0; i < obstacles.Count; i++) {
                    //cheap bounding box test
                    BoundingBox bbox = obstacles[i].GetBoundingBox(false);
                    Point3d closestPt = bbox.ClosestPoint(pt);
                    double dist = pt.DistanceTo(closestPt);
                    if(dist > viewRadius) { continue; }

                    //expensive curve intersection test
                    LineCurve line1 = new LineCurve(line);
                    Rhino.Geometry.Intersect.CurveIntersections crvIntersections = Rhino.Geometry.Intersect.Intersection.CurveCurve(line1, obstacles[i], 0.001, 0.001);
                    for(int k = 0; k < crvIntersections.Count; k++) {
                        double distance = pt.DistanceTo(crvIntersections[k].PointA);
                        if(currentMaxDistance > distance) {
                            intersectionPts[j] = crvIntersections[k].PointA;
                            currentMaxDistance = distance;
                        }
                    }
                }

                //double dist0 = pt.DistanceTo(intersectionPts[j]);
                //if(dist0 > threshold){
                Line l = new Line(pt, intersectionPts[j]);
                updateLines[p][j] = l;
                //}

            }

            double totalLength = 0.0;
            for(int k = 0; k < updateLines[p].Length; k++) {
                double length = updateLines[p][k].Length;
                totalLength += length;
            }
            totalLength *= scalar;

            Color color = Color.FromArgb((int) totalLength);
            colors[p] = color;

        }


        mesh.VertexColors.CreateMonotoneMesh(Color.FromArgb(0));
        mesh.VertexColors.SetColors(colors);

        outLines = updateLines;
        return mesh;
    }
    Mesh computeColors(Mesh mesh, List<Curve> obstacles, double viewRadius, int accuracy, out Line[][] outLines) {

        Line[][] updateLines = new Line[mesh.Vertices.Count][];
        for(int i = 0; i < updateLines.Length; i++) {
            updateLines[i] = new Line[accuracy];
        }
        Color[] colors = new Color[mesh.Vertices.Count];

        //check for divide by zero
        if(viewRadius <= 0) { viewRadius = 0.001; }
        if(accuracy < 1) { accuracy = 1; }

        //preCalculate for 3D
        Sphere sphere = new Sphere(Point3d.Origin, viewRadius);
        Mesh meshSphere = Mesh.CreateFromSphere(sphere, accuracy, 2);
        Point3d[] meshPts = meshSphere.Vertices.ToPoint3dArray();
        //preCalculate for 2D
        Circle circle = new Circle(Point3d.Origin, viewRadius);
        Point3d[] circlePts;
        circle.ToNurbsCurve().DivideByCount(accuracy, true, out circlePts);
        Vector3d[] viewDirections = new Vector3d[circlePts.Length];
        for(int j = 0; j < viewDirections.Length; j++) {
            Vector3d vec = new Vector3d(circlePts[j]);
            vec.Unitize();
            vec *= viewRadius;
            viewDirections[j] = vec;
        }




        for(int p = 0; p < mesh.Vertices.Count; p++) {



            // System.Threading.Tasks.Parallel.For(0, mesh.Vertices.Count, p => {
            Point3d pt = mesh.Vertices[p];
            Point3d[] intersectionPts = new Point3d[viewDirections.Length];
            double currentMaxDistance = double.MaxValue;

            for(int j = 0; j < viewDirections.Length; j++) {
                Line line = new Line(pt, viewDirections[j]);
                intersectionPts[j] = line.To;
                currentMaxDistance = line.Length;



                for(int i = 0; i < obstacles.Count; i++) {
                    //cheap bounding box test
                    BoundingBox bbox = obstacles[i].GetBoundingBox(false);
                    Point3d closestPt = bbox.ClosestPoint(pt);
                    double dist = pt.DistanceTo(closestPt);
                    if(dist > viewRadius) { continue; }

                    //expensive curve intersection test
                    LineCurve line1 = new LineCurve(line);
                    Rhino.Geometry.Intersect.CurveIntersections crvIntersections = Rhino.Geometry.Intersect.Intersection.CurveCurve(line1, obstacles[i], 0.001, 0.001);
                    for(int k = 0; k < crvIntersections.Count; k++) {
                        double distance = pt.DistanceTo(crvIntersections[k].PointA);
                        if(currentMaxDistance > distance) {
                            intersectionPts[j] = crvIntersections[k].PointA;
                            currentMaxDistance = distance;
                        }
                    }
                }

                //double dist0 = pt.DistanceTo(intersectionPts[j]);
                //if(dist0 > threshold){
                Line l = new Line(pt, intersectionPts[j]);
                updateLines[p][j] = l;
                //}

            }

            double totalLength = 0.0;
            for(int k = 0; k < updateLines[p].Length; k++) {
                double length = updateLines[p][k].Length;
                totalLength += length;
            }

            //Polyline polyline = new Polyline(intersectionPts);
            //polyline.Add(polyline[0]);
            //PolylineCurve plCurve = new PolylineCurve(polyline);
            //plCurve.MakeClosed(0.100);
            //AreaMassProperties areaMass = AreaMassProperties.Compute(plCurve, 0.001);
            //double area = areaMass.Area;

            Color color = Color.FromArgb((int) totalLength);
            colors[p] = color;

            // });//end Parallel.For
        }


        mesh.VertexColors.CreateMonotoneMesh(Color.FromArgb(0));
        mesh.VertexColors.SetColors(colors);

        outLines = updateLines;
        return mesh;
    }
    Vector3d[] getViewDirections(Point3d point, List<Surface> axis, double viewRadius) {
        List<Vector3d> vecs = new List<Vector3d>();

        for(int i = 0; i < axis.Count; i++) {
            Vector3d vec = isoVec(axis[i], point);
            vec.Unitize();
            vec *= viewRadius;
            vecs.Add(vec);
            vecs.Add(vec * -1.0);
        }

        return vecs.ToArray();
    }
    Curve iso(Surface axis, Point3d point) {
        Curve crv;
        double u, v;
        axis.ClosestPoint(point, out u, out v);
        Point3d pt = axis.PointAt(u, v);
        crv = axis.IsoCurve(1, u);

        return crv;
    }
    Vector3d isoVec(Surface axis, Point3d point) {
        Vector3d vec;

        double u, v;
        axis.ClosestPoint(point, out u, out v);
        Point3d pt = axis.PointAt(u, v);
        Curve crv;
        crv = axis.IsoCurve(1, u);
        vec = crv.TangentAt(v);

        return vec;
    }
    Mesh meshPlane(Rectangle3d rectangle, double resolution) {
        int xCount = (int) ( rectangle.Width / resolution );
        int yCount = (int) ( rectangle.Height / resolution );
        Mesh mesh = Mesh.CreateFromPlane(rectangle.Plane, rectangle.X, rectangle.Y, xCount, yCount);
        return mesh;
    }
    double map(double value1, double min1, double max1, double min2, double max2) {
        double value2 = min2 + ( value1 - min1 ) * ( max2 - min2 ) / ( max1 - min1 );
        return value2;
    }
    void bake(object obj) {
        if(obj == null) {
            return;
        }

        Guid id = new Guid();
        Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();

        //Bake to the right type of object
        if(obj is GeometryBase) {
            GeometryBase geomObj = obj as GeometryBase;

            switch(geomObj.ObjectType) {
                case Rhino.DocObjects.ObjectType.Brep:
                    id = RhinoDocument.Objects.AddBrep(obj as Brep, att);
                    break;
                case Rhino.DocObjects.ObjectType.Curve:
                    id = RhinoDocument.Objects.AddCurve(obj as Curve, att);
                    break;
                case Rhino.DocObjects.ObjectType.Point:
                    id = RhinoDocument.Objects.AddPoint(( obj as Rhino.Geometry.Point ).Location, att);
                    break;
                case Rhino.DocObjects.ObjectType.Surface:
                    id = RhinoDocument.Objects.AddSurface(obj as Surface, att);
                    break;
                case Rhino.DocObjects.ObjectType.Mesh:
                    id = RhinoDocument.Objects.AddMesh(obj as Mesh, att);
                    break;
                case (Rhino.DocObjects.ObjectType) 1073741824://Rhino.DocObjects.ObjectType.Extrusion:
                    id = (Guid) typeof(Rhino.DocObjects.Tables.ObjectTable).InvokeMember("AddExtrusion", BindingFlags.Instance | BindingFlags.InvokeMethod, null, RhinoDocument.Objects, new object[] { obj, att });
                    break;
                case Rhino.DocObjects.ObjectType.PointSet:
                    id = RhinoDocument.Objects.AddPointCloud(obj as Rhino.Geometry.PointCloud, att); //This is a speculative entry
                    break;
                default:
                    //Print("The script does not know how to handle this type of geometry: " + obj.GetType().FullName);
                    return;
            }
        } else if(obj is IGH_GeometricGoo) {
            Type objectType = obj.GetType();

            if(objectType == typeof(GH_Box)) {
                GH_Box box = obj as GH_Box;
                box.BakeGeometry(RhinoDocument, att, ref id);
            } else if(objectType == typeof(GH_Brep)) {
                GH_Brep brep = obj as GH_Brep;
                brep.BakeGeometry(RhinoDocument, att, ref id);
            } else if(objectType == typeof(GH_Circle)) {
                GH_Circle circle = obj as GH_Circle;
                circle.BakeGeometry(RhinoDocument, att, ref id);
            } else if(objectType == typeof(GH_Curve)) {
                GH_Curve curve = obj as GH_Curve;
                curve.BakeGeometry(RhinoDocument, att, ref id);
            } else if(objectType == typeof(GH_Line)) {
                GH_Line line = obj as GH_Line;
                line.BakeGeometry(RhinoDocument, att, ref id);
            } else if(objectType == typeof(GH_Mesh)) {
                GH_Mesh mesh = obj as GH_Mesh;
                mesh.BakeGeometry(RhinoDocument, att, ref id);
            } else if(objectType == typeof(GH_MeshFace)) {
                GH_MeshFace meshFace = obj as GH_MeshFace;
                GH_Mesh mesh = null;
                meshFace.CastTo<GH_Mesh>(ref mesh);
                mesh.BakeGeometry(RhinoDocument, att, ref id);
            } else if(objectType == typeof(GH_Point)) {
                GH_Point pt = obj as GH_Point;
                pt.BakeGeometry(RhinoDocument, att, ref id);
            } else if(objectType == typeof(GH_Rectangle)) {
                GH_Rectangle rect = obj as GH_Rectangle;
                rect.BakeGeometry(RhinoDocument, att, ref id);
            } else if(objectType == typeof(GH_Surface)) {
                GH_Surface surf = obj as GH_Surface;
                surf.BakeGeometry(RhinoDocument, att, ref id);
            } else {
                Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unhandled type: " + objectType.FullName);
                return;
            }
        } else {
            Type objectType = obj.GetType();
            if(objectType == typeof(GH_Arc)) {
                id = RhinoDocument.Objects.AddArc((Arc) obj, att);
            }
            if(objectType == typeof(Arc)) {
                id = RhinoDocument.Objects.AddArc((Arc) obj, att);
            } else if(objectType == typeof(Box)) {
                id = RhinoDocument.Objects.AddBrep(( (Box) obj ).ToBrep(), att);
            } else if(objectType == typeof(Circle)) {
                id = RhinoDocument.Objects.AddCircle((Circle) obj, att);
            } else if(objectType == typeof(Ellipse)) {
                id = RhinoDocument.Objects.AddEllipse((Ellipse) obj, att);
            } else if(objectType == typeof(Polyline)) {
                id = RhinoDocument.Objects.AddPolyline((Polyline) obj, att);
            } else if(objectType == typeof(Sphere)) {
                id = RhinoDocument.Objects.AddSphere((Sphere) obj, att);
            } else if(objectType == typeof(Point3d)) {
                id = RhinoDocument.Objects.AddPoint((Point3d) obj, att);
            } else if(objectType == typeof(Line)) {
                id = RhinoDocument.Objects.AddLine((Line) obj, att);
            } else if(objectType == typeof(Vector3d)) {
                //Print("Impossible to bake vectors");
                return;
            } else if(obj is IEnumerable) {
                //int newGroupIndex;
                //    newGroupIndex = RhinoDocument.Groups.Add();
                foreach(object o in obj as IEnumerable) {
                    bake(o);
                }
                return;
            } else {
                Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unhandled type: " + objectType.FullName);
                return;
            }
        }

    }
    #endregion


    // </Custom additional code> 
}