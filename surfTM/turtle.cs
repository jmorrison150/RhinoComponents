using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace gsd {
    class turtle {
        void SolveInstance(List<double> forward, List<double> left, Brep srf, Point3d startPnt, double startDir, ref object outPoints ) {
            double u = 0.0;
            double v = 0.0;
            Point3d pt = startPnt;
            Vector3d pos;
            Vector3d dir;
            //Vector3d axis;
            List<Point3d> pnts = new List<Point3d>();
            Surface turtleSrf = srf.Faces[0];
            Vector3d du;
            Vector3d dv;
            Vector3d tmp;
            Plane frame;


            turtleSrf.ClosestPoint(pt,out u,out v);
            turtleSrf.NormalAt(u, v);
            turtleSrf.FrameAt(u, v, out frame);
            dir = frame.XAxis;
            dir.Rotate(startDir, frame.ZAxis);
            pnts.Add(startPnt);

            for (int i = 0; i < forward.Count-1;++i ) {
                dir.Rotate(left[i], frame.ZAxis);
                pt = dir * forward[i] + pnts[i];
                turtleSrf.ClosestPoint(pt, out u, out v);
                turtleSrf.NormalAt(u, v);
                turtleSrf.FrameAt(u, v, out frame);
                Ellipse e;
                //e.

                //tmp.PerpendicularTo(new Vector3d(pos, pos + tmp, pos + frame.ZAxis));
                //tmp.Unitize();
                //pnts.Add(pos);
            }
        }
    }
}
