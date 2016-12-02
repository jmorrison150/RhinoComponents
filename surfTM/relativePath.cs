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
    class path {
        private void RunScript(ref object path) {
            //change these two lines to customize the file path
            string relativeFolder = "\\01";
            string relativeFile = "\\iThread3.txt";

            string dirPath;
            string defaultPath;

            Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
            System.IO.DirectoryInfo dir = System.IO.Directory.GetParent(doc.Path);
            dirPath = dir.FullName;
            defaultPath = dir.FullName + relativeFolder + relativeFile;
            path = defaultPath;

            System.IO.FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = defaultPath;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime;
            watcher.Filter = "*.txt";

            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        // <Custom additional code> 
        void OnChanged(object sender, FileSystemEventArgs e) {
            System.Timers.Timer countDown = new System.Timers.Timer(3 * 1000);
            countDown.Start();
            owner.ExpireSolution(true);
        }

        // </Custom additional code> 
    }
}