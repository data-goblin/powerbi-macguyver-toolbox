// Get table name
string _TableName = 
    Selected.Table.DaxObjectFullName;

// Count table rows
string _dax = 
    "{ FORMAT( COUNTROWS (" + _TableName + "), \"#,##0\" ) }";

// Evaluate DAX
string _TableRows = 
    Convert.ToString(EvaluateDax( _dax ));

// Return output in pop-up
Info ( "Number of rows in " + _TableName + ": " + _TableRows);