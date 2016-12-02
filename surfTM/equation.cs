//using Rhino;
//using Rhino.Geometry;
//using Rhino.DocObjects;
//using Rhino.Collections;

//using GH_IO;
//using GH_IO.Serialization;
//using Grasshopper;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Data;
//using Grasshopper.Kernel.Types;

//using System;
//using System.IO;
//using System.Xml;
//using System.Xml.Linq;
//using System.Linq;
//using System.Data;
//using System.Drawing;
//using System.Reflection;
//using System.Collections;
//using System.Windows.Forms;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;



///// <summary>
///// This class will be instantiated on demand by the Script component.
///// </summary>
//public class Script_Instance : GH_ScriptInstance {
//    #region Utility functions
//    /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
//    /// <param name="text">String to print.</param>
//    private void Print(string text) { /* Implementation hidden. */ }
//    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
//    /// <param name="format">String format.</param>
//    /// <param name="args">Formatting parameters.</param>
//    private void Print(string format, params object[] args) { /* Implementation hidden. */ }
//    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
//    /// <param name="obj">Object instance to parse.</param>
//    private void Reflect(object obj) { /* Implementation hidden. */ }
//    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
//    /// <param name="obj">Object instance to parse.</param>
//    private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
//    #endregion

//    #region Members
//    /// <summary>Gets the current Rhino document.</summary>
//    private readonly RhinoDoc RhinoDocument;
//    /// <summary>Gets the Grasshopper document that owns this script.</summary>
//    private readonly GH_Document GrasshopperDocument;
//    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
//    private readonly IGH_Component Component;
//    /// <summary>
//    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
//    /// Any subsequent call within the same solution will increment the Iteration count.
//    /// </summary>
//    private readonly int Iteration;
//    #endregion

//    /// <summary>
//    /// This procedure contains the user code. Input parameters are provided as regular arguments,
//    /// Output parameters as ref arguments. You don't have to assign output parameters,
//    /// they will have a default value.
//    /// </summary>
//    private void RunScript(double a, ref object A)
//  {



//    Grasshopper.Kernel.Expressions.GH_ExpressionParser p = new Grasshopper.Kernel.Expressions.GH_ExpressionParser();
//    string exp = "1+2+3";
//    Grasshopper.Kernel.Expressions.GH_ExpressionSyntaxWriter.RewriteForEvaluator(exp);
//    Grasshopper.Kernel.Expressions.GH_Variant v = p.Evaluate(exp);




//    Print(sliderValue);
//    A = getValues().ToList();
//    Reflect(sliderChanged);




//  }

//    // <Custom additional code> 

//    string sliderValue = "blank";

//    void checkValues() {
//        //   this.ValuesChanged += new 
//        //   ghDigitScroller1.ValueChanged += new GH_DigitScroller.ValueChangedEventHandler(this.Menu_ScreenSizeChanged);

//    }
//    public double[] getValues() {
//        List<Grasshopper.Kernel.Special.GH_NumberSlider> sliders = getSliders();
//        double[] values = new double[sliders.Count];
//        for (int i = 0; i < sliders.Count; i++) {
//            values[i] = (double)sliders[i].CurrentValue;
//        }
//        return values;
//    }
//    public List<Grasshopper.Kernel.Special.GH_NumberSlider> getSliders() {
//        //empty list
//        List<Grasshopper.Kernel.Special.GH_NumberSlider> sliders = new List<Grasshopper.Kernel.Special.GH_NumberSlider>();

//        //Get the document that owns this object.
//        GH_Document ghDoc = owner.OnPingDocument();
//        //Abort if no such document can be found.
//        if (ghDoc == null) { return null; }

//        //Iterate over all objects inside the document.
//        for (int i = 0; i < ghDoc.ObjectCount; i++) {

//            //Then try to cast the object to a GH_NumberSlider.
//            Grasshopper.Kernel.Special.GH_NumberSlider sld_obj = ghDoc.Objects[i] as Grasshopper.Kernel.Special.GH_NumberSlider;
//            if (sld_obj != null) { sliders.Add(sld_obj); }
//        }
//        return sliders;
//    }


//    public delegate void ValueChangedEventHandler(Object sender, Grasshopper.GUI.Base.GH_SliderEventArgs e);
//    public event ValueChangedEventHandler sliderChanged;
//    void handleSliderChanged(Object sender, Grasshopper.GUI.Base.GH_SliderEventArgs e) {
//        //add custom code
//        sliderValue = sliderChanged.ToString();
//    }
//    //myObj.sliderChanged += ValueChangedEventHandler(handleSliderChanged);



//    // </Custom additional code> 
//}