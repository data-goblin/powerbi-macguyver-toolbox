
// Add table to view the last refresh
var _Table = Model.AddTable( "Last Refresh", false );

string _MPartition = @"let
    Source = DateTimeZone.FixedLocalNow()
in
    Source";

_Table.AddMPartition( "Last Refresh", _MPartition );
var _c = _Table.AddDataColumn( "Last Refresh", "Last Refresh", "Columns", DataType.DateTime );
_c.IsHidden = true;
_c.IsAvailableInMDX = false;
_Table.AddMeasure( "Last Refresh Date", @"FORMAT ( MAX ( 'Last Refresh'[Last Refresh] ), ""MMM DD, HH:MM"" )" );


// Add measure table
_MPartition = @"
    Table.FromRows( 
        { 
            {""Measure Table""} 
        }, 
        type table
        [#""Measure Table"" = text]
    )";

_Table = Model.AddTable( "_Measures", false );
_Table.AddMPartition( "Measure Table", _MPartition ) ; 
_c = _Table.AddDataColumn( "Measure Table", "Measure Table", "Columns", DataType.String );
_c.IsHidden = true;
_c.IsAvailableInMDX = false;

Info ( "Created a measure table '_Measures'." + "\nCreated a data table 'Last Refresh'. The measure [Last Refresh Date] will show the DateTime this table was last processed." );