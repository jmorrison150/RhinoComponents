using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;

using System;
using System.Drawing;
using System.Collections.Generic;


namespace gsd {
    class meshOffset {
        private void RunScript(List<Brep> x, double y, ref object A) {

            #region beginScript

            List<Mesh> updateMeshes = new List<Mesh>();
            for (int i = 0; i < x.Count; ++i) {
                Mesh[] ms = Mesh.CreateFromBrep(x[i], MeshingParameters.Smooth);
                for (int j = 1; j < ms.Length; ++j) {
                    ms[0].Append(ms[j]);
                }

                ms[0].Weld(0.001);
                Mesh m1 = ms[0].Offset(y, true);
                Mesh m2 = ms[0].Offset(-y, true);
                m1.Append(m2);
                updateMeshes.Add(m1);
            }

            A = updateMeshes;
            #endregion

        }
    }
}