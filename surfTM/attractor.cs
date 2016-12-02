using Rhino;
using Rhino.Geometry;

using Grasshopper;

using System;
using System.Drawing;
using System.Collections.Generic;


namespace gsd {
    
    public class Attractor {
        public Point3d[] run(int objNo, double res0, double res1, double value0, double value1, double value3, double value4, double value5, ref object outPoints) {


            #region beginScript


            //int objNo = 21;
            //double value0 = input.X * 2.0;
            //double value1 = input.Y * 2.0;




            Point3d[] xyz = new Point3d[objNo];
            xyz[0] = Point3d.Origin;
            //for (int i = 1; i <= xyz.Length; i++) {

            //    nextX = ((Math.Sin(value0 * xyz[i-1].Y)) + (value1 * (Math.Cos(value0 * xyz[i-1].X ))));
            //    nextY = ((Math.Sin(-value0 * xyz[i-1].X)) + (-value1 * (Math.Cos(-value0 * xyz[i-1].Y ))));
            //    nextZ = ((Math.Sin(-value0 * xyz[i-1].Y)) + (value1 * (Math.Cos(-value0 * xyz[i-1].Y ))));

            //    xyz[i%xyz.Length] = new Point3d(nextX,nextY,nextZ);
            //}


            for (int i = 1; i < xyz.Length; i++) {

                double nextX;
                double nextY;
                double nextZ;

                nextZ = ((Math.Sin(i * res1 * -value1 * xyz[i - 1].Y)) + (value5 * (Math.Cos(-value1 * i * res1 * xyz[i - 1].X))));
                nextX = ((Math.Sin(i * res1 * value1 * xyz[i - 1].X)) + (value3 * (Math.Cos(value0 * i * res0 * xyz[i - 1].Y)))) + nextZ * 0.5;
                nextY = ((Math.Sin(i * res0 * value0 * xyz[i - 1].Y)) + (-value4 * (Math.Cos(value1 * i * res1 * xyz[i - 1].Y)))) + nextZ * 0.5;


                xyz[i % xyz.Length] = new Point3d(nextX, nextY, nextZ);
            }


            //outPoints = xyz.ToList();
            List<Point3d> updatePoints = new List<Point3d>();
            for (int i = 1; i < xyz.Length - 1; i++) {
                updatePoints.Add(xyz[i]);
            }
            outPoints = updatePoints;
            return updatePoints.ToArray();
            #endregion

        }
    }

}