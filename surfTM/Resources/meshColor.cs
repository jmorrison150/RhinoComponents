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


namespace gsd{
//   class meshColor{



//   private void RunScript(bool enable, List<Point3d> points, List<Curve> curves, List<Surface> surfaces, List<Brep> breps, List<Mesh> meshes, Color color) {
//       displayOn = enable;
//       updateLines = curves.ToArray();
//       updateSurfaces = surfaces.ToArray();
//       updateBreps = breps.ToArray();
//       updatePoints = points.ToArray();
//       updateColor = color;
//       Rhino.Display.DisplayMaterial mat = new Rhino.Display.DisplayMaterial(color);
//       material = mat;

//       for (int i = 0; i < meshes.Count; ++i) {
//           for (int j = 0; j < meshes[i].Vertices.Count; ++j) {
//               meshes[i].VertexColors.Add(color);
//           }
//       }
//       updateMeshes = meshes.ToArray();
//   }

//   // <Custom additional code> 
//   bool displayOn;
//   Curve[] updateLines;
//   Brep[] updateBreps;
//   Mesh[] updateMeshes;
//   Surface[] updateSurfaces;
//   Point3d[] updatePoints;
//   Color updateColor;
//   Rhino.Display.DisplayMaterial material;

//   double map(double number, double low1, double high1, double low2, double high2) {
//       return low2 + (high2 - low2) * (number - low1) / (high1 - low1);
//   }
//   public override void DrawViewportMeshes(IGH_PreviewArgs args) {     //Draw all lines
//       if (displayOn) {
//           for (int i = 0; i < updateLines.Length; ++i) {
//               args.Display.DrawCurve(updateLines[i], updateColor);
//           }

//           for (int i = 0; i < updateSurfaces.Length; ++i) {
//               args.Display.DrawSurface(updateSurfaces[i], updateColor, 1);
//           }

//           for (int i = 0; i < updateBreps.Length; ++i) {
//               args.Display.DrawBrepShaded(updateBreps[i], material);
//           }

//           for (int i = 0; i < updateMeshes.Length; ++i) {

//               //////////////////////////////////////////////////////////////////////////////////////////////////////


//               BoundingBox box = updateMeshes[i].GetBoundingBox(false);
//               double range = box.Max.Z - box.Min.Z;

//               for (int j = 0; j < updateMeshes[i].Vertices.Count; j++) {
//                   Point3d pt = updateMeshes[i].Vertices[j];
//                   int heigth = (int)map(pt.Z, box.Min.Z, box.Max.Z, 0, 255);
//                   args.Display.DrawPoint(pt, Rhino.Display.PointStyle.Simple, 2, System.Drawing.Color.FromArgb(255, 0, 0, heigth));
//               }

//               args.Display.DrawMeshFalseColors(updateMeshes[i]);
//               args.Display.DrawMeshWires(updateMeshes[i],System.Drawing.Color.FromArgb(255,0,0,0));
//               args.Display.DrawMeshWires(updateMeshes[i], System.Drawing.Color.FromArgb(255, 0, 0, 0),1);



//               //args.Display.DisplayPipelineAttributes.MeshSpecificAttributes.ShowMeshWires = true;
//               //args.Display.DisplayPipelineAttributes.MeshSpecificAttributes.AllMeshWiresColor = System.Drawing.Color.FromArgb(255,0,0,0);
//               //Point3d[] points = new Point3d[updateMeshes[i].Vertices.Count];
//               //for (int j = 0; j < points.Length; j++){			        
//               //    points[j] = updateMeshes[i].Vertices[j];			}
//               //PointCloud cloud = new PointCloud(points);
//               //args.Display.DrawPointCloud(cloud, 1,System.Drawing.Color.FromArgb(255,0,0,255));


            
            
            
            
//           }
//           args.Display.DrawPoints(updatePoints, Rhino.Display.PointStyle.Simple, 2, updateColor);


//       }

//   }

//   // </Custom additional code> 
//}
}