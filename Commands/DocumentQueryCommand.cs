//using Cocona;
//using System.Threading.Tasks;
//using HyRest.DocumentManagement;
//using console = Spectre.Console.AnsiConsole;
//using Color = Spectre.Console.Color;

//namespace HyRest.OnCmd;

//public class DocumentQueryCommand : BaseCommand
//{

//    [Command("assist", Description = "Walk through Building a Query step by step")]
//    public static async Task Assist()
//    {
        
//        var type = console.Prompt(
//            new SelectionPrompt<QueryType>()
//            .HighlightStyle(new Style(Color.FromHex("#12d399")))
//            .Title("[#12d399]Select the type of Query to build:[/]")
//            .AddChoices(QueryType.DocumentType, QueryType.DocumentTypeGroup, QueryType.CustomQuery));
        
//        var itemPrompt = await ExecuteAsync("Getting eligable items", () =>
//        {
//            return Task.Run(() => GetItems(type));            
//        });
//        var item = console.Prompt(itemPrompt);
//        var keywordPrompt = await ExecuteAsync("Getting eligable keywords...", () =>
//        {
//            return Task.Run(() => GetKeywords(type, item));
//        });
//        var keyword = console.Prompt(keywordPrompt);
//        var value = console.Ask<string>("[#12d399]Enter a value:[/]");
//        var operatr = console.Prompt(
//            new SelectionPrompt<QueryKeywordOperator>().Title("[#12d399]Select an operator:[/]")
//            .HighlightStyle(new Style(Color.FromHex("#12d399")))
//            .AddChoices(
//                QueryKeywordOperator.Equal,
//                QueryKeywordOperator.NotEqual,
//                QueryKeywordOperator.GreaterThanEqual,
//                QueryKeywordOperator.LessThanEqual,
//                QueryKeywordOperator.GreaterThan,
//                QueryKeywordOperator.LessThan,
//                QueryKeywordOperator.Literal
//            ).DefaultValue(QueryKeywordOperator.NotEqual));
//        var relation = console.Prompt(
//            new SelectionPrompt<QueryKeywordRelation>().Title("[#12d399]Select an relation:[/]")
//            .HighlightStyle(new Style(Color.FromHex("#12d399")))
//            .AddChoices(QueryKeywordRelation.And, QueryKeywordRelation.Or, QueryKeywordRelation.To)
//            .DefaultValue(QueryKeywordRelation.And));
//        var options = new DocumentQueryOptions(type,item,keyword,value,operatr,relation);
//        await Run(options);
//        await ExploreCommand.Explore();
//    }

//    private static async Task<SelectionPrompt<string>> GetKeywords(QueryType type, string item)
//    {
//        var prompt = new SelectionPrompt<string>()
//            .HighlightStyle(new Style(Color.FromHex("#12d399")))
//            .Title($"[#12d399]Select a keyword type to add:[/]");
//        if (type == QueryType.CustomQuery)
//        {            
//            using var app = GetApp();
//            var cq = app.Core.CustomQueries[item];
//            cq?.KeywordTypes.ForEach(k => prompt.AddChoice(k.Name));
//        }
//        if (type == QueryType.DocumentType)
//        {
//            using var app = GetApp();
//            var dt = app.Core.DocumentTypes[item];
//            dt?.KeywordTypeCollection
//                .StandAloneKeywordTypes
//                ?.KeywordTypes.ToList().ForEach(k => prompt.AddChoice(k.Name));
//            dt?.KeywordTypeCollection
//                .SingleInstanceKeywordTypeGroups.ToList()
//                .ForEach(sikg =>
//                {
//                    sikg.KeywordTypes.ToList().ForEach(k => prompt.AddChoice(k.Name));
//                });
//            dt?.KeywordTypeCollection.MultiInstanceKeywordTypeGroups.ToList()
//                .ForEach(mikg =>
//                {
//                    mikg.KeywordTypes.ToList().ForEach(k => prompt.AddChoice(k.Name));
//                });
//        }
//        if (type == QueryType.DocumentTypeGroup)
//        {
//            using var app = GetApp();
//            var dtg = app.Core.DocumentTypeGroups[item];
//            prompt.AddChoice("This is an example app... the logic to find all the common keywords is a bit complex, use the other query types.");
//        }
//        return prompt;
//    }
//    private static SelectionPrompt<string> GetItems(QueryType type)
//    {
//        var prompt = new SelectionPrompt<string>()
//            .HighlightStyle(new Style(Color.FromHex("#12d399")))
//            .Title($"[#12d399]Select the {type} to add:[/]");
//        if (type == QueryType.CustomQuery)
//        {
//            using var app = GetApp();
//            app.Core.CustomQueries.ToList()
//                .ForEach(s => prompt.AddChoice(s.Name));
//        }
//        if(type == QueryType.DocumentType)
//        {
//            using var app = GetApp();
//            app.Core.DocumentTypes.ToList()
//                .ForEach(s => prompt.AddChoice(s.Name));
//        }
//        if(type == QueryType.DocumentTypeGroup)
//        {
//            using var app = GetApp();
//            app.Core.DocumentTypeGroups.ToList()
//                .ForEach(s => prompt.AddChoice(s.Name));
//        }      

//        return prompt;
//    }

//    [Command("query", Description = "Run a Document Query")]
//    public static async Task Run(DocumentQueryOptions options)
//    {
//        try
//        {
//            DocumentQuery? docQuery = await Execute("Building Query...", async () =>
//            {
//                using var app = GetApp();
//                DocumentQueryBuilder? builder = null;
//                IOnBaseItemTypeService? item = null;
//                if (options.Type == QueryType.DocumentType)
//                {
//                    builder = app.Core.CreateDocumentQueryBuilder<DocumentTypeQueryBuilder>();
//                    item = app.Core.DocumentTypes[options.Item];                    
//                }
//                else if (options.Type == QueryType.DocumentTypeGroup)
//                {
//                    builder = app.Core.CreateDocumentQueryBuilder<DocumentTypeGroupQueryBuilder>();
//                    item = app.Core.DocumentTypeGroups[options.Item];                    
//                }
//                else
//                {
//                    builder = app.Core.CreateDocumentQueryBuilder<CustomQueryBuilder>();
//                    item = app.Core.CustomQueries[options.Item];
//                }
//                if (item == null)
//                    throw new Exception($"Could not retrieve the item '{options.Item}' for the {builder.Type} query.");
//                //Add the Document Type, Document Type Group or Custom Query
//                builder.AddItem(item);    
//                //Add the Query Keyword - multiple can be added.
//                builder.AddQueryKeyword(options.Keyword, options.SearchValue, options.Operator, options.Relation);
//                //Add a data range
//                builder.AddDateRange(DateTime.Today.AddDays(-30), DateTime.Now);


//                var query = await builder.CreateQueryAsync(true);                
//                return query;
//            });

//            var queryResults = await ExecuteAsync("Executing Query...", async () => await docQuery.GetResultsAsync());
//            Log($"Query Results ({queryResults.Count})");
//            var table = BuildTableFromQueryResults(queryResults);
//            AnsiConsole.Write(table);

//        }
//        catch (Exception ex)
//        {
//            LogEx(ex.InnerException ?? ex);
//        }
//    }
//    private static Table BuildTableFromQueryResults(IReadOnlyCollection<DocumentResult> results)
//    {
//        var table = new Table()
//            .RoundedBorder()
//            .Expand()
//            .AddColumn("Document Id");

//        if(results.Count == 0)
//        {
//            return table;
//        }
//        int columnCount = results.First().DisplayColumns.Count;
//        foreach (var column in results.First().DisplayColumns.OrderBy(c => c.Index))
//        {
//            table.AddColumn($"Result Column {column.Index}");
//        }
//        foreach(var result in results)
//        {
//            List<string> items = [result.DocumentId.ToString()];
//            foreach (var item in result.DisplayColumns.OrderBy(c => c.Index))
//            {
//                items.Add(string.Join(", ", item.Values));
//            }
//            table.AddRow(items.ToArray());
//        }
//        return table;
//    }
//}

//public record DocumentQueryOptions(
//    [Argument("Type")] QueryType Type,
//    [Argument("Item", Description = "The name or id of the Document Type, DocumentType Group, or Custom Query")]string Item,
//    [Argument("keyword", Description = "Keyword Name or Id")]string Keyword,
//    [Argument("Value", Description = "Keyword Value")] string SearchValue,
//    [Argument("Operator")] QueryKeywordOperator Operator = QueryKeywordOperator.Equal,
//    [Argument("Relation")] QueryKeywordRelation Relation = QueryKeywordRelation.And
//    ) : ICommandParameterSet;
