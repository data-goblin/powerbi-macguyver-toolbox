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
VAR _LabelFont = ""Segoe UI""
VAR _LabelWeight = ""600""          -- Semibold-ish
VAR _LabelSize = ""11""          -- Semibold-ish


-- Chart Config
VAR _BarMax = 100
VAR _BarMin = 0
VAR _Scope = ALLSELECTED ( __GROUPBY_COLUMN ) -- Table comprising all values that group the actuals and targets


-- Color config.
---- Bar fill
VAR _BarFillColor = 
    -- Lighter color for category vs. total
    IF (
        HASONEVALUE ( __GROUPBY_COLUMN ),
        ""#dad9d8"",
        ""#878582""
    )

VAR _LabelColor =
    -- Lighter color for category vs. total
    IF (
        HASONEVALUE ( __GROUPBY_COLUMN ),
        ""#87858299"",
        ""#EBEBEB""
    )
    
    -- How to format actuals
    VAR _NumberFormat =
        SWITCH (
            TRUE (),
            _Actual <= 1E3,  FORMAT ( _Actual, ""#,0"" ),
            _Actual <= 1E4,  FORMAT ( _Actual, ""#,0,.00 K"" ),
            _Actual <= 1E5,  FORMAT ( _Actual, ""#,0,.0 K"" ),
            _Actual <= 1E6,  FORMAT ( _Actual, ""#,0, K"" ),
            _Actual <= 1E7,  FORMAT ( _Actual, ""#,0,,.00 M"" ),
            _Actual <= 1E8,  FORMAT ( _Actual, ""#,0,,.0 M"" ),
            _Actual <= 1E9,  FORMAT ( _Actual, ""#,0,, M"" ),
            _Actual <= 1E10, FORMAT ( _Actual, ""#,0,,,.00 bn"" ),
            _Actual <= 1E11, FORMAT ( _Actual, ""#,0,,,.0 bn"" ),
            _Actual <= 1E12, FORMAT ( _Actual, ""#,0,,, bn"" )
        )


----------------------------------------------------------------------------------------
----------------------- END CONFIG - BEYOND HERE THERE BE DRAGONS ----------------------
----------------------------------------------------------------------------------------


-- Only change the parts of this code if you want to adjust how the SVG visual works or add / remove stuff.


    -- Get axis maximum
    VAR _ByCategory =
        ADDCOLUMNS (
            _Scope,
            ""@Actual"", __ACTUAL_MEASURE
        )
        
    VAR _FilterTable = 
        SELECTCOLUMNS (
            _Scope,
            ""@CategoryPR"", __GROUPBY_COLUMN,
            ""@ActualPR"", __ACTUAL_MEASURE
        )
        
    VAR _ResultTable = 
        GENERATEALL (
            _ByCategory,
            OFFSET (
                -1,
                _FilterTable,
                ORDERBY ( [@ActualPR] )
            )
        )
        
    VAR _CumulationPR =
        FILTER (
            _ResultTable,
            [@ActualPR] >= _Actual
        )
        
    VAR _CumulativeAmountPR =
        SUMX ( _CumulationPR, [@Actual] )
    
    VAR _Total =
        SUMX ( _ByCategory, [@Actual] )
    
    VAR _CumulativePercentagePR =
        DIVIDE (
            _CumulativeAmountPR,
            _Total
        )
        
    VAR _PercentageByCategoryPR = 
        IF (
            HASONEVALUE ( __GROUPBY_COLUMN ),
            _CumulativePercentagePR
        )
    
    
    -- Normalize values (to get position along X-axis)
    VAR _AxisRange = 
        _BarMax - _BarMin
    
    VAR _ActualNormalized = 
        DIVIDE ( _Actual, _Total ) * _AxisRange

    VAR _StartPos = 
        _PercentageByCategoryPR * _AxisRange
        
    VAR _EndPos = MIN ( _StartPos + _ActualNormalized, 99 )


-- Vectors and SVG code
VAR _SvgPrefix = ""data:image/svg+xml;utf8, <svg xmlns='http://www.w3.org/2000/svg'>""

---- To sort the SVG in a table or matrix by the bar length
VAR _Sort = ""<desc>"" & FORMAT ( _Actual, ""000000000000"" ) & ""</desc>""

VAR _ActualBar  = ""<rect x='"" & MAX ( _StartPos, 0 ) & ""' y='3' width='"" & _ActualNormalized & ""' height='75%' fill='"" & _BarFillColor & ""'/>""

VAR _ConnectorStart  = ""<rect x='"" & _StartPos & ""' y='0' width='0.5' height='100%' fill='#333333'/>""
VAR _ConnectorEnd = ""<rect x='"" & _EndPos & ""' y='0' width='0.5' height='100%' fill='#333333'/>""

VAR _ActualLabelRS = ""<text x='"" & _EndPos + 4 & ""' y='16' font-family='"" & _LabelFont & ""' font-size='"" & _LabelSize & ""' font-weight='"" & _LabelWeight & ""'  text-anchor='start' fill='"" & _LabelColor & ""'>"" & _NumberFormat & ""</text>""
VAR _ActualLabelLS = ""<text x='"" & _StartPos - 4 & ""' y='16' font-family='"" & _LabelFont & ""' font-size='"" & _LabelSize & ""' font-weight='"" & _LabelWeight & ""'  text-anchor='end' fill='"" & _LabelColor & ""'>"" & _NumberFormat & ""</text>""
VAR _ActualLabel = IF ( _EndPos > _AxisRange * 0.5, _ActualLabelLS, _ActualLabelRS )


VAR _SvgSuffix = ""</svg>""


-- Final result
VAR _SVG = 
    _SvgPrefix
    
    & _Sort
    
    & _ActualBar
    & _ConnectorStart
    & _ConnectorEnd
    
    & _ActualLabel
    
    & _SvgSuffix
    
RETURN
    _SVG
";


// Selected values you want to use in the plot.
var _AllMeasures = Model.AllMeasures.Where(m => m.IsHidden != true).OrderBy(m => m.Name);
var _AllColumns = Model.AllColumns.Where(c => c.IsHidden != true).OrderBy(c => c.DaxObjectFullName);

Measure _Actual;
Column _GroupBy;
_Actual = Model.AllMeasures.FirstOrDefault(m => m.DaxObjectName == Model.GetAnnotation("_svg_actual"));
_GroupBy = Model.AllColumns.FirstOrDefault(c => c.DaxObjectFullName == Model.GetAnnotation("_svg_column"));
if(_Actual == null) _Actual = SelectMeasure(_AllMeasures, null,"Select the measure that you want to measure:");
if(_Actual == null) return;
if(_GroupBy == null) _GroupBy = SelectColumn(_AllColumns, null, "Select the column for which you will group the data in\nthe table or matrix visual:");
if(_GroupBy == null) return;
_SvgString = _SvgString.Replace( "__ACTUAL_MEASURE", _Actual.DaxObjectFullName ).Replace( "__GROUPBY_COLUMN", _GroupBy.DaxObjectFullName );


// Adding the measure.
var _SelectedTable = Selected.Table;
string _Name = "SVG Waterfall Chart";
string _Desc = _Name + " of " + _Actual.Name + ", grouped by " + _GroupBy.Name;
var _SvgMeasure = _SelectedTable.AddMeasure( "New " + _Name, _SvgString, "SVGs\\Waterfall");

// Setting measure properties.
_SvgMeasure.DataCategory = "ImageUrl";
_SvgMeasure.IsHidden = true;
_SvgMeasure.Description = _Desc;

// Notification InfoBox.
if(!Model.HasAnnotation("_svg_actual"))
Info("Added new SVG measure to the table " + _SelectedTable.Name + ".\n\nValidate the SVG specification and test the measure carefully in many different filter contexts before using it in reports.\nDon't forget to rename the new measure.");