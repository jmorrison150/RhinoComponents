using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Geometry;

namespace _1777_HainanHotelandServiceApartment
{
    class Class9
    {

        void run(){
        
            Plane plane = Plane.WorldXY;
            
            //Point3d from = new Point3d(0,0,0);
            //Point3d to = new Point3d(1,0,0);
            //LineCurve profile = new LineCurve(from, to);
            //ClippingPlaneSurface clip = ClippingPlaneSurface.CreateExtrusion(profile, Vector3d.YAxis);
            //clip.Plane = plane;

            ClippingPlaneSurface clip;
            clip.Plane = plane;





        }
        

    }
}
