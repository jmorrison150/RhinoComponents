using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;

using System;
using System.Collections.Generic;

namespace gsd {

    public class Range : Grasshopper.Kernel.GH_Component {

        public Range() : base(".range", ".range", "two dimensional array", "Extra", "surfTM") { }
        public override Guid ComponentGuid {
            get { return new Guid("{EEFDD2A7-6C53-4F8A-B546-B39756F65EAA}"); }
        }
        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                return gsd.Properties.Resources.gsd13;
            }
        }
        //input
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddIntegerParameter("last_I", "i", "end of i range", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("last_J", "j", "end of j range", GH_ParamAccess.item, 2);

        }

        //output
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_DoubleParam("i", "i", "i indexes formatted for grasshopper branches", GH_ParamAccess.tree);
            pManager.Register_DoubleParam("j", "j", "j indexes formatted for grasshopper branches", GH_ParamAccess.tree);

        }
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {
            //empty lists
            int index0 = 2;
            int index1 = 2;
            Grasshopper.DataTree<double> outI;
            Grasshopper.DataTree<double> outJ;

            //get input from grasshopper
            DA.GetData<int>(0, ref index0);
            DA.GetData<int>(1, ref index1);

            //logic
            run(index0, index1, out outI, out outJ);

            //output
            DA.SetDataTree(0, outI);
            DA.SetDataTree(1, outJ);

        }

        private void run(int i, int j, out Grasshopper.DataTree<double> outI, out Grasshopper.DataTree<double> outJ) {

            //initialize
            double[][] indexes = new double[i][];
            for(int m = 0; m < indexes.Length; m++) {
                indexes[m] = new double[j];
                for(int n = 0; n < indexes[m].Length; n++) {
                    //assignment
                    //indexes[m][n] = 1.0;
                }
            }

            outI = formatI(indexes);
            outJ = formatJ(indexes);






        }

        private Grasshopper.DataTree<double> formatTree(double[][] indexes) {
            Grasshopper.DataTree<double> tree = new Grasshopper.DataTree<double>();
            for(int m = 0; m < indexes.Length; m++) {
                Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
                for(int n = 0; n < indexes[m].Length; ++n) {
                    tree.Insert(indexes[m][n], path, n);
                }
            }
            return tree;
        }

        private Grasshopper.DataTree<double> formatI(double[][] indexes) {
            Grasshopper.DataTree<double> tree = new Grasshopper.DataTree<double>();
            for(int m = 0; m < indexes.Length; m++) {
                Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
                for(int n = 0; n < indexes[m].Length; ++n) {
                    tree.Insert((double) m, path, n);
                }
            }
            return tree;
        }

        private Grasshopper.DataTree<double> formatJ(double[][] indexes) {
            Grasshopper.DataTree<double> tree = new Grasshopper.DataTree<double>();
            for(int m = 0; m < indexes.Length; m++) {
                Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
                for(int n = 0; n < indexes[m].Length; ++n) {
                    tree.Insert((double) n, path, n);
                }
            }
            return tree;
        }

    }
}