// Create Time Intelligence measures (standard) 
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


// Namespaces
using System.Windows.Forms;
using System.Drawing;


// Don't show the script execution dialog or cursor
ScriptHelper.WaitFormVisible = false;
Application.UseWaitCursor = false;


// Init vars
string _ToDateDAXTemplate;
string _PrevPeriodDAXTemplate;

List<string> _TimeIntelligence = new List<string>();
List<string> _ToDateOptions = new List<string> { "MTD", "QTD", "YTD" };
List<string> _PreviousPeriodOptions = new List<string> { "1YP", "2YP", "1MP", "1QP" };

// Count measures before running the script
int _Measures = Model.AllMeasures.Count();

// Create a new form
var _Form = new Form()
{
Text = "Add which measures?",
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
var _Checkbox1 = new CheckBox()
{
Text = "MTD",
AutoSize = true,
Checked = true
};

var _Checkbox2 = new CheckBox()
{
Text = "QTD",
AutoSize = true,
Checked = true
};

var _Checkbox3 = new CheckBox()
{
Text = "YTD",
AutoSize = true,
Checked = true
};

var _Checkbox4 = new CheckBox()
{
Text = "1 Year Prior",
AutoSize = true
};

var _Checkbox5 = new CheckBox()
{
Text = "2 Years Prior",
AutoSize = true
};

var _Checkbox6 = new CheckBox()
{
Text = "1 Quarter Prior",
AutoSize = true
};

var _Checkbox7 = new CheckBox()
{
Text = "1 Month Prior",
AutoSize = true
};

// Arrange checkboxes vertically
var _CheckBoxPanel = new FlowLayoutPanel()
{
FlowDirection = FlowDirection.TopDown,
AutoSize = true,
AutoSizeMode = AutoSizeMode.GrowOnly,
Dock = DockStyle.Top,
Padding = new Padding(0, 0, 0, 10), // Padding bottom
WrapContents = false
};


// Add checkboxes to the panel
_CheckBoxPanel.Controls.Add(_Checkbox1);
_CheckBoxPanel.Controls.Add(_Checkbox2);
_CheckBoxPanel.Controls.Add(_Checkbox3);
_CheckBoxPanel.Controls.Add(_Checkbox4);
_CheckBoxPanel.Controls.Add(_Checkbox5);
_CheckBoxPanel.Controls.Add(_Checkbox6);
_CheckBoxPanel.Controls.Add(_Checkbox7);

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
Text = "Create measures",
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
_Form.Controls.Add(_CheckBoxPanel);
_Form.Controls.Add(_ButtonPanel);

// Display the form and capture the result
var _Result = _Form.ShowDialog();

if (_Result == DialogResult.OK)
{
    // Retrieve checkbox values
    bool _MtdChecked = _Checkbox1.Checked;
    bool _QtdChecked = _Checkbox2.Checked;
    bool _YtdChecked = _Checkbox3.Checked;
    bool _1YPChecked = _Checkbox4.Checked;
    bool _2YPChecked = _Checkbox5.Checked;
    bool _1QPChecked = _Checkbox6.Checked;
    bool _1MPChecked = _Checkbox7.Checked;

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

    if (_1YPChecked)
    {
        _TimeIntelligence.Add("1YP");
    }

    if (_2YPChecked)
    {
        _TimeIntelligence.Add("2YP");
    }

    if (_1QPChecked)
    {
        _TimeIntelligence.Add("1QP");
    }

    if (_1MPChecked)
    {
        _TimeIntelligence.Add("1MP");
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


        // For MTD, QTD, YTD
        if ( _ToDateOptions.Contains( _l ) )
        {


        // Template for column date cutoff
        if ( _CutoffColumn is Column )
            {
                _ToDateDAXTemplate = @"CALCULATE (
    __ACTUAL_FIELD,
    CALCULATETABLE (
        DATES" + _l + @" ( __DATE_COLUMN ),
        " + _CutoffColumn.DaxObjectFullName + @"
    )
)";

                _PrevPeriodDAXTemplate = @"CALCULATE (
    __ACTUAL_FIELD,
    CALCULATETABLE (
        PARALLELPERIOD( __DATE_COLUMN, __NR_PERIODS, __PERIOD ),
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

                    _ToDateDAXTemplate = @"IF (
    " + _CutoffMeasure.DaxObjectFullName + @" = TRUE(),
    CALCULATE (
        __ACTUAL_FIELD,
        DATES" + _l + @" ( __DATE_COLUMN )
    )
)";

                    _PrevPeriodDAXTemplate = @"IF (
    " + _CutoffMeasure.DaxObjectFullName + @" = TRUE(),
    CALCULATE (
    __ACTUAL_FIELD,
    PARALLELPERIOD( __DATE_COLUMN, __NR_PERIODS, __PERIOD )
    )
)";

                }


                // Template for no date cutoff
                else if ( _CutoffMeasure is null)
                {

                    _ToDateDAXTemplate = @"CALCULATE (
    __ACTUAL_FIELD,
    DATES" + _l + @" ( __DATE_COLUMN )
)";

                _PrevPeriodDAXTemplate = @"CALCULATE (
    __ACTUAL_FIELD,
    PARALLELPERIOD( __DATE_COLUMN, __NR_PERIODS, __PERIOD )
)";

                }
            }
        

            // Apply the selected date column to the template
            _ToDateDAXTemplate = _ToDateDAXTemplate.Replace("__DATE_COLUMN", _DateColumn );
            _PrevPeriodDAXTemplate = _PrevPeriodDAXTemplate.Replace("__DATE_COLUMN", _DateColumn );
            
            
            // Apply the pattern to selected measures
            if ( Selected.Measures.Count() > 0 )
            {
            
                foreach ( var _m in Selected.Measures )
                {
            
                    var _NewMeasure = _m.Table.AddMeasure(
                    _m.Name + " (" + _l + ")",
                    _ToDateDAXTemplate.Replace("__ACTUAL_FIELD", _m.DaxObjectFullName),
                    "Measures\\" + _l);
            
                    _NewMeasure.FormatString = "#,##0.00";
            
                }
            }
        
        
            // Apply the pattern to selected columns
            if ( Selected.Columns.Count() > 0 )
            {
            
                foreach ( var _c in Selected.Columns )
                {
            
                    var _ValidDtypes = new [] { DataType.Int64, DataType.Decimal, DataType.Double };
            
                    // Only proceed if column is a valid data type
                    if ( _ValidDtypes.Contains(_c.DataType) )
                    {
                        var _NewMeasure = _c.Table.AddMeasure(
                        _c.Name + " (" + _l + ")",
                        _ToDateDAXTemplate.Replace("__ACTUAL_FIELD", "SUM ( " + _c.DaxObjectFullName + " )" ),
                        "Measures\\" + _l);
                        _NewMeasure.FormatString = "#,##0.00";
                    }
                    
                    _c.IsHidden = true;
            
                    // Error if column is invalid data type
                    if ( !_ValidDtypes.Contains(_c.DataType) )
                    {
                        Error( _c.DaxObjectFullName + " is not a valid data type for this measure. Skipping measure creation." );
                    }
                }
            }
        }

        
        else if ( _PreviousPeriodOptions.Contains( _l ) )
        {

            string _NrPeriods = null;
            string _Period = null;

            if ( _l == "1YP" ) 
            {
                _NrPeriods = "-1";
                _Period = "YEAR";
            }
            if ( _l == "2YP" ) 
            {
                _NrPeriods = "-2";
                _Period = "YEAR";
            }
            if ( _l == "1MP" ) 
            {
                _NrPeriods = "-1";
                _Period = "MONTH";
            }
            if ( _l == "1QP" ) 
            {
                _NrPeriods = "-1";
                _Period = "QUARTER";
            }


            // Apply the pattern to selected measures
            if ( Selected.Measures.Count() > 0 )
            {
            
                foreach ( var _m in Selected.Measures )
                {
            
                    var _NewMeasure = _m.Table.AddMeasure(
                    _m.Name + " (" + _l + ")",
                    _PrevPeriodDAXTemplate.Replace("__ACTUAL_FIELD", _m.DaxObjectFullName).Replace("__NR_PERIODS", _NrPeriods).Replace("__PERIOD", _Period),
                    "Measures\\" + _l);
            
                    _NewMeasure.FormatString = "#,##0.00";
            
                }
            }


            // Apply the pattern to selected columns
            if ( Selected.Columns.Count() > 0 )
            {
            
                foreach ( var _c in Selected.Columns )
                {
            
                    var _ValidDtypes = new [] { DataType.Int64, DataType.Decimal, DataType.Double };
            
                    // Only proceed if column is a valid data type
                    if ( _ValidDtypes.Contains(_c.DataType) )
                    {
                        var _NewMeasure = _c.Table.AddMeasure(
                        _c.Name + " (" + _l + ")",
                        _PrevPeriodDAXTemplate.Replace("__ACTUAL_FIELD", "SUM ( " + _c.DaxObjectFullName + " )" ).Replace("__NR_PERIODS", _NrPeriods).Replace("__PERIOD", _Period),
                        "Measures\\" + _l);
                        _NewMeasure.FormatString = "#,##0.00";
                    }
                    
                    _c.IsHidden = true;
            
                    // Error if column is invalid data type
                    if ( !_ValidDtypes.Contains(_c.DataType) )
                    {
                        Error( _c.DaxObjectFullName + " is not a valid data type for this measure. Skipping measure creation." );
                    }
                }
            }
        }
    }

    // Returns feedback if successful
    if ( _Measures < Model.AllMeasures.Count() )
    {
        Info("Added measures to the currently selected table");
    }

}

else
{
    // The user cancelled the operation
    Error("Cancelled script. No modifications made to the model.");
}