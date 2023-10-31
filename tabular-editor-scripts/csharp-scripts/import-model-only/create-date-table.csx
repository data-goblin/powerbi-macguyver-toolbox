// To use this C# Script:
//
// 1. Run the script
// 2. Select the column that has the earliest date
// 3. Select the column that has the latest date

// List of all DateTime columns in the model
var _dateColumns = Model.AllColumns.Where(c => c.DataType == DataType.DateTime ).ToList();

// Select the column with the earliest date in the model
try
{
    string _EarliestDate = 
        SelectColumn(
            _dateColumns, 
            null, 
            "Select the Column with the Earliest Date:"
        ).DaxObjectFullName;
    
    try
    {
        // Select the column with the latest date in the model
        string _LatestDate = 
            SelectColumn(
                _dateColumns, 
                null, 
                "Select the Column with the Latest Date:"
            ).DaxObjectFullName;
        
        
        // Create measure for reference date
        var _RefDateMeasure = _dateColumns[0].Table.AddMeasure(
            "RefDate",
            "CALCULATE ( MAX ( " + _LatestDate + " ), REMOVEFILTERS ( ) )"
        );
        
        
        // Formatted date table DAX
        // Based on date table from https://www.sqlbi.com/topics/date-table/
        // To adjust, copy everything after the @" into a DAX query window & replace
        
        var _DateDaxExpression = @"-- Reference date for the latest date in the report
        -- Until when the business wants to see data in reports
        VAR _Refdate_Measure = [RefDate]
        VAR _Today = TODAY ( )
        
        -- Replace with ""Today"" if [RefDate] evaluates blank
        VAR _Refdate = IF ( ISBLANK ( _Refdate_Measure ), _Today, _Refdate_Measure )
            VAR _RefYear        = YEAR ( _Refdate )
            VAR _RefQuarter     = _RefYear * 100 + QUARTER(_Refdate)
            VAR _RefMonth       = _RefYear * 100 + MONTH(_Refdate)
            VAR _RefWeek_EU     = _RefYear * 100 + WEEKNUM(_Refdate, 2)
        
        -- Earliest date in the model scope
        VAR _EarliestDate       = DATE ( YEAR ( MIN ( " + _EarliestDate + @" ) ) - 2, 1, 1 )
        VAR _EarliestDate_Safe  = MIN ( _EarliestDate, DATE ( YEAR ( _Today ) + 1, 1, 1 ) )
        
        -- Latest date in the model scope
        VAR _LatestDate_Safe    = DATE ( YEAR ( _Refdate ) + 2, 12, 1 )
        
        ------------------------------------------
        -- Base calendar table
        VAR _Base_Calendar      = CALENDAR ( _EarliestDate_Safe, _LatestDate_Safe )
        ------------------------------------------
        
        
        
        ------------------------------------------
        VAR _IntermediateResult = 
            ADDCOLUMNS ( _Base_Calendar,
        
                    ------------------------------------------
                ""Calendar Year Number (ie 2021)"",           --|
                    YEAR ([Date]),                          --|-- Year
                                                            --|
                ""Calendar Year (ie 2021)"",                  --|
                    FORMAT ([Date], ""YYYY""),                --|
                    ------------------------------------------
        
                    ------------------------------------------
                ""Calendar Quarter Year (ie Q1 2021)"",       --|
                    ""Q"" &                                   --|-- Quarter
                    CONVERT(QUARTER([Date]), STRING) &      --|
                    "" "" &                                   --|
                    CONVERT(YEAR([Date]), STRING),          --|
                                                            --|
                ""Calendar Year Quarter (ie 202101)"",        --|
                    YEAR([Date]) * 100 + QUARTER([Date]),   --|
                    ------------------------------------------
        
                    ------------------------------------------
                ""Calendar Month Year (ie Jan 21)"",          --|
                    FORMAT ( [Date], ""MMM YY"" ),            --|-- Month
                                                            --|
                ""Calendar Year Month (ie 202101)"",          --|
                    YEAR([Date]) * 100 + MONTH([Date]),     --|
                                                            --|
                ""Calendar Month (ie Jan)"",                  --|
                    FORMAT ( [Date], ""MMM"" ),               --|
                                                            --|
                ""Calendar Month # (ie 1)"",                  --|
                    MONTH ( [Date] ),                       --|
                    ------------------------------------------
                    
                    ------------------------------------------
                ""Calendar Week EU (ie WK25)"",               --|
                    ""WK"" & WEEKNUM( [Date], 2 ),            --|-- Week
                                                            --|
                ""Calendar Week Number EU (ie 25)"",          --|
                    WEEKNUM( [Date], 2 ),                   --|
                                                            --|
                ""Calendar Year Week Number EU (ie 202125)"", --|
                    YEAR ( [Date] ) * 100                   --|
                    +                                       --|
                    WEEKNUM( [Date], 2 ),                   --|
                                                            --|
                ""Calendar Week US (ie WK25)"",               --|
                    ""WK"" & WEEKNUM( [Date], 1 ),            --|
                                                            --|
                ""Calendar Week Number US (ie 25)"",          --|
                    WEEKNUM( [Date], 1 ),                   --|
                                                            --|
                ""Calendar Year Week Number US (ie 202125)"", --|
                    YEAR ( [Date] ) * 100                   --|
                    +                                       --|
                    WEEKNUM( [Date], 1 ),                   --|
                                                            --|
                ""Calendar Week ISO (ie WK25)"",              --|
                    ""WK"" & WEEKNUM( [Date], 21 ),           --|
                                                            --|
                ""Calendar Week Number ISO (ie 25)"",         --|
                    WEEKNUM( [Date], 21 ),                  --|
                                                            --|
                ""Calendar Year Week Number ISO (ie 202125)"",--|
                    YEAR ( [Date] ) * 100                   --|
                    +                                       --|
                    WEEKNUM( [Date], 21 ),                  --|
                    ------------------------------------------
        
                    ------------------------------------------
                ""Weekday Short (i.e. Mon)"",                 --|
                    FORMAT ( [Date], ""DDD"" ),               --|-- Weekday
                                                            --|
                ""Weekday Name (i.e. Monday)"",               --|
                    FORMAT ( [Date], ""DDDD"" ),              --|
                                                            --|
                ""Weekday Number EU (i.e. 1)"",               --|
                    WEEKDAY ( [Date], 2 ),                  --|
                    ------------------------------------------
                    
                    ------------------------------------------
                ""Calendar Month Day (i.e. Jan 05)"",         --|
                    FORMAT ( [Date], ""MMM DD"" ),            --|-- Day
                                                            --|
                ""Calendar Month Day (i.e. 0105)"",           --|
                    MONTH([Date]) * 100                     --|
                    +                                       --|
                    DAY([Date]),                            --|
                                                            --|
                ""YYYYMMDD"",                                 --|
                    YEAR ( [Date] ) * 10000                 --|
                    +                                       --|
                    MONTH ( [Date] ) * 100                  --|
                    +                                       --|
                    DAY ( [Date] ),                         --|
                    ------------------------------------------
        
        
                    ------------------------------------------
                ""IsDateInScope"",                            --|
                    [Date] <= _Refdate                      --|-- Boolean
                    &&                                      --|
                    YEAR([Date]) > YEAR(_EarliestDate),     --|
                                                            --|
                ""IsBeforeThisMonth"",                        --|
                    [Date] <= EOMONTH ( _Refdate, -1 ),     --|
                                                            --|
                ""IsLastMonth"",                              --|
                    [Date] <= EOMONTH ( _Refdate, 0 )       --|
                    &&                                      --|
                    [Date] > EOMONTH ( _Refdate, -1 ),      --|
                                                            --|
                ""IsYTD"",                                    --|
                    MONTH([Date])                           --|
                    <=                                      --|
                    MONTH(EOMONTH ( _Refdate, 0 )),         --|
                                                            --|
                ""IsActualToday"",                            --|
                    [Date] = _Today,                        --|
                                                            --|
                ""IsRefDate"",                                --|
                    [Date] = _Refdate,                      --|
                                                            --|
                ""IsHoliday"",                                --|
                    MONTH([Date]) * 100                     --|
                    +                                       --|
                    DAY([Date])                             --|
                        IN {0101, 0501, 1111, 1225},        --|
                                                            --|
                ""IsWeekday"",                                --|
                    WEEKDAY([Date], 2)                      --|
                        IN {1, 2, 3, 4, 5})                 --|
                    ------------------------------------------
        
        VAR _Result = 
            
                    --------------------------------------------
            ADDCOLUMNS (                                      --|
                _IntermediateResult,                          --|-- Boolean #2
                ""IsThisYear"",                                 --|
                    [Calendar Year Number (ie 2021)]          --|
                        = _RefYear,                           --|
                                                            --|
                ""IsThisMonth"",                                --|
                    [Calendar Year Month (ie 202101)]         --|
                        = _RefMonth,                          --|
                                                            --|
                ""IsThisQuarter"",                              --|
                    [Calendar Year Quarter (ie 202101)]       --|
                        = _RefQuarter,                        --|
                                                            --|
                ""IsThisWeek"",                                 --|
                    [Calendar Year Week Number EU (ie 202125)]--|
                        = _RefWeek_EU                         --|
            )                                                 --|
                    --------------------------------------------
                    
        RETURN 
            _Result";
        
        // Create date table
        var _date = Model.AddCalculatedTable(
            "Date",
            _DateDaxExpression        );
        
        //-------------------------------------------------------------------------------------------//
        
        // Sort by...
        
        // Sort Weekdays
        (_date.Columns["Weekday Name (i.e. Monday)"] as CalculatedTableColumn).SortByColumn = (_date.Columns["Weekday Number EU (i.e. 1)"] as CalculatedTableColumn);
        (_date.Columns["Weekday Short (i.e. Mon)"] as CalculatedTableColumn).SortByColumn = (_date.Columns["Weekday Number EU (i.e. 1)"] as CalculatedTableColumn);
        
        // Sort Weeks
        (_date.Columns["Calendar Week EU (ie WK25)"] as CalculatedTableColumn).SortByColumn = (_date.Columns["Calendar Week Number EU (ie 25)"] as CalculatedTableColumn);
        (_date.Columns["Calendar Week ISO (ie WK25)"] as CalculatedTableColumn).SortByColumn = (_date.Columns["Calendar Week Number ISO (ie 25)"] as CalculatedTableColumn);
        (_date.Columns["Calendar Week US (ie WK25)"] as CalculatedTableColumn).SortByColumn = (_date.Columns["Calendar Week Number US (ie 25)"] as CalculatedTableColumn);
        
        // Sort Months
        (_date.Columns["Calendar Month (ie Jan)"] as CalculatedTableColumn).SortByColumn = (_date.Columns["Calendar Month # (ie 1)"] as CalculatedTableColumn);
        (_date.Columns["Calendar Month Day (i.e. Jan 05)"] as CalculatedTableColumn).SortByColumn = (_date.Columns["Calendar Month Day (i.e. 0105)"] as CalculatedTableColumn);
        (_date.Columns["Calendar Month Year (ie Jan 21)"] as CalculatedTableColumn).SortByColumn = (_date.Columns["Calendar Year Month (ie 202101)"] as CalculatedTableColumn);
        
        // Sort Quarters
        (_date.Columns["Calendar Quarter Year (ie Q1 2021)"] as CalculatedTableColumn).SortByColumn = (_date.Columns["Calendar Year Quarter (ie 202101)"] as CalculatedTableColumn);
        
        // Sort Years
        (_date.Columns["Calendar Year (ie 2021)"] as CalculatedTableColumn).SortByColumn = (_date.Columns["Calendar Year Number (ie 2021)"] as CalculatedTableColumn);
        
        
        //-------------------------------------------------------------------------------------------//
        
        
        // For all the columns in the date table:
        foreach (var c in _date.Columns )
        {
        c.DisplayFolder = "7. Boolean Fields";
        c.IsHidden = true;
        
        // Organize the date table into folders
            if ( ( c.DataType == DataType.DateTime & c.Name.Contains("Date") ) )
                {
                c.DisplayFolder = "6. Calendar Date";
                c.IsHidden = false;
                c.IsKey = true;
                }
        
            if ( c.Name == "YYMMDDDD" )
                {
                c.DisplayFolder = "6. Calendar Date";
                c.IsHidden = true;
                }
        
            if ( c.Name.Contains("Year") & c.DataType != DataType.Boolean )
                {
                c.DisplayFolder = "1. Year";
                c.IsHidden = false;
                }
        
            if ( c.Name.Contains("Week") & c.DataType != DataType.Boolean )
                {
                c.DisplayFolder = "4. Week";
                c.IsHidden = true;
                }
        
            if ( c.Name.Contains("day") & c.DataType != DataType.Boolean )
                {
                c.DisplayFolder = "5. Weekday / Workday\\Weekday";
                c.IsHidden = false;
                }
        
            if ( c.Name.Contains("Month") & c.DataType != DataType.Boolean )
                {
                c.DisplayFolder = "3. Month";
                c.IsHidden = false;
                }
        
            if ( c.Name.Contains("Quarter") & c.DataType != DataType.Boolean )
                {
                c.DisplayFolder = "2. Quarter";
                c.IsHidden = false;
                }
        
        }
        
        // Mark as date table
        _date.DataCategory = "Time";
        
        
        //-------------------------------------------------------------------------------------------//
        
        
        // Create Workdays MTD, QTD, YTD logic 
        //      (separate into measures & calc. column to be easier to maintain)
        //
        // Add calculated columns for Workdays MTD, QTD, YTD
        
        string _WorkdaysDax = @"VAR _Holidays =
            CALCULATETABLE (
                DISTINCT ('Date'[Date]),
                'Date'[IsHoliday] <> TRUE
            )
        VAR _WeekdayName = CALCULATE ( SELECTEDVALUE ( 'Date'[Weekday Short (i.e. Mon)] ) )
        VAR _WeekendDays = SWITCH (
                _WeekdayName,
                ""Sat"", 2,
                ""Sun"", 3,
                0
            )
        VAR _WorkdaysMTD =
            CALCULATE (
                NETWORKDAYS (
                    CALCULATE (
                        MIN ('Date'[Date]),
                        ALLEXCEPT ('Date', 'Date'[Calendar Month Year (ie Jan 21)])
                    ),
                    CALCULATE (MAX ('Date'[Date]) - _WeekendDays),
                    1,
                    _Holidays
                )
            )
                + 1
        RETURN
            IF (_WorkdaysMTD < 1, 1, _WorkdaysMTD)";
        
        _date.AddCalculatedColumn(
            "Workdays MTD",
            _WorkdaysDax,
            "5. Weekday / Workday\\Workdays"
        );
        
        _date.AddCalculatedColumn(
            "Workdays QTD",
            _WorkdaysDax.Replace("'Date'[Calendar Month Year (ie Jan 21)]", "'Date'[Calendar Quarter Year (ie Q1 2021)]"),
            "5. Weekday / Workday\\Workdays"
        );
        
        _date.AddCalculatedColumn(
            "Workdays YTD",
            _WorkdaysDax.Replace("'Date'[Calendar Month Year (ie Jan 21)]", "'Date'[Calendar Year (ie 2021)]"),
            "5. Weekday / Workday\\Workdays"
        );
        
        
        //-------------------------------------------------------------------------------------------//
        
        
        // Create measures for showing how many workdays passed
        _WorkdaysDax = @"CALCULATE(
            MAX( 'Date'[Workdays MTD] ),
            'Date'[IsDateInScope] = TRUE
        )";
        
        _date.AddMeasure(
            "# Workdays MTD",
            _WorkdaysDax,
            "5. Weekday / Workday\\Measures\\# Workdays"
        );
        
        _date.AddMeasure(
            "# Workdays QTD",
            _WorkdaysDax.Replace("MTD", "QTD"),
            "5. Weekday / Workday\\Measures\\# Workdays"
        );
        
        _date.AddMeasure(
            "# Workdays YTD",
            _WorkdaysDax.Replace("MTD", "YTD"),
            "5. Weekday / Workday\\Measures\\# Workdays"
        );
        
        // Create measures showing how many workdays are in the selected period
        
        _WorkdaysDax = @"IF (
            HASONEVALUE ('Date'[Calendar Month Year (ie Jan 21)]),
            CALCULATE (
                MAX ('Date'[Workdays MTD]),
                VALUES ('Date'[Calendar Month Year (ie Jan 21)])
            )
        )";
        
        _date.AddMeasure(
            "# Workdays in Selected Month",
            _WorkdaysDax,
            "5. Weekday / Workday\\Measures\\# Workdays"
        );
        
        _date.AddMeasure(
            "# Workdays in Selected Quarter",
            _WorkdaysDax.Replace("MTD", "QTD").Replace("'Date'[Calendar Month Year (ie Jan 21)]", "'Date'[Calendar Quarter Year (ie Q1 2021)]"),
            "5. Weekday / Workday\\Measures\\# Workdays"
        );
        
        _date.AddMeasure(
            "# Workdays in Selected Year",
            _WorkdaysDax.Replace("MTD", "YTD").Replace("'Date'[Calendar Month Year (ie Jan 21)]", "'Date'[Calendar Year (ie 2021)]"),
            "5. Weekday / Workday\\Measures\\# Workdays"
        );
        
        
        // Create measures showing how many workdays passed as a %
        
        _WorkdaysDax = @"IF (
            HASONEVALUE ('Date'[Calendar Month Year (ie Jan 21)]),
            MROUND (
                DIVIDE ([# Workdays MTD], [# Workdays in Selected Month]),
                0.01
            )
        )";
        
        _date.AddMeasure(
            "% Workdays MTD",
            _WorkdaysDax,
            "5. Weekday / Workday\\Measures\\# Workdays"
        );
        
        _date.AddMeasure(
            "% Workdays QTD",
            _WorkdaysDax.Replace("MTD", "QTD").Replace("'Date'[Calendar Month Year (ie Jan 21)]", "'Date'[Calendar Quarter Year (ie Q1 2021)]").Replace("Month", "Quarter"),
            "5. Weekday / Workday\\Measures\\# Workdays"
        );
        
        _date.AddMeasure(
            "% Workdays YTD",
            _WorkdaysDax.Replace("MTD", "YTD").Replace("'Date'[Calendar Month Year (ie Jan 21)]", "'Date'[Calendar Year (ie 2021)]").Replace("Month", "Year"),
            "5. Weekday / Workday\\Measures\\# Workdays"
        );
        
        
        //-------------------------------------------------------------------------------------------//
        
        
        // Move the reference measure to the newly created 'Date' table.
        _RefDateMeasure.Delete();
        _RefDateMeasure = Model.Tables["Date"].AddMeasure(
            "RefDate",
            "CALCULATE ( MAX ( " + _LatestDate + " ), REMOVEFILTERS ( ) )",
            "0. Measures"
        );
        
        _RefDateMeasure.IsHidden = true;
        
        Info ( "Created a new, organized 'Date' table based on the template in the C# Script.\nThe Earliest Date is taken from " + _EarliestDate + "\nThe Latest Date is taken from " + _LatestDate );
    
        }
        catch
        {
            Error( "Latest column not selected! Ending script without making changes." );
        }
}
catch
{
    Error( "Earliest column not selected! Ending script without making changes." );
}
