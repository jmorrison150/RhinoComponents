using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

namespace _2087_Rome {
    class bake {

        RhinoDoc doc;
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

        void bakery(object obj, string layer, System.Drawing.Color color, int groupIndex, bool activate) {

            if(!activate) { return; }
            if(obj == null) { return; }

            //Guid id;



            //Make new attribute
            Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();

            ////Set object name
            //if (name != null && Rhino.DocObjects.Layer.IsValidName(name) == true) {
            //    att.Name = name;
            //}

            //Set layer
            if(layer != null && Rhino.DocObjects.Layer.IsValidName(layer) == true) {

                //Get the current layer index

                Rhino.DocObjects.Tables.LayerTable layerTable = doc.Layers;
                int layerIndex = layerTable.Find(layer, true);

                //if this layer does not exist, add it
                if(layerIndex == -1) {
                    //make new layer
                    Rhino.DocObjects.Layer myLayer = new Rhino.DocObjects.Layer();
                    myLayer.Name = layer;
                    myLayer.Color = color;
                    myLayer.PlotWeight = 0.0;
                    //Add the layer to the layer table
                    layerIndex = layerTable.Add(myLayer);
                }

                if(layerIndex > -1) {
                    //We manged to add layer!
                    att.LayerIndex = layerIndex;
                }




                //Set color
                if(color != null) {
                    att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromLayer;
                    att.ObjectColor = color;
                    att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromLayer;
                    att.PlotColor = color;
                    att.PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromLayer;
                }



                bake(obj, att, groupIndex);


            }
        }
        void bake(object obj, Rhino.DocObjects.ObjectAttributes att, int groupIndex) {
            if(obj == null) {
                return;
            }
            Guid id = new Guid();

            //Bake to the right type of object
            if(obj is GeometryBase) {
                GeometryBase geomObj = obj as GeometryBase;

                switch(geomObj.ObjectType) {
                    case Rhino.DocObjects.ObjectType.Brep:
                        id = doc.Objects.AddBrep(obj as Brep, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Curve:
                        id = doc.Objects.AddCurve(obj as Curve, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Point:
                        id = doc.Objects.AddPoint(( obj as Rhino.Geometry.Point ).Location, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Surface:
                        id = doc.Objects.AddSurface(obj as Surface, att);
                        break;
                    case Rhino.DocObjects.ObjectType.Mesh:
                        id = doc.Objects.AddMesh(obj as Mesh, att);
                        break;
                    case (Rhino.DocObjects.ObjectType) 1073741824://Rhino.DocObjects.ObjectType.Extrusion:
                        id = (Guid) typeof(Rhino.DocObjects.Tables.ObjectTable).InvokeMember("AddExtrusion", BindingFlags.Instance | BindingFlags.InvokeMethod, null, doc.Objects, new object[] { obj, att });
                        break;
                    case Rhino.DocObjects.ObjectType.PointSet:
                        id = doc.Objects.AddPointCloud(obj as Rhino.Geometry.PointCloud, att); //This is a speculative entry
                        break;
                    default:
                        //Print("The script does not know how to handle this type of geometry: " + obj.GetType().FullName);
                        return;
                }
            } else if(obj is IGH_GeometricGoo) {
                Type objectType = obj.GetType();

                if(objectType == typeof(GH_Box)) {
                    GH_Box box = obj as GH_Box;
                    box.BakeGeometry(doc, att, ref id);
                } else if(objectType == typeof(GH_Brep)) {
                    GH_Brep brep = obj as GH_Brep;
                    brep.BakeGeometry(doc, att, ref id);
                } else if(objectType == typeof(GH_Circle)) {
                    GH_Circle circle = obj as GH_Circle;
                    circle.BakeGeometry(doc, att, ref id);
                } else if(objectType == typeof(GH_Curve)) {
                    GH_Curve curve = obj as GH_Curve;
                    curve.BakeGeometry(doc, att, ref id);
                } else if(objectType == typeof(GH_Line)) {
                    GH_Line line = obj as GH_Line;
                    line.BakeGeometry(doc, att, ref id);
                } else if(objectType == typeof(GH_Mesh)) {
                    GH_Mesh mesh = obj as GH_Mesh;
                    mesh.BakeGeometry(doc, att, ref id);
                } else if(objectType == typeof(GH_MeshFace)) {
                    GH_MeshFace meshFace = obj as GH_MeshFace;
                    GH_Mesh mesh = null;
                    meshFace.CastTo<GH_Mesh>(ref mesh);
                    mesh.BakeGeometry(doc, att, ref id);
                } else if(objectType == typeof(GH_Point)) {
                    GH_Point pt = obj as GH_Point;
                    pt.BakeGeometry(doc, att, ref id);
                } else if(objectType == typeof(GH_Rectangle)) {
                    GH_Rectangle rect = obj as GH_Rectangle;
                    rect.BakeGeometry(doc, att, ref id);
                } else if(objectType == typeof(GH_Surface)) {
                    GH_Surface surf = obj as GH_Surface;
                    surf.BakeGeometry(doc, att, ref id);
                } else {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unhandled type: " + objectType.FullName);
                    return;
                }
            } else {
                Type objectType = obj.GetType();
                if(objectType == typeof(GH_Arc)) {
                    id = doc.Objects.AddArc((Arc) obj, att);
                }
                if(objectType == typeof(Arc)) {
                    id = doc.Objects.AddArc((Arc) obj, att);
                } else if(objectType == typeof(Box)) {
                    id = doc.Objects.AddBrep(( (Box) obj ).ToBrep(), att);
                } else if(objectType == typeof(Circle)) {
                    id = doc.Objects.AddCircle((Circle) obj, att);
                } else if(objectType == typeof(Ellipse)) {
                    id = doc.Objects.AddEllipse((Ellipse) obj, att);
                } else if(objectType == typeof(Polyline)) {
                    id = doc.Objects.AddPolyline((Polyline) obj, att);
                } else if(objectType == typeof(Sphere)) {
                    id = doc.Objects.AddSphere((Sphere) obj, att);
                } else if(objectType == typeof(Point3d)) {
                    id = doc.Objects.AddPoint((Point3d) obj, att);
                } else if(objectType == typeof(Line)) {
                    id = doc.Objects.AddLine((Line) obj, att);
                } else if(objectType == typeof(Vector3d)) {
                    //Print("Impossible to bake vectors");
                    return;
                } else if(obj is IEnumerable) {
                    int newGroupIndex;
                    if(groupIndex == -1)
                        newGroupIndex = doc.Groups.Add();
                    else
                        newGroupIndex = groupIndex;
                    foreach(object o in obj as IEnumerable) {
                        bake(o, att, newGroupIndex);
                    }
                    return;
                } else {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unhandled type: " + objectType.FullName);
                    return;
                }
            }

            if(groupIndex != -1) {
                doc.Groups.AddToGroup(groupIndex, id);
                //Print("Added " + obj.GetType().Name + " to group number " + groupIndex);
            } else {
                //Print("Added " + obj.GetType().Name);
            }
        }



    }
}
