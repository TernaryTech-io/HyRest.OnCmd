//using Cocona;
//using HyRest.DocumentManagement;
//using console = Spectre.Console.AnsiConsole;
//using Color = Spectre.Console.Color;

//namespace HyRest.OnCmd;

//public partial class UserInterface
//{
//    [Command("assist", Description = "Walk through the process of retrieving a document")]
//    public static async Task Assist()
//    {
//        try
//        {
//            string docHandle = console.Ask<string>("[#12d399]Enter the document handle of a document:[/]");
//            var doc = await ExecuteAsync("Retrieving the document", () => Retrieve(docHandle));
//            if (doc == null)
//            {
//                Log("No document found...", Microsoft.Extensions.Logging.LogLevel.Warning);
//                return;
//            }
//            var next = console.Prompt(new SelectionPrompt<string>().Title("What would you like to do?")
//                .HighlightStyle(new Style(Color.FromHex("#12d399")))
//                .AddChoices("View Keywords", "Download Content"));
//            if (next == "View Keywords")
//                await Execute("Getting Keywords", () => Keywords(docHandle));
//            else
//                await Execute("Downloading Content...", () => Content(docHandle, null));
//        }
//        catch(Exception ex)
//        {
//            LogEx(ex);
//        }
//        await ExploreCommand.Explore();
//    }
//    [Command("content", Description = "Download the content of the document.")]
//    public static async Task Content([Argument("Document Handle")] string documentHandle, [Option('p')]string? folderPath)
//    {
//        try
//        {
//            using var app = GetApp();
//            var doc = await app.Core.GetDocumentByIdAsync(documentHandle);
//            if (doc == null)
//            {
//                Log("No document found...", Microsoft.Extensions.Logging.LogLevel.Warning);
//                return;
//            }
//            var content = await doc.GetContentAsync();
//            if(folderPath == null)
//            {
//                folderPath = Path.GetTempPath();
//            }
//            var item = await content.SaveToFileAsync(folderPath);

//            Log($"The file was downloaded to {item}", TaskStatus.RanToCompletion);
//        }
//        catch (Exception ex)
//        {
//            LogEx(ex);
//        }
//    }
//    [Command("Keywords", Description = "Get the keywords of a document.")]
//    public static async Task Keywords([Argument("Document Handle")] string documentHandle)
//    {
//        try
//        {
//            using var app = GetApp();
//            var doc = await app.Core.GetDocumentByIdAsync(documentHandle);
//            if (doc == null)
//            {
//                Log("No document found...", Microsoft.Extensions.Logging.LogLevel.Warning);
//                return;
//            }
//            await LogKeywordCollection(doc.KeywordCollection);
            
//        }    
//        catch(Exception ex)
//        {
//            LogEx(ex);
//        }
//    }
//    [Command("retrieve", Description = "Get a Document by Document Handle")]
//    public static async Task<Document?> Retrieve([Argument("Document Handle")]string documentHandle)
//    {
//        try
//        {
//            using var app = GetApp();
//            var doc = await app.Core.GetDocumentByIdAsync(documentHandle);
//            if (doc == null)
//            {
//                Log("No document found...", Microsoft.Extensions.Logging.LogLevel.Warning);
//                return null;
//            }
            
//            var gridColumn1 = new GridColumn().Padding(2, 4).Alignment(Justify.Right);
//            var gridColumn2 = new GridColumn().Padding(2, 4).Alignment(Justify.Left);
//            var grid = new Grid()                
//                .AddColumns(gridColumn1,gridColumn2);
//            grid.AddRow("Id", doc.Id.ToString());
//            grid.AddRow("Document Name", $"{doc.Name}");
//            grid.AddRow("Document Type", $"{doc.DocumentType?.Name} ({doc.DocumentType?.Id})");
//            grid.AddRow("Document Date", $"{doc.DocumentDate}");
//            grid.AddRow("Status", doc.Status.ToString());
//            grid.AddRow("Created By", doc.CreatedByUserId ?? "Unknown");
//            var panel = new Panel(grid).Header("[#12d399]Document Result[/]").RoundedBorder();
//            console.Write(panel);
//            return doc;
//        }
//        catch(Exception ex)
//        {
//            LogEx(ex);
//        }
//        return null;
//    }
//    internal static async Task LogKeywordCollection(KeywordCollection collection)
//    {
//        var gridColumn1 = new GridColumn().Padding(2, 4).Alignment(Justify.Right);
//        var gridColumn2 = new GridColumn().Padding(2, 4).Alignment(Justify.Left);
//        var grid = new Grid()
//            .AddColumns(gridColumn1, gridColumn2);
//        foreach (var sak in collection.StandAloneKeywords.Keywords)
//        {
//            grid.AddRow(sak.Name, string.Join(", ", sak.Values.Select(v => v.ToString())));
//        }
//        var panel = new Panel(grid).Header("[#12d399]Standalone Keywords[/]").RoundedBorder();
//        console.Write(panel);
//        List<Panel> panels = [];
//        foreach (var group in collection.SingleInstanceGroups.ToList())
//        {
//            var sikgColumn1 = new GridColumn().Padding(2, 4).Alignment(Justify.Right);
//            var sikgColumn2 = new GridColumn().Padding(2, 4).Alignment(Justify.Left);
//            var sikggrid = new Grid()
//            .AddColumns(sikgColumn1, sikgColumn2);
//            foreach (var keyword in group.Keywords)
//            {
//                sikggrid.AddRow(keyword.Name, string.Join(", ", keyword.Values.Select(v => v.ToString())));
//            }
//            panels.Add(new Panel(sikggrid).Header($"[#12d399]{group.Name}[/]").RoundedBorder());
//        }
//        var sikgPanelGrid = new Grid().AddColumn();
//        panels.ForEach(p => sikgPanelGrid.AddRow(p));
//        console.Write(new Panel(sikgPanelGrid).Header("[#12d399]Single Instance Keyword Groups[/]").Expand().RoundedBorder());

//        foreach (var mikgCollection in collection.MultiInstanceGroups.GroupCollection)
//        {
//            List<Panel> mikgPanels = [];
//            var mikggrid = new Grid()
//            .AddColumn();
//            foreach (var group in mikgCollection.GroupRecords)
//            {
//                var mikgColumn1 = new GridColumn().Padding(2, 4).Alignment(Justify.Right);
//                var mikgColumn2 = new GridColumn().Padding(2, 4).Alignment(Justify.Left);
//                var groupgrid = new Grid()
//                .AddColumns(mikgColumn1, mikgColumn2);
//                foreach (var keyword in group.Keywords)
//                {
//                    groupgrid.AddRow(keyword.Name, string.Join(", ", keyword.Values.Select(v => v.ToString())));
//                }
//                mikgPanels.Add(new Panel(groupgrid).Header($"[#12d399]{group.GroupId}[/]").RoundedBorder());
//            }
//            mikgPanels.ForEach(p => mikggrid.AddRow(p));
//            console.Write(new Panel(mikggrid).Header($"[#12d399]MultiInstance Group {mikgCollection.Name}[/]").RoundedBorder());
//        }
//        return;
//    }
//}
