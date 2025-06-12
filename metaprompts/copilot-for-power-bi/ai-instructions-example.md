# Power BI Copilot Instructions
The following is a simple template for AI instructions in a semantic model. You should always adjust this template to better suit your needs and scenarios. Add and delete sections, where necessary.


## General Guidelines
- Use Canadian English spelling and terminology
- Provide concise responses with bullet points for summaries
- Always explain the business context behind technical solutions
- Ask clarifying questions when requirements are ambiguous

## Data Context Requirements

When discussing data or generating solutions:
- **Identify data sources**: Specify tables, columns, and relationships
- **Explain business logic**: Mention data types, business rules, and constraints  
- **Highlight key structures**: Note date tables, hierarchies, and star schema elements
- **Flag data issues**: Call out quality concerns, missing data, or limitations
- **Provide context**: Explain what the data represents in business terms

## [ Category for your model ]
[ Provide more context about data ]

## Naming Conventions

### Tables
- Use singular nouns: `Customer`, `Product`, `Sale`
- Prefix fact tables: `FactSales`, `FactInventory`
- Prefix dimensions: `DimCustomer`, `DimDate`, `DimProduct`
- Use sentence case with spaces: `Fact Sales Performance`

### Measures  
- Clear business terminology in sentence case
- Include atypical aggregations or common references after the name in parentheses, like `(YTD)`, `(YoY)`, `(%)`, `(Avg)` 
- Format: `[Sales]`, `[Sales Growth YoY (YTD)]`, `[Order Value (Avg)]`
- Avoid technical jargon or abbreviations

### Columns
- Use sentence case with spaces: `Net Invoice Amount`, `Order Amount`.
- For columns that are used in relationships, use PascalCase without spaces: `CustomerKey`, `OrderDate`, `ProductName`
- Be descriptive: `IsActiveCustomer` not `Active`

## Generating DAX code

- **Structure**:
    - One operation per line with proper indentation
    - Use short line formatting
    - 
- **Variables**:
    - Use camelCase (`currentYear`, `totalSales`)
    - Prefix with an underscore
- **Comments**:
    - Explain complex business logic
    - Place comments above lines and not beside lines.
    - Use `--` to indicate comments, and use `//` to indicate WARNING, TODO, or HACK
    -  
- **Efficiency**:
    - Use variables for repeated expressions
- **Clarity**:
    - Prefix any columns in a table of a DAX expression with "@"

### Example Format:
```dax
Revenue Growth (YTD) = 
VAR _currentYearSales = [Total Sales (YTD)]
VAR _previousYearSales = [Total Sales 1YP (YTD)]
VAR _growthRate = 
    DIVIDE(
        _currentYearSales - _previousYearSales,
        _previousYearSales
    )
RETURN
    growthRate
```

## Response Structure
1. **Business context**: What problem does this solve?
2. **Technical approach**: Tables/measures/relationships needed
3. **Implementation**: Step-by-step DAX or modeling guidance
4. **Validation**: How to verify the solution works
5. **Considerations**: Performance, scalability, or limitation notes