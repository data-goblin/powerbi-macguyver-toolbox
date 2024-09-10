// Script author: Kurt Buhler; Data Goblins
// Script created: Sept 9, 2024
// Script supports: Tabular Editor 2.X, Tabular Editor 3.X.
//
// Original template author: Kurt Buhler
//
// Script instructions: Use this script when connected with any Power BI semantic model. Doesn't support AAS models.
//
// 1. Select your measure table - or the table where you want to place the measure - in the TOM Explorer.
// 2. Run the script and validate the resulting DAX. Troubleshoot any possible errors, if necessary.
// 3. Add the measure to a table or matrix visual.
// 4. Set the "Image size" property of the visual to Height: 25 and Width: 100. If you use another size, you might need to adjust the measure DAX.
// 5. Change the values or colors in the measure DAX to ensure that the formatting works as you expect.
// 6. Validate the SVG visual in different filter contexts to ensure that it is accurate and performant.


// DAX template
string _SvgString = @"
-- Use this inside of a Table or a Matrix visual.
-- The 'Image size' property of the Table or Matrix should be set to ""Height"" of 25 px and ""Width"" of 100 px for the best results.


----------------------------------------------------------------------------------------
-------------------- START CONFIG - SAFELY CHANGE STUFF IN THIS AREA -------------------
----------------------------------------------------------------------------------------


-- Input field config
IF (
    HASONEVALUE( __GROUPBY_COLUMN ),

VAR _CategoryValue =
    SELECTEDVALUE (
        __GROUPBY_COLUMN
    )

VAR _LabelCased = UPPER ( _CategoryValue )

-- Color config.
-- IMPORTANT: You need to change these values to the values you want to color.

VAR _Color =
    SWITCH (
        _CategoryValue,
        ""Value"",     ""#e03131"", -- Red
        ""Value"",     ""#e8590c"", -- Orange
        ""Value"",     ""#f08c00"", -- Yellow
        ""Value"",     ""#2f9e44"", -- Green
        ""Value"",     ""#099268"", -- Teal
        ""Value"",     ""#0c8599"", -- Cyan
        ""Value"",     ""#1971c2"", -- Blue
        ""Value"",     ""#6741d9"", -- Purple
        ""Value"",     ""#9c36b5"", -- Violet
        ""Value"",     ""#c2255c"", -- Pink
        ""Value"",     ""#846358"", -- Brown
        ""Value"",     ""#343a40"", -- Dark Grey
        ""Value"",     ""#1e1e1e"", -- Blackish
        ""Value"",     ""#000000"", -- Black
        ""Value"",   ""#FFFFFF00"", -- Transparent
        ""#000000""
    )
    

-- Font config.
-- IMPORTANT: You need to change these values to the values you want to bold / lighten.

VAR _FontWeight =
    SWITCH (
        _CategoryValue,
        ""Value"", ""950"", -- Extra Black
        ""Value"", ""900"", -- Black
        ""Value"", ""800"", -- Extra Bold
        ""Value"", ""700"", -- Bold
        ""Value"", ""600"", -- Semi Bold
        ""Value"", ""500"", -- Medium
        ""Value"", ""400"", -- Normal
        ""Value"", ""350"", -- Semi Light
        ""Value"", ""300"", -- Light
        ""Value"", ""200"", -- Extra Light
        ""400""
    )

----------------------------------------------------------------------------------------
----------------------- END CONFIG - BEYOND HERE THERE BE DRAGONS ----------------------
----------------------------------------------------------------------------------------


-- Vectors and SVG code
VAR _SvgPrefix =
    ""data:image/svg+xml;utf8, <svg xmlns='http://www.w3.org/2000/svg'>""
    
VAR _Background =
    ""<rect x='0.5' y='0.5' width='98%' height='95%' rx='15%' fill='""
        & _Color & ""22""
        & ""' stroke='""
        & _Color
        & ""'/>""
        
VAR _Label =
    ""<text x='50%' y='53%' font-family='Segoe UI' font-size='10.5' font-weight='""
        & _FontWeight
        & ""' fill='""
        & _Color
        & ""' text-anchor='middle' dominant-baseline='middle'>""
        & _LabelCased
        & ""</text>""
        
VAR _SvgSuffix = ""</svg>""


-- Result
VAR _SVG =
    _SvgPrefix 
    & _Background
    & _Label
    & _SvgSuffix


RETURN
    _SVG
)
";


// Selected values you want to use in the plot.
var _AllColumns = Model.AllColumns.Where(m => m.IsHidden != true).OrderBy(m => m.DaxObjectFullName);
var _GroupBy = SelectColumn(_AllColumns, null, "Select the column that has the text you want to label:");

_SvgString = _SvgString.Replace( "__GROUPBY_COLUMN", _GroupBy.DaxObjectFullName );


// Adding the measure.
var _SelectedTable = Selected.Table;
string _Name = "SVG Status Pill";
string _Desc = _Name + " of " + _GroupBy.Name;
var _SvgMeasure = _SelectedTable.AddMeasure( "New " + _Name, _SvgString, "SVGs\\Text or Callouts");


// Setting measure properties.
_SvgMeasure.DataCategory = "ImageUrl";
_SvgMeasure.IsHidden = true;
_SvgMeasure.Description = _Desc;

// Notification InfoBox.
Info("Added new SVG measure to the table " + _SelectedTable.Name + ".\n\nValidate the SVG specification and test the measure carefully in many different filter contexts before using it in reports.\nDon't forget to rename the new measure.\n\nDon't forget to adjust the labels or colors for formatting, either.");