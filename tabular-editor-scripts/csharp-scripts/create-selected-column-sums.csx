
// Config
string _agg = "Sum";
string _tablename = "";

// Creates a SUM measure for every currently selected column and hide the column.
foreach(var c in Selected.Columns)
{

    // Add the measures to the last table in the array Model.Tables
    var m = Model.Tables.Last().AddMeasure(

        // Measure name
        "Total " + c.Name,

        // Measure DAX
        FormatDax(
        _agg.ToUpper() 
            //+ "X "                                   // Currency Conversion
            + "("                                      
            //+ c.Table.DaxObjectFullName + ", "       // Currency Conversion
            + c.DaxObjectFullName                      
            
            //+ "/ RELATED ( 'Budget Rate'[Rate] )"    // Currency Conversion
            + " ) "                                    
            //+ " * MAX ( 'Exchange Rate'[Rate] )"     // Currency Conversion
        , shortFormat:true),

        // Display folder
        //"\\Measures\\Value"                          // Currency Conversion
        "\\Measures\\Quantity"                       // Quantity / No currency conversion
    );

    _tablename = m.Table.Name;

    // Set the format string on the new measure:
    m.FormatString = "#,##0";

    // Provide some documentation:
    m.Description = "This measure is the" + _agg + "of column " + c.DaxObjectFullName;

    // Add to perspective 'All'
    // m.InPerspective["All"] = true;

    // Hide the base column:
    c.IsHidden = true;

    // Hide the base column:
    c.SummarizeBy = AggregateFunction.None;


    // Hide the base column:
    c.SummarizeBy = AggregateFunction.None;

    // Add columns to display folder
    c.DisplayFolder = "\\Columns\\0. Facts";

}

Info ( "Created " + Convert.ToString(Selected.Columns.Count()) + " measures.\nThey were added to the '" + _tablename + "' table." );