//using Cocona;
//using console = Spectre.Console.AnsiConsole;
//using Color = Spectre.Console.Color;

//namespace HyRest.OnCmd;


//public class ExploreCommand : BaseCommand
//{
//    [Command("Explore")]
//    public static async Task Explore()
//    {
//        //try
//        //{            
//        //    MainMenuItem choice = console.Prompt(
//        //    UserInterface.NewPrompt<MainMenuItem>("Welcome to the OnBase Terminal Client")
//        //    .AddChoiceGroup(MainMenuItem.MainMenu, [MainMenuItem.Retrieval, MainMenuItem.Import, MainMenuItem.DocumentQuery])
//        //    .AddChoice(MainMenuItem.LogOut)
//        //    .AddCancelResult(MainMenuItem.LogOut));

//        //    await GetTask(choice);
//        //    if (choice != MainMenuItem.LogOut)
//        //        await Explore();
//        //}
//        //catch(Exception ex)
//        //{
//        //    LogEx(ex);
//        //}
//    }

//    private static Task GetTask(MainMenuItem choice) => choice switch
//    {
//        MainMenuItem.Retrieval => DocumentRetrievalCommand.Assist(),
//        MainMenuItem.DocumentQuery => DocumentQueryCommand.Assist(),    
//        //MainMenuItem.ImportFile => DocumentUploadCommand.Import(),
//        _ => ExploreCommand.RunGet(choice)

//    };

//    private static Task RunGet(MainMenuItem choice)
//    {
//        using var app = GetApp();

//        //if (choice == MainMenuItem.AutofillKeySets)
//        //    return CreateTable(app.Core.AutoFillKeywordSets.OrderBy(i => i.Id).ToList());
//        //if (choice == MainMenuItem.CustomQueries)
//        //    return CreateTable(app.Core.CustomQueries.OrderBy(i => i.Id).ToList());
//        //if (choice == MainMenuItem.DocumentTypeGroups)
//        //    return CreateTable(app.Core.DocumentTypeGroups.OrderBy(i => i.Id).ToList());
//        //if (choice == MainMenuItem.DocumentTypes)
//        //    return CreateTable(app.Core.DocumentTypes.OrderBy(i => i.Id).ToList());
//        //if (choice == MainMenuItem.FileTypes)
//        //    return CreateTable(app.Core.FileTypes.OrderBy(i => i.Id).ToList());
//        //if (choice == MainMenuItem.KeywordTypes)
//        //    return CreateTable(app.Core.KeywordTypes.OrderBy(i => i.Id).ToList());
//        //if (choice == MainMenuItem.KeywordTypeGroups)
//        //    return CreateTable(app.Core.KeywordTypeGroups.OrderBy(i => i.Id).ToList());
//        //if (choice == MainMenuItem.NoteTypes)
//        //    return CreateTable(app.Core.NoteTypes.ToList());
//        return Task.Delay(500);        
//    }

//    private static Task CreateTable<T>(List<T> services)
//        where T : class, IOnBaseItemTypeService
//    {
//        var gridColumn1 = new GridColumn().Padding(2, 4).Alignment(Justify.Right);
//        var gridColumn2 = new GridColumn().Padding(2, 4).Alignment(Justify.Left);
//        var grid = new Grid()
//            .AddColumns(gridColumn1, gridColumn2);
//        if(services.Count > 0)
//        {
//            foreach (var service in services)
//            {
//                grid.AddRow(service.Id.ToString(), service.Name);
//            }
//            var panel = new Panel(grid).Header($"[#12d399]{typeof(T).Name}[/]").RoundedBorder();
//            console.Write(panel);
//        }
//        else
//        {
//            var panel = new Panel(new Text("Nothing to retrieve.", new Style(Color.Yellow)))
//                .Header($"[#12d399]{typeof(T).Name}[/]").RoundedBorder();
//            console.Write(panel);
//        }
//        return Task.Delay(500);
//    }
//}

