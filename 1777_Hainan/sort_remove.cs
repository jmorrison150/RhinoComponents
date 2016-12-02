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
public class Script_Instance : GH_ScriptInstance
{
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
  private void RunScript(List<Box> boxes, int item, ref object outPlane, ref object outBoxes)
  {


    #region beginScript




      Grasshopper.GUI.GH_Slider_Obsolete slider = FindSlider("animate");

      slider.Max = (boxes.Count);



    //// array of custom type
    //User[] users = new User[3] { new User("Betty", 23),  // name, age
    //                     new User("Susan", 20),
    //                     new User("Lisa", 25) };
    //// sort array by age
    //Array.Sort(users, delegate(User user1, User user2)
    //{
    //    return user1.Age.CompareTo(user2.Age); // (user1.Age - user2.Age)
    //});
    //// write array (output: Susan20 Betty23 Lisa25)
    //foreach (User user in users) Console.Write(user.Name + user.Age + " ");




    ////Point sort
    //Point3d[] pts = new Point3d[boxes.Count];

    //for (int i = 0; i < boxes.Count; i++)
    //{
    //    pts[i] = boxes[i].Center;
    //}
    //Array.Sort(pts, delegate(Point3d pt1, Point3d pt2)
    //{
    //    if(pt1.Z==pt2.Z){
    //        return pt1.X.CompareTo(pt2.X);
    //    }
    //    else{
    //    return pt1.Z.CompareTo(pt2.Z);
    //    }
    //}
    //    );



    //box sort
    Box[] boxArray = boxes.ToArray();
    Array.Sort(boxArray, delegate(Box box1, Box box2)
      {
        if (box1.Center.Z == box2.Center.Z)
        {
          return box1.Center.X.CompareTo(box2.Center.X);
        }
        else
        {
          return box1.Center.Z.CompareTo(box2.Center.Z);
        }
      });



    //remove box
    List<Box> boxList2 = boxArray.ToList();
    Box currentBox = boxList2[item];
    boxList2.RemoveAt(item);


    List<Surface> surfaces = new List<Surface>();

    for (int i = 0; i < boxList2.Count; i++)
    {
      Brep b = boxList2[i].ToBrep();


      try
      {
        for (int j = 0; j < b.Surfaces.Count; j++)
        {
          surfaces.Add(b.Surfaces[j]);
        }
      }
      catch { }

    }







    //output
    outPlane = new Plane(currentBox.Center, Vector3d.ZAxis);
    outBoxes = surfaces;



    #endregion




  }

  // <Custom additional code> 
  public Grasshopper.GUI.GH_Slider_Obsolete FindSlider(string name)
  {
      //Get the document that owns this object.
      GH_Document doc = owner.OnPingDocument();
      //Abort if no such document can be found.
      if (doc == null) { return null; }

      //Iterate over all objects inside the document.
      for (int i = 0; i < doc.ObjectCount; i++)
      {
          IGH_DocumentObject obj = doc.Objects[i];
          //First test the NickName of the object against the search name.
          if (obj.NickName.Equals(name, StringComparison.Ordinal))
          {
              //Then try to cast the object to a GH_NumberSlider.
              Grasshopper.GUI.GH_Slider_Obsolete sld_obj = obj as Grasshopper.GUI.GH_Slider_Obsolete;
              if (sld_obj != null) { return sld_obj; }
          }
      }
      return null;
  }


  // </Custom additional code> 
}