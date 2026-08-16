using HyRest.OnBase.Core;
using System.Diagnostics;

namespace HyRest.OnCmd.UserInterface;

public class DocumentScreen : Screen
{
    public static MenuResult Go(CliHost host, Document doc, IScreen returnScreen)
        => new DocumentScreen(host, doc, returnScreen).RunScreen();
    private readonly CliHost _host;
    private readonly Document _doc;
    private SelectionPrompt<DocumentOptions> Prompt = UI.NewEnumPrompt<DocumentOptions>("Select An Option");
    private Grid DocumentGrid => new Grid()
        .AddColumns(
        new GridColumn().Centered().Padding(2, 4),
        new GridColumn().LeftAligned().Padding(2, 4),
        new GridColumn().Padding(2,4));
    public DocumentScreen(CliHost host, Document doc, IScreen returnScreen) :base(returnScreen)
    {
        _host = host;
        _doc = doc;
    }

    public override MenuResult RunScreen()
    {
        var grid = DocumentGrid;
        grid.AddRow("Document Handle", _doc.Id.ToString());
        grid.AddRow("Document Date", _doc.DocumentDate.ToShortDateString(), "");
        grid.AddRow("Document Type", _doc.DocumentType.Name, "");
        grid.AddRow("Status", _doc.Status.ToString(), "");
        grid.AddRow("Created By", _doc.CreatedByUserId.ToString(), "");
        UI.Write(UI.NewPanel("🭁 Document", grid));
        var choice = UI.Prompt(Prompt);
        return RouteChoice(choice);
    }

    protected override MenuResult RouteChoice<TOption>(TOption choice) => choice switch
    {
        DocumentOptions.View => View(),
        DocumentOptions.Keywords => KeywordCollection(),
        DocumentOptions.Notes => Notes(),
        DocumentOptions.History => History(),
        DocumentOptions.Back => Return()
    };

    private MenuResult View()
    {
        string filename = string.Empty;
        UI.Execute("Fetching document content...", () =>
        {
            var content = _doc.GetContent();
            filename = content.SaveToFile(Path.GetTempPath());
            Process.Start(new ProcessStartInfo
            {
                FileName = filename,
                UseShellExecute = true,
            });
        });
        if (File.Exists(filename))
            UI.Write($"Success! File located at: {filename}", UI.PrimaryColor);
        else
            UI.Write($"Something went wrong, the file is not present at the temp path {filename}", UI.ErrorColor);        
        var choice = UI.Prompt(Prompt);
        return RouteChoice(choice);
    }

    private MenuResult History()
    {
        var panel = UI.Execute("Fetching document history...", () =>
        {
            Thread.Sleep(5000);
            var history = _doc.GetHistory();
            var grid = new Grid().Expand()
            .AddColumns(
                new GridColumn().Padding(1, 1).Centered(),
                new GridColumn().Padding(1, 1).Centered(),
                new GridColumn().Padding(1, 1).Centered(),
                new GridColumn().Padding(1, 1).LeftAligned()
            );
            foreach (var hist in history.Items)
            {
                var date = DateTime.Parse(hist.LogDate).ToString();
                grid.AddRow(Markup.Escape(hist.Action), Markup.Escape(date), Markup.Escape(hist.UserId), Markup.Escape(hist.Message));
            }
            return UI.NewPanel("◔ History", grid);
        });        
        UI.Write(panel);
        var choice = UI.Prompt(Prompt);
        return RouteChoice(choice);
    }

    private MenuResult Notes()
    {
        var grid = new Grid().Expand()
            .AddColumns(
            new GridColumn().Padding(2,2), 
            new GridColumn().Padding(2, 2), 
            new GridColumn().Padding(2, 2), 
            new GridColumn().Padding(2, 2));
        var notesList = _doc.GetNotesForRevision();
        var notechunks = notesList.Chunk(4);      
        foreach(var notes in notechunks)
        {
            grid.AddRow(
                NoteToPanel(notes.ElementAtOrDefault(0)),
                NoteToPanel(notes.ElementAtOrDefault(1)),
                NoteToPanel(notes.ElementAtOrDefault(2)),
                NoteToPanel(notes.ElementAtOrDefault(3))
                );    
                                 
            Panel NoteToPanel(Note? note)
            {
                var noteGrid = new Grid().Expand().AddColumn();
                if (note == null)
                {
                    noteGrid.AddRow(Text.Empty);
                    return new Panel(noteGrid).NoBorder();
                }                
                noteGrid.AddRow(note.Text);
                return UI.NewPanel($"◪{note.NoteType?.Name ?? "NoteType"}", noteGrid).Expand();
            }
        }
        UI.Write(UI.NewPanel("◪ Notes", grid));
        var choice = UI.Prompt(Prompt);
        return RouteChoice(choice);
    }

    private MenuResult KeywordCollection()
    {
        var grid = DocumentGrid;
        
        grid = KeywordGroup(grid, [_doc.KeywordCollection.StandAloneKeywords]);
        grid = SingleInstance(grid, _doc.KeywordCollection.SingleInstanceGroups);
        grid = MultiInstance(grid, _doc.KeywordCollection.MultiInstanceGroups);        
        UI.Write(UI.NewPanel("🭪 Keywords", grid));
        var choice = UI.Prompt(Prompt);
        return RouteChoice(choice);
    }

    private Grid MultiInstance(Grid grid, SortedMultiInstanceCollections collection)
    {
        foreach(var list in collection.GroupCollection)
        {
            List<IKeywordGroup> newList = [];
            list.ToList().ForEach(g => newList.Add(g));
            grid = KeywordGroup(grid, newList);
        }
        return grid;
    }

    private Grid SingleInstance(Grid grid, SingleInstanceGroupCollection list)
    {        
        List<IKeywordGroup> newList = [];
        list.ToList().ForEach(g =>  newList.Add(g));
        return KeywordGroup(grid, newList);
    }

    private Grid KeywordGroup(Grid grid, List<IKeywordGroup> list)
    {
        if (list.Count == 0)
            return grid;
        grid.AddRow(list[0].GroupType.ToString(), list[0].Name ?? "", "");
        foreach(var group in list)
        {
            grid = KeywordList(grid, group.ToList());
        }
        return grid;
    }
    private Grid KeywordList(Grid grid, List<Keyword> keywords)
    {
        foreach (var key in keywords)
        {
            var name = key.Name;
            var value = string.Join(", ", key.Values.Select(v => v.ToString()));
            grid.AddRow("", name, value);
        }
        return grid;
    }
}

public enum DocumentOptions
{
    View,
    Keywords,
    Notes,
    History,
    //Update Keyword Collection Screen
    Back
}