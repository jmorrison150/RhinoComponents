using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsd {
    
    class group {
        //Rhino.RhinoDoc doc;
        //void groupBake() {
        //    int groupIndex = -1;
        //    groupIndex = doc.Groups.Add();
        //    Bake(obj, att, groupIndex);


        //    double pWidth;
        //    int wires;
        //    att.PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject;
        //    att.PlotWeight = pWidth;
        //    att.WireDensity = wires;



        //    if (groupIndex != -1) {
        //        doc.Groups.AddToGroup(groupIndex, id);            }
        //}

        //void Bake(object obj, Rhino.DocObjects.ObjectAttributes att, int groupIndex) {
        //    if (obj == null)
        //        return;

        //    Guid id;

        //    //Bake to the right type of object
        //    if (obj is Rhino.Geometry.GeometryBase) {
        //        Rhino.Geometry.GeometryBase geomObj = obj as Rhino.Geometry.GeometryBase;

        //        switch (geomObj.ObjectType) {
        //            case Rhino.DocObjects.ObjectType.Brep:
        //                id = doc.Objects.AddBrep(obj as Rhino.Geometry.Brep, att);
        //                break;
        //            case Rhino.DocObjects.ObjectType.Curve:
        //                id = doc.Objects.AddCurve(obj as Rhino.Geometry.Curve, att);
        //                break;
        //            case Rhino.DocObjects.ObjectType.Point:
        //                id = doc.Objects.AddPoint((obj as Rhino.Geometry.Point).Location, att);
        //                break;
        //            case Rhino.DocObjects.ObjectType.Surface:
        //                id = doc.Objects.AddSurface(obj as Rhino.Geometry.Surface, att);
        //                break;
        //            case Rhino.DocObjects.ObjectType.Mesh:
        //                id = doc.Objects.AddMesh(obj as Rhino.Geometry.Mesh, att);
        //                break;
        //            case (Rhino.DocObjects.ObjectType)1073741824://Rhino.DocObjects.ObjectType.Extrusion:
        //                id = (Guid)typeof(Rhino.DocObjects.Tables.ObjectTable).InvokeMember("AddExtrusion", BindingFlags.Instance | BindingFlags.InvokeMethod, null, doc.Objects, new object[] { obj, att });
        //                break;
        //            case Rhino.DocObjects.ObjectType.PointSet:
        //                id = doc.Objects.AddPointCloud(obj as Rhino.Geometry.PointCloud, att); //This is a speculative entry
        //                break;
        //            default:
        //                Print("The script does not know how to handle this type of geometry: " + obj.GetType().FullName);
        //                return;
        //        }
        //    } else {
        //        Type objectType = obj.GetType();

        //        if (objectType == typeof(Arc)) {
        //            id = doc.Objects.AddArc((Arc)obj, att);
        //        } else if (objectType == typeof(Box)) {
        //            id = doc.Objects.AddBrep(((Box)obj).ToBrep(), att);
        //        } else if (objectType == typeof(Circle)) {
        //            id = doc.Objects.AddCircle((Circle)obj, att);
        //        } else if (objectType == typeof(Ellipse)) {
        //            id = doc.Objects.AddEllipse((Ellipse)obj, att);
        //        } else if (objectType == typeof(Polyline)) {
        //            id = doc.Objects.AddPolyline((Polyline)obj, att);
        //        } else if (objectType == typeof(Sphere)) {
        //            id = doc.Objects.AddSphere((Sphere)obj, att);
        //        } else if (objectType == typeof(Point3d)) {
        //            id = doc.Objects.AddPoint((Point3d)obj, att);
        //        } else if (objectType == typeof(Line)) {
        //            id = doc.Objects.AddLine((Line)obj, att);
        //        } else if (objectType == typeof(Vector3d)) {
        //            Print("Impossible to bake vectors");
        //            return;
        //        } else if (obj is IEnumerable) {
        //            int newGroupIndex;
        //            if (groupIndex == -1)
        //                newGroupIndex = doc.Groups.Add();
        //            else
        //                newGroupIndex = groupIndex;
        //            foreach (object o in obj as IEnumerable) {
        //                Bake(o, att, newGroupIndex);
        //            }
        //            return;
        //        } else {
        //            Print("Unhandled type: " + objectType.FullName);
        //            return;
        //        }
        //    }

        //    if (groupIndex != -1) {
        //        doc.Groups.AddToGroup(groupIndex, id);
        //        Print("Added " + obj.GetType().Name + " to group number " + groupIndex);
        //    } else
        //        Print("Added " + obj.GetType().Name);
        //}
    }
}
