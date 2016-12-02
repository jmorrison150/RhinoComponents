using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsd {
    public class Util {
        
        public double map(double number, double low1, double high1, double low2, double high2) {
            return low2 + (high2 - low2) * (number - low1) / (high1 - low1);
}


    }
}
