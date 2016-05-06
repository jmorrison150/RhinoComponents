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



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance {
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
	private void RunScript(Brep brep, double xy, double z, int count, ref object A, ref object B) {



		#region runScript

		List<Curve> curves = new List<Curve>();
		Vector3d axis = Vector3d.XAxis;
		BoundingBox b = brep.GetBoundingBox(false);
		LineCurve lineCurve;
		lineCurve = new LineCurve(new Point3d(b.Min.X, b.Min.Y, b.Min.Z)+( axis*z ), new Point3d(b.Max.X, b.Min.Y, b.Min.Z));
		Point3d[] points;
		Plane[][] planes;
		Curve[][] cvs;


			lineCurve.DivideByLength(xy, true, out points);
			planes = new Plane[points.Length][];
			cvs = new Curve[planes.Length][];


			for(int j = 0; j < planes.Length; j++) {
				cvs[j] = new Curve[3];
				planes[j] = new Plane[2];
				planes[j][0] = new Plane(points[j], axis);
				planes[j][1] = new Plane(points[j]-( axis*z ), axis);
				Curve[] intCurves;
				Point3d[] intPts;
				Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, planes[j][0], Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intCurves, out intPts);
				for(int k = 0; k < 1; k++) {
					cvs[j][0] = intCurves[k];
				}

				Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, planes[j][1], Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, out intCurves, out intPts);
				for(int k = 0; k < 1; k++) {
					cvs[j][1] = intCurves[k];
				}
		}

		for(int i = 0; i < cvs.Length; i++) {
			double scaleXY = 0.85;
			double scaleZ = 0.98;
			BoundingBox bb = cvs[i][0].GetBoundingBox(true);
			BoundingBox bb1 = cvs[i][1].GetBoundingBox(true);
			bb.Union(bb1);
			Point3d mid = new Point3d(( bb.Max.X + bb.Min.X ) * 0.5, ( bb.Max.Y + bb.Min.Y ) * 0.5, bb.Min.Z);
			Plane plane = new Plane(mid, Vector3d.ZAxis);
			Transform scaleHorizontal = Transform.Scale(plane, scaleXY, scaleXY, scaleZ);
			cvs[i][2] = Curve.CreateMeanCurve(cvs[i][0],cvs[i][1]);
			cvs[i][2].Transform(scaleHorizontal);

			for(int j = 0; j < cvs[i].Length; j++) {
				curves.Add(cvs[i][j]);
			}



		}

			List<Line> lines = new List<Line>();
		for(int i = 0; i < cvs.Length; i++) {
			Curve x = cvs[i][0];
			Curve y = cvs[i][2];

			Point3d[] pointsX0, pointsY;
			x.DivideByCount(count,true, out pointsX0);
			y.DivideByCount(count, true, out pointsY);

			for(int j = 2; j < pointsX0.Length; j+=2) {
				Line l = new Line(pointsX0[j],pointsY[j-1]);
				lines.Add(l);
				Line l1 = new Line(pointsX0[j-2],pointsY[j-1]);
				lines.Add(l1);
			}

			Point3d[] pointsX1;
			x = cvs[i][1];
			y = cvs[i][2];
			x.DivideByCount(count, true, out pointsX1);
			y.DivideByCount(count, true, out pointsY);

			for(int j = 2; j < pointsX1.Length; j+=2) {
				Line l = new Line(pointsX1[j], pointsY[j-1]);
				lines.Add(l);
				Line l1 = new Line(pointsX1[j-2], pointsY[j-1]);
				lines.Add(l1);
				Line l2 = new Line(pointsX0[j],pointsX1[j]);
				lines.Add(l2);
			}

		}



		A = curves;
		B = lines;
		#endregion







	}

	// <Custom additional code> 

	// </Custom additional code> 
}