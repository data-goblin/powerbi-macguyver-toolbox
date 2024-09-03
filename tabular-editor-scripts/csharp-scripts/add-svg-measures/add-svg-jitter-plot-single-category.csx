// Script author: Kurt Buhler; Data Goblins
// Script created: Sept 3, 2024
// Script supports: Tabular Editor 2.X, Tabular Editor 3.X.
//
// Original template author: Kerry Kolosko
// Original template reference: https://kerrykolosko.com/portfolio/barcode-jitter-scatter/
// Template modified by: Kurt Buhler
//
// Template limitations: This template supports a limited number of datapoints (dots) to display at once, due to limitations of the DAX measure string length.
//
// Script instructions: Use this script when connected with any Power BI semantic model. Doesn't support AAS models.
//
// 1. Select your measure table - or the table where you want to place the measure - in the TOM Explorer.
// 2. Run the script and validate the resulting DAX. Troubleshoot any possible errors, if necessary.
// 3. Add the measure to a table or matrix visual.
// 4. Set the "Image size" property of the visual to Height: 102 and Width: 20. If you use another size, ensure that you adjust the measure DAX.
// 5. 


// DAX template
string _SvgString = @"
-- SVG measure
-- Use this inside of a Table or a Matrix visual.
-- The 'Image size' property of the Table or Matrix must match the values in the config below

VAR _Values = VALUES( __DETAIL_COLUMN ) -- NOTE: This column has a limited number of values that you can use.
VAR _ValuesByMeasure = ADDCOLUMNS( _Values, ""@Measure"", __MEASURE )

VAR _AxisMax = CALCULATE(MAXX( _ValuesByMeasure, ''[@Measure] ), REMOVEFILTERS( )) * 1.1
// VAR _AxisMin = CALCULATE(MINX( _ValuesByMeasure, ''[@Measure] ), REMOVEFILTERS( ))

VAR _SvgWidth = 102     -- NOTE: Match this value in the Image size property of the table or matrix
VAR _SvgHeight = 20     -- NOTE: Match this value in the Image size property of the table or matrix

VAR _JitterAmount = 10  -- NOTE: Adjust this value to control the amount of jitter

VAR _Avg = DIVIDE( AVERAGEX( _ValuesByMeasure, ''[@Measure] ), _AxisMax ) * _SvgWidth
VAR _AvgLine = ""<rect x='"" & _Avg & ""' y='2' width='1.25' height='80%' fill='red'/>""

RETURN
IF(
    HASONEVALUE( __GROUP_BY_COLUMN ),
    ""data:image/svg+xml;utf8, "" &
    ""<svg width='"" & _SvgWidth & ""' height='"" & _SvgHeight & ""' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' overflow='visible'>"" & 
    CONCATENATEX(
        _Values, 
        VAR _JitterValue = RAND() * _JitterAmount - (_JitterAmount / 2)
        RETURN 
        ""<circle cx='"" & 
        (__MEASURE / _AxisMax * _SvgWidth) & 
        ""' cy='"" & ( _SvgHeight / 2 + _JitterValue) & ""' stroke='#333333' stroke-width='1' stroke-opacity='0.5' r='1.5' fill='gainsboro' fill-opacity='0.5' />"",
        """"
    ) &
    _AvgLine &
    ""</svg>""
)";


// Selected values you want to use in the plot.
var _Measure = SelectMeasure(Model.AllMeasures, null,"Select the measure that you want to plot:");
var _Detail = SelectColumn(Model.AllColumns, null, "Select the column for which you want to plot each value as a dot\nin the plot (WARNING: LIMITED UNIQUE VALUES):");
var _GroupBy = SelectColumn(Model.AllColumns, null, "Select the column for which you will group the data in\nthe table or matrix visual:");

_SvgString = _SvgString.Replace( "__DETAIL_COLUMN", _Detail.DaxObjectFullName ).Replace( "__GROUP_BY_COLUMN", _GroupBy.DaxObjectFullName ).Replace( "__MEASURE", _Measure.DaxObjectFullName );


// Optional: If you don't want the table context-sensitivity in the TOM Explorer, use the below.
// var _SelectedTable = SelectTable();
// var _SvgMeasure = _SelectedTable.AddMeasure( "New SVG Jitter Plot (Single Category)", _SvgString, "SVGs");

// Adding the measure.
var _SelectedTable = Selected.Table;
string _Desc = "SVG Jitter Plot of " + _Measure.Name + " for each " + _Detail.Name + ", grouped by " + _GroupBy.Name;
var _SvgMeasure = _SelectedTable.AddMeasure( "New SVG Jitter Plot (Single Category)", _SvgString, "SVGs");

// Setting measure properties.
_SvgMeasure.DataCategory = "ImageUrl";
_SvgMeasure.IsHidden = true;
_SvgMeasure.Description = _Desc;

// Notification InfoBox.
Info("Added new SVG measure to the table " + _SelectedTable.Name + ".\n\nValidate the SVG definition and test the measure carefully in many different filter contexts before using it in reports.\nDon't forget to rename the new measure.");