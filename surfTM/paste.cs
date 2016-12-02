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
            pManager.AddGeometryParameter("lines", "lines", "intersection lines", Grasshopper.Kernel.GH_ParamAccess.list);
            //pManager.AddLineParameter("lines", "lines", "intersection lines", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddNumberParameter("offsetWidth", "offsetWidth", "this needs to overlap the edge", Grasshopper.Kernel.GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("thickness", "thickness", "material thickness", Grasshopper.Kernel.GH_ParamAccess.item, 0.250);
            pManager.AddBooleanParameter("side", "side", "uses midpoint of the line. Which side?", Grasshopper.Kernel.GH_ParamAccess.list, true);
            pManager[0].Optional = false;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_CurveParam("curves", "curves", "rectangle notches");
        }
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {
            List<Point3d> points = new List<Point3d>();
            List<Line> lines = new List<Line>();

            double offsetWidth = 1.0;
            double thickness = 0.250;
            List<double> inputAngles = new List<double>();
            List<bool> inputSides = new List<bool>();

            DA.GetData<double>(1, ref offsetWidth);
            DA.GetData<double>(2, ref thickness);
            DA.GetDataList<bool>(3, inputSides);

            if (!DA.GetDataList<Line>(0, lines)) {
                if (DA.GetDataList<Point3d>(0, points)) {
                    //for point inputs
                    double[] angles = new double[points.Count];
                    for (int i = 0; i < angles.Length; i++) {
                        //angles[i] = inputAngles[i%inputAngles.Count];
                        angles[i] = 0;
                    }
                    Rectangle3d[] rects0 = rect(points, offsetWidth, thickness, angles);
                    DA.SetDataList(0, rects0);
                }
                    return;
            }

            //for line inputs
            bool[] sides = new bool[lines.Count];
            for (int i = 0; i < sides.Length; i++) {
                sides[i] = inputSides[i % inputSides.Count];
            }
            Rectangle3d[] rects1 = rect(lines, offsetWidth, thickness, sides);
            DA.SetDataList(0, rects1);


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
        private Rectangle3d[] rect(List<Line> lines, double offsetWidth, double thickness, bool[] sides) {


            Rectangle3d[] rectangles = new Rectangle3d[lines.Count];
            for (int i = 0; i < lines.Count; i++) {
                double up = -1.0;
                if (sides[i]) {
                    up = 1.0;
                }
                Vector3d yVector = Vector3d.CrossProduct((lines[i].To - lines[i].From), Vector3d.ZAxis);
                Plane plane = new Plane(lines[i].PointAt(0.5), lines[i].To - lines[i].PointAt(0.5), yVector);

                //plane.Rotate(angles[i] * Math.PI / 180.0, plane.ZAxis);
                Point3d cornerA = plane.PointAt(offsetWidth * up, -thickness * 0.5);
                Point3d cornerB = plane.PointAt(0.0, thickness * 0.5);
                Rectangle3d rect = new Rectangle3d(plane, cornerA, cornerB);

                rectangles[i] = rect;
            }
            return rectangles;

        }
    }
}
