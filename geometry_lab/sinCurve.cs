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


namespace gsd {
    /// <summary>
    /// This class will be instantiated on demand by the Script component.
    /// </summary>
    public class Equation : GH_ScriptInstance {
        Equation() { }
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
        public override void RunScript(Curve curve, double slider, ref object outCurve) {
            if (curve == null) { return; }



            //declare variables
            int curveResolution = 100;
            double amplitude = 50.0;
            double frequency = 10.0;
            double phaseShift = 0.0;

            //draw the curve
            double[] parameters = curve.DivideByCount(curveResolution, true);
            Point3d[] points = new Point3d[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                double equation;
                Point3d curvePoint = curve.PointAt(parameters[i]);
                Plane frame;
                curve.FrameAt(parameters[i], out frame);




                //the equation is divided into several lines for legibility
                //start with something that changes, like normalized length
                //you can change the slider to override any variable, try amplitude
                //amplitude = slider;

                equation = (double)i / (parameters.Length - 1);
                equation *= (2 * Math.PI);
                equation *= frequency;
                equation *= decay(i);
                equation += phaseShift;
                equation = Math.Sin(equation);
                equation *= amplitude;
                equation *= decay(i);




                //output
                points[i] = curvePoint + (frame.YAxis * equation);
            }
            outCurve = Curve.CreateInterpolatedCurve(points, 3);



        }

        // <Custom additional code> 

        double decay(double i) {
            return Math.Sin(i * i * 0.0001);
        }
        // </Custom additional code> 
    }
}