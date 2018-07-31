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
using System.Collections.Generic;
using System.Runtime.InteropServices;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance50 : GH_ScriptInstance {
    #region Utility functions
    /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
    /// <param name="text">String to print.</param>
    private void Print(string text) { /* Implementation hidden. */ }
    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
    /// <param name="format">String format.</param>
    /// <param name="args">Formatting parameters.</param>
    private void Print(string format, params object[] args) { /* Implementation hidden. */ }
    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj) { /* Implementation hidden. */ }
    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
    #endregion

    #region Members
    /// <summary>Gets the current Rhino document.</summary>
    private readonly RhinoDoc RhinoDocument;
    /// <summary>Gets the Grasshopper document that owns this script.</summary>
    private readonly GH_Document GrasshopperDocument;
    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
    private readonly IGH_Component Component;
    /// <summary>
    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
    /// Any subsequent call within the same solution will increment the Iteration count.
    /// </summary>
    private readonly int Iteration;
    #endregion

    /// <summary>
    /// This procedure contains the user code. Input parameters are provided as regular arguments,
    /// Output parameters as ref arguments. You don't have to assign output parameters,
    /// they will have a default value.
    /// </summary>
    private void RunScript(List<double> in0, List<double> in1, ref object out0, ref object out1) {


        #region beginScript
        Guid id0 = this.Component.Params.Input[0].Sources[0].InstanceGuid;
        ghSlider0 = findSlider(id0);
        double max0 = (double)ghSlider0.Slider.Maximum;
        double min0 = (double)ghSlider0.Slider.Minimum;

        int count = 10;

        double[] updateValues0 = new double[count * count];
        for (int j = 0; j < count; j++) {
            for (int i = 0; i < count; i++) {
                updateValues0[(j * count) + i] = map(i, 0, count - 1, min0, max0);
            }
        }
        out0 = updateValues0;






        Guid id1 = this.Component.Params.Input[1].Sources[0].InstanceGuid;
        ghSlider1 = findSlider(id1);
        double max1 = (double)ghSlider1.Slider.Maximum;
        double min1 = (double)ghSlider1.Slider.Minimum;


        double[] updateValues1 = new double[count * count];
        for (int j = 0; j < count; j++) {
            for (int i = 0; i < count; i++) {
                updateValues1[(i * count) + j] = map(j, 0, count - 1, min1, max1);
            }
        }
        out1 = updateValues1;



        //Guid id;
        //List<double> updateMax = new List<double>();
        //List<double> updateSlider = new List<double>();
        //List<double> updateMin = new List<double>();


        //for (int i = 0; i < this.Component.Params.Input[0].Sources.Count; i++) {
        //    id = this.Component.Params.Input[0].Sources[i].InstanceGuid;
        //    ghSlider = findSlider(id);
        //    double max = (double)ghSlider.Slider.Maximum;
        //    double min = (double)ghSlider.Slider.Minimum;
        //    double slideValue = (double)ghSlider.Slider.Value;

        //    updateMax.Add(max);
        //    updateSlider.Add(slideValue);
        //    updateMin.Add(min);
        //}

        ////outMax = updateMax;
        //out0 = updateSlider;
        ////outMin = updateMin;



        #endregion












    }

    // <Custom additional code> 











    #region customCode
    Grasshopper.Kernel.Special.GH_NumberSlider ghSlider0;
    Grasshopper.Kernel.Special.GH_NumberSlider ghSlider1;

    double map(double value1, double min1, double max1, double min2, double max2) {
        double value2 = min2 + (value1 - min1) * (max2 - min2) / (max1 - min1);
        return value2;
    }
    public Grasshopper.Kernel.Special.GH_NumberSlider findSlider(Guid id) {
        //Get the document that owns this object.
        //GH_Document ghDoc = this.OnPingDocument();
        GH_Document ghDoc = this.GrasshopperDocument;
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
                //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, obj.InstanceGuid.ToString());
            }
        }
        return null;
    }
    #endregion












    // </Custom additional code> 
}