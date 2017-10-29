using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;


using Rhino;
using Rhino.Geometry;
//using Grasshopper.Kernel;

namespace gsd {
    public class SurfTM : Grasshopper.Kernel.GH_Component {
        Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
        List<string> paths;
        
        //methods
        public SurfTM() : base(".surfTM", ".surfTM", "MathCAD link", "Extra", "surfTM") {
            //base. Grasshopper.Kernel.IGH_ActiveObject owner;
            //public IGH_ActiveObject owner;
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("{0731c1e2-03e1-46ae-a138-8694dee13337}");
            }
        }
        protected override Bitmap Internal_Icon_24x24 {
            get {

                //return surfTM.Properties.Resources.surfTM_icon;
                return Properties.Resources.surfTM_icon;
                //return gsd.Properties.Resources.surfTM_icon;
            }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            pManager.AddTextParameter("paths","paths","filepaths for iThread.txt",Grasshopper.Kernel.GH_ParamAccess.list);
            pManager[0].Optional = true;
        }
        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager) {
            pManager.Register_PointParam("outPoints","outPoints","Point3d output",Grasshopper.Kernel.GH_ParamAccess.tree);
            pManager.Register_SurfaceParam("outSurface", "outSurface", "Surface", Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_MeshParam("outMesh", "outMesh","Mesh output",Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_CurveParam("iThreads","iThreads","iThreads",Grasshopper.Kernel.GH_ParamAccess.list);
            pManager.Register_CurveParam("jThreads","jThreads","jThreads",Grasshopper.Kernel.GH_ParamAccess.list);

        }
        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA) {
            

            #region beginScript


            //get input
            paths = new List<string>();
            if (!DA.GetDataList<string>(0, paths)) {
                paths.Add(getDefaultPath());
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,"the default FilePath has been used: save MathCAD and GH file in the same directory");
            }


            Grasshopper.DataTree<Point3d> updatePoints = new Grasshopper.DataTree<Point3d>();

            //List<Point3d> updatePoints = new List<Point3d>();
            List<Surface> updateSurfaces = new List<Surface>();
            List<Mesh> updateMeshes = new List<Mesh>();
            List<Polyline> updateIThreads = new List<Polyline>();
            List<Polyline> updateJThreads = new List<Polyline>();



            //select only the iThread.txt
            //many iThreads_n.txt can be selected
            for (int i = 0; i < paths.Count;++i ) {
           
       
                //get directory
                if (paths[i][0] == '.') {
                        GH_Document ghDoc = this.OnPingDocument();
                        string path = ghDoc.FilePath;
                        //paths[i] = System.IO.Directory.GetParent(path) + paths[i];

                        if (paths[i][1] == '\\' && paths[i][2] == '\\') {
                            paths[i] = System.IO.Directory.GetParent(path) + paths[i].Substring(2);

                            //paths[i] = path + paths[i].Substring(2);
                            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "FilePath = relative directory of GH Document");

                        } else {
                            paths[i] = System.IO.Directory.GetParent(path) + paths[i].Substring(1);

                            //paths[i] = path + paths[i].Substring(1);
                            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "FilePath = relative directory of GH Document");

                        }
                    //paths[i] = System.IO.Directory.GetParent(doc.Path) + paths[i].Substring(1);
                }else if(paths[i][0]=='\\' && paths[i][1]=='\\'){
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,"is iThread.txt on a network drive?");
                }else if (paths[i][0]=='\\'){
                    try {
                        GH_Document ghDoc = this.OnPingDocument();
                        string path = ghDoc.FilePath;
                        paths[i] = System.IO.Directory.GetParent(path) + paths[i];

                        //paths[i] = path + paths[i];
                        //paths[i] = System.IO.Directory.GetParent(doc.Path) + paths[i];
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "FilePath = relative directory of GH Document");
                    } catch { this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,"relative path failed"); }
                }

                System.IO.DirectoryInfo dir = System.IO.Directory.GetParent(paths[i]);
                string currentPath = dir.FullName;


                //sets file names
                string[] threadPaths = new string[3];
                threadPaths[0] =currentPath + "\\iThread" + paths[i].Substring(currentPath.Length+8);
                threadPaths[1] =currentPath + "\\jThread" + paths[i].Substring(currentPath.Length+8);
                threadPaths[2] =currentPath + "\\shape"   + paths[i].Substring(currentPath.Length+8);


                //get length of rows and columns
                string[] stringRow = System.IO.File.ReadAllLines(threadPaths[0]);
                if (stringRow.Length== 1) {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "is iThread.txt blank? Check the MathCAD. Make sure APPENDPRN is assigned");
                        return;
                    }
                //this number should be constant
                int fixedWidth = 17;
                if (stringRow[0][fixedWidth]!=' ') {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,"Is iThread.txt correct? Check the MathCAD. PRNCOLWIDTH = 17");
                }
                //find the size of the matrix
                string r0 = stringRow[0].Substring(0, fixedWidth);
                int rowsLength = int.Parse(r0, System.Globalization.NumberStyles.Float);
                //assert(stringRow.Length - 1 != rowsLength);
                int columnsLength = stringRow[0].Length / fixedWidth;
                //make arrays
                //point array with [m][n] indexing
                Point3d[][] pts = new Point3d[columnsLength][];
                for (int m = 0; m < pts.Length; ++m) {
                    pts[m] = new Point3d[rowsLength];
                    for (int n = 0; n < pts[m].Length; ++n) {
                        pts[m][n] = Point3d.Origin;
                    }
                }

                //do this for each one of the triplets
                for (int j = 0; j < 3; ++j) {
                    stringRow = System.IO.File.ReadAllLines(threadPaths[j]);

                    for (int n = 1; n <= rowsLength; ++n) {
                        for (int m = 0; m < columnsLength; ++m) {
                            string stringValue = stringRow[n].Substring(m * fixedWidth, fixedWidth);
                            double doubleValue = double.Parse(stringValue, System.Globalization.NumberStyles.Float);

                            //(i / pathCount)-fileNumber
                            switch (j) {
                                case 0:
                                    pts[m][n-1].X = doubleValue;
                                    break;
                                case 1:
                                    pts[m][n-1].Y = doubleValue;
                                    break;
                                case 2:
                                    pts[m][n-1].Z = doubleValue;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }


                //output

                for (int m = 0; m < pts.Length; ++m) {
                    Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(m);
                    for (int n = 0; n < pts[m].Length; ++n) {
                        updatePoints.Insert(pts[m][n], path, n);
                        //updatePoints[m*n+n] = (pts[m][n]);
                    }
                }


         
                

 
                //surface
                bool uClosed = false;
                bool vClosed = false;
                //if (iThreads[0][0] == iThreads[0][iThreads[0].Count - 1]) {                    uClosed = true;                }
                //if (jThreads[0][0] == jThreads[0][jThreads[0].Count - 1]) {                    vClosed = true;                }
                int degree = 3;
                List<Point3d> points = new List<Point3d>();
                for (int m = 0; m < pts.Length; m++) {
                    points.AddRange(pts[m]);
                }
                NurbsSurface ns = NurbsSurface.CreateThroughPoints(points, columnsLength, rowsLength, degree, degree, uClosed, vClosed);
                updateSurfaces.Add(ns);

                //mesh
                Mesh mesh = new Mesh();
                for (int n = 0; n < rowsLength; ++n) {
                    for (int m = 0; m < columnsLength; ++m) {
                        mesh.Vertices.Add(pts[m][n]);
                    }
                }

                for (int n = 0; n < rowsLength - 1; ++n) {
                    for (int m = 0; m < columnsLength - 1; ++m) {
                        mesh.Faces.AddFace((n * columnsLength) + m, (n * columnsLength) + m + 1, ((n + 1) * columnsLength) + m + 1, ((n + 1) * columnsLength) + m);
                    }
                }

                //mesh.UnifyNormals();
                mesh.FaceNormals.ComputeFaceNormals();
                updateMeshes.Add(mesh);


                //output threads
                //updateIThreads = new Polyline[columnsLength];
                for (int m = 0; m < columnsLength; ++m) {
                    updateIThreads.Add(new Polyline(pts[m]));
                }


                //updateJThreads = new Polyline[rowsLength];
                for (int n = 0; n < rowsLength; ++n) {
                    //Point3d
                    Point3d[] currentPts = new Point3d[columnsLength];
                    for (int m = 0; m < columnsLength; ++m) {
                        currentPts[m] = pts[m][n];
                    }
                    updateJThreads.Add(new Polyline(currentPts));
                }

            }//end each file

            DA.SetDataTree(0, updatePoints);
            DA.SetDataList(1, updateSurfaces);
            DA.SetDataList(2, updateMeshes);
            DA.SetDataList(3, updateIThreads);
            DA.SetDataList(4, updateJThreads);

            #endregion
        }
        
        private List<GH_FileWatcher> m_watchers;

        protected override void AfterSolveInstance() {
            m_watchers = new List<GH_FileWatcher>();
            base.AfterSolveInstance();
            if(paths.Count>0){                
                
                //fileWatcher(paths);
            for (int i = 0; i < paths.Count; i++) {
                
            this.m_watchers.Add(GH_FileWatcher.CreateFileWatcher(paths[i], GH_FileWatcherEvents.All,
                new GH_FileWatcher.FileChangedSimple(this.FileEventHandler)));
            }
            }
        }
        private void FileEventHandler(string filename) {
            m_watchers.Clear();
            this.ExpireSolution(false);
            GH_Document ghDocument = this.OnPingDocument();
            if (ghDocument == null)
                return;
            ghDocument.NewSolution(false);
        }


        private string getDefaultPath() {
            //Grasshopper.Kernel.GH_Document.EnableSolutions = true;
            string dirPath;
            string defaultPath;

                try {
                    GH_Document ghDoc = this.OnPingDocument();
                    string filePath = ghDoc.FilePath;

                    System.IO.DirectoryInfo dir = System.IO.Directory.GetParent(filePath);
                    dirPath = dir.FullName;
                } catch {
                    try {
                        //System.IO.DirectoryInfo dir = System.IO.Directory.GetParent("%USERPROFILE%");
                        string pathWithEnv = @"%USERPROFILE%\Desktop";
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "FilePath = Desktop");
                        dirPath = Environment.ExpandEnvironmentVariables(pathWithEnv);
                    } catch {
                        dirPath = "C:\\";
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,"FilePath = C:\\");
                    }

                }
                defaultPath = dirPath + "\\iThread.txt";
                return defaultPath;
            }
        void fileWatcher(List<string>paths) {
            if(paths.Count>0){
            //get directory
            if (paths[0][0] == '.') {
                paths[0] = System.IO.Directory.GetParent(doc.Path) + paths[0].Substring(1);
            }

            System.IO.DirectoryInfo dir = System.IO.Directory.GetParent(paths[0]);
            string currentPath = dir.FullName;


            //for file sync
            //GH_Document.EnableSolutions = true;
            System.IO.FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime;
            watcher.Filter = "*.txt";
            watcher.Path = currentPath;
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }
        }
        void OnChanged(object sender, FileSystemEventArgs e) {
            //System.Timers.Timer wait = new System.Timers.Timer(2 * 1000);
            //wait.Start();

            System.IO.WatcherChangeTypes type = e.ChangeType;
            this.ExpireDownStreamObjects();
        }
    }
}