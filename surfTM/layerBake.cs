using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;


namespace gsd {
    public class LayerBake : Grasshopper.Kernel.GH_Component {
        public LayerBake() : base(".layerBake", ".layerBake", "bakes to Layers", "Extra", "Util") { }
        public override Guid ComponentGuid {
            get { return new Guid("{B116F63D-D761-471A-AA05-4722DB5CCAFD}"); }
        }
        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                return gsd.Properties.Resources.gsd08;
            }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {

            pManager.AddGeometryParameter("Color3", "Score", "bakes geometry. This layer cuts first", GH_ParamAccess.tree);
            pManager.AddGeometryParameter("Color5", "Cut1", "bakes geometry", GH_ParamAccess.tree);
            pManager.AddGeometryParameter("Color6", "Cut2", "bakes geometry. This layer cuts last", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("bake", "bake", "turn on. adjust sliders. turn off.", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;


        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_GeometryParam("", "", "");
            pManager.Register_GeometryParam("", "", "");
            pManager.Register_GeometryParam("", "", "");
            pManager.Register_GeometryParam("", "", "");

        }




        Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;



        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {
            //check activate
            bool activate = false;
            DA.GetData<bool>(3, ref activate);

            //get input from grasshopper            
            Grasshopper.Kernel.Data.GH_Structure<IGH_GeometricGoo> geometries0 = new Grasshopper.Kernel.Data.GH_Structure<IGH_GeometricGoo>();
            Grasshopper.Kernel.Data.GH_Structure<IGH_GeometricGoo> geometries1 = new Grasshopper.Kernel.Data.GH_Structure<IGH_GeometricGoo>();
            Grasshopper.Kernel.Data.GH_Structure<IGH_GeometricGoo> geometries2 = new Grasshopper.Kernel.Data.GH_Structure<IGH_GeometricGoo>();

            DA.GetDataTree<IGH_GeometricGoo>(0, out geometries0);
            DA.GetDataTree<IGH_GeometricGoo>(1, out geometries1);
            DA.GetDataTree<IGH_GeometricGoo>(2, out geometries2);


            if(geometries0.DataCount > 0) {
                string layer0;
                layer0 = base.Params.Input[0].NickName;
                System.Drawing.Color green = System.Drawing.Color.FromArgb(0, 255, 0);
                for(int i = 0; i < geometries0.Branches.Count; i++) {
                    int groupIndex0 = -1;
                    groupIndex0 = doc.Groups.Add();
                    for(int j = 0; j < geometries0.Branches[i].Count; j++) {
                        bakery(geometries0.Branches[i][j], layer0, green, groupIndex0, activate);
                    }
                }
                //DA.SetDataTree(0, geometries0);
            }





            if(geometries1.DataCount > 0) {
                string layer1;
                layer1 = base.Params.Input[1].NickName;
                for(int i = 0; i < geometries1.Branches.Count; i++) {
                    int groupIndex1 = -1;
                    groupIndex1 = doc.Groups.Add();
                    for(int j = 0; j < geometries1.Branches[i].Count; j++) {
                        bakery(geometries1.Branches[i][j], layer1, System.Drawing.Color.Blue, groupIndex1, activate);
                    }
                }
                //DA.SetDataTree(0, geometries1);
            }




            if(geometries2.DataCount > 0) {
                string layer2;
                layer2 = base.Params.Input[2].NickName;
                for(int i = 0; i < geometries2.Branches.Count; i++) {
                    int groupIndex2 = -1;
                    groupIndex2 = doc.Groups.Add();
                    for(int j = 0; j < geometries2.Branches[i].Count; j++) {
                        bakery(geometries2.Branches[i][j], layer2, System.Drawing.Color.Magenta, groupIndex2, activate);
                    }
                }
                //DA.SetDataTree(0, geometries2);
            }











        }
        void bakeText(List<string> objs, List<Point3d> locations, double height, string layer, int groupIndex, bool activate) {
            System.Drawing.Color color = System.Drawing.Color.Green;


            //Inserts geometry into the Rhino document, with custom attributes
            //Written by Giulio Piacentino - rewritten by [uto] for GH 07
            //Version written 2010 09 27 - for Grasshopper 0.7.0055

            //obj is geometry
            if(objs == null) return;
            if(activate) {
                for(int i = 0; i < objs.Count; ++i) {
                    Plane plane = new Plane(locations[i], Vector3d.ZAxis);
                    Rhino.Display.Text3d obj = new Rhino.Display.Text3d(objs[i], plane, height);
                    //Make new attribute
                    Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
                    Guid o_id = default(Guid);

                    //Set object name
                    if(objs[i] != null && Rhino.DocObjects.Layer.IsValidName(objs[i]) == true) {
                        att.Name = objs[i];
                    }



                    //int currentLayerIndex = RhinoDocument.Layers.CurrentLayerIndex;
                    //att.LayerIndex = currentLayerIndex;
                    //Set layer
                    if(layer != null && Rhino.DocObjects.Layer.IsValidName(layer) == true) {
                        //Get the current layer index

                        Rhino.DocObjects.Tables.LayerTable layerTable = doc.Layers;
                        int layerIndex = layerTable.Find(layer, true);
                        if(layerIndex == -1) { //This layer does not exist, we add it
                            //make new layer
                            Rhino.DocObjects.Layer myLayer = new Rhino.DocObjects.Layer();
                            myLayer.Name = layer;
                            //Add the layer to the layer table
                            layerIndex = layerTable.Add(myLayer);

                            if(layerIndex > -1) { //We manged to add layer!
                                att.LayerIndex = layerIndex;
                                //__out.Add("Added new layer to the document at position " + layerIndex + " named " + layer + ". ");
                            } else {
                                //__out.Add("Layer did not add. Try cleaning up your layers."); //This never happened to me.
                            }
                        } else {
                            att.LayerIndex = layerIndex;
                            //__out.Add("Added to existing layer " + layerIndex + " named " + layer + ". ");
                        }
                    }


                    //Set color
                    if(color != null) {
                        att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
                        att.ObjectColor = color;
                        att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject;
                        att.PlotColor = color;
                    }

                    o_id = doc.Objects.AddText(obj, att);
                    if(groupIndex != -1) { doc.Groups.AddToGroup(groupIndex, o_id); }

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

            att.WireDensity = -1;

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




