
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
    class Class4 {

        double map(double currentValue, double oldMin, double oldMax, double newMin, double newMax) {
            return 1;
        }

        private void brickExample (){ 
            Point3d[] pts = new Point3d[3];
            Curve c = Curve.CreateInterpolatedCurve(pts,3);
            c.ChangeDimension(1);
            c.Degree;
            c.DerivativeAt()
                c.FrameAt()
                    double t;
                    c.DerivativeAt(t,2);
            c.
        
        
        
        }
        void run(){

            List<double> lengths = new List<double>();



            Curve c;
            c.PointAtLength(lengths[lengths.Count-2]);






        string file = "";
            System.Drawing.Bitmap bitmap1 = new Bitmap(file);
            Point3d[] pts = new Point3d[3];
            for (int i = 0; i < pts.Length; i++)
			{
			 bool clip = imageClip(testPoint,surface,bitmap,128);
                if(clip){brick.}
			}
        }
            
            
            
            bool imageClip(Point3d testPoint, Surface surface, System.Drawing.Bitmap bitmap, double threshold){

            int normalizedPixelX, normalizedPixelY;
            double u, v, normalizedU, normalizedV;
            surface.ClosestPoint(testPoint, out u, out v);
            normalizedU = surface.Domain(0).NormalizedParameterAt(u);
            normalizedV = surface.Domain(1).NormalizedParameterAt(v);


    
    
            normalizedPixelX = (int) map(normalizedU, 0.0, 1.0, 1.0, (double) bitmap.Width);
            normalizedPixelY = (int) map(normalizedV, 0.0, 1.0, 1.0, (double) bitmap.Height);


            System.Drawing.Color pixelColor = bitmap.GetPixel(normalizedPixelX, normalizedPixelY);
            if(pixelColor.R<threshold) {
                return true;
            } else { return false; }
        }
            
            //Rhino.Display.Text3d txt = new Rhino.Display.Text3d(text, plane, height);


        Bitmap image1 = new Bitmap(@"C:\Documents and Settings\All Users\" 
            + @"Documents\My Music\music.bmp", true);






        }
    }
}
