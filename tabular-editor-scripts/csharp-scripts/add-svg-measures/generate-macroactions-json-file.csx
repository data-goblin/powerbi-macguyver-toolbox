using System.IO;

// Place here your path to the svg measures repo
var path = @"C:\repos\macguyver-toolbox\tabular-editor-scripts\csharp-scripts\add-svg-measures";
var _MacroPath = @"C:\AppData\Local\TabularEditor3\MacroActions.json"
var d = new DirectoryInfo(path);
var cultInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;
var orgJson = File.ReadAllText(_MacroPath);
macroCollection = JsonConvert.DeserializeObject<MacroCollection>(orgJson);
foreach (var f in d.EnumerateFiles("*.csx", SearchOption.AllDirectories))
{
    var macroName = f.FullName.Replace(path, "").Substring(1);
    if (!macroName.Contains(@"\")) continue;
    if (macroName.Count(c => c == '\\') == 1)
    {
        var category = cultInfo.ToTitleCase(macroName.Split(@"\")[0].Replace("-", " "));
        var itemName = cultInfo.ToTitleCase(macroName.Split(@"\")[1].Replace("add-svg-", "").Replace("-", " ").Replace(".csx", "").Trim()).Replace(category, "").Trim();
        if (!string.IsNullOrEmpty(itemName)) itemName = "\\" + itemName;
        macroName = "Add SVG measure\\" + category + itemName;
    }
    else
    {
        var category = cultInfo.ToTitleCase(macroName.Split(@"\")[0].Replace("-", " "));
        var itemName = cultInfo.ToTitleCase(macroName.Split(@"\")[1].Replace("add-svg-", "").Replace("-", " ").Replace(".csx", "").Trim()).Replace(category, "").Trim();
        var itemName2 = cultInfo.ToTitleCase(macroName.Split(@"\")[2].Replace("add-svg-", "").Replace("-", " ").Replace(".csx", "").Trim()).Replace(category, "").Replace(itemName, "").Trim();
        if (!string.IsNullOrEmpty(itemName)) itemName = "\\" + itemName;
        if (!string.IsNullOrEmpty(itemName2)) itemName2 = "\\" + itemName2; else itemName2 = "\\Standard";
        macroName = "Add SVG measure\\" + category + itemName + itemName2;
    }
    macros.Add(new Macro(macroName, File.ReadAllText(f.FullName)));
}
var macroCollection = new MacroCollection(macros);
var json = JsonConvert.SerializeObject(macroCollection, new JsonSerializerSettings { Formatting = Formatting.Indented });
SaveFile(_MacroPath, json);
record class MacroCollection(List<Macro> Actions);
record class Macro(int Id, string Name, string execute, string Enabled = "Selected.Count == 1", string Tooltip = "", string ValidContexts = "Table");