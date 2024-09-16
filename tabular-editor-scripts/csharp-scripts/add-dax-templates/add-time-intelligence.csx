// Create Time Intelligence measures (standard) 
//
// Script author: Kurt Buhler; Data Goblins
// Script created: Sept 16, 2024
// Script supports: Tabular Editor 2.X, Tabular Editor 3.X.
//
// Original template author: Kurt Buhler
//
//
// Script instructions: Use this script when connected with any Power BI or AAS/SSAS semantic model.
//
// 1. Select the measures and columns
// 2. 


// Namespaces
using System.Windows.Forms;
using System.Drawing;


// Don't show the script execution dialog or cursor
ScriptHelper.WaitFormVisible = false;
Application.UseWaitCursor = false;


// Init vars
string _DAXTemplate;
var _TimeIntelligence = new List<string>();


// Create a new form
var _Form = new Form();
_Form.Text = "Add which measures?";
_Form.Padding = new Padding(5);  // 5 px padding around the edges
_Form.StartPosition = FormStartPosition.CenterScreen;
_Form.FormBorderStyle = FormBorderStyle.FixedDialog;
_Form.AutoSize = true;
_Form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
_Form.MaximizeBox = false;
_Form.MinimizeBox = false;
_Form.MinimumSize = new Size(300, 0);

// Create checkboxes
var _CheckBox1 = new CheckBox();
_CheckBox1.Text = "MTD";
_CheckBox1.AutoSize = true;
_CheckBox1.Checked = true;

var _CheckBox2 = new CheckBox();
_CheckBox2.Text = "QTD";
_CheckBox2.AutoSize = true;
_CheckBox2.Checked = true;

var _CheckBox3 = new CheckBox();
_CheckBox3.Text = "YTD";
_CheckBox3.AutoSize = true;
_CheckBox3.Checked = true;

// Arrange checkboxes vertically
var _CheckBoxPanel = new FlowLayoutPanel();
_CheckBoxPanel.FlowDirection = FlowDirection.TopDown;
_CheckBoxPanel.AutoSize = true;
_CheckBoxPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
_CheckBoxPanel.Dock = DockStyle.Top;
_CheckBoxPanel.Padding = new Padding(0, 0, 0, 10); // Padding bottom
_CheckBoxPanel.WrapContents = false;

// Add checkboxes to the panel
_CheckBoxPanel.Controls.Add(_CheckBox1);
_CheckBoxPanel.Controls.Add(_CheckBox2);
_CheckBoxPanel.Controls.Add(_CheckBox3);

// Create panel for buttons
var _ButtonPanel = new FlowLayoutPanel();
_ButtonPanel.FlowDirection = FlowDirection.RightToLeft;
_ButtonPanel.AutoSize = true;
_ButtonPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
_ButtonPanel.Dock = DockStyle.Bottom;
_ButtonPanel.Padding = new Padding(0, 10, 0, 0); // Padding top

// Create OK and Cancel buttons
var _OkButton = new Button();
_OkButton.Text = "Create measures";
_OkButton.DialogResult = DialogResult.OK;
_OkButton.AutoSize = true;

var _CancelButton = new Button();
_CancelButton.Text = "Cancel";
_CancelButton.DialogResult = DialogResult.Cancel;
_CancelButton.AutoSize = true;

// Add buttons to the panel
_ButtonPanel.Controls.Add(_CancelButton);
_ButtonPanel.Controls.Add(_OkButton);

// Set form's Accept and Cancel buttons
_Form.AcceptButton = _OkButton;
_Form.CancelButton = _CancelButton;

// Add panels to the form
_Form.Controls.Add(_CheckBoxPanel);
_Form.Controls.Add(_ButtonPanel);

// Display the form and capture the result
var _Result = _Form.ShowDialog();

if (_Result == DialogResult.OK)
{
    // Retrieve checkbox values
    bool _MtdChecked = _CheckBox1.Checked;
    bool _QtdChecked = _CheckBox2.Checked;
    bool _YtdChecked = _CheckBox3.Checked;

    // Add options
    if (_MtdChecked)
    {
        _TimeIntelligence.Add("MTD");
    }

    if (_QtdChecked)
    {
        _TimeIntelligence.Add("QTD");
    }

    if (_YtdChecked)
    {
        _TimeIntelligence.Add("YTD");
    }


    // Select date cutoff column
    Column _CutoffColumn = SelectColumn(Model.AllColumns.Where(c => c.DataType == DataType.Boolean).OrderBy(c => c.DaxObjectFullName), null, "Select a boolean column that indicates the date cutoff.\nPress 'Cancel' if you don't use this column.\n\nThis column is used to hide future dates.");
    Measure _CutoffMeasure = null;

    // Ask for measure date cutoff if no column date cutoff
    if ( _CutoffColumn is null )
    {

        // Get a measure that indicates cutoff
        _CutoffMeasure = SelectMeasure(Model.AllMeasures.Where(m => m.DataType == DataType.Boolean).OrderBy(m => m.DaxObjectFullName), null, "Select a boolean measure that indicates the date cutoff.\nPress 'Cancel' if you don't use this measure.\n\nThis measure is used to hide future dates.");

    }


    // Select date column
    string _DateColumn = SelectColumn(Model.AllColumns.Where(c => c.DataType == DataType.DateTime).OrderBy(c => c.DaxObjectFullName), null, "Select your 'Date' column.").DaxObjectFullName;


    // Loop through each selected option to create the measures
    foreach ( var _l in _TimeIntelligence )
    {
        
        // Template for column date cutoff
        if ( _CutoffColumn is Column )
        {

            _DAXTemplate = @"CALCULATE (
            __ACTUAL_FIELD,
    CALCULATETABLE (
        DATES" + _l + @" ( __DATE_COLUMN ),
        " + _CutoffColumn.DaxObjectFullName + @"
    )
)";

        }


        // Date cutoff not determined by column
        else if ( _CutoffColumn is null)
        {

            // Template for measure date cutoff
            if ( _CutoffMeasure is Measure )
            {

                _DAXTemplate = @"IF (
    " + _CutoffMeasure.DaxObjectFullName + @" = TRUE(),
    CALCULATE (
        __ACTUAL_FIELD,
        DATES" + _l + @" ( __DATE_COLUMN )
    )
)";

            }


            // Template for no date cutoff
            else if ( _CutoffMeasure is null)
            {

            _DAXTemplate = @"CALCULATE (
    __ACTUAL_FIELD,
    DATES" + _l + @" ( __DATE_COLUMN )
)";
            }
        }
        

        // Apply the selected date column to the template
        _DAXTemplate = _DAXTemplate.Replace("__DATE_COLUMN", _DateColumn );
        

        // Count measures before running the script
        int _Measures = Model.AllMeasures.Count();
        
        
        // Apply the pattern to selected measures
        if ( Selected.Measures.Count() > 0 )
        {
        
            foreach ( var _m in Selected.Measures )
            {
        
                var _NewMeasure = _m.Table.AddMeasure(
                _m.Name + " (" + _l + ")",
                _DAXTemplate.Replace("__ACTUAL_FIELD", _m.DaxObjectFullName),
                "Measures\\" + _l);
        
                _NewMeasure.FormatString = "#,##0.00";
        
            }
        }
        
        
        // Apply the pattern to selected columns
        else if ( Selected.Columns.Count() > 0 )
        {
        
            foreach ( var _c in Selected.Columns )
            {
        
                var _ValidDtypes = new [] { DataType.Int64, DataType.Decimal, DataType.Double };
        
                // Only proceed if column is a valid data type
                if ( _ValidDtypes.Contains(_c.DataType) )
                {
                    
                    var _NewMeasure = _c.Table.AddMeasure(
                    _c.Name + " (" + _l + ")",
                    _DAXTemplate.Replace("__ACTUAL_FIELD", " SUM ( " + _c.DaxObjectFullName + " )" ),
                    "Measures\\" + _l);
        
                    _NewMeasure.FormatString = "#,##0.00";
                    _c.IsHidden = true;
        
                }
        
                // Only proceed if column is a valid data type
                if ( !_ValidDtypes.Contains(_c.DataType) )
                {
                    Error( _c.DaxObjectFullName + " is not a valid data type for this measure. Skipping measure creation." );
                }
            }
        }
        
        
        // Returns feedback if successful
        if ( _Measures < Model.AllMeasures.Count() )
        {
            Info("Added " + ( Model.AllMeasures.Count() - _Measures ) + " new " + _l + @" measures");
        }
    }
}

else
{
    // The user cancelled the operation
    Error("Cancelled script. No modifications made to the model.");
}