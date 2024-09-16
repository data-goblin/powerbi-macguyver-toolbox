// Add measure selection to the model
//
// Script author: Kurt Buhler; Data Goblins
// Script created: Sept 16, 2024
// Script supports: Tabular Editor 3.X (Tabular Editor 2.X untested)
// Disclaimer: Script created with help from Chat-GPT 1o (productivity enhancement and some C# patterns)
//
//
// Script instructions: Use this script when connected with any Power BI or AAS/SSAS semantic model.
//
// 1. Select the measures and columns
// 2. Run the script
// 3. Validate the DAX and set properties
// 4. Test the measure in different filter contexts and conditions before using
// 5. Hide any reporting objects that shouldn't be used by end-users connect to the model


// Namespaces
using System.Windows.Forms;
using System.Drawing;
using _TOM = Microsoft.AnalysisServices.Tabular;

// Don't show the script execution dialog or cursor
ScriptHelper.WaitFormVisible = false;
Application.UseWaitCursor = false;


// Init vars
string _SelectionTableValues;

string _SwitchMeasureValues;
string _SwitchMeasureDax;

string _SwitchMeasureFormat;
string _SwitchMeasureFormatStringDax;

string _FieldParamValues;
string _CalcGroupValues;

// Count measures before running the script
int _Measures = Model.AllMeasures.Count();

// Create a new form
var _Form = new Form()
{
    Text = "Which patterns for measure selection?",
    Padding = new Padding(5),  // 5 px padding around the edges
    StartPosition = FormStartPosition.CenterParent,
    FormBorderStyle = FormBorderStyle.FixedDialog,
    AutoSize = true,
    AutoSizeMode = AutoSizeMode.GrowAndShrink,
    MaximizeBox = false,
    MinimizeBox = false,
    ShowIcon = false,
    MinimumSize = new Size(400, 0)
};

// Create checkboxes
var _Radiobutton1 = new RadioButton()
{
    Text = "SWITCH measure selection",
    AutoSize = true
};

var _Radiobutton2 = new RadioButton()
{
    Text = "Field Parameter",
    AutoSize = true
};

var _Radiobutton3 = new RadioButton()
{
    Text = "Calc. Group measure replacement",
    AutoSize = true
};


// Arrange checkboxes vertically
var _RadiobuttonPanel = new FlowLayoutPanel()
{
    FlowDirection = FlowDirection.TopDown,
    AutoSize = true,
    AutoSizeMode = AutoSizeMode.GrowOnly,
    Dock = DockStyle.Top,
    Padding = new Padding(0, 0, 0, 10), // Padding bottom
    WrapContents = false
};


// Add checkboxes to the panel
_RadiobuttonPanel.Controls.Add(_Radiobutton1);
_RadiobuttonPanel.Controls.Add(_Radiobutton2);
_RadiobuttonPanel.Controls.Add(_Radiobutton3);

// Create panel for buttons
var _ButtonPanel = new FlowLayoutPanel()
{
    FlowDirection = FlowDirection.RightToLeft,
    AutoSize = true,
    AutoSizeMode = AutoSizeMode.GrowAndShrink,
    Dock = DockStyle.Bottom,
    Padding = new Padding(0, 10, 0, 0) // Padding top
};

// Create OK and Cancel buttons
var _OkButton = new Button()
{
    Text = "Add pattern",
    DialogResult = DialogResult.OK,
    AutoSize = true
};

var _CancelButton = new Button()
{
    Text = "Cancel",
    DialogResult = DialogResult.Cancel,
    AutoSize = true
};

// Add buttons to the panel
_ButtonPanel.Controls.Add(_CancelButton);
_ButtonPanel.Controls.Add(_OkButton);

// Set form's Accept and Cancel buttons
_Form.AcceptButton = _OkButton;
_Form.CancelButton = _CancelButton;

// Add panels to the form
_Form.Controls.Add(_RadiobuttonPanel);
_Form.Controls.Add(_ButtonPanel);

// Display the form and capture the result
var _Result = _Form.ShowDialog();

if (_Result == DialogResult.OK)
{
    // Retrieve checkbox values
    bool _SWChecked = _Radiobutton1.Checked;
    bool _FPChecked = _Radiobutton2.Checked;
    bool _CGChecked = _Radiobutton3.Checked;

    // Add options
    if (_SWChecked)
    {

        int _NrMeasures = 0;

        foreach (var _m in Selected.Measures)
        {
            _SelectionTableValues = _SelectionTableValues + @",
        {""" + _m.Name + @""", " + _NrMeasures + @"}";

            _SwitchMeasureValues = _SwitchMeasureValues + @"
            """ + _m.Name + @""", " + _m.DaxObjectFullName + @",";

            _SwitchMeasureFormat = _SwitchMeasureFormat + @"
            """ + _m.Name + @""", """ + _m.FormatString + @""",";

            _NrMeasures = _NrMeasures + 1;

        }

        var _TableDax = @"DATATABLE (
    ""Measure"", STRING,
    ""Order"", INTEGER,
    {__" + _SelectionTableValues + @"
    }
)";
        _TableDax = _TableDax.Replace("__,", "");

        CalculatedTable _NewTable = Model.AddCalculatedTable("Switch Selector Table", _TableDax);
        Column _Selector = _NewTable.Columns[0];
        Column _Order = _NewTable.Columns[1];
        _Selector.SortByColumn = _Order;
        _Selector.Description = "Use this column in a slicer or filter to select the measure.";
        _Order.Description = "Adjust this column to change the sort order.";
        _Order.IsHidden = true;

        foreach (var _m in Selected.Measures)
        {
            _SwitchMeasureDax = @"IF ( 
    HASONEVALUE ( __SELECTION_COLUMN ),
    
    VAR _SelectedMeasure = SELECTEDVALUE ( __SELECTION_COLUMN )
    
    VAR _Result =
        SWITCH (
            _SelectedMeasure," + _SwitchMeasureValues + @"
            BLANK()
        )
        
    RETURN
        _Result
)";

            _SwitchMeasureFormatStringDax = @"IF ( 
    HASONEVALUE ( __SELECTION_COLUMN ),
    
    VAR _SelectedMeasure = SELECTEDVALUE ( __SELECTION_COLUMN )
    
    VAR _Result =
        SWITCH (
            _SelectedMeasure," + _SwitchMeasureFormat + @"
            ""#,##0""
        )
        
    RETURN
        _Result
)";

            string _SelectionColumn = _Selector.DaxObjectFullName;
            _Selector.IsHidden = true;
            _SwitchMeasureDax = _SwitchMeasureDax.Replace("__SELECTION_COLUMN", _SelectionColumn);
            _SwitchMeasureFormatStringDax = _SwitchMeasureFormatStringDax.Replace("__SELECTION_COLUMN", _SelectionColumn);

        }

        var _NewMeasure = _NewTable.AddMeasure("Switch Measure Selector", _SwitchMeasureDax);
        _NewMeasure.FormatStringExpression = _SwitchMeasureFormatStringDax;
        _NewMeasure.Description = "Use this measure in visuals and pivot tables. You must select a value from " + _Selector.DaxObjectFullName + ".";

    }

    if (_FPChecked)
    {
        // Field parameter script from Daniel Otykier

        var name = "FP - Measure Selector Table";

        if (Selected.Columns.Count == 0 && Selected.Measures.Count == 0) throw new Exception("No columns or measures selected!");


        // Construct the DAX for the calculated table based on the current selection:
        var objects = Selected.Columns.Any() ? Selected.Columns.Cast<ITabularTableObject>() : Selected.Measures;
        var dax = "{\n    " + string.Join(",\n    ", objects.Select((c, i) => string.Format("(\"{0}\", NAMEOF('{1}'[{0}]), {2})", c.Name, c.Table.Name, i))) + "\n}";


        // Add the calculated table to the model:
        var table = Model.AddCalculatedTable(name, dax);


        // In TE2 columns are not created automatically from a DAX expression, so 
        // we will have to add them manually:
        var te2 = table.Columns.Count == 0;
        var nameColumn = te2 ? table.AddCalculatedTableColumn(name, "[Value1]") : table.Columns["Value1"] as CalculatedTableColumn;
        var fieldColumn = te2 ? table.AddCalculatedTableColumn(name + " Fields", "[Value2]") : table.Columns["Value2"] as CalculatedTableColumn;
        var orderColumn = te2 ? table.AddCalculatedTableColumn(name + " Order", "[Value3]") : table.Columns["Value3"] as CalculatedTableColumn;


        if (!te2)
        {
            // Rename the columns that were added automatically in TE3:
            nameColumn.IsNameInferred = false;
            nameColumn.Name = "Measure";
            fieldColumn.IsNameInferred = false;
            fieldColumn.Name = "Fields";
            orderColumn.IsNameInferred = false;
            orderColumn.Name = "Order";
        }


        // Set remaining properties for field parameters to work
        nameColumn.SortByColumn = orderColumn;
        nameColumn.GroupByColumns.Add(fieldColumn);

        fieldColumn.SortByColumn = orderColumn;
        fieldColumn.SetExtendedProperty("ParameterMetadata", "{\"version\":3,\"kind\":2}", ExtendedPropertyType.Json);
        fieldColumn.IsHidden = true;

        orderColumn.IsHidden = true;

    }

    if (_CGChecked)
    {

        var _NewCG = Model.AddCalculationGroupTable("CG - Measure Replacement");

        foreach (var _m in Selected.Measures)
        {
            _NewCG.AddCalculationItem(
                _m.Name,
                _m.DaxObjectFullName
            );
        }
        
        _NewCG.IsHidden = true;

        var _NewTable = Model.AddCalculatedTable("Measure Selector Table", _NewCG.DaxObjectFullName);
        _NewTable.Columns[0].Name = "Measure";
        _NewTable.Columns[1].Name = "Order";
        _NewTable.Columns[0].IsHidden = true;
        _NewTable.Columns[1].IsHidden = true;

        var _Selector = _NewTable.Columns[0].DaxObjectFullName;

        if (Model.AllMeasures.Where(_blank => _blank.Name == "Blank").Count() == 0) _NewTable.AddMeasure( "Blank", "BLANK()" );
        var _NewMeasure = _NewTable.AddMeasure("Measure Selector", @"IF ( 
    HASONEVALUE ( " + _Selector + @" ),
        CALCULATE (
        [Blank],
        TREATAS (
            DISTINCT (" + _Selector + @"),
            " + _NewCG.Columns[0].DaxObjectFullName + @"
        )
    )
)");

        _NewMeasure.Description = "Use this measure in visuals and pivot tables. To use, you must first select a value from " + _Selector;

        
        foreach (var _m in Selected.Measures)
        {
            _SwitchMeasureFormat = _SwitchMeasureFormat + @"
            """ + _m.Name + @""", """ + _m.FormatString + @""",";
        }

        _SwitchMeasureFormatStringDax = @"IF ( 
    HASONEVALUE ( " + _Selector + @" ),
    
    VAR _SelectedMeasure = SELECTEDVALUE ( " + _Selector + @" )
    
    VAR _Result =
        SWITCH (
            _SelectedMeasure," + _SwitchMeasureFormat + @"
            ""#,##0""
        )
        
    RETURN
        _Result
)";

        _NewMeasure.FormatStringExpression = _SwitchMeasureFormatStringDax;

    }
}


else
{
    // The user cancelled the operation
    Error("Cancelled script. No modifications made to the model.");
}