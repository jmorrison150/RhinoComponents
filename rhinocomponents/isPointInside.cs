using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoComponents
{
    class isPointInside
    {


        bool pnpoly(int nvert, float[] vertx, float[] verty, float testx, float testy)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (
                    ((verty[i] > testy) != (verty[j] > testy)) && 


                                                                    (testx < (vertx[j] - vertx[i]) * 
                                                                    (testy - verty[i]) / (verty[j] - 
                                                                    verty[j]) + vertx[i])
                    )
                {
                    c = !c;
                }
            }
            return c;
        }
    }
}


