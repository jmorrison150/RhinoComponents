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
    private void Print(string text) { __out.Add(text); }
    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
    /// <param name="format">String format.</param>
    /// <param name="args">Formatting parameters.</param>
    private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
    #endregion

    #region Members
    /// <summary>Gets the current Rhino document.</summary>
    private RhinoDoc RhinoDocument;
    /// <summary>Gets the Grasshopper document that owns this script.</summary>
    private GH_Document GrasshopperDocument;
    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
    private IGH_Component Component;
    /// <summary>
    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
    /// Any subsequent call within the same solution will increment the Iteration count.
    /// </summary>
    private int Iteration;
    #endregion

    /// <summary>
    /// This procedure contains the user code. Input parameters are provided as regular arguments,
    /// Output parameters as ref arguments. You don't have to assign output parameters,
    /// they will have a default value.
    /// </summary>
    private void RunScript(List<System.Object> objs, List<string> names, List<string> layers, List<Color> colors, List<double> pWidths, List<int> wiresEach, List<System.Object> materials, bool activate, bool groupListTgthr) {

        //Inserts geometry into the Rhino document, with custom attributes
        //Written by Giulio Piacentino
        //Version written 2011 10 18 - for Grasshopper 0.8.52

        if (activate) {
            int groupIndex = -1;
            if (groupListTgthr)
                groupIndex = doc.Groups.Add();

            for (int i = 0; i < objs.Count; i++) {
                object obj = objs[i];
                if (obj == null) {
                    Print("No object to bake");
                    return;
                }

                //Make new attribute to set name
                Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();

                string name = names.Count > 0 ? names[i % names.Count] : null;
                //Set object name
                if (!string.IsNullOrEmpty(name)) {
                    att.Name = name;
                }

                Color color = colors.Count > 0 ? colors[i % colors.Count] : new Color();
                //Set color
                if (!color.IsEmpty) {
                    att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject; //Make the color type "by object"
                    att.ObjectColor = color;

                    att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject; //Make the plot color type "by object"
                    att.PlotColor = color;
                }

                string layer = layers.Count > 0 ? layers[i % layers.Count] : null;
                //Set layer
                if (!string.IsNullOrEmpty(layer) && Rhino.DocObjects.Layer.IsValidName(layer)) {
                    //Get the current layer index
                    Rhino.DocObjects.Tables.LayerTable layerTable = doc.Layers;
                    int layerIndex = layerTable.Find(layer, true);

                    if (layerIndex < 0) //This layer does not exist, we add it
          {
                        Rhino.DocObjects.Layer onlayer = new Rhino.DocObjects.Layer(); //Make a new layer
                        onlayer.Name = layer;

                        layerIndex = layerTable.Add(onlayer); //Add the layer to the layer table
                        if (layerIndex > -1) //We manged to add layer!
            {
                            att.LayerIndex = layerIndex;
                            Print("Added new layer to the document at position " + layerIndex + " named " + layer + ". ");
                        } else
                            Print("Layer did not add. Try cleaning up your layers."); //This never happened to me.
                    } else
                        att.LayerIndex = layerIndex; //We simply add to the existing layer
                }


                double pWidth = pWidths.Count > 0 ? pWidths[i % pWidths.Count] : 0;
                //Set plotweight
                if (pWidth > 0) {
                    att.PlotWeightSource = Rhino.DocObjects.ObjectPlotWeightSource.PlotWeightFromObject;
                    att.PlotWeight = pWidth;
                }


                object material = materials.Count > 0 ? materials[i % materials.Count] : null;
                //Set material
                bool materialByName = !string.IsNullOrEmpty(material as string);
                Rhino.Display.DisplayMaterial inMaterial = material as Rhino.Display.DisplayMaterial;
                if (material is Color) {
                    inMaterial = new Rhino.Display.DisplayMaterial((Color)material);
                }
                if (material != null && inMaterial == null && !materialByName) {
                    if (!(material is string)) {
                        try //We also resort to try with IConvertible
                        {
                            inMaterial = (Rhino.Display.DisplayMaterial)Convert.ChangeType(material, typeof(Rhino.Display.DisplayMaterial));
                        } catch (InvalidCastException) {
                        }
                    }
                }
                if (inMaterial != null || materialByName) {
                    string matName;

                    if (!materialByName) {
                        matName = string.Format("A:{0}-D:{1}-E:{2}-S:{3},{4}-T:{5}",
                          Format(inMaterial.Ambient),
                          Format(inMaterial.Diffuse),
                          Format(inMaterial.Emission),
                          Format(inMaterial.Specular),
                          inMaterial.Shine.ToString(),
                          inMaterial.Transparency.ToString()
                          );
                    } else {
                        matName = (string)material;
                    }

                    int materialIndex = doc.Materials.Find(matName, true);
                    if (materialIndex < 0 && !materialByName) //Material does not exist and we have its specs
          {
                        materialIndex = doc.Materials.Add(); //Let's add it
                        if (materialIndex > -1) {
                            Print("Added new material at position " + materialIndex + " named \"" + matName + "\". ");
                            Rhino.DocObjects.Material m = doc.Materials[materialIndex];
                            m.Name = matName;
                            m.AmbientColor = inMaterial.Ambient;
                            m.DiffuseColor = inMaterial.Diffuse;
                            m.EmissionColor = inMaterial.Emission;
                            //m.ReflectionColor = no equivalent
                            m.SpecularColor = inMaterial.Specular;
                            m.Shine = inMaterial.Shine;
                            m.Transparency = inMaterial.Transparency;
                            //m.TransparentColor = no equivalent
                            m.CommitChanges();

                            att.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;
                            att.MaterialIndex = materialIndex;
                        } else
                            Print("Material did not add. Try cleaning up your materials."); //This never happened to me.
                    } else if (materialIndex < 0 && materialByName) //Material does not exist and we do not have its specs. We do nothing
          {
                        Print("Warning: material name not found. I cannot set the source to this material name. Add a material with name: " + matName);
                    } else {
                        //If this material exists, we do not replace it!
                        att.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;
                        att.MaterialIndex = materialIndex;
                    }
                }


                int wires = wiresEach.Count > 0 ? wiresEach[i % wiresEach.Count] : 0;
                //Set wire density
                if (wires == -1 || wires > 0) {
                    att.WireDensity = wires;
                }

                Bake(obj, att, groupIndex);
            }
        } else {
            Print("Inactive");
        }

        NurbsSurface ns;
        NurbsSurface.


    }

    // <Custom additional code> 

    void Bake(object obj, Rhino.DocObjects.ObjectAttributes att, int groupIndex) {
        if (obj == null)
            return;

        Guid id;

        //Bake to the right type of object
        if (obj is GeometryBase) {
            GeometryBase geomObj = obj as GeometryBase;

            switch (geomObj.ObjectType) {
                case Rhino.DocObjects.ObjectType.Brep:
                    id = doc.Objects.AddBrep(obj as Brep, att);
                    break;
                case Rhino.DocObjects.ObjectType.Curve:
                    id = doc.Objects.AddCurve(obj as Curve, att);
                    break;
                case Rhino.DocObjects.ObjectType.Point:
                    id = doc.Objects.AddPoint((obj as Rhino.Geometry.Point).Location, att);
                    break;
                case Rhino.DocObjects.ObjectType.Surface:
                    id = doc.Objects.AddSurface(obj as Surface, att);
                    break;
                case Rhino.DocObjects.ObjectType.Mesh:
                    id = doc.Objects.AddMesh(obj as Mesh, att);
                    break;
                case (Rhino.DocObjects.ObjectType)1073741824://Rhino.DocObjects.ObjectType.Extrusion:
                    id = (Guid)typeof(Rhino.DocObjects.Tables.ObjectTable).InvokeMember("AddExtrusion", BindingFlags.Instance | BindingFlags.InvokeMethod, null, doc.Objects, new object[] { obj, att });
                    break;
                case Rhino.DocObjects.ObjectType.PointSet:
                    id = doc.Objects.AddPointCloud(obj as Rhino.Geometry.PointCloud, att); //This is a speculative entry
                    break;
                default:
                    Print("The script does not know how to handle this type of geometry: " + obj.GetType().FullName);
                    return;
            }
        } else {
            Type objectType = obj.GetType();

            if (objectType == typeof(Arc)) {
                id = doc.Objects.AddArc((Arc)obj, att);
            } else if (objectType == typeof(Box)) {
                id = doc.Objects.AddBrep(((Box)obj).ToBrep(), att);
            } else if (objectType == typeof(Circle)) {
                id = doc.Objects.AddCircle((Circle)obj, att);
            } else if (objectType == typeof(Ellipse)) {
                id = doc.Objects.AddEllipse((Ellipse)obj, att);
            } else if (objectType == typeof(Polyline)) {
                id = doc.Objects.AddPolyline((Polyline)obj, att);
            } else if (objectType == typeof(Sphere)) {
                id = doc.Objects.AddSphere((Sphere)obj, att);
            } else if (objectType == typeof(Point3d)) {
                id = doc.Objects.AddPoint((Point3d)obj, att);
            } else if (objectType == typeof(Line)) {
                id = doc.Objects.AddLine((Line)obj, att);
            } else if (objectType == typeof(Vector3d)) {
                Print("Impossible to bake vectors");
                return;
            } else if (obj is IEnumerable) {
                int newGroupIndex;
                if (groupIndex == -1)
                    newGroupIndex = doc.Groups.Add();
                else
                    newGroupIndex = groupIndex;
                foreach (object o in obj as IEnumerable) {
                    Bake(o, att, newGroupIndex);
                }
                return;
            } else {
                Print("Unhandled type: " + objectType.FullName);
                return;
            }
        }

        if (groupIndex != -1) {
            doc.Groups.AddToGroup(groupIndex, id);
            Print("Added " + obj.GetType().Name + " to group number " + groupIndex);
        } else
            Print("Added " + obj.GetType().Name);
    }

    static string Format(Color c) {
        return (new System.Text.StringBuilder("A")).Append(c.A.ToString()).Append("R").Append(c.R.ToString()).Append("G")
          .Append(c.G.ToString()).Append("B").Append(c.B.ToString()).ToString();
    }

    // </Custom additional code> 

    private List<string> __err = new List<string>(); //Do not modify this list directly.
    private List<string> __out = new List<string>(); //Do not modify this list directly.
    private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
    private IGH_ActiveObject owner;                  //Legacy field.
    private int runCount;                            //Legacy field.

}