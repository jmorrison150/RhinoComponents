

using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace gsd {
    public class Canvas : Grasshopper.Kernel.GH_Component {
        public Canvas() : base(".canvas", ".canvas", "format for 11 x 17 ", "Extra", "Util") { }

        public override Guid ComponentGuid {
            get {
                return new Guid("{99B0E880-2B79-454D-B2C8-9AFD7F2D1515}");
            }
        }
        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                //return base.Internal_Icon_24x24;
                return gsd.Properties.Resources.gsd;
            }
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager) {
            pManager.AddIntegerParameter("", "", "change wire color", Grasshopper.Kernel.GH_ParamAccess.item, 200);
            pManager.AddIntegerParameter("", "", "change background color", Grasshopper.Kernel.GH_ParamAccess.item, 10);

            //pManager.AddBooleanParameter("x", "x", "draw crazy border", Grasshopper.Kernel.GH_ParamAccess.item, false);

        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
        }
        int input0;
        int input1;
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {
            //int userInput = 240;
            DA.GetData<int>(0, ref input0);
            DA.GetData<int>(1, ref input1);

            if (input0 < 0) { input0 = 0; } else if (input0 > 255) { input0 = 255; }
            if (input1 < 0) { input1 = 0; } else if (input1 > 255) { input1 = 255; }


            setColors();
            drawPage();

            //bool crazyBorder = false;
            //DA.GetData<bool>(1, ref crazyBorder);
            //draw(crazyBorder);
            
            //Grasshopper.GUI.GH_ProgressBar bar = new Grasshopper.GUI.GH_ProgressBar();
            //bar.Width = 250;
            //bar.Height = 100;
            //bar.Progress = 0.5;
            //bar.Visible = true;
            //bar.Show();
        }
        void setColors() {
            Grasshopper.GUI.Canvas.GH_Skin.canvas_edge = System.Drawing.Color.FromArgb(0, 0, 0, 0);
            Grasshopper.GUI.Canvas.GH_Skin.canvas_grid = System.Drawing.Color.FromArgb(255, 0, 0, 0);
            Grasshopper.GUI.Canvas.GH_Skin.canvas_back = System.Drawing.Color.FromArgb(255, input1, input1, input1);
            Grasshopper.GUI.Canvas.GH_Skin.group_back = System.Drawing.Color.FromArgb(127, 200, 200, 200);
            Grasshopper.GUI.Canvas.GH_Skin.panel_back = System.Drawing.Color.FromArgb(127, 200, 200, 200);


            Grasshopper.GUI.Canvas.GH_Skin.canvas_grid_col = 1650;
            Grasshopper.GUI.Canvas.GH_Skin.canvas_grid_row = 2550;
            Grasshopper.GUI.Canvas.GH_Skin.canvas_mono = false;
            Grasshopper.GUI.Canvas.GH_Skin.canvas_shade = System.Drawing.Color.FromArgb(0, input1, input1, input1);
            Grasshopper.GUI.Canvas.GH_Skin.wire_default = System.Drawing.Color.FromArgb(255, input0, input0, input0);
            Grasshopper.GUI.Canvas.GH_Skin.wire_empty = System.Drawing.Color.FromArgb(255, input0, input0, input0);
            


            Grasshopper.GUI.Canvas.GH_Skin.palette_grey_standard.Fill = System.Drawing.Color.FromArgb(255, 200, 200, 200);
            Grasshopper.GUI.Canvas.GH_Skin.palette_normal_standard.Fill = System.Drawing.Color.FromArgb(255, 225, 225, 225);
            Grasshopper.GUI.Canvas.GH_Skin.palette_hidden_standard.Fill = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            Grasshopper.GUI.Canvas.GH_Skin.palette_white_standard.Fill = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            Grasshopper.GUI.Canvas.GH_Skin.palette_locked_standard.Fill = System.Drawing.Color.FromArgb(255, 255, 255, 255);


            Grasshopper.GUI.Canvas.GH_Skin.palette_normal_standard.Edge = System.Drawing.Color.FromArgb(255, 0, 0, 0);
            Grasshopper.GUI.Canvas.GH_Skin.palette_hidden_standard.Edge = System.Drawing.Color.FromArgb(75, 0, 0, 0);

            Grasshopper.GUI.Canvas.GH_Skin.palette_black_standard.Fill = System.Drawing.Color.FromArgb(0, 200, 200, 200);
            Grasshopper.GUI.Canvas.GH_Skin.palette_black_standard.Edge = System.Drawing.Color.FromArgb(20, 0, 0, 0);
            Grasshopper.GUI.Canvas.GH_Skin.palette_black_standard.Text = System.Drawing.Color.FromArgb(255, 0, 0, 0);

            //Grasshopper.GUI.Canvas.GH_Skin.SaveSkin();
        }
        void drawPage() {
            Grasshopper.Instances.ActiveCanvas.CanvasPrePaintGroups += page;        
        }
        public void page(Grasshopper.GUI.Canvas.GH_Canvas sender) { 



        System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black, 0.1f);
        
            //top left
        System.Drawing.Point pt0 = new System.Drawing.Point(0, 0);
        System.Drawing.Point pt1 = new System.Drawing.Point(0, 2550);
        System.Drawing.Point pt2 = new System.Drawing.Point(1650, 2550);
        System.Drawing.Point pt3 = new System.Drawing.Point(1650, 0);




        sender.Graphics.DrawLine(pen, pt0, pt1);
        sender.Graphics.DrawLine(pen, pt1, pt2);
        sender.Graphics.DrawLine(pen, pt2, pt3);
        sender.Graphics.DrawLine(pen, pt3, pt0);

       sender.Graphics.Flush();

        }
        //public void wireHandler(Grasshopper.GUI.Canvas.GH_Canvas sender) {
           
        //}
        //public void paintHandler(Grasshopper.GUI.Canvas.GH_Canvas sender) {
        //    //draw on top of the screen
        //    //begin
        //    //sender.Graphics.ResetTransform();
        //    //end
        //    //sender.Viewport.ApplyProjection(sender.Graphics);
            
        //    //draw line


   

        //    ////reset the display transformation (ie pan and zoom)
        //    //sender.Graphics.ResetTransform();
        //    ////figure out the bouondary rectangel of the canvas
        //    //Rectangle3d boundary = new Rectangle3d(Plane.WorldXY, new Point3d(0, 0, 0), new Point3d(sender.Width, sender.Height, 0));
        //    ////divide the boundary into a bunch of points
        //    //Point3d[] points;
        //    //boundary.ToNurbsCurve().DivideByLength(30, true, out points);
        //    //randomize the points
        //    //Random random = new Random();
        //    //for (int i = 0; i < points.Length; i++) {
        //    //    double x = 8 * random.NextDouble() - 4;
        //    //    double y = 8 * random.NextDouble() - 4;
        //    //    points[i] += new Vector3d(x, y, 0.0);
        //    //}

        //    ////convert the Rhino points to GDI+points
        //    //System.Drawing.PointF[] gdiPoints = new System.Drawing.PointF[points.Length];
        //    //for (int i = 0; i < points.Length; i++) {
        //    //    gdiPoints[i] = new System.Drawing.PointF(Convert.ToSingle(points[i].X), Convert.ToSingle(points[i].Y));
        //    //}

        //    ////finally draw the randomized gdi points using a thick pen
        //    //System.Drawing.Pen edge0 = new System.Drawing.Pen(System.Drawing.Color.DarkBlue, 20);
        //    //System.Drawing.Pen edge1 = new System.Drawing.Pen(System.Drawing.Color.Crimson, 16);

        //    //sender.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        //    //sender.Graphics.DrawLines(edge0, gdiPoints);
        //    //sender.Graphics.DrawLines(edge1, gdiPoints);

        //    //edge0.Dispose();
        //    //edge1.Dispose();

        //    //put the correct display transformation back
        //    //sender.Viewport.ApplyProjection(sender.Graphics);
        //}


        //    void environmentMap(Brep B) {
        //    string T = null;
        //    if (!System.IO.File.Exists(T)) { return; }

        //    Rhino.Display.DisplayMaterial material = new Rhino.Display.DisplayMaterial(System.Drawing.Color.White);
        //    material.SetEnvironmentTexture(T, true);
        //    material.SetEnvironmentTexture(T, false);

        //    Mesh[] meshes = Mesh.CreateFromBrep(B, MeshingParameters.Smooth);
        //    Mesh mesh = new Mesh();
        //    for (int i = 0; i < meshes.Length; i++) {
        //        mesh.Append(meshes[i]);
        //    }
        //    meshShapes.Add(mesh);
        //    meshMaterials.Add(material);



        //}
        //BoundingBox meshBox;
        //List<Mesh> meshShapes;
        //List<Rhino.Display.DisplayMaterial> meshMaterials;

        //protected override void BeforeSolveInstance() {
        //    base.BeforeSolveInstance();
        //    meshBox = BoundingBox.Empty;
        //    meshShapes = new List<Mesh>();
        //    meshMaterials = new List<Rhino.Display.DisplayMaterial>();
        //}

        //public override BoundingBox ClippingBox {
        //    get {
        //        return meshBox;
        //    }
        //}
        //public override void DrawViewportMeshes(Grasshopper.Kernel.IGH_PreviewArgs args) {
        //    base.DrawViewportMeshes(args);
        //    for (int i = 0; i < meshShapes.Count; i++) {
        //        args.Display.DrawMeshShaded(meshShapes[i], meshMaterials[i]);

        //    }
        //}
    
    
    
    
    
    
    
            //GH_Document ghDoc = owner.OnPingDocument();
            //List<IGH_ActiveObject> activeObjects = ghDoc.ActiveObjects();
            
            //string[] nicknames = new string[activeObjects.Count];
            //for (int i = 0; i < activeObjects.Count; i++)			{
            //    nicknames[i] = activeObjects[i].NickName;            }
            //IList<IGH_Attributes> atts = ghDoc.Attributes;
            //for (int i = 0; i < atts.Count; i++) {
            //    atts[i].ToString();
            //}
            //Guid id;

            //for (int i = 0; i < activeObjects.Count; i++)			{
            //    id = activeObjects[2].InstanceGuid;
            //    string name = activeObjects[i].Name;
            //    string pathName = activeObjects[i].Attributes.PathName;
            //    string hasInput = activeObjects[i].Attributes.HasInputGrip.ToString();
            //    string inputGrip = activeObjects[i].Attributes.InputGrip.ToString();
                
            //    //activeObjects[i].InstanceGuid.ToString();
            //    //for (int j = i+1; j < nicknames.Length; j++) {
            //    //    if(activeObjects[i].NickName == nicknames[j]){
            //    //        activeObjects[i].NickName += i.ToString();
            //    //    }
            //    //}
            //}
            //SortedDictionary<string,Grasshopper.Kernel.Expressions.GH_Variant> constants = ghDoc.ConstantServer;
            //string con = constants.ToString();
            //Print(con);
            //string[] keys;
            //constants.Keys.CopyTo(keys, 0);
            //for (int i = 0; i < keys.Length; i++) {
            //    string key = keys[i];
            //    Print(key);
            //}
            ////Print(key);

            //Grasshopper.Kernel.Expressions.GH_ExpressionParser exp = ghDoc.CreateExpressionParser();
            //Transform.Mirror(


            //Grasshopper.GUI.GH_FontScriptSettingsUI fonts;
            //string fontName = fonts.SettingsUI().Font.Name;
    
    
    }



}
