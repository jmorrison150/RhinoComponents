using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace gsd {
    public class Skip : Grasshopper.Kernel.GH_Component {
        public Skip() : base(".skip", ".skip", "only keep every 'nth' item", "Extra", "Util") { }
        public override Guid ComponentGuid {
            get { return new Guid("{C025B912-B116-463B-8F34-6BD33C6C8025}"); }
        }
        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                return gsd.Properties.Resources.gsd05;
            }
        }
        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager) {
            pManager.AddGenericParameter("list", "list", "list of geometry. null items will be removed", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.AddIntegerParameter("#", "#", "the number of items to skip", Grasshopper.Kernel.GH_ParamAccess.item, 2);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_GenericParam("list", "list", "the abridged list", Grasshopper.Kernel.GH_ParamAccess.list);
        }
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {
            //empty lists
            List<Grasshopper.Kernel.Types.IGH_Goo> listInput = new List<Grasshopper.Kernel.Types.IGH_Goo>();
            List<Grasshopper.Kernel.Types.IGH_Goo> listOutput = new List<Grasshopper.Kernel.Types.IGH_Goo>();


            int skip = 1;
            DA.GetData<int>(1, ref skip);
            if (skip < 1) {
                skip = 1;
            }

            //get input from grasshopper
            if (!DA.GetDataList<Grasshopper.Kernel.Types.IGH_Goo>(0, listInput)) { return; }
                //remove null items
                listInput.RemoveAll(item => item == null);
                //skip
                for (int i = 0; i < listInput.Count; i += skip) {
                    listOutput.Add(listInput[i]);
                }
                //output
                DA.SetDataList(0, listOutput);

            
        }
    }
}
