//using Cocona;
//using System.Runtime.CompilerServices;
//using HyRest.DocumentManagement;
//using console = Spectre.Console.AnsiConsole;
//using Color = Spectre.Console.Color;

//namespace HyRest.OnCmd;

//public class DocumentUploadCommand : BaseCommand
//{
//    public async static Task Import()
//    {
//        try
//        {
//            var docTypeId = console.Ask<string>("[#12d399]Enter the document type name or id:[/]");
//            using var app = GetApp();
//            var docType = app.Core.DocumentTypes[docTypeId];
//            if (docType == null)
//                throw new Exception("Could not find the document type.");

//            var props = docType.CreateNewDocumentArchiveProperties();
//            props.DocumentDate = DateTime.Now;
            
//        filePath:
//            var filePth = console.Ask<string>("[#12d399]Provide the full path to the file:[/]");
//            if(!File.Exists(filePth))
//            {
//                var choice = console.Prompt(new SelectionPrompt<string>().Title("File does not exist. Try Again?")
//                    .HighlightStyle(new Style(Color.FromHex("#12d399")))
//                    .AddChoices("yes", "no"));
//                if (choice == "yes")
//                    goto filePath;
//                else
//                    return;
//            }
//            else
//            {
//                props.WithFile(filePth);
//            }
//        keywordChoice:
//            var keywordChoice = console.Prompt(new SelectionPrompt<KeywordChoice>().Title("Select a Keyword Record type to add:")
//                    .AddChoices(KeywordChoice.StandAloneKeyword, KeywordChoice.SingleInstanceGroup, KeywordChoice.MultiInstanceGroup, KeywordChoice.Done)
//                    .AddCancelResult(KeywordChoice.Done));
//            if(keywordChoice == KeywordChoice.StandAloneKeyword)
//            {
//                var prompt = GetKeywordsPrompt(props.KeywordCollection.StandAloneKeywords.Keywords.ToList());
//                if (prompt != null)
//                {
//                    string keyword = console.Prompt(prompt);
//                    string value = console.Ask<string>("Provide a value for the keyword: ");
//                    props.KeywordCollection.CreateEditableKeyword(keyword).Add(value);
//                }    
//                goto keywordChoice;
//            }
//            else if(keywordChoice == KeywordChoice.SingleInstanceGroup)
//            {
//                var prompt = GetSikgGroups(props.KeywordCollection.SingleInstanceGroups);
//                if (prompt != null)
//                {
//                    string groupName = console.Prompt(prompt);
//                    var group = props.KeywordCollection.SingleInstanceGroups[groupName];

//                    var editable = props.KeywordCollection.CreateEditableSingleInstanceRecord(groupName);
                        
//                    foreach (var k in group.Keywords)
//                    {
//                        string? value = console.Ask<string?>($"Provide a value for the keyword [#12d399]{k.Name}[/] or hit enter to skip:", null);
//                        if (value != null)
//                          editable.CreateEditableKeyword(k.Name)
//                                .Add(value);
//                    }
//                }
//                goto keywordChoice;
//            }
//            else if(keywordChoice == KeywordChoice.MultiInstanceGroup)
//            {
//                var prompt = GetMikgGroups(props.KeywordCollection.MultiInstanceGroups);
//                if (prompt != null)
//                {
//                    string groupName = console.Prompt(prompt);
//                    var group = props.KeywordCollection.MultiInstanceGroups[groupName]?.GroupRecords.FirstOrDefault();
//                    var editable = props.KeywordCollection.CreateEditableMultiInstanceRecord(groupName);
//                    foreach (var k in group.Keywords)
//                    {
//                        string? value = console.Ask<string?>($"Provide a value for the keyword [#12d399]{k.Name}[/] or hit esc to skip:", null);
//                        if (value != null)
//                            editable.CreateEditableKeyword(k.Name)
//                                .Add(value);
//                    }
//                }
//                goto keywordChoice;
//            }
//            else
//            {
//                Document? document = null;
//                console.Progress()
//                    .Start(ctx =>
//                    {
//                        var task = ctx.AddTask("Uploading File", maxValue: 100);
//                        var import = props.ArchiveDocument();             
//                        while(!ctx.IsFinished)
//                        {
//                            if (import.IsCompleted)
//                                task.Value = 100;
//                            task.Increment(1);
//                            Thread.Sleep(50);
//                        }
//                        if (import.IsCompletedSuccessfully)
//                            document = import.Result;
//                        else
//                            Log("Failed to import document", import.Status);
//                    });
//                Log($"Document was uploaded with id: {document.Id}");
//            }

//        }
//        catch(Exception ex)
//        {
//            LogEx(ex);
//        }
//        await ExploreCommand.Explore();
//    }

//    private static SelectionPrompt<string>? GetMikgGroups(SortedMultiInstanceCollections collection)
//    {
//        if (collection.GroupCollection.Count == 0)
//        {
//            Log("There are no Multi Instance Keyword Groups in the Keyword Collection");
//            return null;
//        }
//        var prompt = new SelectionPrompt<string>().Title("Select a Keyword Group:")
//            .HighlightStyle(new Style(Color.FromHex("#12d399")));
//        foreach (var group in collection.GroupCollection)
//        {
//            prompt.AddChoice(group.Name);
//        }
//        return prompt;
//    }
//    private static SelectionPrompt<string>? GetSikgGroups(SingleInstanceGroupCollection collection)
//    {
//        if(collection.GroupRecords.Count == 0)
//        {
//            Log("There are no Single Instance Keyword Groups in the Keyword Collection");
//            return null;
//        }
//        var prompt = new SelectionPrompt<string>().Title("Select a Keyword Group:")
//            .HighlightStyle(new Style(Color.FromHex("#12d399")));
//        foreach(var group in collection.GroupRecords)
//        {
//            prompt.AddChoice(group.Name);
//        }
//        return prompt;
//    }
//    private static SelectionPrompt<string>? GetKeywordsPrompt(List<Keyword> keywords)
//    {
//        if(keywords.Count == 0)
//        {
//            Log("There are no Keywords in the Keyword Collection");
//            return null;
//        }
//        var prompt = new SelectionPrompt<string>().Title("Select a Keyword: ")
//            .HighlightStyle(new Style(Color.FromHex("#12d399")));
//        foreach(var kw in keywords)
//        {
//            prompt.AddChoice(kw.Name);
//        }
//        return prompt;
//    }

//}

//public enum KeywordChoice
//{
//    StandAloneKeyword,
//    SingleInstanceGroup,
//    MultiInstanceGroup,
//    Done,
//}