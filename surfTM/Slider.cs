using System;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;



namespace gsd {
    public class Slider : Grasshopper.Kernel.GH_Component {
        public Slider() : base(".slider", ".slider", "access the values for an existing slider", "Extra", "Util") { }


        public override Guid ComponentGuid {
            get { return new Guid("{74434093-73B6-45CE-985C-CE6C1E23D1DB}"); }
        }


        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                return gsd.Properties.Resources.gsd07;
            }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            //pManager.AddTextParameter("name", "name", " this is not guaranteed to update interactively, Each slider needs a unique name.", GH_ParamAccess.item);
            //pManager.AddGenericParameter("", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("min", "min", "sets the minimum of a slider", GH_ParamAccess.item);
            pManager.AddNumberParameter("slider", "slider", "plug in the slider", GH_ParamAccess.item);
            pManager.AddNumberParameter("max", "max", "sets the maximum of a slider", GH_ParamAccess.item);


            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            //pManager[3].Optional = true;


        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            //pManager.Register_GenericParam("", "", "", GH_ParamAccess.item);
            pManager.Register_DoubleParam("min", "min", "min", GH_ParamAccess.item);
            pManager.Register_DoubleParam("slider", "slider", "number", GH_ParamAccess.item);
            pManager.Register_DoubleParam("max", "max", "max", GH_ParamAccess.item);

        }

        Grasshopper.Kernel.Special.GH_NumberSlider ghSlider;
    
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {


            //string name = null;
            double min = 0.0;
            double number = 0.0;
            double max = 0.0;

            //if (!DA.GetData<string>(0, ref name)) { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,"add a panel. make it the Multiline Data type"); return; }

            Guid id;

            if (DA.GetData<double>(1, ref number)) {



                int sourceCount = this.Params.Input[2].Sources.Count;
                for (int i = 0; i < sourceCount; i++) {



                    id = this.Params.Input[1].Sources[i].InstanceGuid;

                    ghSlider = findSlider(id);
                    if (ghSlider != null) {


                        if (DA.GetData<double>(0, ref min)) {
                            ghSlider.Slider.Minimum = (decimal)min;
                        } else { min = (double)ghSlider.Slider.Minimum; }
                        if (DA.GetData<double>(2, ref max)) {
                            if (min > max) {
                                max = min + 1.0;
                            }
                            ghSlider.Slider.Maximum = (decimal)max;
                        } else { max = (double)ghSlider.Slider.Maximum; }

                        number = (double)ghSlider.CurrentValue;
                        if (number > max) {
                            number = max;
                            ghSlider.Slider.Value = (decimal)number;
                        }
                        if (number < min) {
                            //number = min;
                            //ghSlider.Slider.Value = (decimal)number;

                        }
                        min = (double)ghSlider.Slider.Minimum;
                        max = (double)ghSlider.Slider.Maximum;

                    } else {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "did not findSlider");
                    }






                }
            } else {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "add a slider"); return;
            }







            DA.SetData(0, min);
            DA.SetData(1, number);
            DA.SetData(2, max);

        }


        public Grasshopper.Kernel.Special.GH_NumberSlider findSlider(Guid id) {
            //Get the document that owns this object.
            GH_Document ghDoc = this.OnPingDocument();
            //Abort if no such document can be found.
            if (ghDoc == null) { return null; }

            //Iterate over all objects inside the document.
            for (int i = 0; i < ghDoc.ObjectCount; i++) {
                IGH_DocumentObject obj = ghDoc.Objects[i];

                //First test the NickName of the object against the search name.
                if (obj.InstanceGuid == id) {
                    //Then try to cast the object to a GH_NumberSlider.
                    Grasshopper.Kernel.Special.GH_NumberSlider sld_obj = obj as Grasshopper.Kernel.Special.GH_NumberSlider;
                    if (sld_obj != null) { return sld_obj; }
                } else {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, obj.InstanceGuid.ToString());
                }
            }
            return null;
        }
        //public Grasshopper.Kernel.Special.GH_NumberSlider findSlider(string name) {
        //    //Get the document that owns this object.
        //    GH_Document ghDoc = this.OnPingDocument();
        //    //Abort if no such document can be found.
        //    if (ghDoc == null) { return null; }

        //    //Iterate over all objects inside the document.
        //    for (int i = 0; i < ghDoc.ObjectCount; i++) {
        //        IGH_DocumentObject obj = ghDoc.Objects[i];

        //        //First test the NickName of the object against the search name.
        //        if (obj.NickName.Equals(name, StringComparison.Ordinal)) {
        //            //Then try to cast the object to a GH_NumberSlider.
        //            Grasshopper.Kernel.Special.GH_NumberSlider sld_obj = obj as Grasshopper.Kernel.Special.GH_NumberSlider;
        //            if (sld_obj != null) { return sld_obj; }
        //        }
        //    }
        //    return null;
        //}


    }


}
