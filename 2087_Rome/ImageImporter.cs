// Type: IOComponents.ImageImporter
// Assembly: IOComponents, Version=1.0.0.0, Culture=neutral, PublicKeyToken=dda4f5ec2cd80803
// MVID: F0174C7D-BBF6-46C9-B82C-C3DC285C74AA
// Assembly location: C:\Users\JMorrison\Desktop\New folder (2)\IOLibrary.dll

using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using IOComponents.Properties;
using Rhino.Geometry;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace IOComponents {
    public class ImageImporter : GH_Component {
        protected override Bitmap Icon {
            get {
                return Resources.ImportImage_24x24;
            }
        }

        public override GH_Exposure Exposure {
            get {
                return GH_Exposure.quarternary | GH_Exposure.dropdown;
            }
        }

        public override Guid ComponentGuid {
            get {
                return new Guid("{C2C0C6CF-F362-4047-A159-21A72E7C272A}");
            }
        }

        public ImageImporter()
            : base("Import Image", "IMG", "Import image data from bmp, jpg or png files.", "Params", "Input") {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddParameter((IGH_Param) new Param_FilePath() 
            { FileFilter = "All image files | ",ExpireOnFileEvent=true});



            pManager.AddParameter((IGH_Param) new Param_FilePath() {
                FileFilter = "All image files|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff",
                ExpireOnFileEvent = true
            }, "File", "F", "Location of image file", GH_ParamAccess.item);
            pManager.AddRectangleParameter("Rectangle", "R", "Optional image destination rectangle", GH_ParamAccess.item);
            pManager.AddIntegerParameter("X Samples", "X", "Number of samples along image X direction", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Y Samples", "Y", "Number of samples along image Y direction", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddMeshParameter("Image", "I", "A mesh representation of the image", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA) {
            string destination1 = (string) null;
            Rectangle3d destination2 = Rectangle3d.get_Unset();
            int destination3 = -1;
            int destination4 = -1;
            if(!DA.GetData<string>(0, ref destination1))
                return;
            DA.GetData<Rectangle3d>(1, ref destination2);
            DA.GetData<int>(2, ref destination3);
            DA.GetData<int>(3, ref destination4);
            if(!File.Exists(destination1)) {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Image file does not exist");
            } else {
                Bitmap bitmap1 = (Bitmap) null;
                try {
                    bitmap1 = new Bitmap(destination1);
                    if(destination3 <= 0)
                        destination3 = bitmap1.Width;
                    if(destination4 <= 0)
                        destination4 = bitmap1.Height;
                    // ISSUE: explicit reference operation
                    if(!destination2.IsValid)
                        destination2 = new Rectangle3d(Plane.get_WorldXY(), (double) bitmap1.Width, (double) bitmap1.Height);
                    bool flag = false;
                    switch(bitmap1.PixelFormat) {
                        case PixelFormat.Format24bppRgb:
                        case PixelFormat.Format32bppRgb:
                        case PixelFormat.Format32bppArgb:
                            if(bitmap1.Width != destination3 || bitmap1.Height != destination4) {
                                flag = true;
                                break;
                            } else
                                break;
                        default:
                            flag = true;
                            break;
                    }
                    if(flag) {
                        Bitmap bitmap2 = GH_IconTable.ResizeImage((Image) bitmap1, new Size(destination3, destination4), InterpolationMode.HighQualityBicubic, PixelFormat.Format24bppRgb);
                        bitmap1.Dispose();
                        bitmap1 = bitmap2;
                    }
                    Mesh mesh = new Mesh();
                    GH_MemoryBitmap ghMemoryBitmap = new GH_MemoryBitmap(bitmap1, WrapMode.TileFlipXY);
                    for(int y = 0; y < destination4; ++y) {
                        double num1 = 1.0 - (double) y / (double) ( destination4 - 1 );
                        for(int x = 0; x < destination3; ++x) {
                            double num2 = (double) x / (double) ( destination3 - 1 );
                            Color color = ghMemoryBitmap.Colour(x, y);
                            // ISSUE: explicit reference operation
                            mesh.get_Vertices().Add(destination2.PointAt(num2, num1));
                            mesh.get_VertexColors().Add(color);
                        }
                    }
                    ghMemoryBitmap.Release(false);
                    bitmap1.Dispose();
                    bitmap1 = (Bitmap) null;
                    for(int index1 = 0; index1 < destination3 - 1; ++index1) {
                        for(int index2 = 0; index2 < destination4 - 1; ++index2) {
                            int num1 = index1 + index2 * destination3;
                            int num2 = num1 + 1;
                            int num3 = num1 + destination3;
                            int num4 = num3 + 1;
                            mesh.get_Faces().AddFace(num1, num2, num4, num3);
                        }
                    }
                    DA.SetData(0, (object) mesh);
                } catch(Exception ex) {
                    if(bitmap1 != null)
                        bitmap1.Dispose();
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error while parsing image: " + ex.Message);
                }
            }
        }
    }
}
