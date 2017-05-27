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
    public class BakeCount : Grasshopper.Kernel.GH_Component {
        public BakeCount() : base(".bakeCount", ".bakeCount", "an EZ bake oven", "Extra", "Util") { }
        public override Guid ComponentGuid {
            get { return new Guid("{A046BB31-E980-4C24-A68E-CED2C7A2FA10}"); }
        }
        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                return gsd.Properties.Resources.gsd12;
            }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {

            pManager.AddGeometryParameter("geom", "geom", "geometry", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("bake", "bake", "creates a numbered layer, and bakes the design option to that layer", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("reset", "reset", "reset the number to zero", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_GenericParam("", "", "");
        }




        Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
        int bakeCount = 0;
        //double bakeDistance = 0.0;
        //List<Curve> updateCurves;
        //List<IGH_GeometricGoo> updateGeom;
        //List<string> outTexts;
        //List<Point3d> outLocations;
        //double outSize = 0.0;


        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {
            //check activate
            bool activate = false;
            DA.GetData<bool>(1, ref activate);
            bool reset = false;
            DA.GetData<bool>(2,ref reset);
            
            if(reset) { bakeCount =0; }
            if(!activate) { return; }

                Grasshopper.Kernel.Data.GH_Structure<IGH_GeometricGoo> tree = new Grasshopper.Kernel.Data.GH_Structure<IGH_GeometricGoo>();
                DA.GetDataTree<Grasshopper.Kernel.Types.IGH_GeometricGoo>(0, out tree);


                #region beginScript
                //format the data tree
                IGH_GeometricGoo[][] geometries = new IGH_GeometricGoo[tree.Branches.Count][];
                for (int i = 0; i < geometries.Length; ++i) {
                    geometries[i] = new IGH_GeometricGoo[tree.Branches[i].Count];
                    for (int j = 0; j < geometries[i].Length; ++j) {
                        geometries[i][j] = tree.Branches[i][j];
                    }
                }//end format data tree




                //make empty lists
                //outTexts = new List<string>();
                //outLocations = new List<Point3d>();



                //bounding box union
                BoundingBox geoBox = BoundingBox.Empty;
                for (int i = 0; i < geometries.Length; ++i) {
                    for (int j = 0; j < geometries[i].Length; ++j) {
                        BoundingBox bb = geometries[i][j].GetBoundingBox(Transform.Identity);
                        geoBox.Union(bb);
                    }
                }

                //compute bounding box
                //double dx = (geoBox.Max.X - geoBox.Min.X) * 1.2;
                //try changing dx to a constant to make a regular grid
                //dx=20;
                //if (dx < 5.0) { dx = 5.0; }
                //outSize = dx / 20;
                //double translateX = 0.0;

            

                //get slider info
                //for (int i = 0; i < sliders.Count; ++i) {
                //    Grasshopper.Kernel.Special.GH_NumberSlider ghSlider;
                //    ghSlider = findSlider(sliders[i]);

                //    if (ghSlider != null) {
                //        decimal sliderValue = ghSlider.Slider.Value;
                //        outTexts.Add(sliders[i] + " = " + sliderValue.ToString());

                //        Point3d location = new Point3d(geoBox.Min.X + (translateX), geoBox.Min.Y - ((i + 1) * dx / 10), geoBox.Min.Z);
                //        outLocations.Add(location);
                //    } else { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,"make sure each slider has a unique name"); }
                //}


            //string[] bakeLayers  =new string[geometries.Length];
            //for (int i = 0; i < bakeLayers.Length; i++) {
            //    bakeLayers[i] = layers[i%layers.Count];
            //}
            //System.Drawing.Color[] colors = new System.Drawing.Color[geometries.Length];
            //for (int i = 0; i < colors.Length; i++) {
            //    colors[i] = System.Drawing.Color.Cyan;
            //}
            
                    //bakery
                for (int i = 0; i < geometries.Length; ++i) {
                    //updateGeom = new List<IGH_GeometricGoo>();
                    int groupIndex = -1;
                    groupIndex = doc.Groups.Add();
                    string bakeLayer = string.Format("{0:00}",bakeCount);







                    for (int j = 0; j < geometries[i].Length; ++j) {
                        IGH_GeometricGoo c = geometries[i][j].DuplicateGeometry();
                        //activate this line to auto move the geometry
                        //Transform move = Transform.Translation(translateX, 0, 0);
                        //c.Transform(move);
                        //updateGeom.Add(c);
                        //bake
                        bakery(c, bakeLayer, groupIndex, activate);
                    }
                //bakeText(outTexts, outLocations, outSize, "a_Score", groupIndex,activate);

                //outTexts.Clear();
                //outLocations.Clear();

   
                }

                bakeCount++;


            
            
                #endregion



            

        }
        public override void DrawViewportMeshes(IGH_PreviewArgs args) {     //Draw all lines
            //for (int i = 0; i < outTexts.Count; ++i) {
            //    Plane plane = new Plane(outLocations[i], Vector3d.ZAxis);
            //    string fontface = "Arial";
            //    args.Display.Draw3dText(outTexts[i], Color.Gray, plane, outSize, fontface);
            //}
            //for (int i = 0; i < updateCurves.Count; ++i) {
            //    Curve c = updateCurves[i];
            //    args.Display.DrawCurve(c, Color.Cyan);
            //}
        }

        public Grasshopper.Kernel.Special.GH_NumberSlider findSlider(string name) {
            //Get the document that owns this object.
            GH_Document ghDoc = this.OnPingDocument();
            //owner.OnPingDocument();
            //Abort if no such document can be found.
            if (ghDoc == null) { return null; }

            //Iterate over all objects inside the document.
            for (int i = 0; i < ghDoc.ObjectCount; i++) {
                IGH_DocumentObject obj = ghDoc.Objects[i];
                //First test the NickName of the object against the search name.
                if (obj.NickName.Equals(name, StringComparison.Ordinal)) {
                    //Then try to cast the object to a GH_NumberSlider.
                    Grasshopper.Kernel.Special.GH_NumberSlider sld_obj = obj as Grasshopper.Kernel.Special.GH_NumberSlider;
                    if (sld_obj != null) { return sld_obj; }
                }
            }
            return null;
        }
        //void bakery(List<IGH_GeometricGoo> objs, string layer, System.Drawing.Color color, bool activate) {
        //  //Inserts geometry into the Rhino document, with custom attributes
        //  //Written by Giulio Piacentino - rewritten by [uto] for GH 07
        //  //Version written 2010 09 27 - for Grasshopper 0.7.0055

        //  //obj is geometry
        //  if (objs == null) return;
        //  if (activate) {
        //    for (int i = 0; i < objs.Count; ++i) {
        //      Curve obj = objs[i];
        //      //Make new attribute
        //      Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
        //      Guid o_id = default(Guid);

        //      ////Set object name
        //      //if (name != null && Rhino.DocObjects.Layer.IsValidName(name) == true) {
        //      //    att.Name = name;
        //      //}

        //      //Set layer
        //      if (layer != null && Rhino.DocObjects.Layer.IsValidName(layer) == true) {
        //        //Get the current layer index

        //        Rhino.DocObjects.Tables.LayerTable layerTable = doc.Layers;
        //        int layerIndex = layerTable.Find(layer, true);
        //        if (layerIndex == -1) { //This layer does not exist, we add it
        //          //make new layer
        //          Rhino.DocObjects.Layer myLayer = new Rhino.DocObjects.Layer();
        //          myLayer.Name = layer;
        //          //Add the layer to the layer table
        //          layerIndex = layerTable.Add(myLayer);
        //        }

        //        if (layerIndex > -1) { //We manged to add layer!
        //          att.LayerIndex = layerIndex;
        //          //__out.Add("Added new layer to the document at position " + layerIndex + " named " + layer + ". ");
        //        }
        //        //else {
        //        //att.LayerIndex = layerIndex;
        //        ////__out.Add("Added to existing layer " + layerIndex + " named " + layer + ". ");
        //        //}

        //      }

        //      //Set color
        //      if (color != null) {
        //        att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromLayer;
        //        att.ObjectColor = color;
        //        att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromLayer;
        //        att.PlotColor = color;
        //      }

        //      o_id = doc.Objects.AddCurve((Curve) obj, att);

        //    }
        //  }
        //}
        void bakeText(List<string> objs, List<Point3d> locations, double height, string layer, int groupIndex, bool activate) {
          System.Drawing.Color color = System.Drawing.Color.Green;
            
            
            //Inserts geometry into the Rhino document, with custom attributes
          //Written by Giulio Piacentino - rewritten by [uto] for GH 07
          //Version written 2010 09 27 - for Grasshopper 0.7.0055

          //obj is geometry
          if (objs == null) return;
          if (activate) {
            for (int i = 0; i < objs.Count; ++i) {
              Plane plane = new Plane(locations[i], Vector3d.ZAxis);
              Rhino.Display.Text3d obj = new Rhino.Display.Text3d(objs[i], plane, height);
              //Make new attribute
              Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
              Guid o_id = default(Guid);

              //Set object name
              if (objs[i] != null && Rhino.DocObjects.Layer.IsValidName(objs[i]) == true) {
                att.Name = objs[i];
              }



              //int currentLayerIndex = RhinoDocument.Layers.CurrentLayerIndex;
              //att.LayerIndex = currentLayerIndex;
              //Set layer
              if (layer != null && Rhino.DocObjects.Layer.IsValidName(layer) == true) {
                //Get the current layer index

                Rhino.DocObjects.Tables.LayerTable layerTable = doc.Layers;
                int layerIndex = layerTable.Find(layer, true);
                if (layerIndex == -1) { //This layer does not exist, we add it
                  //make new layer
                  Rhino.DocObjects.Layer myLayer = new Rhino.DocObjects.Layer();
                  myLayer.Name = layer;
                  //Add the layer to the layer table
                  layerIndex = layerTable.Add(myLayer);

                  if (layerIndex > -1) { //We manged to add layer!
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
              if (color != null) {
                  att.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
                  att.ObjectColor = color;
                  att.PlotColorSource = Rhino.DocObjects.ObjectPlotColorSource.PlotColorFromObject;
                  att.PlotColor = color;
              }

              o_id = doc.Objects.AddText(obj, att);
              if (groupIndex != -1) { doc.Groups.AddToGroup(groupIndex, o_id); }

            }
          }
        }
        void bakery(object obj, string layer, int groupIndex, bool activate) {

            if (!activate) { return; }
            if (obj == null) { return; }

            //Guid id;



            //Make new attribute
            Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();

            ////Set object name
            //if (name != null && Rhino.DocObjects.Layer.IsValidName(name) == true) {
            //    att.Name = name;
            //}
            att.WireDensity = -1;


            //Set layer
            if (layer != null && Rhino.DocObjects.Layer.IsValidName(layer) == true) {
                System.Drawing.Color color = System.Drawing.Color.LightGray;

                //Get the current layer index

                Rhino.DocObjects.Tables.LayerTable layerTable = doc.Layers;
                int layerIndex = layerTable.Find(layer, true);

                if (layerIndex == -1) { //This layer does not exist, we add it
                    //make new layer
                    Rhino.DocObjects.Layer myLayer = new Rhino.DocObjects.Layer();
                    myLayer.Name = layer;
                    myLayer.Color = color;
                    myLayer.PlotWeight = 0.0;
                    myLayer.IsVisible = false;
                  
          //Add the layer to the layer table
                    layerIndex = layerTable.Add(myLayer);
                }

                if (layerIndex > -1) { //We manged to add layer!
                    att.LayerIndex = layerIndex;
                }




                //Set color
                if (color != null) {
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
            if (obj == null) {
                return;
            }
            Guid id = new Guid();

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
                        //Print("The script does not know how to handle this type of geometry: " + obj.GetType().FullName);
                        return;
                }
            } else if (obj is IGH_GeometricGoo) {
                Type objectType = obj.GetType();

                if (objectType == typeof(GH_Box)) {
                    GH_Box box = obj as GH_Box;
                    box.BakeGeometry(doc, att, ref id);
                } else if (objectType == typeof(GH_Brep)) {
                    GH_Brep brep = obj as GH_Brep;
                    brep.BakeGeometry(doc, att, ref id); 
                } else if (objectType == typeof(GH_Circle)) {
                    GH_Circle circle = obj as GH_Circle;
                    circle.BakeGeometry(doc, att, ref id);
                } else if (objectType == typeof(GH_Curve)) {
                    GH_Curve curve = obj as GH_Curve;
                    curve.BakeGeometry(doc, att, ref id);
                } else if (objectType == typeof(GH_Line)) {
                    GH_Line line = obj as GH_Line;
                    line.BakeGeometry(doc, att, ref id);
                } else if (objectType == typeof(GH_Mesh)) {
                    GH_Mesh mesh = obj as GH_Mesh;
                    mesh.BakeGeometry(doc, att, ref id);
                } else if (objectType == typeof(GH_MeshFace)) {
                    GH_MeshFace meshFace = obj as GH_MeshFace;
                    GH_Mesh mesh = null;
                    meshFace.CastTo<GH_Mesh>(ref mesh);
                    mesh.BakeGeometry(doc, att, ref id);
                } else if (objectType == typeof(GH_Point)) {
                    GH_Point pt = obj as GH_Point;
                    pt.BakeGeometry(doc, att, ref id);
                } else if (objectType == typeof(GH_Rectangle)) {
                    GH_Rectangle rect = obj as GH_Rectangle;
                    rect.BakeGeometry(doc, att, ref id);
                } else if (objectType == typeof(GH_Surface)) {
                    GH_Surface surf = obj as GH_Surface;
                    surf.BakeGeometry(doc, att, ref id);
                } else {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unhandled type: " + objectType.FullName);
                    return;
                }
            } else {
                Type objectType = obj.GetType();
                 if (objectType == typeof(GH_Arc)) {
                    id = doc.Objects.AddArc((Arc)obj, att);
                }
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
                    //Print("Impossible to bake vectors");
                    return;
                } else if (obj is IEnumerable) {
                    int newGroupIndex;
                    if (groupIndex == -1)
                        newGroupIndex = doc.Groups.Add();
                    else
                        newGroupIndex = groupIndex;
                    foreach (object o in obj as IEnumerable) {
                        bake(o, att, newGroupIndex);
                    }
                    return;
                } else {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unhandled type: " + objectType.FullName);
                    return;
                }
            }

            if (groupIndex != -1) {
                doc.Groups.AddToGroup(groupIndex, id);
                //Print("Added " + obj.GetType().Name + " to group number " + groupIndex);
            } else {
                //Print("Added " + obj.GetType().Name);
            }
        }

        protected override void BeforeSolveInstance() {
            base.BeforeSolveInstance();
            this.Message = string.Format("{0:00}", bakeCount);
        }






    }
}




