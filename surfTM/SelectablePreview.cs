// Type: SelectablePreview.SelectablePreviewComponent
// Assembly: SelectablePreview, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: E404D15B-DBC5-48CF-9A20-2C9AA344E5C8
// Selectable Preview by Alan Tai 
// http://flowingbits.com/tool/selectable-preview-component-for-grasshopper/


using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using Rhino;
using Rhino.DocObjects;
//using SelectablePreview.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;




namespace SelectablePreview {
    public class SelectablePreviewComponent : GH_Component {
        private bool maintainPath = false;
        private bool sortByIndex = false;
        private List<string> storedPath = new List<string>();

        public bool inputLock { get; set; }
        public bool respondToSelection { get; set; }
        public bool freezePreviewObjects { get; set; }
        public bool resetStoredPath { get; set; }
        public bool generatePreview { get; set; }
        protected override Bitmap Icon {
            get {
                return gsd.Properties.Resources.gsd08;
            }
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("{6468af16-af06-471b-9992-1e88cc7b7c20}");
            }
        }
        public SelectablePreviewComponent(): base("selectable", "selectable", "Selectable Preview Object", "Extra", "Util") {
            this.inputLock = false;
            this.resetStoredPath = false;
            this.freezePreviewObjects = false;
            this.generatePreview = true;
            this.respondToSelection = true;
            this.Params.ParameterSourcesChanged += new GH_ComponentParamServer.ParameterSourcesChangedEventHandler(this.Params_ParameterSourcesChanged);
            this.ObjectChanged += new IGH_DocumentObject.ObjectChangedEventHandler(this.SelectablePreviewComponent_ObjectChanged);
        }
        private void SelectablePreviewComponent_ObjectChanged(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e) {
            this.UpdatePreview();
        }
        private void Params_ParameterSourcesChanged(object sender, GH_ParamServerEventArgs e) {
            if (this.inputLock || e.Parameter.SourceCount != 0)
                return;
            this.RemovePreviewObjects();
        }
        public void UpdatePreview() {
            if (this.inputLock)
                return;
            this.OnPingDocument();
            if (this.Locked)
                this.RemovePreviewObjects();
            else if (this.Hidden)
                this.HidePreviewObjects();
            else
                this.ShowPreviewObjects();
        }
        public void ForceExpireSolution(bool freezePreview, bool resetPath, bool generatePreview = false) {
            this.generatePreview = generatePreview;
            this.freezePreviewObjects = freezePreview;
            this.resetStoredPath = resetPath;
            this.ExpireSolution(true);
            this.freezePreviewObjects = false;
            this.resetStoredPath = false;
            this.generatePreview = false;
        }
        public void RemovePreviewObjects() {
            this.respondToSelection = false;
            foreach (RhinoObject rhinoObject in RhinoDoc.ActiveDoc.Objects.FindByUserString("Preview(" + (object)this.InstanceGuid + ")", "*", true))
                RhinoDoc.ActiveDoc.get_Objects().Delete(rhinoObject, true);
            this.respondToSelection = true;
        }
        public void GeneratePreViewObjectsI(GH_Structure<IGH_GeometricGoo> inGeoTree) {
            string str = "Preview(" + (object)this.InstanceGuid + ")";
            RhinoDoc.ActiveDoc.Layers.Add("GH_Preview", Color.Maroon);
            int num1 = RhinoDoc.ActiveDoc.Layers.Find("GH_Preview", true);
            if (RhinoDoc.ActiveDoc.Objects.FindByUserString(str, "*", true).Length != 0)
                return;
            int num2 = 0;
            foreach (IGH_GeometricGoo ghGeometricGoo in (IEnumerable<IGH_Goo>)inGeoTree.AllData(false)) {
                ObjectAttributes objectAttributes = new ObjectAttributes();
                objectAttributes.SetUserString(str, num2.ToString());
                objectAttributes.LayerIndex = num1;
                ++num2;
                if (ghGeometricGoo is IGH_BakeAwareData) {
                    Guid obj_guid;
                    ((IGH_BakeAwareData)ghGeometricGoo).BakeGeometry((RhinoDoc)RhinoDoc.ActiveDoc, (ObjectAttributes)objectAttributes, out obj_guid);
                }
            }
        }
        public void HidePreviewObjects() {
            this.respondToSelection = false;
            foreach (RhinoObject rhinoObject in RhinoDoc.ActiveDoc.Objects.FindByUserString("Preview(" + (object)this.InstanceGuid + ")", "*", true))
                RhinoDoc.ActiveDoc.get_Objects().Hide(rhinoObject, true);
            this.respondToSelection = true;
        }
        public void ShowPreviewObjects() {
            this.respondToSelection = false;
            foreach (RhinoObject rhinoObject in RhinoDoc.ActiveDoc.Objects.FindByUserString("Preview(" + (object)this.InstanceGuid + ")", "*", true))
                RhinoDoc.ActiveDoc.get_Objects().Show(rhinoObject, true);
            this.respondToSelection = true;
        }
        public override void CreateAttributes() {
            this.m_attributes = (IGH_Attributes)new AttributesButton((GH_Component)this);
        }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu) {
            GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Toggle Maintain Path", new EventHandler(this.Menu_MaintainPath), true);
            GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Toggle Sort By Tree Order", new EventHandler(this.Menu_SortIndex), true);
            base.AppendAdditionalMenuItems(menu);
        }
        public void Menu_MaintainPath(object s, EventArgs e) {
            this.maintainPath = !this.maintainPath;
            this.ForceExpireSolution(true, false, false);
        }
        public void Menu_SortIndex(object s, EventArgs e) {
            this.sortByIndex = !this.sortByIndex;
            this.ForceExpireSolution(true, false, false);
        }
        public override void AddedToDocument(GH_Document document) {
            base.AddedToDocument(document);
            document.SelectedObjects();
            RhinoDoc.add_SelectObjects(new EventHandler<RhinoObjectSelectionEventArgs>(this.SelectObjects));
            RhinoDoc.DeselectObjects(new EventHandler<RhinoObjectSelectionEventArgs>(this.DeselectObjects));
            RhinoDoc.DeselectAllObjects(new EventHandler<RhinoDeselectAllObjectsEventArgs>(this.DeselectAllObjects));
            document.EnabledChanged += new GH_Document.EnabledChangedEventHandler(this.EnabledChanged);
            document.SettingsChanged += new GH_Document.SettingsChangedEventHandler(this.document_SettingsChanged);
        }
        private void document_SettingsChanged(object sender, GH_DocSettingsEventArgs e) {
            this.UpdatePreview();
        }
        public override void MovedBetweenDocuments(GH_Document oldDocument, GH_Document newDocument) {
            base.MovedBetweenDocuments(oldDocument, newDocument);
            oldDocument.SettingsChanged -= new GH_Document.SettingsChangedEventHandler(this.document_SettingsChanged);
            oldDocument.EnabledChanged -= new GH_Document.EnabledChangedEventHandler(this.EnabledChanged);
            newDocument.EnabledChanged += new GH_Document.EnabledChangedEventHandler(this.EnabledChanged);
            newDocument.SettingsChanged += new GH_Document.SettingsChangedEventHandler(this.document_SettingsChanged);
        }
        public override void RemovedFromDocument(GH_Document document) {
            base.RemovedFromDocument(document);
            this.RemovePreviewObjects();
            EventHandler<RhinoObjectSelectionEventArgs>(this.SelectObjects) += RhinoDoc.SelectObjects;
            //RhinoDoc.SelectObjects .remove_SelectObjects(new );
            RhinoDoc.remove_DeselectObjects(new EventHandler<RhinoObjectSelectionEventArgs>(this.DeselectObjects));
            RhinoDoc.remove_DeselectAllObjects(new EventHandler<RhinoDeselectAllObjectsEventArgs>(this.DeselectAllObjects));
            document.EnabledChanged -= new GH_Document.EnabledChangedEventHandler(this.EnabledChanged);
            document.SettingsChanged -= new GH_Document.SettingsChangedEventHandler(this.document_SettingsChanged);
        }
        public void EnabledChanged(object e, GH_DocEnabledEventArgs arg) {
            if (!arg.Enabled)
                this.RemovePreviewObjects();
            else
                this.ForceExpireSolution(false, false, false);
        }
        public void SelectObjects(object e, RhinoObjectSelectionEventArgs arg) {
            if (this.inputLock || !this.respondToSelection || this.Locked)
                return;
            bool flag = false;
            string str = "Preview(" + (object)this.InstanceGuid + ")";
            RhinoObject[] rhinoObjects = arg.RhinoObjects;
            GH_Structure<IGH_GeometricGoo> ghStructure = new GH_Structure<IGH_GeometricGoo>();
            foreach (RhinoObject rhinoObject in rhinoObjects) {
                string userString = rhinoObject.Attributes.GetUserString(str);
                if (!string.IsNullOrEmpty(userString)) {
                    this.storedPath.Add(userString);
                    flag = true;
                }
            }
            if (flag)
                this.ForceExpireSolution(true, false, false);
        }
        public void DeselectObjects(object e, RhinoObjectSelectionEventArgs arg) {
            if (!this.inputLock || !this.respondToSelection || this.Locked)
                return;
            bool flag = false;
            string str = "Preview(" + (object)this.InstanceGuid + ")";
            RhinoObject[] rhinoObjects = arg.RhinoObjects;
            GH_Structure<IGH_GeometricGoo> ghStructure = new GH_Structure<IGH_GeometricGoo>();
            foreach (RhinoObject rhinoObject in rhinoObjects) {
                string userString = rhinoObject.Attributes.GetUserString(str);
                if (!string.IsNullOrEmpty(userString)) {
                    this.storedPath.Remove(userString);
                    flag = true;
                }
            }
            if (flag)
                this.ForceExpireSolution(true, false, false);
        }
        public void DeselectAllObjects(object e, RhinoDeselectAllObjectsEventArgs arg) {
            if (this.inputLock || !this.respondToSelection || this.Locked)
                return;
            this.ForceExpireSolution(true, true, false);
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
            pManager.AddGeometryParameter("Geometry", "G", "Items to bake", GH_ParamAccess.tree);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
            pManager.AddGeometryParameter("Geometry", "G", "output", GH_ParamAccess.tree);
            pManager.AddTextParameter("Path", "P", "output", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Index", "I", "output", GH_ParamAccess.tree);
        }
        protected override void SolveInstance(IGH_DataAccess DA) {
            string str1 = "Preview(" + (object)this.InstanceGuid + ")";
            GH_Structure<IGH_GeometricGoo> tree;
            if (!DA.GetDataTree<IGH_GeometricGoo>(0, out tree))
                return;
            if (this.resetStoredPath)
                this.storedPath.Clear();
            if (!this.freezePreviewObjects)
                this.RemovePreviewObjects();
            if (!this.inputLock && (this.generatePreview || !this.freezePreviewObjects))
                this.GeneratePreViewObjectsI(tree);
            GH_Structure<GH_String> ghStructure1 = new GH_Structure<GH_String>();
            for (int index1 = 0; index1 < tree.PathCount; ++index1) {
                string str2 = tree.Paths[index1].ToString();
                for (int index2 = 0; index2 < tree.Branches[index1].Count; ++index2) {
                    string str3 = str2 + "(" + index2.ToString() + ")";
                    ghStructure1.Append(new GH_String(str3));
                }
            }
            List<string> list1 = new List<string>();
            foreach (GH_String ghString in (IEnumerable<IGH_Goo>)ghStructure1.AllData(false))
                list1.Add(ghString.ToString());
            GH_Structure<GH_Integer> ghStructure2 = new GH_Structure<GH_Integer>();
            string[] path_segments;
            string index_segment;
            for (int number = 0; number < list1.Count; ++number) {
                GH_Path.SplitPathLikeString(list1[number], out path_segments, out index_segment);
                int[] numArray = Array.ConvertAll<string, int>(path_segments, (Converter<string, int>)(str => Convert.ToInt32(str)));
                int index = Convert.ToInt32(index_segment);
                ghStructure2.Insert(new GH_Integer(number), new GH_Path(numArray), index);
            }
            GH_Structure<IGH_Goo> gooTree1 = new GH_Structure<IGH_Goo>();
            List<IGH_Goo> list2 = new List<IGH_Goo>();
            GH_Structure<IGH_Goo> ghStructure3 = new GH_Structure<IGH_Goo>();
            List<IGH_Goo> list3 = new List<IGH_Goo>();
            GH_Structure<IGH_Goo> gooTree2 = new GH_Structure<IGH_Goo>();
            List<IGH_Goo> list4 = new List<IGH_Goo>();
            GH_Structure<GH_Integer> indTree = new GH_Structure<GH_Integer>();
            List<GH_Integer> indexList = new List<GH_Integer>();
            for (int index = 0; index < this.storedPath.Count; ++index) {
                if (GH_Path.SplitPathLikeString(list1[Convert.ToInt32(this.storedPath[index])], out path_segments, out index_segment)) {
                    GH_Path path = new GH_Path(Array.ConvertAll<string, int>(path_segments, (Converter<string, int>)(str => Convert.ToInt32(str))));
                    int number = Convert.ToInt32(index_segment);
                    if (this.maintainPath) {
                        gooTree1.Append((IGH_Goo)tree.get_Branch(path)[number], path);
                        ghStructure3.Append((IGH_Goo)new GH_String(path.ToString()), path);
                        gooTree2.Append((IGH_Goo)new GH_Integer(number), path);
                        indTree.Append((GH_Integer)ghStructure2.get_Branch(path)[number], path);
                    } else {
                        list2.Add((IGH_Goo)tree.get_Branch(path)[number]);
                        list3.Add((IGH_Goo)new GH_String(path.ToString()));
                        list4.Add((IGH_Goo)new GH_Integer(number));
                        indexList.Add((GH_Integer)ghStructure2.get_Branch(path)[number]);
                    }
                }
            }
            if (this.maintainPath) {
                if (this.sortByIndex) {
                    gooTree1 = this.SortTreeByIndex(gooTree1, indTree);
                    gooTree2 = this.SortTreeByIndex(gooTree2, indTree);
                }
                DA.SetDataTree(0, (IGH_Structure)gooTree1);
                DA.SetDataTree(1, (IGH_Structure)ghStructure3);
                DA.SetDataTree(2, (IGH_Structure)gooTree2);
            } else {
                if (this.sortByIndex) {
                    List<IGH_Goo> gooList1 = list2;
                    list2.Sort((IComparer<IGH_Goo>)new CompareGoo(gooList1, indexList));
                    List<IGH_Goo> gooList2 = list3;
                    list3.Sort((IComparer<IGH_Goo>)new CompareGoo(gooList2, indexList));
                    List<IGH_Goo> gooList3 = list4;
                    list4.Sort((IComparer<IGH_Goo>)new CompareGoo(gooList3, indexList));
                }
                DA.SetDataList(0, (IEnumerable)list2);
                DA.SetDataList(1, (IEnumerable)list3);
                DA.SetDataList(2, (IEnumerable)list4);
            }
        }
        public GH_Structure<IGH_Goo> SortTreeByIndex(GH_Structure<IGH_Goo> gooTree, GH_Structure<GH_Integer> indTree) {
            GH_Structure<IGH_Goo> ghStructure = new GH_Structure<IGH_Goo>();
            foreach (GH_Path path in (IEnumerable<GH_Path>)gooTree.Paths) {
                List<IGH_Goo> list = (List<IGH_Goo>)gooTree.get_Branch(path);
                CompareGoo compareGoo = new CompareGoo(new List<IGH_Goo>((IEnumerable<IGH_Goo>)list), (List<GH_Integer>)indTree.get_Branch(path));
                list.Sort((IComparer<IGH_Goo>)compareGoo);
                ghStructure.AppendRange((IEnumerable<IGH_Goo>)list, path);
            }
            return ghStructure;
        }
        public void SelectStoredPathObj() {
            string str1 = "Preview(" + (object)this.InstanceGuid + ")";
            RhinoDoc.ActiveDoc.Objects.UnselectAll();
            List<Guid> list = new List<Guid>();
            foreach (string str2 in this.storedPath) {
                RhinoObject[] byUserString = RhinoDoc.ActiveDoc.Objects.FindByUserString(str1, str2, true);
                list.Add(byUserString[0].Id);
            }
            this.respondToSelection = false;
            RhinoDoc.ActiveDoc.Objects.Select((IEnumerable<Guid>)list);
            this.respondToSelection = true;
            RhinoDoc.ActiveDoc.Views.Redraw();
        }
        public override bool Write(GH_IWriter writer) {
            string item_value = "";
            if (this.storedPath.Count > 0) {
                foreach (string str in this.storedPath)
                    item_value = item_value + str + "|";
                item_value = item_value.Remove(item_value.Length - 1);
            }
            writer.SetString("selectedobject", item_value);
            writer.SetBoolean("lock", this.inputLock);
            writer.SetBoolean("maintain", this.maintainPath);
            writer.SetBoolean("order", this.sortByIndex);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader) {
            string str = "";
            if (reader.TryGetString("selectedobject", ref str) && str.Length > 0)
                this.storedPath = new List<string>((IEnumerable<string>)str.Split(new char[1]
        {
          '|'
        }));
            bool flag1 = false;
            bool flag2 = false;
            bool flag3 = false;
            if (reader.TryGetBoolean("lock", ref flag1))
                this.inputLock = flag1;
            if (reader.TryGetBoolean("maintain", ref flag2))
                this.maintainPath = flag2;
            if (reader.TryGetBoolean("order", ref flag3))
                this.sortByIndex = flag2;
            return base.Read(reader);
        }
    }

    internal class CompareGoo : IComparer<IGH_Goo> {
        public List<IGH_Goo> gooList { get; set; }
        public List<GH_Integer> indexList { get; set; }
        public CompareGoo(List<IGH_Goo> gooList, List<GH_Integer> indexList) {
            this.gooList = gooList;
            this.indexList = indexList;
        }
        public int Compare(IGH_Goo g0, IGH_Goo g1) {
            return this.indexList[this.gooList.IndexOf(g0)].QC_CompareTo((IGH_QuickCast)this.indexList[this.gooList.IndexOf(g1)]);
        }
    }

    public class SelectablePreviewInfo : GH_AssemblyInfo {
        public override string AssemblyName {
            get {
                return "SelectablePreview";
            }
        }
    }

    public class AttributesButton : GH_ComponentAttributes {
        private Rectangle ButtonBounds { get; set; }

        public override bool Selected {
            get {
                return base.Selected;
            }
            set {
                base.Selected = value;
                ((SelectablePreviewComponent)this.Owner).UpdatePreview();
            }
        }

        public AttributesButton(GH_Component owner)
            : base((IGH_Component)owner) {
        }

        protected override void Layout() {
            base.Layout();
            Rectangle rectangle1 = GH_Convert.ToRectangle(this.Bounds);
            rectangle1.Height += 22;
            Rectangle rectangle2 = rectangle1;
            rectangle2.Y = rectangle2.Bottom - 22;
            rectangle2.Height = 22;
            rectangle2.Inflate(-2, -2);
            this.Bounds = (RectangleF)rectangle1;
            this.ButtonBounds = rectangle2;
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel) {
            base.Render(canvas, graphics, channel);
            if (channel != GH_CanvasChannel.Objects)
                return;
            GH_Capsule ghCapsule = 
                !((SelectablePreviewComponent)this.Owner).inputLock 
                ? GH_Capsule.CreateTextCapsule(this.ButtonBounds, this.ButtonBounds, GH_Palette.Black, "Select", 2, 0) 
                : GH_Capsule.CreateTextCapsule(this.ButtonBounds, this.ButtonBounds, GH_Palette.Black, "Locked", 2, 0);
            ghCapsule.Render(graphics, this.Selected, this.Owner.Locked, false);
            ghCapsule.Dispose();
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e) {

            System.Drawing.RectangleF rect = this.ButtonBounds;
            if (e.Button == MouseButtons.Left && (rect.Contains(e.CanvasLocation))) {
                ((SelectablePreviewComponent)this.Owner).inputLock = !((SelectablePreviewComponent)this.Owner).inputLock;
                if (!((SelectablePreviewComponent)this.Owner).inputLock) {
                    ((SelectablePreviewComponent)this.Owner).ForceExpireSolution(true, false, true);
                    ((SelectablePreviewComponent)this.Owner).SelectStoredPathObj();
                    ((GH_DocumentObject)this.Owner).OnPingDocument();
                } else
                    ((SelectablePreviewComponent)this.Owner).ForceExpireSolution(false, false, false);
            }
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }
}
