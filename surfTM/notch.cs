using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace gsd {
    public class notch:Grasshopper.Kernel.GH_Component {
        public notch() : base(".paste", ".paste", "copies a rectangle at each point", "Extra", "surfTM") { }
        public override Guid ComponentGuid {
            get {
                return new Guid("{BABC86A7-4EC8-4F0F-AE3C-AC06F71D9B9E}");
            }
        }
        protected override System.Drawing.Bitmap Icon {
            get {

                return gsd.Properties.Resources.gsd03;
            }
        }
        public override Grasshopper.Kernel.GH_Exposure Exposure {
            get {
                return Grasshopper.Kernel.GH_Exposure.tertiary;
            }
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager) {
            pManager.AddPointParameter("points", "points", "intersection points", Grasshopper.Kernel.GH_ParamAccess.list,Point3d.Origin);
            pManager.AddNumberParameter("offsetWidth", "offsetWidth", "this needs to overlap the edge", Grasshopper.Kernel.GH_ParamAccess.item,1.0);
            pManager.AddNumberParameter("thickness", "thickness", "material thickness", Grasshopper.Kernel.GH_ParamAccess.item,0.250);
            pManager.AddNumberParameter("angle", "angle", "rotation angle around the intersection point", Grasshopper.Kernel.GH_ParamAccess.list,0.0);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_CurveParam("curves", "curves", "rectangle notches");
        }
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {
            List<Point3d> points = new List<Point3d>();
            double offsetWidth = 1.0;
            double thickness = 0.250;
            List<double> inputAngles = new List<double>();


            if(!DA.GetDataList<Point3d>(0, points)){return;}
            DA.GetData<double>(1, ref offsetWidth);
            DA.GetData<double>(2, ref thickness);
            DA.GetDataList<double>(3, inputAngles);

            double[] angles = new double[points.Count];
            for (int i = 0; i < angles.Length; i++) {
                angles[i] = inputAngles[i%inputAngles.Count];
            }
            Rectangle3d[] rects = rect(points, offsetWidth, thickness, angles);

            DA.SetDataList(0, rects);

        }
        private Rectangle3d[] rect(List<Point3d> points, double offsetWidth, double thickness, double[] angles) {


            Rectangle3d[] rectangles = new Rectangle3d[points.Count];
            for (int i = 0; i < points.Count; i++) {
                Plane plane = new Plane(points[i], Vector3d.ZAxis);
                plane.Rotate(angles[i] * Math.PI / 180.0, plane.ZAxis);
                Point3d cornerA = plane.PointAt(offsetWidth, -thickness * 0.5);
                Point3d cornerB = plane.PointAt(0.0, thickness * 0.5);
                Rectangle3d rect = new Rectangle3d(plane, cornerA, cornerB);

                rectangles[i] = rect;
            }
            return rectangles;

        }

    }
}
