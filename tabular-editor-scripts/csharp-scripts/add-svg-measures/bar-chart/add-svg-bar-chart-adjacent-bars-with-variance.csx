// Script author: Kurt Buhler; Data Goblins
// Script created: Sept 3, 2024
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
// 5. Validate the SVG visual in different filter contexts to ensure that it is accurate and performant.


// DAX template
string _SvgString = @"
-- SVG measure
-- Use this inside of a Table or a Matrix visual.
-- The 'Image size' property of the Table or Matrix must match the values in the config below


----------------------------------------------------------------------------------------
-------------------- START CONFIG - SAFELY CHANGE STUFF IN THIS AREA -------------------
----------------------------------------------------------------------------------------


-- Input field config
VAR _Actual = __ACTUAL_MEASURE
VAR _Target = __TARGET_MEASURE
VAR _Performance = DIVIDE ( _Actual - _Target, _Target )


-- Chart Config
VAR _BarMax = 100
VAR _BarMin = 0
VAR _Scope = ALLSELECTED ( __GROUPBY_COLUMN )


-- Color config.
VAR _ActualColor        = ""#686868"" -- Charcoal
VAR _TargetColor        = ""#e1dfdd"" -- Grey 
VAR _VarianceColor     = 
    IF ( 
        _Performance < 0, 
        ""#fab005"",  -- Yellow
        ""#2094ff""   -- Blue
    )


----------------------------------------------------------------------------------------
----------------------- END CONFIG - BEYOND HERE THERE BE DRAGONS ----------------------
----------------------------------------------------------------------------------------


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


-- Normalize values (to get position along X-axis)
    VAR _AxisRange = 
        _BarMax - _BarMin
    
    VAR _ActualNormalized = 
        DIVIDE ( _Actual, _AxisMax ) * _AxisRange

    VAR _TargetNormalized = 
        DIVIDE ( _Target, _AxisMax ) * _AxisRange


-- Vectors and SVG code
VAR _SvgPrefix = ""data:image/svg+xml;utf8, <svg xmlns='http://www.w3.org/2000/svg'>""

VAR _Sort = ""<desc>"" & FORMAT ( _Actual, ""000000000000"" ) & ""</desc>""

VAR _ActualBar     = ""<rect x='"" & _BarMin & ""' y='4' width='"" & _ActualNormalized & ""' height='8' fill='"" & _ActualColor & ""'/>""
VAR _TargetBar     = ""<rect x='"" & _BarMin & ""' y='13' width='"" & _TargetNormalized & ""' height='8' fill='"" & _TargetColor & ""'/>""
VAR _VarianceBar   = ""<rect x='"" & _BarMin + MIN( _ActualNormalized, _TargetNormalized ) & ""' y='"" & IF ( _Target > _Actual, 4, 13 ) & ""' width='"" & ABS( _ActualNormalized - _TargetNormalized ) & ""' height='8' fill='"" & _VarianceColor & ""'/>""

VAR _SvgSuffix = ""</svg>""


-- Final result
VAR _SVG = 
    _SvgPrefix 
    
    & _Sort

    & _TargetBar
    & _ActualBar 
    & _VarianceBar

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

// Adding the measure.
var _SelectedTable = Selected.Table;
string _Name = "SVG Bar Chart (Adjacent Target and Variance)";
string _Desc = _Name + " of " + _Actual.Name + " vs. " + _Target.Name + ", grouped by " + _GroupBy.Name;
var _SvgMeasure = _SelectedTable.AddMeasure( "New " + _Name, _SvgString, "SVGs\\Bar Chart");

// Setting measure properties.
_SvgMeasure.DataCategory = "ImageUrl";
_SvgMeasure.IsHidden = true;
_SvgMeasure.Description = _Desc;

// Notification InfoBox.
if(!Model.HasAnnotation("_svg_actual"))
Info("Added new SVG measure to the table " + _SelectedTable.Name + ".\n\nValidate the SVG specification and test the measure carefully in many different filter contexts before using it in reports.\nDon't forget to rename the new measure.");