using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace gsd {
    public class move : Grasshopper.Kernel.GH_Component {

        public move() : base(".thickness", ".thckness", "makes Solid. gives flat surface material thickness", "Extra", "surfTM") { }
        public override Guid ComponentGuid {
            get {
                return new Guid("{090E0A9A-60BA-42CA-8F10-EA780C7FB75C}");
            }
        }
        protected override System.Drawing.Bitmap Icon {
            get {
                return gsd.Properties.Resources.gsd02;
            }
        }
        public override Grasshopper.Kernel.GH_Exposure Exposure {
            get {
                return Grasshopper.Kernel.GH_Exposure.tertiary;
            }
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager) {
            pManager.AddSurfaceParameter("surfaces", "surfaces", "surfaces", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddNumberParameter("thickness", "thickness", "thickness", Grasshopper.Kernel.GH_ParamAccess.list, 1.0);
            pManager.AddBooleanParameter("center", "center", "center", Grasshopper.Kernel.GH_ParamAccess.list, true);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;


        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_BRepParam("solids", "solids", "solids", Grasshopper.Kernel.GH_ParamAccess.list);
        }


        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {


            List<Surface> inputSurfaces = new List<Surface>();
            List<double> inputThicknesses = new List<double>();
            List<bool> inputCenters = new List<bool>();
            List<Brep> outSolids = new List<Brep>();



            DA.GetDataList<Surface>(0, inputSurfaces);
            DA.GetDataList<double>(1, inputThicknesses);
            DA.GetDataList<bool>(2, inputCenters);

            double[] thickness = new double[inputSurfaces.Count];
            bool[] center = new bool[inputSurfaces.Count];

            for(int i = 0; i < inputSurfaces.Count; i++) {
                center[i] = inputCenters[i%inputCenters.Count];
                thickness[i] = inputThicknesses[i%inputThicknesses.Count];
                if(center[i]) { thickness[i] = thickness[i] * 0.5; }
            }
            for(int i = 0; i < inputSurfaces.Count; i++) {



                outSolids.Add(Brep.CreateFromOffsetFace(inputSurfaces[i].ToBrep().Faces[0], thickness[i], 0.001, center[i], true));
            }

            DA.SetDataList(0, outSolids);


        }
    }
}
