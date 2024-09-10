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
-- The 'Image size' property of the Table or Matrix should be set to ""Height"" of 25 px and ""Width"" of 100 px for the best results.


----------------------------------------------------------------------------------------
-------------------- START CONFIG - SAFELY CHANGE STUFF IN THIS AREA -------------------
----------------------------------------------------------------------------------------


-- Input field config
VAR _Actual = __ACTUAL_MEASURE

VAR _TargetOneNameText = ""1YP"" -- Change this label
VAR _TargetOne = __TARGET_ONE_MEASURE
VAR _PerformanceOne = DIVIDE ( _Actual - _TargetOne, _TargetOne ) 

VAR _TargetTwoNameText = ""2YP"" -- Change this label
VAR _TargetTwo = __TARGET_TWO_MEASURE
VAR _PerformanceTwo = DIVIDE ( _Actual - _TargetTwo, _TargetTwo ) 


-- Chart Config
VAR _BarMax = 100
VAR _BarMin = 30
VAR _Scope = ALLSELECTED ( __GROUPBY_COLUMN ) -- Table comprising all values that group the actuals and targets


-- Sentiment config (percent)
VAR _VeryBad    = -0.1
VAR _Bad        = 0
VAR _Good       = 0
VAR _VeryGood   = 0.1


-- Color config.
---- Bar color
VAR _BackgroundColor = ""#F5F5F5""  -- Light grey
VAR _BarFillColor  = ""#CFCFCF""    -- Medium grey
VAR _BaselineColor = ""#C7C7C7""    -- Dark grey

---- Target color
VAR _TargetOneColor = ""#33333399"" -- Charcoal
VAR _TargetTwoColor = ""#7177d899"" -- Purple


---- Action dot color (conditional)
VAR _ActionDotOneFill = 
    SWITCH (
        TRUE(),
        _PerformanceOne < _VeryBad, ""#f4ae4c"",  -- Dark yellow
        _PerformanceOne < _Bad, ""#ffe075"",      -- Light yellow
        _PerformanceOne > _Good, ""#74B2FF"",     -- Light blue
        _PerformanceOne > _VeryGood, ""#2D6390"", -- Dark blue
        ""#FFFFFF00""                             -- Transparent (i.e. hide the dot)
        )

VAR _ActionDotTwoFill = 
    SWITCH (
        TRUE(),
        _PerformanceTwo < _VeryBad, ""#f4ae4c"",  -- Dark yellow
        _PerformanceTwo < _Bad, ""#ffe075"",      -- Light yellow
        _PerformanceTwo > _Good, ""#74B2FF"",     -- Light blue
        _PerformanceTwo > _VeryGood, ""#2D6390"", -- Dark blue
        ""#FFFFFF00""                             -- Transparent (i.e. hide the dot)
    )

----------------------------------------------------------------------------------------
----------------------- END CONFIG - BEYOND HERE THERE BE DRAGONS ----------------------
----------------------------------------------------------------------------------------


-- Only change the parts of this code if you want to adjust how the SVG visual works or add / remove stuff.


    -- Get axis maximum
    VAR _MaxActualsInScope = 
        CALCULATE(
            MAXX(
                _Scope,
                __ACTUAL_MEASURE
            ),
            REMOVEFILTERS( __GROUPBY_COLUMN )
        )
    
    VAR _MaxTargetOneInScope = 
        CALCULATE(
            MAXX(
                _Scope,
                __TARGET_ONE_MEASURE
            ),
            REMOVEFILTERS( __GROUPBY_COLUMN )
        )
    
    VAR _MaxTargetTwoInScope = 
        CALCULATE(
            MAXX(
                _Scope,
                __TARGET_TWO_MEASURE
            ),
            REMOVEFILTERS( __GROUPBY_COLUMN )
        )
    
    VAR _AxisMax = 
        IF (
            HASONEVALUE ( __GROUPBY_COLUMN ),
            MAX( _MaxActualsInScope, MAX( _MaxTargetOneInScope, _MaxTargetTwoInScope ) ),
            CALCULATE( MAX( __ACTUAL_MEASURE, MAX( __TARGET_ONE_MEASURE, __TARGET_TWO_MEASURE ) ), REMOVEFILTERS( __GROUPBY_COLUMN ) )
        ) * 1.1
    
    
    -- Normalize values (to get position along X-axis)
    VAR _AxisRange = 
        _BarMax - _BarMin
    
    VAR _ActualNormalized = 
        DIVIDE ( _Actual, _AxisMax ) * _AxisRange
        
    VAR _TargetOneNormalized = 
        ( DIVIDE ( _TargetOne, _AxisMax ) * _AxisRange ) + _BarMin - 1
        
    VAR _TargetTwoNormalized =
        ( DIVIDE ( _TargetTwo, _AxisMax ) * _AxisRange ) + _BarMin - 1


-- Vectors and SVG code
VAR _SvgPrefix = ""data:image/svg+xml;utf8, <svg xmlns='http://www.w3.org/2000/svg'>""

---- To sort the SVG in a table or matrix by the bar length
VAR _Sort = ""<desc>"" & FORMAT ( _Actual, ""000000000000"" ) & ""</desc>""

VAR _TargetLabelOne = ""<text x='0' y='10' font-family='Segoe UI' font-size='8' font-weight='700' fill='"" & _TargetOneColor & ""'>"" & _TargetOneNameText  & ""</text>""
VAR _ActionDotOne  = ""<circle cx='20' cy='7' r='2.5' fill='"" & _ActionDotOneFill &""'/>""

VAR _TargetLabelTwo = ""<text x='0' y='20' font-family='Segoe UI' font-size='8' font-weight='700' fill='"" & _TargetTwoColor & ""'>"" & _TargetTwoNameText & ""</text>""
VAR _ActionDotTwo  = ""<circle cx='20' cy='17' r='2.5' fill='"" & _ActionDotTwoFill &""'/>""

VAR _BarBaseline = ""<rect x='"" & _BarMin & ""' y='0' width='1' height='100%' fill='"" & _BaselineColor & ""'/>""
VAR _BarBackground = ""<rect x='"" & _BarMin & ""' y='2' width='"" & _BarMax & ""' height='80%' fill='"" & _BackgroundColor & ""'/>""

VAR _ActualBar  = ""<rect x='"" & _BarMin & ""' y='6' width='"" & _ActualNormalized & ""' height='50%' fill='"" & _BarFillColor & ""'/>""
VAR _TargetOneLine = ""<rect x='"" & _TargetOneNormalized & ""' y='2' width='2' height='80%' fill='"" & _TargetOneColor & ""'/>""
VAR _TargetTwoLine = ""<rect x='"" & _TargetTwoNormalized & ""' y='2' width='2' height='80%' fill='"" & _TargetTwoColor & ""'/>""

VAR _SvgSuffix = ""</svg>""


-- Final result
VAR _SVG = 
    _SvgPrefix
    
    & _Sort
    
    & _ActionDotOne
    & _ActionDotTwo
    
    & _BarBackground
    & _ActualBar
    & _BarBaseline
    
    & _TargetLabelOne
    & _TargetOneLine
    
    & _TargetLabelTwo
    & _TargetTwoLine
    
    & _SvgSuffix
    
RETURN
    _SVG
";


// Selected values you want to use in the plot.
var _AllMeasures = Model.AllMeasures.Where(m => m.IsHidden != true).OrderBy(m => m.Name);
var _AllColumns = Model.AllColumns.Where(c => c.IsHidden != true).OrderBy(c => c.DaxObjectFullName);

var _Actual = SelectMeasure(_AllMeasures, null,"Select the measure that you want to measure:");
var _TargetOne = SelectMeasure(_AllMeasures, null,"Select the first measure that you want to compare to:");
var _TargetTwo = SelectMeasure(_AllMeasures, null,"Select the second measure that you want to compare to:");
var _GroupBy = SelectColumn(_AllColumns, null, "Select the column for which you will group the data in\nthe table or matrix visual:");

_SvgString = _SvgString.Replace( "__ACTUAL_MEASURE", _Actual.DaxObjectFullName ).Replace( "__TARGET_ONE_MEASURE", _TargetOne.DaxObjectFullName ).Replace( "__TARGET_TWO_MEASURE", _TargetTwo.DaxObjectFullName ).Replace( "__GROUPBY_COLUMN", _GroupBy.DaxObjectFullName );


// Adding the measure.
var _SelectedTable = Selected.Table;
string _Name = "SVG Bullet Chart (Multiple Targets with Action Dots)";
string _Desc = _Name + " of " + _Actual.Name + " vs. " + _TargetOne.Name + " and " + _TargetTwo.Name + ", grouped by " + _GroupBy.Name;
var _SvgMeasure = _SelectedTable.AddMeasure( "New " + _Name, _SvgString, "SVGs\\Bullet Chart");

// Setting measure properties.
_SvgMeasure.DataCategory = "ImageUrl";
_SvgMeasure.IsHidden = true;
_SvgMeasure.Description = _Desc;

// Notification InfoBox.
Info("Added new SVG measure to the table " + _SelectedTable.Name + ".\n\nValidate the SVG definition and test the measure carefully in many different filter contexts before using it in reports.\nDon't forget to rename the new measure.");