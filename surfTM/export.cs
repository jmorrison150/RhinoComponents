// Type: ExportLegs.ExportLegs
// Assembly: ExportLegs, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0657D2CD-1428-474A-BFF0-091415777836
// Assembly location: C:\Users\JMorrison\Desktop\ExportLegs.dll

using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace gsd {
    public class Export : GH_Component {
        private int timeline;
        //private Rectangle buttonBounds;
        public Export() : base(".obj", ".obj", "save each state as a numbered .obj file", "Extra", "Util") {
                this.timeline = 0;
        }
        public int timeCount {
            get {
                return timeline;
            }
            set {
                timeline = value;
            }
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("{0A906629-EA18-455C-AA81-D7B8B418C264}");
            }
        }

        protected override Bitmap Icon {
            get {
                //TODO icon
                return null;
            }
        }


        string findPath() {
            string fileDirectory = null;
            try {
                GH_Document ghDoc = this.OnPingDocument();
                fileDirectory = ghDoc.FilePath;
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "filePath = " + fileDirectory);
                //System.IO.DirectoryInfo dir = System.IO.Directory.GetParent(doc.Path);
                //dirPath = dir.FullName;
            } catch { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "no filePath found. Please save ghx, then refresh."); }
            return fileDirectory;
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddMeshParameter("mesh", "mesh", "mesh to export", GH_ParamAccess.list);
            pManager.AddTextParameter("filePath", "filePath", "directory to save files in", GH_ParamAccess.item,findPath());
            pManager.AddBooleanParameter("activate", "activate", "Do you want to save files?", GH_ParamAccess.item,false);
            pManager.AddBooleanParameter("reset", "reset", "restart numbers at zero", GH_ParamAccess.item,false);

//            pManager.AddIntegerParameter("timeline", "timeline", "timeline", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddGenericParameter("", "", "", GH_ParamAccess.item);
        }


        RhinoDoc activeDoc = RhinoDoc.ActiveDoc;
       
        protected override void SolveInstance(IGH_DataAccess DA) {
            List<Mesh> meshes = new List<Mesh>();
            string fileDirectory = (string)null;
            bool activate = false;
            bool reset = false;

            if (!DA.GetDataList<Mesh>(0, meshes)){return;}
            if (!DA.GetData<string>(1, ref fileDirectory)) {  return; }
            if  (!DA.GetData<bool>(2, ref activate)){return;}
            if ( !DA.GetData<bool>(3, ref reset)){return;}
                
            if(reset){timeline = 0;}

            string dotObj = ".obj";
            if (activate) {
                RhinoApp.RunScript("_selnone", true);
                activeDoc.Objects.UnselectAll();
                for (int i = 0; i < meshes.Count; i++) {
                    activeDoc.Objects.Select(activeDoc.Objects.AddMesh(meshes[i]));
                }
                RhinoApp.RunScript("_-export " + fileDirectory + timeline.ToString() + dotObj + " _Enter _Enter", true);
                RhinoApp.RunScript("_delete", true);
                timeline++;
            }
           
            
            //this.Message = "message";
            
        }
        protected override void BeforeSolveInstance() {
            base.BeforeSolveInstance();
            this.Message = timeline.ToString();
        }
        
    }















    //public class TimeCountButton : GH_ComponentAttributes {
    //    private Rectangle buttonBounds;
    //    //private int count;
    //    public TimeCountButton(GH_Component owner)
    //        : base(owner) {

    //    }
    //    protected override void Layout() {
    //        base.Layout();



    //        Rectangle rectangle1 = GH_Convert.ToRectangle(this.Bounds);
    //        rectangle1.Height += 22;
    //        Rectangle rectangle2 = rectangle1;
    //        rectangle2.Y = rectangle2.Bottom - 22;
    //        rectangle2.Height = 22;
    //        rectangle2.Inflate(-2, -2);
    //        this.Bounds = (RectangleF)rectangle1;
    //        this.buttonBounds = rectangle2;
    //    }


    //    protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel) {
    //        //base.Attributes.RenderToCanvas(canvas, channel);
    //        base.Render(canvas, graphics, channel);
    //        if (channel != GH_CanvasChannel.Objects)
    //            return;



    //        GH_Capsule ghCapsule = GH_Capsule.CreateTextCapsule(this.buttonBounds, this.buttonBounds, GH_Palette.Hidden, this.Owner.Message, 2, 0);
    //        ghCapsule.Render(graphics, this.Selected, false, false);
    //        ghCapsule.Dispose();
    //    }

    //}
}
