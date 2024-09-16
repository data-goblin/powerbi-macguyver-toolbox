// Script author: Kurt Buhler; Data Goblins
// Script created: Sept 3, 2024
// Script supports: Tabular Editor 2.X, Tabular Editor 3.X.
//
// Original template author: Kurt Buhler
//
//
// Script instructions: Use this script when connected with any Power BI semantic model. Doesn't support AAS models.
//
// 1. Select your measure table - or the table where you want to place the measure - in the TOM Explorer.
// 2. Run the script and validate the resulting DAX. Troubleshoot any possible errors, if necessary.
// 3. Add the measure to a table or matrix visual.
// 4. Set the "Image size" property of the visual to Height: 25 and Width: 100. If you use another size, you might need to adjust the measure DAX.
// 5. Validate the SVG visual in different filter contexts to ensure that it is accurate and performant.


// DAX template
string _SvgString = @"
-- Use this inside of a Table or a Matrix visual.
-- The 'Image size' property of the Table or Matrix should be set to a 'Height' of 25px and a 'Width' of 100px


----------------------------------------------------------------------------------------
-------------------- START CONFIG - SAFELY CHANGE STUFF IN THIS AREA -------------------
----------------------------------------------------------------------------------------


-- Input field config
VAR _Actual = __ACTUAL_MEASURE
VAR _Target = __TARGET_MEASURE
VAR _Performance = DIVIDE ( _Actual - _Target, _Target ) 


-- Sentiment config (percent)
VAR _VeryBad    = -0.25
VAR _Bad        = -0.1
VAR _Good       = 0.1
VAR _VeryGood   = 0.25


-- Chart Config
VAR _BarMax = 100
VAR _BarMin = 20
VAR _Scope = ALL ( __GROUPBY_COLUMN )


-- Color config.
VAR _BackgroundColor = ""#F5F5F5""                  -- Light grey
VAR _BarFillColor  = ""#CFCFCF""                    -- Medium grey
VAR _BaselineColor = ""#737373""                    -- Dark grey
VAR _TargetColor = ""black""                        -- Black

VAR _ActionDotFill = 
    SWITCH (
        TRUE(),
        _Performance < _VeryBad, ""#f4ae4c"",       -- Dark yellow
        _Performance < _Bad, ""#ffe075"",           -- Light yellow
        _Performance > _Good, ""#74B2FF"",          -- Light blue
        _Performance > _VeryGood, ""#2D6390"",      -- Dark blue
        ""#FFFFFF00""                               -- Transparent
    )


----------------------------------------------------------------------------------------
----------------------- END CONFIG - BEYOND HERE THERE BE DRAGONS ----------------------
----------------------------------------------------------------------------------------


    -- Get axis maximum
    VAR _MaxActualsInScope = 
        CALCULATE(
            MAXX(
                _Scope,
                __ACTUAL_MEASURE
            ),
            REMOVEFILTERS( __GROUPBY_COLUMN )
        )
    
    VAR _MaxTargetInScope = 
        CALCULATE(
            MAXX(
                _Scope,
                __TARGET_MEASURE
            ),
            REMOVEFILTERS( __GROUPBY_COLUMN )
        )
    
    VAR _AxisMax = 
        IF (
            HASONEVALUE ( __GROUPBY_COLUMN ),
            MAX( _MaxActualsInScope, _MaxTargetInScope ),
            CALCULATE( MAX( __ACTUAL_MEASURE, __TARGET_MEASURE ), REMOVEFILTERS( __GROUPBY_COLUMN ) )
        ) * 1.1
    
    VAR _AxisRange = 
        _BarMax - _BarMin

    -- Normalize values (to get position along X-axis)
    VAR _ActualNormalized = ( DIVIDE ( _Actual, _AxisMax ) * _AxisRange )
    VAR _TargetNormalized = ( DIVIDE ( _Target, _AxisMax ) * _AxisRange ) + _BarMin - 1


-- Vectors and SVG code
VAR _SvgPrefix = ""data:image/svg+xml;utf8, <svg xmlns='http://www.w3.org/2000/svg'>""

VAR _Sort = ""<desc>"" & FORMAT ( _Actual, ""000000000000"" ) & ""</desc>""

VAR _ActionDot  = ""<circle cx='10' cy='11' r='5' fill='"" & _ActionDotFill &""'/>""

VAR _BarBaseline = ""<rect x='"" & _BarMin & ""' y='4' width='1' height='60%' fill='"" & _BaselineColor & ""'/>""
VAR _BarBackground = ""<rect x='"" & _BarMin & ""' y='2' width='"" & _BarMax & ""' height='80%' fill='"" & _BackgroundColor & ""'/>""
VAR _ActualBar  = ""<rect x='"" & _BarMin & ""' y='5' width='"" & _ActualNormalized & ""' height='50%' fill='"" & _BarFillColor & ""'/>""
VAR _TargetLine = ""<rect x='"" & _TargetNormalized & ""' y='2' width='2' height='80%' fill='"" & _TargetColor & ""'/>""

VAR _SvgSuffix = ""</svg>""


-- Final result
VAR _SVG =
    _SvgPrefix 

    & _Sort

    & _ActionDot

    & _BarBackground
    & _ActualBar
    & _BarBaseline
    & _TargetLine

    & _SvgSuffix

RETURN
    _SVG
";


// Selected values you want to use in the plot.
var _AllMeasures = Model.AllMeasures.Where(m => m.IsHidden != true).OrderBy(m => m.Name);
var _AllColumns = Model.AllColumns.Where(c => c.IsHidden != true).OrderBy(c => c.DaxObjectFullName);

Measure _Actual;
Measure _Target;
Column _GroupBy;
_Actual = Model.AllMeasures.FirstOrDefault(m => m.DaxObjectName == Model.GetAnnotation("_svg_actual"));
_Target = Model.AllMeasures.FirstOrDefault(m => m.DaxObjectName == Model.GetAnnotation("_svg_target"));
_GroupBy = Model.AllColumns.FirstOrDefault(c => c.DaxObjectFullName == Model.GetAnnotation("_svg_column"));
if(_Actual == null) _Actual = SelectMeasure(_AllMeasures, null,"Select the measure that you want to measure:");
if(_Actual == null) return;
if(_Target == null) _Target = SelectMeasure(_AllMeasures, null,"Select the measure that you want to compare to:");
if(_Target == null) return;
if(_GroupBy == null) _GroupBy = SelectColumn(_AllColumns, null, "Select the column for which you will group the data in\nthe table or matrix visual:");
if(_GroupBy == null) return;

_SvgString = _SvgString.Replace( "__ACTUAL_MEASURE", _Actual.DaxObjectFullName ).Replace( "__TARGET_MEASURE", _Target.DaxObjectFullName ).Replace( "__GROUPBY_COLUMN", _GroupBy.DaxObjectFullName );


// Optional: If you don't want the table context-sensitivity in the TOM Explorer, use the below.
// var _SelectedTable = SelectTable();
// var _SvgMeasure = _SelectedTable.AddMeasure( "New SVG Jitter Plot (Single Category)", _SvgString, "SVGs");

// Adding the measure.
var _SelectedTable = Selected.Table;
string _Name = "SVG Bullet Chart (with Action Dots)";
string _Desc = _Name + " of " + _Actual.Name + " vs. " + _Target.Name + ", grouped by " + _GroupBy.Name;
var _SvgMeasure = _SelectedTable.AddMeasure( "New " + _Name, _SvgString, "SVGs\\Bullet Chart");

// Setting measure properties.
_SvgMeasure.DataCategory = "ImageUrl";
_SvgMeasure.IsHidden = true;
_SvgMeasure.Description = _Desc;

// Notification InfoBox.
if(!Model.HasAnnotation("_svg_actual"))
Info("Added new SVG measure to the table " + _SelectedTable.Name + ".\n\nValidate the SVG definition and test the measure carefully in many different filter contexts before using it in reports.\nDon't forget to rename the new measure.");