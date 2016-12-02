﻿using Rhino;
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
public class Script_Instance3 : GH_ScriptInstance {
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
    private void RunScript(Brep srf, Mesh mesh, bool activate, ref object A) {
        AreaMassProperties area = AreaMassProperties.Compute(srf);
        Point3d pt = area.Centroid;
        PointCloud pc = new PointCloud(mesh.Vertices.ToPoint3dArray());
        int index = pc.ClosestPoint(pt);
        Color c = mesh.VertexColors[index];
        if(activate) { bake(srf, c); }

    }

    // <Custom additional code> 

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

            Rhino.DocObjects.Tables.LayerTable layerTable = RhinoDocument.Layers;
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
    void bake(object obj, System.Drawing.Color color) {
        if(obj == null) { return; }

        //Make new attribute
        Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();

        //Set color
        if(color != null) {
            att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
            att.ObjectColor = color;
            att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject;
            att.PlotColor = color;
            Material m= Material.DefaultMaterial;
            m.DiffuseColor =color;
            
            // materials are stored in the document's material table
            int index = doc.Materials.Add();
            Rhino.DocObjects.Material mat = doc.Materials[index];
            mat.DiffuseColor = System.Drawing.Color.Chocolate;
            mat.SpecularColor = System.Drawing.Color.CadetBlue;
            mat.CommitChanges();

            att.MaterialIndex=index;
        }


        bake(obj, att, -1);


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
                int newGroupIndex;
                if(groupIndex == -1)
                    newGroupIndex = RhinoDocument.Groups.Add();
                else
                    newGroupIndex = groupIndex;
                foreach(object o in obj as IEnumerable) {
                    bake(o, att, newGroupIndex);
                }
                return;
            } else {
                Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unhandled type: " + objectType.FullName);
                return;
            }
        }

        if(groupIndex != -1) {
            RhinoDocument.Groups.AddToGroup(groupIndex, id);
            //Print("Added " + obj.GetType().Name + " to group number " + groupIndex);
        } else {
            //Print("Added " + obj.GetType().Name);
        }
    }


    // </Custom additional code> 
}