// Script author: Kurt Buhler; Data Goblins
// Script created: Sept 3, 2024
// Script supports: Tabular Editor 2.X, Tabular Editor 3.X.
//
// Script instructions: Use this script when connected with any Power BI semantic model. Doesn't support AAS models.
//
// 1. In Figma, add or create the image you want in your report.
// 2. In Figma, select the image and right-click, then select "Copy/Paste as > Copy as SVG".
// 3. With the SVG definition on your clipboard, go to Tabular Editor and select the measure table in your model.
// 4. Run the script, validate the SVG definition and re-name the measure.
// 5. Refresh your report and add the new measure to a table.
// 6. (Optional) Change the image size property of the table to re-size the image.
//               If you do this, you have to also adjust the 'width' and 'height' properties in the SVG definition.


using System.Windows.Forms;
using System.Text.RegularExpressions;

// Hide the 'Running Macro' spinbox
ScriptHelper.WaitFormVisible = false;

if ( Clipboard.ContainsText() )
{
    if ( Clipboard.GetText().Contains("<svg") )
    {

        string _Comments   = "-- SVG measure added on " + System.DateTime.Today.ToShortDateString() +" via a Tabular Editor script.\n-- Use in a visual that supports ImageUrl measures, like Table, Matrix, New Card or New Slicer visuals.\n\n";
        string _SvgPrefix  = "data:image/svg+xml;utf8, \n\n";
        string _SvgViewbox = @"viewBox='\d+\s+\d+\s+\d+\s+\d+' ";
        string _SvgFill    = @"fill='.+' ";
        string _SvgString  = _Comments + "\"" + _SvgPrefix + Clipboard.GetText().Replace("\"","'") + "\"";

        // Remove the viewbox
        _SvgString = Regex.Replace(_SvgString, _SvgViewbox, "");
        
        // Remove the SVG fill
        _SvgString = Regex.Replace(_SvgString, _SvgFill, "");
        
        // Optional: If you don't want the table context-sensitivity in the TOM Explorer, replace Line 29 with Lines 26-27.
        // var _SelectedTable = SelectTable();
        // var _SvgMeasure = _SelectedTable.AddMeasure( "New SVG Measure", _SvgString, "SVGs");
        
        var _SelectedTable = Selected.Table;
        var _SvgMeasure = _SelectedTable.AddMeasure( "New SVG Measure", _SvgString, "SVGs");
        
        _SvgMeasure.DataCategory = "ImageUrl";
        _SvgMeasure.IsHidden = true;
        
        // Notification InfoBox.
        Info("Added new SVG measure to the table " + _SelectedTable.Name + ".\n\nValidate the SVG definition before using it.\nDon't forget to rename the new measure.");

    }
    else
        Error("Clipboard does not contain a valid SVG definition. Try copying an SVG definition from Figma, first.");
}
else
    Error("Clipboard does not contain a valid SVG definition. Try copying an SVG definition from Figma, first.");