using Rhino;
using Rhino.Geometry;
using Grasshopper;


using System;
using System.Collections.Generic;


namespace gsd {
    public class EquateTM : Grasshopper.Kernel.GH_Component {

        public EquateTM() : base(".equateTM", ".equateTM", "plug in 3 equations", "Extra", "surfTM") { }
        public override Guid ComponentGuid {
            get { return new Guid("{7F784857-F965-4373-8988-9A7DC00C526B}"); }
        }
        protected override System.Drawing.Bitmap Internal_Icon_24x24 {
            get {
                return gsd.Properties.Resources.piTm;
            }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {

            pManager.AddNumberParameter("antecedents_i", "antecedents_i", "plug in an 'evaluate' component with i,j ranges", Grasshopper.Kernel.GH_ParamAccess.tree);
            pManager.AddNumberParameter("antecedents_j", "antecedents_j", "plug in an 'evaluate' component with i,j ranges", Grasshopper.Kernel.GH_ParamAccess.tree);
            pManager.AddNumberParameter("antecedents_s", "antecedents_s", "plug in an 'evaluate' component with i,j ranges", Grasshopper.Kernel.GH_ParamAccess.tree);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_PointParam("outPoints", "outPoints", "Point3d output", Grasshopper.Kernel.GH_ParamAccess.tree);
            pManager.Register_SurfaceParam("outSurface", "outSurface", "Surface", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_MeshParam("outMesh", "outMesh", "Mesh output", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_CurveParam("iThreads", "iThreads", "iThreads", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_CurveParam("jThreads", "jThreads", "jThreads", Grasshopper.Kernel.GH_ParamAccess.list);

        }
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {

            Grasshopper.DataTree<Point3d> points;
            //Point3d[] updatePoints = null;
            Surface updateSurface = null;
            Mesh updateMesh = new Mesh();
            Polyline[] updateIThreads = null;
            Polyline[] updateJThreads = null;


            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Number> antecedents_i = null;
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Number> antecedents_j = null;
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Number> antecedents_s = null;


            if (!DA.GetDataTree<Grasshopper.Kernel.Types.GH_Number>(0, out antecedents_i)) { this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Remark, "plugin evaluate component"); };
            if (!DA.GetDataTree<Grasshopper.Kernel.Types.GH_Number>(1, out antecedents_j)) { this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Remark, "plugin evaluate component"); }; 
            if (!DA.GetDataTree<Grasshopper.Kernel.Types.GH_Number>(2, out antecedents_s)) { this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Remark, "plugin evaluate component"); };
            if (antecedents_i == null || antecedents_j == null || antecedents_s == null) { return; }



            run(antecedents_i, antecedents_j, antecedents_s, out points, out updateSurface, out updateMesh, out updateIThreads, out updateJThreads);



            DA.SetDataTree(0, points);
            DA.SetData(1, updateSurface);
            DA.SetData(2, updateMesh);
            DA.SetDataList(3, updateIThreads);
            DA.SetDataList(4, updateJThreads);
        
        }
        private void run(Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Number> antecedent_i,
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Number> antecedent_j,
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Number> antecedent_s,
            out Grasshopper.DataTree<Point3d> outPoints, out Surface outSurface, out Mesh outMesh, out Polyline[] iThreads, out Polyline[] jThreads) {

            //empty lists
            outPoints = null; outSurface = null; outMesh = null; iThreads = null;  jThreads  =null;
            Surface updateSurface;
            Mesh updateMesh;
            Polyline[] updateIThreads;
            Polyline[] updateJThreads;


            //basic test for same size array
            if (antecedent_i.DataCount != antecedent_j.DataCount) {
                this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Warning, "lists don't match. make sure i,j have been sent through the range");
                return;
            }
            if (antecedent_i.DataCount != antecedent_s.DataCount) {
                this.AddRuntimeMessage(Grasshopper.Kernel.GH_RuntimeMessageLevel.Warning, "lists don't match. make sure i,j have been sent through the range");
                return;
            }

            //format data tree
            double[][] ptsX = formatTree(antecedent_i);
            double[][] ptsY = formatTree(antecedent_j);
            double[][] ptsZ = formatTree(antecedent_s);

            //parse the points
            Point3d[][] pts = formatPts(ptsX, ptsY, ptsZ);
            ptsTM(pts, out updateSurface, out updateMesh, out updateIThreads, out updateJThreads);

            Grasshopper.DataTree<Point3d> points = new Grasshopper.DataTree<Point3d>();

            //output
            if (pts.Length > 0) {
              
                for (int m = 0; m < pts.Length; ++m) {
                    Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
                    for (int n = 0; n < pts[m].Length; ++n) {
                        points.Insert(pts[m][n], path, n);
                        //updatePoints[m*n+n] = (pts[m][n]);
                    }
                }
            }
            outPoints = points;
            outSurface = updateSurface;
            outMesh = updateMesh;
            iThreads = updateIThreads;
            jThreads = updateJThreads;

        }

        // <Custom additional code> 


        private Grasshopper.DataTree<double> formatI(double[][] indexes) {
            Grasshopper.DataTree<double> tree = new Grasshopper.DataTree<double>();
            for (int m = 0; m < indexes.Length; m++) {
                Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
                for (int n = 0; n < indexes[m].Length; ++n) {
                    tree.Insert((double)m, path, n);
                }
            }
            return tree;
        }


        double[][] formatTree(Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Number> tree) {
            double[][] input = new double[tree.Branches.Count][];
            for (int i = 0; i < input.Length; ++i) {
                input[i] = new double[tree.Branches[i].Count];
                for (int j = 0; j < input[i].Length; j++) {
                    input[i][j] = (double)tree.Branches[i][j].ScriptVariable();
                }
            }
            return input;
        }
        //useful for inside a Script_Instance
        //double[][] formatTree(Grasshopper.DataTree<double> tree) {
        //    double[][] input = new double[tree.Branches.Count][];
        //    for (int i = 0; i < input.Length; ++i) {
        //        input[i] = new double[tree.Branches[i].Count];
        //        for (int j = 0; j < input[i].Length; j++) {
        //            input[i][j] = tree.Branches[i][j];
        //        }
        //    }
        //    return input;
        //}
        Point3d[][] formatPts(double[][] ptsX, double[][] ptsY, double[][] ptsZ) {
            Point3d[][] pts = new Point3d[ptsX.Length][];
            for (int i = 0; i < pts.Length; i++) {
                pts[i] = new Point3d[ptsX[i].Length];
                for (int j = 0; j < pts[i].Length; j++) {
                    pts[i][j].X = ptsX[i][j];
                    pts[i][j].Y = ptsY[i][j];
                    pts[i][j].Z = ptsZ[i][j];
                }
            }
            return pts;
        }
        public void ptsTM(Point3d[][] pts, out Surface surface, out Mesh mesh, out Polyline[] iThreads, out Polyline[] jThreads) {
            surface = null; mesh = null; iThreads = null; jThreads = null;
            if (pts.Length < 1) { return; }
            if (pts[0].Length < 1) { return; }

            //surface
            bool uClosed = false;
            bool vClosed = false;
            //if (iThreads[0][0] == iThreads[0][iThreads[0].Count - 1]) {                    uClosed = true;                }
            //if (jThreads[0][0] == jThreads[0][jThreads[0].Count - 1]) {                    vClosed = true;                }
            int degree = 3;
            int columnsLength = pts.Length;
            int rowsLength = pts[0].Length;
            List<Point3d> points = new List<Point3d>();
            for (int m = 0; m < pts.Length; m++) {
                points.AddRange(pts[m]);
            }
            NurbsSurface ns = NurbsSurface.CreateThroughPoints(points, columnsLength, rowsLength, degree, degree, uClosed, vClosed);
            surface = ns;

            //mesh
            Mesh currentMesh = new Mesh();
            for (int n = 0; n < rowsLength; ++n) {
                for (int m = 0; m < columnsLength; ++m) {
                    currentMesh.Vertices.Add(pts[m][n]);
                }
            }

            for (int n = 0; n < rowsLength - 1; ++n) {
                for (int m = 0; m < columnsLength - 1; ++m) {
                    currentMesh.Faces.AddFace((n * columnsLength) + m, (n * columnsLength) + m + 1, ((n + 1) * columnsLength) + m + 1, ((n + 1) * columnsLength) + m);
                }
            }

            //mesh.UnifyNormals();
            currentMesh.FaceNormals.ComputeFaceNormals();
            mesh = currentMesh;


            //output threads
            //updateIThreads = new Polyline[columnsLength];
            iThreads = new Polyline[columnsLength];
            for (int m = 0; m < iThreads.Length; ++m) {
                iThreads[m] = new Polyline(pts[m]);
            }


            jThreads = new Polyline[rowsLength];

            //updateJThreads = new Polyline[rowsLength];
            for (int n = 0; n < jThreads.Length; ++n) {
                Point3d[] currentPts = new Point3d[columnsLength];
                for (int m = 0; m < columnsLength; ++m) {
                    currentPts[m] = pts[m][n];
                }
                jThreads[n] = new Polyline(currentPts);
            }
        }



        // </Custom additional code> 

    }
}