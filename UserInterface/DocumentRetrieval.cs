using HyRest.API.Models;
using HyRest.OnBase.Core;

namespace HyRest.OnCmd.UserInterface;

public class DocumentRetrieval : Screen
{
    private CliHost _host;
    public DocumentRetrieval(CliHost host, IScreen returnScreen): base(returnScreen)
    {
        _host = host;
    }
    public static MenuResult Go(CliHost host, IScreen returnScreen) 
        => new DocumentRetrieval(host, returnScreen).RunScreen();
    public override MenuResult RunScreen()
    {
        UI.Clear();
        UI.Write(UI.MenuHeader("Doc Retrieval"));
        var choice = UI.Prompt(UI.NewEnumPrompt<DocRetrievalOptions>("Retrieval Options"));            
        return RouteChoice(choice);
    }

    protected override MenuResult RouteChoice<TOption>(TOption choice) => choice switch
    {
        DocRetrievalOptions.Document_Handle => DocumentHandle(),
        DocRetrievalOptions.DocumentType_Query => DocumentQuery(QueryType.DocumentType),
        DocRetrievalOptions.CustomQuery => DocumentQuery(QueryType.CustomQuery),
        DocRetrievalOptions.DocumentTypeGroup_Query => DocumentQuery(QueryType.DocumentTypeGroup),
        DocRetrievalOptions.Back => Return()
    };

    public MenuResult DocumentQuery(QueryType type)
    {
        var table = new Table()
            .AddColumns("Keyword", "Value", "Operator", "Relation")
            .RoundedBorder()
            .BorderColor(Color.DarkSlateGray3)
            .Expand();  
        var prompt = GetItems(type);
        var choice = UI.Prompt(prompt);
        var keyPrompt = GetKeywords(type, choice);
        List<Tuple<string, string, QueryKeywordOperator, QueryKeywordRelation>> queryParams = [];
        while(true)
        {
            var keyword = UI.Prompt(keyPrompt);
            var value = UI.Prompt(UI.TextPrompt<string>($"{keyword}:"));
            var oper = UI.Prompt(UI.NewEnumPrompt<QueryKeywordOperator>("Select an Operator:"));
            var relation = UI.Prompt(UI.NewEnumPrompt<QueryKeywordRelation>("Select a Relation:"));
            queryParams.Add(Tuple.Create(keyword, value, oper, relation));
            table.AddRow(keyword, value, oper.ToString(), relation.ToString());
            if (!UI.Confirm("Add Another?"))
                break;
        }
        DateRange? dateRange = null;
        if(UI.Confirm("Add a Date Range?", false))
        {
            dateRange = new DateRange();
            dateRange.Start = UI.Prompt(UI.TextPrompt<DateOnly>($"From:")).ToDateTime(TimeOnly.MinValue);
            dateRange.End = UI.Prompt(UI.TextPrompt<DateOnly>($"To:")).ToDateTime(TimeOnly.MinValue); 
        }
        var panel = UI.NewPanel($"Query Parameters: {choice}", table);
        UI.Write(panel);
        var result = RunQuery(type, choice, queryParams, dateRange);
        var docChoice = UI.Prompt(result);
        return DocumentScreen.Go(_host, docChoice.Document, ReturnScreen);
    }

    public MenuResult DocumentHandle()
    {
        try
        {
            long docHandle = UI.Prompt(UI.TextPrompt<long>("Enter the Document Handle"));
            var doc = UI.Execute($"Checking for document {docHandle}...", () =>
            {
                return _host.App.Core.GetDocumentById(docHandle);
            });
            if (doc == null)
            {
                UI.ShowSadPanel("No Document found with that ID!");
                UI.Write("Press Enter to try again.", UI.SecondaryColor);
                Console.ReadLine();
                RunScreen();
            }
            return new DocumentScreen(_host, doc, this).RunScreen();
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
            return Return();
        }
    }

    private SelectionPrompt<DocumentQueryItem> RunQuery(QueryType type, string item,
        List<Tuple<string, string, QueryKeywordOperator, QueryKeywordRelation>> queryParams, DateRange? dateRange = null)
    {
        return UI.Execute("Running Query...", () => {
            DocumentQueryBuilder? builder = null;
            IOnBaseItemTypeService? service = null;
            if (type == QueryType.DocumentType)
            {
                builder = _host.App.Core.CreateDocumentQueryBuilder<DocumentTypeQueryBuilder>();
                service = _host.App.Core.DocumentTypes[item];
            }
            else if (type == QueryType.DocumentTypeGroup)
            {
                builder = _host.App.Core.CreateDocumentQueryBuilder<DocumentTypeGroupQueryBuilder>();
                service = _host.App.Core.DocumentTypeGroups[item];
            }
            else
            {
                builder = _host.App.Core.CreateDocumentQueryBuilder<CustomQueryBuilder>();
                service = _host.App.Core.CustomQueries[item];
            }
            if (service == null)
                throw new Exception($"Could not retrieve the item '{item}' for the {builder.Type} query.");
            //Add the Document Type, Document Type Group or Custom Query
            builder.AddItem(service);
            
            foreach(var tuple in queryParams)
            {
                builder.AddQueryKeyword(tuple.Item1, tuple.Item2,tuple.Item3,tuple.Item4);
            }            
            if(dateRange != null)
                builder.AddDateRange(dateRange);
            var query = builder.CreateQuery(true);
            var results = query.GetResults();            
            if (results.Count == 0)
            {
                UI.ShowSadPanel("No Documents found.");
                Return();
            }            
            return UI.NewPrompt<DocumentQueryItem>("Select a document").AddChoices(GetResultItemList(type, results));
        }) ?? throw new Exception("Something went wrong... :(");
    }
    private SelectionPrompt<string> GetItems(QueryType type)
    {
        return UI.Execute("Getting Items...", () =>
        {
            var prompt = UI.NewPrompt<string>("Select an Item:");
            if (type == QueryType.CustomQuery)
            {
                _host.App.Core.CustomQueries.ToList()
                    .ForEach(s => prompt.AddChoice(s.Name));
            }
            if (type == QueryType.DocumentType)
            {
                _host.App.Core.DocumentTypes.ToList()
                    .ForEach(s => prompt.AddChoice(s.Name));
            }
            if (type == QueryType.DocumentTypeGroup)
            {
                _host.App.Core.DocumentTypeGroups.ToList()
                    .ForEach(s => prompt.AddChoice(s.Name));
            }

            return prompt;
        }) ?? throw new Exception("Something went wrong... :(");        
    }

    private SelectionPrompt<string> GetKeywords(QueryType type, string item)
    {
        return UI.Execute("Fetching Keywords", () => {
            var prompt = UI.NewPrompt<string>("Select a keyword to add to the query:");
            if (type == QueryType.CustomQuery)
            {                
                var cq = _host.App.Core.CustomQueries[item];
                cq?.KeywordTypes.ToList().ForEach(k => prompt.AddChoice(k.Name));
            }
            if (type == QueryType.DocumentType)
            {
                var dt = _host.App.Core.DocumentTypes[item];
                dt?.KeywordTypeCollection
                    .StandAloneKeywordTypes
                    ?.KeywordTypes.ToList().ForEach(k => prompt.AddChoice(k.Name));
                dt?.KeywordTypeCollection
                    .SingleInstanceKeywordTypeGroups.ToList()
                    .ForEach(sikg =>
                    {
                        sikg.KeywordTypes.ToList().ForEach(k => prompt.AddChoice(k.Name));
                    });
                dt?.KeywordTypeCollection.MultiInstanceKeywordTypeGroups.ToList()
                    .ForEach(mikg =>
                    {
                        mikg.KeywordTypes.ToList().ForEach(k => prompt.AddChoice(k.Name));
                    });
            }
            if (type == QueryType.DocumentTypeGroup)
            {                
                var dtg = _host.App.Core.DocumentTypeGroups[item];
                prompt.AddChoice("This is an example app... the logic to find all the common keywords is a bit complex, use the other query types.");
            }
            return prompt;
        }) ?? throw new Exception("Something went wrong... :(");
    }

    private List<DocumentQueryItem> GetResultItemList(QueryType type, IEnumerable<DocumentResult> results)
    {
        List<DocumentQueryItem> list = [];
        foreach(var result in results)
        {
            var item = new DocumentQueryItem
            {
                Id = result.DocumentId,
                Document = result.Document
            };
            if(type == QueryType.CustomQuery && result.DisplayColumns.Count > 1)
            {
                item.DisplayColumns = result.DisplayColumns.Select(c => string.Join(", ", c.Values)).ToList();
            }
            else
            {
                item.DisplayColumns = [
                    result.DocumentId.ToString(), 
                    result.Document.Name,
                    result.Document.DocumentDate.ToString(), 
                    result.Document.Status.ToString()
                    ];
            }
            list.Add(item);
        }
        return list;
    }
}
public enum DocRetrievalOptions
{
    Document_Handle,
    DocumentType_Query,
    DocumentTypeGroup_Query,
    CustomQuery,
    Back
}

public struct DocumentQueryItem
{
    public long Id { get; set; }
    public Document Document { get; set; }
    public List<string> DisplayColumns { get; set; }
    public override string ToString()
    {
        return string.Join(" | ", DisplayColumns);
    }
}