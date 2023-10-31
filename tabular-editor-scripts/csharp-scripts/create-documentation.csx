/////////////////////////////////////////////////////////////////////////////////////////////
//
// Evaluates 9 DMVs and 1 DAX query, then exports results in a .tsv format
//
/////////////////////////////////////////////////////////////////////////////////////////////
//
// DMVs
//
/////////////////////////////////////////////////////////////////////////////////////////////

using System.IO;

// Get all measures
var measures_query =    "SELECT * FROM $SYSTEM.TMSCHEMA_MEASURES";
var measures_outfile =      @"dax_measures";

// Get all calculated columns
var calccol_query =     "SELECT * FROM $SYSTEM.TMSCHEMA_COLUMNS WHERE [Type] = 2";
var calccol_outfile =       @"dax_calculated-columns";

// Get all calculated tables
var calctable_query =   "SELECT * FROM $SYSTEM.TMSCHEMA_PARTITIONS WHERE [Type] = 2";
var calctable_outfile =     @"dax_calculated-tables";

// Get all calculation items
var calcitems_query =   "SELECT * FROM $SYSTEM.TMSCHEMA_CALCULATION_ITEMS";
var calcitems_outfile =     @"dax_calculation-items";

// Get all partitions (M expression + Calculated Tables)
var partitions_query =  "SELECT * FROM $SYSTEM.TMSCHEMA_PARTITIONS";
var partitions_outfile =    @"power-query_partitions";

// Get all M expressions
var expressions_query = "SELECT * FROM $SYSTEM.TMSCHEMA_EXPRESSIONS";
var expressions_outfile =   @"power-query_expressions";

// Get all tables
var tables_query =      "SELECT * FROM $SYSTEM.TMSCHEMA_TABLES";
var tables_outfile =        @"model_tables";

// Get all columns
var columns_query =     "SELECT * FROM $SYSTEM.TMSCHEMA_COLUMNS";
var columns_outfile =       @"model_columns";

// Get all data sources
var datasources_query = "SELECT * FROM $SYSTEM.DISCOVER_POWERBI_DATASOURCES";
var datasources_outfile =   @"model_sources";

/////////////////////////////////////////////////////////////////////////////////////////////
//
// Export a custom DAX query - this one is the 'Date'[Date] values with an index
//
/////////////////////////////////////////////////////////////////////////////////////////////

// DAX Query
var your_dax_query =    "EVALUATE ADDCOLUMNS( DISTINCT ( 'Date'[Date] ), \"Index\", INT([Date]) - MIN('Date'[Date]) )";
var your_dax_outfile =      @"dax-query-results_date";

/////////////////////////////////////////////////////////////////////////////////////////////
//
// Append each query to a list
//
/////////////////////////////////////////////////////////////////////////////////////////////

// To add your own DMV or DAX query you need to add the new variable to the end of each list.
List<string> queryList = new List<string>
            { 
              measures_query, 
              calccol_query, 
              calctable_query, 
              calcitems_query, 
              partitions_query,
              expressions_query, 
              tables_query,
              columns_query, 
              datasources_query,
              your_dax_query
            };

List<string> fileList = new List<string>
            {
              measures_outfile, 
              calccol_outfile, 
              calctable_outfile, 
              calcitems_outfile, 
              partitions_outfile,
              expressions_outfile, 
              tables_outfile,
              columns_outfile, 
              datasources_outfile,
              your_dax_outfile
            };

/////////////////////////////////////////////////////////////////////////////////////////////
//
// Output path
//
/////////////////////////////////////////////////////////////////////////////////////////////

// Get model name
string modelname = Model.Database.Name;

// Set the path for the output 

//           -- <CHANGE TO YOUR OWN PATH> --
var outpath = @"C:\Users\Klonk\Desktop\DMV\" + modelname + "_";

// Column seaprator & file extension
var columnSeparator = "\t";
var fileextension = ".tsv";

/////////////////////////////////////////////////////////////////////////////////////////////
//
// Evaluation & export
//
/////////////////////////////////////////////////////////////////////////////////////////////

// For each item in the query list
foreach ( var i in queryList ) 
    {
        // Get the index of the item in the list
        int index = queryList.FindIndex(a => a == i);

        // Execute the query
        using(var daxReader = ExecuteReader(queryList[index]))
            {
                // Start writing the file at the path with the name and extension
                using(var fileWriter = new StreamWriter(outpath + fileList[index] + fileextension))
                {
                    // Write column headers
                    // *NOTE* If exporting a DAX query and not a DMV result, you need to remove the -1 from daxReader.FieldCount
                    fileWriter.WriteLine(string.Join(columnSeparator, Enumerable.Range(0, daxReader.FieldCount - 1).Select(f => daxReader.GetName(f))));
                
                    // Write rows
                    while(daxReader.Read())
                    {
                        var rowValues = new object[daxReader.FieldCount];
                        daxReader.GetValues(rowValues);
    
                        // Remove newline special characters, which cause issues with the output
                        var row = string.Join(columnSeparator, rowValues.Select(v => v == null ? "" : v.ToString().Replace("\n", "")));
                        fileWriter.WriteLine(row);
                    }
                }
                // Close the reader
                daxReader.Close();
            }
    }

// Notification
Info( "Successfully exported " + fileList.Count + " " + fileextension + " files." );