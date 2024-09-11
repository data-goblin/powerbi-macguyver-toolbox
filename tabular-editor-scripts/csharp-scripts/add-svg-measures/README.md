These C# scripts add SVG visuals to your semantic model with Tabular Editor.
- The objective is to make it easier and more accessible for people to use SVG microvisualizations.
- There are ~30 templates provided at first, but more will follow.
- If you want your SVG template converted to a C# script, please reach out to me (Kurt Buhler) to convert it.


You can use these scripts and templates for free for non-commercial use. I'd appreciate that you please cite [data-goblins.com](https://www.data-goblins.com) if you do. For any future templates originally authored by someone else, please cite the original author.


## ⚠️ Notice
These templates are provided as-is without warranties or guarantees. They are not maintained nor are they all necessarily suitable for use in production solutions.

Feel free to use them, but do so at your own risk.


## 💡 To use these templates
To use these templates, you must first download and install [Tabular Editor 2 (Open Source)](https://docs.tabulareditor.com/te2/Getting-Started.html) or [Tabular Editor 3](https://tabulareditor.com/downloads).

Then, proceed as follows:
1. Find the chart you want to add and copy the script to your clipboard.
2. In Tabular Editor, select a table in the TOM Explorer and paste the script in the C# Script or Advanced Scripting window.
3. Run the script and select the appropriate fields. Only unhidden fields can be selected.
4. After creating the measure, validate the DAX and SVG specification for errors. If necessary, adjust it yourself by i.e. changing colors or adding additional columns to group by.
5. In the Power BI report connected to the model, add the measure to a new table visual together with the column you selected to group the data.
6. In the Formatting Pane for the table, set the Image size property to 'Height' of 25px and 'Width' of 100px. 
7. Test the measure to ensure good performance and expected results. If necessary, modify the DAX or SVG specification, but test thoroughly before using.


## Overview of Provided Templates

(WIP)