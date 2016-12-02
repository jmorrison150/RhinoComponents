using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Expressions;


using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.HTML;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Expressions;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.HTML;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Expressions;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace gsd {
    public class Evaluate :GH_Component, IGH_VariableParameterComponent{
        private bool m_valid;
        private SortedDictionary<string, Queue<GH_ParserSymbol>> m_cache;

        protected override Bitmap Internal_Icon_24x24 {
            get {
                return base.Internal_Icon_24x24;
            }
        }
        public override Guid ComponentGuid {
            get {
                return new Guid("{AB27ACB8-EA0B-4D86-B3FD-C03C8288C47D}");
            }
        }
        public Evaluate() : base(".antecedent",".antecedent","equation","Extra","surfTM") {
            this.m_valid = true;
            this.m_cache = new SortedDictionary<string, Queue<GH_ParserSymbol>>();
            this.Params.ParameterNickNameChanged += new GH_ComponentParamServer.ParameterNickNameChangedEventHandler(this.ParamNameChanged);
        }
        public override void CreateAttributes() {
            //this.m_attributes = new Component_Ev
        }







    }
    public class Component_Evaluate_Attributes : GH_ComponentAttributes {
        public Component_Evaluate_Attributes(Evaluate owner)
            : base((IGH_Component)owner) {
        }

        public override void SetupTooltip(PointF canvasPoint, Grasshopper.GUI.GH_TooltipDisplayEventArgs e) {
            base.SetupTooltip(canvasPoint, e);
            if (!this.m_innerBounds.Contains(canvasPoint) || this.Owner.Params.Input[0].SourceCount != 0)
                return;
            e.Description = "Double click to edit the expression";
        }

        public override Grasshopper.GUI.Canvas.GH_ObjectResponse RespondToMouseDoubleClick(Grasshopper.GUI.Canvas.GH_Canvas sender, Grasshopper.GUI.Canvas.GH_CanvasMouseEvent e) {
            if (this.m_innerBounds.Contains(e.CanvasLocation) && this.Owner.Params.Input[0].SourceCount == 0) {
                ExpressionParameter expressionParameter = this.Owner.Params.Input[0] as ExpressionParameter;
                if (expressionParameter != null) {
                    expressionParameter.DisplayExpressionEditor();
                    return Grasshopper.GUI.Canvas.GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }


}
namespace MathComponents.ExpressionComponents {
    public class ExpressionParameter : GH_Param<GH_String> {
        private string m_expression;

        public override GH_Exposure Exposure {
            get {
                return GH_Exposure.hidden;
            }
        }

        public override Guid ComponentGuid {
            get {
                return new Guid("{619660D6-F033-4D92-A2E0-4E836C20DDF9}");
            }
        }

        protected override Bitmap Icon {
            get {
                return Resources.ExpressionParameter_24x24;
            }
        }

        public override string TypeName {
            get {
                return "Expression";
            }
        }

        public ExpressionParameter()
            : base((IGH_InstanceDescription)new GH_InstanceDescription("Expression", "Exp", "A textual expression", "Params", "Util")) {
        }

        protected override void CollectVolatileData_Custom() {
            if (this.m_expression == null || this.m_expression.Length == 0)
                return;
            this.AddVolatileData(new GH_Path(0), 0, new GH_String(this.m_expression));
        }

        public List<VariantParameter> VariantInputParameters() {
            if (this.Attributes == null)
                return (List<VariantParameter>)null;
            IGH_Attributes getTopLevel = this.Attributes.GetTopLevel;
            if (getTopLevel == null)
                return (List<VariantParameter>)null;
            if (getTopLevel.DocObject == null)
                return (List<VariantParameter>)null;
            if (!(getTopLevel.DocObject is GH_Component))
                return (List<VariantParameter>)null;
            GH_Component ghComponent = (GH_Component)getTopLevel.DocObject;
            List<VariantParameter> list = new List<VariantParameter>();
            List<IGH_Param>.Enumerator enumerator;
            try {
                enumerator = ghComponent.Params.Input.GetEnumerator();
                while (enumerator.MoveNext()) {
                    IGH_Param current = enumerator.Current;
                    if (current is VariantParameter) {
                        VariantParameter variantParameter = (VariantParameter)current;
                        list.Add(variantParameter);
                    }
                }
            } finally {
                enumerator.Dispose();
            }
            if (list.Count == 0)
                return (List<VariantParameter>)null;
            else
                return list;
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu) {
            base.AppendAdditionalMenuItems(menu);
            string text = "";
            if (this.SourceCount == 0)
                text = this.m_expression;
            GH_DocumentObject.Menu_AppendItem((ToolStrip)menu, "Expression Editor", new EventHandler(this.Menu_ExpressionEditorClick), this.SourceCount == 0);
            GH_DocumentObject.Menu_AppendTextItem(menu, text, new GH_MenuTextBox.KeyDownEventHandler(this.Menu_ExpressionBoxKeyDown), (GH_MenuTextBox.TextChangedEventHandler)null, this.SourceCount == 0, 200, true);
        }

        protected void Menu_ExpressionBoxKeyDown(GH_MenuTextBox sender, KeyEventArgs e) {
            Keys keyCode = e.KeyCode;
            if (keyCode == Keys.Return || keyCode == Keys.Return) {
                this.m_expression = sender.Text;
                this.m_expression = GH_ExpressionSyntaxWriter.RewriteForGraphicInterface(this.m_expression);
                sender.Text = this.m_expression;
                this.ExpireSolution(true);
            } else {
                if (keyCode != Keys.Escape && keyCode != Keys.Cancel)
                    return;
                this.m_expression = sender.OriginalText;
                sender.CloseEntireMenuStructure();
                this.ExpireSolution(true);
            }
        }

        protected void Menu_ExpressionEditorClick(object sender, EventArgs e) {
            this.TriggerAutoSave();
            GH_ExpressionEditor expressionEditor = new GH_ExpressionEditor();
            expressionEditor.PreviewDelegate = new GH_ExpressionEditor.GH_PreviewExpression(this.ExpressionEditorPreviewButtonClicked);
            List<MathComponents.ExpressionComponents.VariantParameter> list = this.VariantInputParameters();
            List<VariantParameter>.Enumerator enumerator;
            if (list != null) {
                try {
                    enumerator = list.GetEnumerator();
                    while (enumerator.MoveNext()) {
                        VariantParameter current = enumerator.Current;
                        if (expressionEditor.Variables.ContainsKey(current.NickName)) {
                            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "You can only use a variable name once.");
                        } else {
                            VariantType variantType = ((GH_Structure<VariantType>)current.VolatileData).get_FirstItem(true);
                            if (variantType == null)
                                expressionEditor.Variables.Add(GH_Convert.ToVariableName(current.NickName), new GH_Variant());
                            else
                                expressionEditor.Variables.Add(GH_Convert.ToVariableName(current.NickName), variantType.Value);
                        }
                    }
                } finally {
                    enumerator.Dispose();
                }
            }
            expressionEditor.Expression = this.m_expression;
            GH_WindowsFormUtil.CenterFormOnCursor((Form)expressionEditor, true);
            if (expressionEditor.ShowDialog((IWin32Window)Instances.DocumentEditor) == DialogResult.OK) {
                this.RecordUndoEvent("Change Expression");
                this.m_expression = expressionEditor.Expression;
                if (this.m_expression.Length == 0)
                    this.m_expression = (string)null;
            }
            this.ExpireSolution(true);
        }

        protected void ExpressionEditorPreviewButtonClicked(string expression) {
            this.TriggerAutoSave();
            string str = this.m_expression;
            this.m_expression = expression;
            if (this.m_expression.Length == 0)
                this.m_expression = (string)null;
            this.ExpireSolution(true);
            this.m_expression = str;
        }

        public bool DisplayExpressionEditor() {
            this.Menu_ExpressionEditorClick((object)null, (EventArgs)null);
            return true;
        }

        protected override string HtmlHelp_Source() {
            GH_HtmlFormatter ghHtmlFormatter = new GH_HtmlFormatter((IGH_InstanceDescription)this);
            ghHtmlFormatter.Title = "Expression parameter";
            ghHtmlFormatter.Description = "Represents a list of Expressions. Explicit History ships with an expression language for numeric and geometric computations. This parameter type is used by components that implement these expressions.";
            ghHtmlFormatter.ContactURI = "david@mcneel.com";
            ghHtmlFormatter.AddRemark("This parameter contains text data. Whether or not a specific text turns out to be a valid expression is very hard to tell in advance. If one or more expressions in this parameter are faulty (invalid syntax or erroneous function calls) the component that owns this parameter should provide warning or error messages.", GH_HtmlFormatterPalette.Black, GH_HtmlFormatterPalette.White);
            return ghHtmlFormatter.HtmlFormat();
        }

        public override bool Write(GH_IWriter writer) {
            if (!string.IsNullOrEmpty(this.m_expression))
                writer.SetString("Equation", this.m_expression);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader) {
            this.m_expression = !reader.ItemExists("Equation") ? (string)null : reader.GetString("Equation");
            return base.Read(reader);
        }
    }

    public class VariantParameter : GH_Param<VariantType> {
        public override GH_Exposure Exposure {
            get {
                return GH_Exposure.hidden;
            }
        }

        public override Guid ComponentGuid {
            get {
                return new Guid("{BA80FD98-91A1-4958-B6A7-A94E40E52BDB}");
            }
        }

        protected override Bitmap Icon {
            get {
                return Resources.VariantParameter_24x24;
            }
        }

        public override string TypeName {
            get {
                return "Expression Variant";
            }
        }

        public VariantParameter()
            : base((IGH_InstanceDescription)new GH_InstanceDescription("Variant", "Var", "An expression variant", "Params", "Util")) {
        }

        protected override string HtmlHelp_Source() {
            GH_HtmlFormatter ghHtmlFormatter = new GH_HtmlFormatter((IGH_InstanceDescription)this);
            ghHtmlFormatter.Title = "Expression Variant parameter";
            ghHtmlFormatter.Description = "Represents a list of Expression Variants. Expressions in Explicit History can deal with several different data types such as: numbers, booleans, text and points (incomplete list). The Expression Variant parameter collects and attempts to convert all data into these types so they can be used as expression variables.";
            ghHtmlFormatter.ContactURI = "david@mcneel.com";
            ghHtmlFormatter.AddRemark("If this parameter encounters data it cannot convert into an Expression Variant, it will add a NULL entry to the variant list.", GH_HtmlFormatterPalette.Black, GH_HtmlFormatterPalette.White);
            return ghHtmlFormatter.HtmlFormat();
        }
    }
}