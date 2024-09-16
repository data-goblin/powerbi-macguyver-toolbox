// Apply format strings
//
// Script author: Kurt Buhler; Data Goblins
// Script created: Sept 16, 2024
// Script supports: Tabular Editor 3.X (Tabular Editor 2.X untested)
// Disclaimer: Script created with help from Chat-GPT 1o (productivity enhancement and some C# patterns)
//
//
//// Script instructions: Use this script when connected with any Power BI or AAS/SSAS semantic model.
//
// 1. Select the measures you want to format
// 2. Run the script
// 3. Select the format that you want to apply in the dialog
// 4. Validate the format string and test the measure in reports

// Namespaces
using System.Windows.Forms;
using System.Drawing;

// Don't show the script execution dialog or cursor
ScriptHelper.WaitFormVisible = false;
Application.UseWaitCursor = false;

// Init vars
string _BaseFormat = "#,##0";

// Create a new form
var _Form = new Form()
{
    Text = "Format Options",
    Padding = new Padding(10),
    StartPosition = FormStartPosition.CenterParent,
    FormBorderStyle = FormBorderStyle.FixedDialog,
    AutoSize = true,
    AutoSizeMode = AutoSizeMode.GrowAndShrink,
    MaximizeBox = false,
    MinimizeBox = false,
    ShowIcon = false,
    MinimumSize = new Size(700, 0)
};

// Format options container
var _FormatOptionsPanel = new TableLayoutPanel()
{
    ColumnCount = 2,
    AutoSize = true,
    AutoSizeMode = AutoSizeMode.GrowAndShrink,
    Dock = DockStyle.Top,
    Padding = new Padding(0, 0, 0, 12),
    CellBorderStyle = TableLayoutPanelCellBorderStyle.None
};

// Add column styles
_FormatOptionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
_FormatOptionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));


// Symbol select
var _SymbolLabel = new Label()
{
    Text = "Symbol:",
    AutoSize = true,
    Padding = new Padding(0, 5, 0, 5)
};

// Create radio buttons for symbol selection
var _SymbolOption0 = new RadioButton()
{
    Text = "None",
    AutoSize = true,
    Checked = true
};

var _SymbolOption1 = new RadioButton()
{
    Text = "+/-",
    AutoSize = true
};

var _SymbolOption2 = new RadioButton()
{
    Text = "↓/↑",
    AutoSize = true
};

var _SymbolOption3 = new RadioButton()
{
    Text = "▲/▼",
    AutoSize = true
};

var _SymbolOption4 = new RadioButton()
{
    Text = "↗ / ↘",
    AutoSize = true
};

// Radio container
var _SymbolOptionsPanel = new FlowLayoutPanel()
{
    FlowDirection = FlowDirection.LeftToRight,
    AutoSize = true,
    AutoSizeMode = AutoSizeMode.GrowAndShrink,
    WrapContents = false  // Prevent wrapping to a new line
};

// Add radio buttons to the panel
_SymbolOptionsPanel.Controls.Add(_SymbolOption0);
_SymbolOptionsPanel.Controls.Add(_SymbolOption1);
_SymbolOptionsPanel.Controls.Add(_SymbolOption2);
_SymbolOptionsPanel.Controls.Add(_SymbolOption3);
_SymbolOptionsPanel.Controls.Add(_SymbolOption4);

// Option for symbol position
var _SymbolPositionLabel = new Label()
{
    Text = "Symbol Position:",
    AutoSize = true,
    Padding = new Padding(0, 5, 0, 5)
};

var _SymbolPositionOption1 = new RadioButton()
{
    Text = "Beginning",
    AutoSize = true,
    Checked = true
};

var _SymbolPositionOption2 = new RadioButton()
{
    Text = "End",
    AutoSize = true
};

// Symbol position
var _SymbolPositionPanel = new FlowLayoutPanel()
{
    FlowDirection = FlowDirection.LeftToRight,
    AutoSize = true,
    AutoSizeMode = AutoSizeMode.GrowAndShrink,
    WrapContents = false
};

// Add radio buttons to the panel
_SymbolPositionPanel.Controls.Add(_SymbolPositionOption1);
_SymbolPositionPanel.Controls.Add(_SymbolPositionOption2);

// Spacing options
var _SpacingLabel = new Label()
{
    Text = "Symbol Spacing:",
    AutoSize = true,
    Padding = new Padding(0, 5, 0, 5)
};

var _SpacingOption1 = new RadioButton()
{
    Text = "No Space",
    AutoSize = true,
    Checked = true
};

var _SpacingOption2 = new RadioButton()
{
    Text = "Space at Start",
    AutoSize = true
};

var _SpacingOption3 = new RadioButton()
{
    Text = "Space at End",
    AutoSize = true
};

// Spacing options layout
var _SpacingOptionsPanel = new FlowLayoutPanel()
{
    FlowDirection = FlowDirection.LeftToRight,
    AutoSize = true,
    AutoSizeMode = AutoSizeMode.GrowAndShrink,
    WrapContents = false
};

// Add radio buttons to the panel
_SpacingOptionsPanel.Controls.Add(_SpacingOption1);
_SpacingOptionsPanel.Controls.Add(_SpacingOption2);
_SpacingOptionsPanel.Controls.Add(_SpacingOption3);


// Nr decimal places
var _DecimalPlacesLabel = new Label()
{
    Text = "Decimal Places:",
    AutoSize = true,
    Padding = new Padding(0, 5, 0, 5)
};
var _DecimalPlacesComboBox = new ComboBox()
{
    DropDownStyle = ComboBoxStyle.DropDownList,
    Width = 100
};
_DecimalPlacesComboBox.Items.AddRange(new string[] { "0", "1", "2", "3", "4", "Over 9000" });
_DecimalPlacesComboBox.SelectedIndex = 0;


// Percentage checkbox
var _PercentageLabel = new Label()
{
    Text = "Percentage:",
    AutoSize = true,
    Padding = new Padding(0, 5, 0, 5)
};
var _PercentageCheckBox = new CheckBox()
{
    AutoSize = true
};

// Adding a suffix
var _SuffixLabel = new Label()
{
    Text = "Suffix:",
    AutoSize = true,
    Padding = new Padding(0, 5, 0, 5)
};
var _SuffixTextBox = new TextBox()
{
    Width = 100
};

// Adding a prefix
var _PrefixLabel = new Label()
{
    Text = "Prefix:",
    AutoSize = true,
    Padding = new Padding(0, 5, 0, 5)
};
var _PrefixTextBox = new TextBox()
{
    Width = 100
};


// Sample formatted number label
var _SampleLabel = new Label()
{
    Text = "Sample Output:",
    AutoSize = true,
    Padding = new Padding(0, 5, 0, 5)
};
var _SampleValueLabel = new Label()
{
    Text = "",
    AutoSize = true
};


// Add controls to the format options panel
int rowIndex = 0;

_FormatOptionsPanel.Controls.Add(_SymbolLabel, 0, rowIndex);
_FormatOptionsPanel.Controls.Add(_SymbolOptionsPanel, 1, rowIndex);
rowIndex++;

_FormatOptionsPanel.Controls.Add(_SymbolPositionLabel, 0, rowIndex);
_FormatOptionsPanel.Controls.Add(_SymbolPositionPanel, 1, rowIndex);
rowIndex++;

_FormatOptionsPanel.Controls.Add(_SpacingLabel, 0, rowIndex);
_FormatOptionsPanel.Controls.Add(_SpacingOptionsPanel, 1, rowIndex);
rowIndex++;

_FormatOptionsPanel.Controls.Add(_DecimalPlacesLabel, 0, rowIndex);
_FormatOptionsPanel.Controls.Add(_DecimalPlacesComboBox, 1, rowIndex);
rowIndex++;

_FormatOptionsPanel.Controls.Add(_PercentageLabel, 0, rowIndex);
_FormatOptionsPanel.Controls.Add(_PercentageCheckBox, 1, rowIndex);
rowIndex++;

_FormatOptionsPanel.Controls.Add(_SuffixLabel, 0, rowIndex);
_FormatOptionsPanel.Controls.Add(_SuffixTextBox, 1, rowIndex);
rowIndex++;

_FormatOptionsPanel.Controls.Add(_PrefixLabel, 0, rowIndex);
_FormatOptionsPanel.Controls.Add(_PrefixTextBox, 1, rowIndex);
rowIndex++;

_FormatOptionsPanel.Controls.Add(_SampleLabel, 0, rowIndex);
_FormatOptionsPanel.Controls.Add(_SampleValueLabel, 1, rowIndex);
rowIndex++;

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
    Text = "Apply Format",
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
_Form.Controls.Add(_FormatOptionsPanel);
_Form.Controls.Add(_ButtonPanel);

// Event handler to update sample output
void UpdateSampleOutput(object sender, EventArgs e)
{
    // Retrieve format options
    string _SymbolPositive = "";
    string _SymbolNegative = "";

    if (_SymbolOption1.Checked)
    {
        _SymbolPositive = "+";
        _SymbolNegative = "-";
    }
    else if (_SymbolOption2.Checked)
    {
        _SymbolPositive = "\u2191"; // Up arrow ↑
        _SymbolNegative = "\u2193"; // Down arrow ↓
    }
    else if (_SymbolOption3.Checked)
    {
        _SymbolPositive = "\u25B2"; // Black up-pointing triangle ▲
        _SymbolNegative = "\u25BC"; // Black down-pointing triangle ▼
    }
    else if (_SymbolOption4.Checked)
    {
        _SymbolPositive = "\u2197"; // North East Arrow ↗
        _SymbolNegative = "\u2198"; // South East Arrow ↘
    }

    int _DecimalPlaces;
    if (_DecimalPlacesComboBox.SelectedItem.ToString() == "Over 9000")
    {
        _DecimalPlaces = 9001;
    }
    else
    {
        _DecimalPlaces = int.Parse(_DecimalPlacesComboBox.SelectedItem.ToString());
    }
    bool _IsPercentage = _PercentageCheckBox.Checked;
    string _Suffix = _SuffixTextBox.Text;
    string _Prefix = _PrefixTextBox.Text;

    // Symbol position
    bool _SymbolAtBeginning = _SymbolPositionOption1.Checked;

    // Spacing option
    string _SpacePrefix = "";
    string _SpaceSuffix = "";
    if (_SpacingOption2.Checked)
    {
        _SpacePrefix = " ";
    }
    else if (_SpacingOption3.Checked)
    {
        _SpaceSuffix = " ";
    }

    // Generate numeric format string
    string _NumericFormat = "#,##0";
    if (_DecimalPlaces > 0)
    {
        _NumericFormat += "." + new string('0', _DecimalPlaces);
    }
    if (_IsPercentage)
    {
        _NumericFormat += "%";
    }

    // Adjust format string based on options
    string _PositiveFormat, _NegativeFormat, _ZeroFormat;

    if (_SymbolAtBeginning)
    {
        _PositiveFormat = _Prefix + _SymbolPositive + _SpacePrefix + "{0:" + _NumericFormat + "}" + _Suffix + _SpaceSuffix;
        _NegativeFormat = _Prefix + _SymbolNegative + _SpacePrefix + "{0:" + _NumericFormat + "}" + _Suffix + _SpaceSuffix;
    }
    else
    {
        _PositiveFormat = _Prefix + "{0:" + _NumericFormat + "}" + _SpaceSuffix + _SymbolPositive + _Suffix;
        _NegativeFormat = _Prefix + "{0:" + _NumericFormat + "}" + _SpaceSuffix + _SymbolNegative + _Suffix;
    }
    _ZeroFormat = _Prefix + "{0:" + _NumericFormat + "}" + _Suffix;

    string _FormatString = _PositiveFormat + ";" + _NegativeFormat + ";" + _ZeroFormat;

    // Format the sample number
    double sampleNumber = 1234.56;
    string formattedNumber = string.Format(_FormatString, sampleNumber);

    // Update the sample value label
    _SampleValueLabel.Text = formattedNumber;
}

// Attach event handlers to controls
_SymbolOption1.CheckedChanged += UpdateSampleOutput;
_SymbolOption2.CheckedChanged += UpdateSampleOutput;
_SymbolOption3.CheckedChanged += UpdateSampleOutput;
_SymbolOption4.CheckedChanged += UpdateSampleOutput;
_SymbolPositionOption1.CheckedChanged += UpdateSampleOutput;
_SymbolPositionOption2.CheckedChanged += UpdateSampleOutput;
_SpacingOption1.CheckedChanged += UpdateSampleOutput;
_SpacingOption2.CheckedChanged += UpdateSampleOutput;
_SpacingOption3.CheckedChanged += UpdateSampleOutput;
_DecimalPlacesComboBox.SelectedIndexChanged += UpdateSampleOutput;
_PercentageCheckBox.CheckedChanged += UpdateSampleOutput;
_SuffixTextBox.TextChanged += UpdateSampleOutput;
_PrefixTextBox.TextChanged += UpdateSampleOutput;

// Initialize the sample output
UpdateSampleOutput(null, null);

// Display the form and capture the result
var _Result = _Form.ShowDialog();

if (_Result == DialogResult.OK)
{
    // Retrieve format options
    string _SymbolPositive = "";
    string _SymbolNegative = "";

    if (_SymbolOption1.Checked)
    {
        _SymbolPositive = "+";
        _SymbolNegative = "-";
    }
    else if (_SymbolOption2.Checked)
    {
        _SymbolPositive = "\u2191"; // Up arrow ↑
        _SymbolNegative = "\u2193"; // Down arrow ↓
    }
    else if (_SymbolOption3.Checked)
    {
        _SymbolPositive = "\u25B2"; // Black up-pointing triangle ▲
        _SymbolNegative = "\u25BC"; // Black down-pointing triangle ▼
    }
    else if (_SymbolOption4.Checked)
    {
        _SymbolPositive = "\u2197"; // North East Arrow ↗
        _SymbolNegative = "\u2198"; // South East Arrow ↘
    }

    int _DecimalPlaces;
    if (_DecimalPlacesComboBox.SelectedItem.ToString() == "Over 9000")
    {
        _DecimalPlaces = 9001;
    }
    else
    {
        _DecimalPlaces = int.Parse(_DecimalPlacesComboBox.SelectedItem.ToString());
    }
    bool _IsPercentage = _PercentageCheckBox.Checked;
    string _Suffix = _SuffixTextBox.Text;
    string _Prefix = _PrefixTextBox.Text;

    // Symbol position
    bool _SymbolAtBeginning = _SymbolPositionOption1.Checked;

    // Spacing option
    string _SpacePrefix = "";
    string _SpaceSuffix = "";
    if (_SpacingOption2.Checked)
    {
        _SpacePrefix = " ";
    }
    else if (_SpacingOption3.Checked)
    {
        _SpaceSuffix = " ";
    }

    // Generate numeric format string
    string _NumericFormat = "#,##0";
    if (_DecimalPlaces > 0)
    {
        _NumericFormat += "." + new string('0', _DecimalPlaces);
    }
    if (_IsPercentage)
    {
        _NumericFormat += "%";
    }

    // Adjust format string based on options
    string _PositiveFormat, _NegativeFormat, _ZeroFormat;

    if (_SymbolAtBeginning)
    {
        _PositiveFormat = _Prefix + _SymbolPositive + _SpacePrefix + _NumericFormat + _Suffix + _SpaceSuffix;
        _NegativeFormat = _Prefix + _SymbolNegative + _SpacePrefix + _NumericFormat + _Suffix + _SpaceSuffix;
    }
    else
    {
        _PositiveFormat = _Prefix + _NumericFormat + _SpaceSuffix + _SymbolPositive + _Suffix;
        _NegativeFormat = _Prefix + _NumericFormat + _SpaceSuffix + _SymbolNegative + _Suffix;
    }
    _ZeroFormat = _Prefix + _NumericFormat + _Suffix;

    string _FormatString = _PositiveFormat + ";" + _NegativeFormat + ";" + _ZeroFormat;


    // Apply the format string to selected measures
    foreach (var _m in Selected.Measures)
    {
        _m.FormatString = _FormatString;
    }

    // Inform the user
    Info("Format string applied to " + Selected.Measures.Count() + " measures:\n" + _FormatString);
}
else
{
    Error("Script Cancelled. No changes made to the model.");
}
