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

-- Input field config
VAR _Actual = __ACTUAL_MEASURE
VAR _Target = __TARGET_MEASURE


-- SVG configuration
VAR _SvgWidth = 100
VAR _SvgMin = 0
VAR _SvgHeight = 25

VAR _Scope = ALLSELECTED ( __GROUPBY_COLUMN )
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

VAR _AxisRange = _SvgWidth - _SvgMin

VAR _ActualNormalized = DIVIDE ( _Actual, _AxisMax ) * _AxisRange
VAR _TargetNormalized = DIVIDE ( _Target, _AxisMax ) * _AxisRange


-- Color config
VAR _TargetCircleColor = ""#F5F5F5""
VAR _TargetStrokeColor = ""#C7C7C7""
VAR _AxisColor =         ""#C7C7C7""

-- Blue
VAR _OnTargetFill      = ""#448FD6""
VAR _OnTargetStroke    = ""#2F6698""

-- Red
VAR _OffTargetFill     = ""#D64444""
VAR _OffTargetStroke   = ""#982F2F""

VAR _Fill = IF ( _Actual > _Target, _OnTargetFill, _OffTargetFill )
VAR _Stroke = IF ( _Actual > _Target, _OnTargetStroke, _OffTargetStroke )


-- Vectors and SVG specification
VAR _SvgPrefix = ""data:image/svg+xml;utf8, <svg width='"" & _SvgWidth & ""' height='"" & _SvgHeight & ""' xmlns='http://www.w3.org/2000/svg'>""

VAR _Sort = ""<desc>"" & FORMAT ( _Actual, ""000000000000"" ) & ""</desc>""

VAR _Axis = ""<line x1='0' y1='"" & _SvgHeight / 2 & ""' x2='"" & _SvgWidth & ""' y2='"" & _SvgHeight / 2 & ""' stroke='"" & _AxisColor & ""'/>""
VAR _Origin = ""<circle cx='2' cy='"" & _SvgHeight / 2 & ""' r='2' fill='"" & _AxisColor & ""'/>""

VAR _ActualCircle = ""<circle cx='"" & _ActualNormalized & ""' cy='"" & _SvgHeight / 2 & ""' r='4' fill='"" & _Fill & ""' stroke='"" & _Stroke & ""' stroke-width='1.5'/>""
VAR _TargetCircle = ""<circle cx='"" & _TargetNormalized & ""' cy='"" & _SvgHeight / 2 & ""' r='4' fill='"" & _TargetCircleColor & ""' stroke='"" & _TargetStrokeColor & ""' stroke-width='1.5'/>""
VAR _DumbbellLine = ""<line x1='"" & _ActualNormalized & ""' y1='"" & _SvgHeight / 2 & ""' x2='"" & _TargetNormalized & ""' y2='"" & _SvgHeight / 2 & ""' stroke='"" & _Fill & ""' stroke-width='3'/>""

VAR _SvgSuffix = ""</svg>""


-- Final result
VAR _Svg = 
    _SvgPrefix 

    & _Sort

    & _Axis
    & _Origin
    
    & _DumbbellLine
    & _TargetCircle
    & _ActualCircle

    & _SvgSuffix

RETURN
	 _Svg
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
string _Desc = "SVG Dumbbell Chart of " + _Actual.Name + " vs. " + _Target.Name + ", grouped by " + _GroupBy.Name;
var _SvgMeasure = _SelectedTable.AddMeasure( "New SVG Dumbbell Chart", _SvgString, "SVGs\\Dumbbell Plot");


// Setting measure properties.
_SvgMeasure.DataCategory = "ImageUrl";
_SvgMeasure.IsHidden = true;
_SvgMeasure.Description = _Desc;


// Notification InfoBox.
if(!Model.HasAnnotation("_svg_actual"))
Info("Added new SVG measure to the table " + _SelectedTable.Name + ".\n\nValidate the SVG definition and test the measure carefully in many different filter contexts before using it in reports.\nDon't forget to rename the new measure.");