using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// URL of the powerqueryformatter.com API
string powerqueryformatterAPI = "https://m-formatter.azurewebsites.net/api/v2";

// HttpClient method to initiate the API call POST method for the URL
HttpClient client = new HttpClient();
HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, powerqueryformatterAPI);

// Get the M Expression of the selected partition or shared expression
string _type = Selected.Object.ObjectTypeName;
string _powerquery = "";
if ( _type == "Expression" )
    {
        _powerquery = Selected.Expression;
    }
else if ( _type == "Partition (M - Import)" )
    {
        _powerquery = Selected.Partition.Expression;
    }
else
    {
    Error ("Invalid object selected. Terminating.");
    }

// Serialize the request body as a JSON object
var requestBody = JsonConvert.SerializeObject(
    new { 
        code = _powerquery,                     // Mandatory config
        resultType = "text",                    // Mandatory config
        lineWidth = 40                          // Optional config
        // alignLineCommentsToPosition = true,  // Optional config
        // includeComments = true               // Optional config
    });

// Set the "Content-Type" header of the request to "application/json" and the encoding to UTF-8
var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

// Retrieve the response
var response = client.PostAsync(powerqueryformatterAPI, content).Result;

// If the response is successful
if (response.IsSuccessStatusCode)
{
    // Get the result of the response
    var result = response.Content.ReadAsStringAsync().Result;

    // Parse the response JSON object from the string
    JObject data = JObject.Parse(result.ToString());

    // Get the formatted Power Query response
    string formattedPowerQuery = (string)data["result"];

    ////////////////////////////////////////////////////////////////////////// Can remove everything in this section
    // OPTIONAL MANUAL FORMATTING                                           // Additional formatting on top of API config
    // Manually add a new line and comment to each step                     //
    var replace = new Dictionary<string, string>                            //
    {                                                                       //
        { "\n//", "\n\n//" },                                               // New line at comment
        { "\n  #", "\n\n  // Step\n  #" },                                  // New line & comment at new standard step
        { "\n  Source", "\n\n  // Data Source\n  Source" },                 // New line & comment at Source step
        { "\n  Dataflow", "\n\n  // Dataflow Connection Info\n  Dataflow" },// New line & comment at Dataflow step
        {"\n  Data =", "\n\n  // Step\n  Data ="},                          // New line & comment at Data step
        {"\n  Navigation =", "\n\n  // Step\n  Navigation ="},              // New line & comment at Navigation step
        {"in\n\n  // Step\n  #", "in\n  #"},                                // 
        {"\nin", "\n\n// Result\nin"}                                       // Format final step as result
    };                                                                      //
                                                                            //
    // Replace the first string in the dictionary with the second           //
    var manuallyformattedPowerQuery = replace.Aggregate(                    //
        formattedPowerQuery,                                                //
        (before, after) => before.Replace(after.Key, after.Value));         //
                                                                            //
    // Replace the auto-formatted code with the manually formatted version  //
    formattedPowerQuery = manuallyformattedPowerQuery;                      //
    //////////////////////////////////////////////////////////////////////////

    // Replace the unformatted M expression with the formatted expression
    if ( _type == "Expression" )
        {
            Selected.Expression = formattedPowerQuery;
        }
    else if ( _type == "Partition (M - Import)" )
        {
            Selected.Partition.Expression = formattedPowerQuery;
        }
    else
        {
        Error( "Invalid object selected. Format either a Partition (M - Import) or Shared Expression" );
        }

    // Pop-up to inform of completion
    Info("Formatted " + Selected.Object.Name);
}

// Otherwise return an error message
else
{
Info(
    "API call unsuccessful." +
    "\nCheck that you are selecting a partition with a valid M Expression."
    );
}