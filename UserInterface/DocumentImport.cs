using HyRest.DocumentManagement;

namespace HyRest.OnCmd.UserInterface;

public class DocumentImport : Screen
{
    private readonly CliHost _host;
    public DocumentImport(CliHost host, IScreen returnScreen) : base(returnScreen)
    {
        _host = host;
    }
    public static MenuResult Go(CliHost host, IScreen returnScreen)
        => new DocumentImport(host, returnScreen).RunScreen();
    private SelectionPrompt<DocumentImportOptions> Prompt = UI.NewEnumPrompt<DocumentImportOptions>("Select An Option");
    internal DocumentArchiveProperties? DocProps { get; set; }
    internal List<FileInfo> FileInfos { get; set; } = [];
    internal DocumentType? DocumentType { get; set; }
    internal FileType? FileType { get; set; }
    public override MenuResult RunScreen()
    {
        UI.Clear();
        UI.Write(UI.MenuHeader("Doc Upload"));
        var choice = UI.Prompt(Prompt);
        return RouteChoice(choice);
    }

    protected override MenuResult RouteChoice<TOption>(TOption choice) => choice switch
    {
        DocumentImportOptions.Pick_DocumentType => PickDocumentTypeGroup(),
        DocumentImportOptions.Add_Keywords => AddKeywordTypes(),
        DocumentImportOptions.Add_File => AddFiles(),
        DocumentImportOptions.Import => Import(),
        DocumentImportOptions.Clear => UI.Execute("Clearing current changes", () => DocumentImport.Go(_host, ReturnScreen)),
        DocumentImportOptions.Back => Return()
    };

    private MenuResult Import()
    {
        if (DocumentType == null)
        {
            UI.WriteLine("You need to add a document type first before importing.", UI.WarnColor);
            return RouteChoice(UI.Prompt(Prompt));
        }
        if(DocProps.Files.Count == 0)
        {
            UI.WriteLine("There are no files to import.", UI.WarnColor);
            return RouteChoice(UI.Prompt(Prompt));
        }
        var task = DocProps.ArchiveDocument();
        task.Wait();
        if(task.IsCompletedSuccessfully)
        {
            DocumentScreen.Go(_host, task.Result, this);
        }
        else
        {
            if (task.Exception != null)
                UI.ShowError(task.Exception.InnerException ?? task.Exception);
            else
                UI.ShowSadPanel("The upload was not successful");
            return RouteChoice(UI.Prompt(Prompt));
        }
        return MenuResult.Create("...");
    }
    private MenuResult AddFiles()
    {
        if (DocumentType == null)
        {
            UI.WriteLine("You need to add a document type first before adding files.", UI.WarnColor);
            return RouteChoice(UI.Prompt(Prompt));
        }
    tryagain: 
        var choice = UI.Prompt(UI.NewEnumPrompt<FileOptions>("Add a File or Search a directory:"));
        if(choice == FileOptions.File_Path)
        {
            var path = UI.Prompt(PathPrompt("Provide the full path to the file."));
            if(!File.Exists(path))
            {
                if (UI.Confirm("The file does not exist at the path provided. Try again?"))
                    goto tryagain;
                else
                    return RouteChoice(UI.Prompt(Prompt));
            }
            else
            {
                DocProps.WithFile(path);
                UI.Write(new Grid().AddColumns(2).AddRow(UI.ToStyle("File Type: "), DocProps.FileType.Name));
                return RouteChoice(UI.Prompt(Prompt));
            }
        }
        else
        {
            var path = UI.Prompt(PathPrompt("Provide the full path to the directory"));
            if(!Directory.Exists(path))
            {
                if (UI.Confirm("The directory does not exist at the path provided. Try again?"))
                    goto tryagain;
                else
                    return RouteChoice(UI.Prompt(Prompt));
            }
            var selection = UI.Prompt(GetFilesFromDirectory(new DirectoryInfo(path)));
            var first = selection.FirstOrDefault();            
            foreach (var file in selection)
            {
                DocProps.WithFile(file.FullPath);
            }
            UI.Write(new Grid().AddColumns(2).AddRow(UI.ToStyle("File Type: "), DocProps.FileType.Name));

            return RouteChoice(UI.Prompt(Prompt));
        }
        
    }

    private TextPrompt<string> PathPrompt(string message) => UI.TextPrompt<string>(message);
    private MultiSelectionPrompt<FileItem> GetFilesFromDirectory(DirectoryInfo di)
    {
        return UI.Execute("Collecting files...", () =>
        {
            var prompt = UI.NewMultiSelectPrompt<FileItem>("Select files to import. (Must be the same type to combine into a single file)");
            var files = di.GetFiles();
            foreach (var file in files)
            {
                prompt.AddChoice(new FileItem { FileName = file.Name, FullPath = file.FullName });
            }
            return prompt;
        }) ?? throw new Exception("Something went wrong... :(");
    }

    private MenuResult PickDocumentTypeGroup()
    {
        var dtgPrompt = GetDocumentTypeGroups();
        var dt = UI.Prompt(dtgPrompt);
        SetDocumentType(_host.App.Core.DocumentTypes[dt]);
        UI.Write(new Grid().AddColumns(2).AddRow(UI.ToStyle("Document Type: "), DocumentType.Name));
        var choice = UI.Prompt(Prompt);
        return RouteChoice(choice);
    }
    private MenuResult AddKeywordTypes()
    {
        if (DocumentType == null)
        {
            UI.WriteLine("You need to add a document type first before adding Keywords.", UI.WarnColor);
            return RouteChoice(UI.Prompt(Prompt));
        }
        var ktgPrompt = GetKeywordTypeGroups();
        var ktg = UI.Prompt(ktgPrompt);
        var kwColl = DocProps.KeywordCollection;
        EditableKeywordGroup? editGroup = null;
        var keyPrompt = GetKeywordTypes(ktg);
        while (true)
        {            
            var keyword = UI.Prompt(keyPrompt);
            var kt = _host.App.Core.KeywordTypes[keyword];
            var valprompt = UI.TextPrompt<string>($"{keyword} ({kt.DataType.DataType}):")
                .Validate(input =>
                {
                    var provider = kt.CreateKeywordDataTypeHandler();
                    if (provider.TryParse(input, kt.DataType.CommonType, out object result))
                        return true;
                    else
                        return false;
                })
                .ValidationErrorMessage("The value entered can not be converted to the intended datatype.");
            var value = UI.Prompt(valprompt);
            if(ktg == "Standalone Keywords")
                kwColl.CreateEditableKeyword(keyword)
                        .Add(value);
            else
            {
                var group = _host.App.Core.KeywordTypeGroups[ktg];
                if(group.StorageType == KeywordTypeGroupType.MultiInstance)
                {
                    if (editGroup == null)
                        editGroup = kwColl.CreateEditableMultiInstanceRecord(ktg);
                    editGroup.CreateEditableKeyword(keyword).Add(value);
                }
                else
                {
                    if (editGroup == null)
                        editGroup = kwColl.CreateEditableSingleInstanceRecord(ktg);
                    editGroup.CreateEditableKeyword(keyword).Add(value);
                }
            }
            if (!UI.Confirm("Add another keyword?"))
                break;
        }
        if (!UI.Confirm("Add another keyword type group?"))
            return RouteChoice(UI.Prompt(Prompt));
        else
            return AddKeywordTypes();
    }

    private void SetDocumentType(DocumentType? docType)
    {
        if (docType == null)
            throw new Exception("Document Type is null");
        DocumentType = docType;
        DocProps = docType.CreateNewDocumentArchiveProperties();
        if(FileInfos.Count > 0)
            FileInfos.ForEach(f => DocProps.WithFile(f));
        if(FileType != null)
            DocProps.WithFileType(FileType);
    }

    private SelectionPrompt<string> GetKeywordTypes(string ktgName)
    {
        return UI.Execute("Fetching keyword types", () =>
        {
            var prompt = UI.NewPrompt<string>("Keyword Types");
            List<KeywordType> kwtypes = [];
            if (ktgName == "Standalone Keywords")
                kwtypes = DocumentType.KeywordTypeCollection.StandAloneKeywordTypes.KeywordTypes.ToList();
            else
            {
                var group = _host.App.Core.KeywordTypeGroups[ktgName];
                kwtypes = group.KeywordTypes.ToList();
            }
            kwtypes.ForEach(kt => prompt.AddChoice(kt.Name));
            return prompt;
        }) ?? throw new Exception("Something went wrong... :(");
    }
    private SelectionPrompt<string> GetKeywordTypeGroups()
    {
        return UI.Execute("Fetching keyword type groups...", () =>
        {
            var prompt = UI.NewPrompt<string>("Keyword Type Group");
            var standAlone = DocumentType.KeywordTypeCollection.StandAloneKeywordTypes;
            var sikgs = DocumentType.KeywordTypeCollection.SingleInstanceKeywordTypeGroups;
            var mikgs = DocumentType.KeywordTypeCollection.MultiInstanceKeywordTypeGroups;
            if (standAlone.Count > 0)
            {
                var sak = prompt.AddChoice("Standalone Keywords");
            }
            sikgs.ToList()
            .ForEach(sikg =>
            {
                var s = prompt.AddChoice(sikg.Name);
            });
            mikgs.ToList()
            .ForEach(mikg =>
            {
                var m = prompt.AddChoice(mikg.Name);
            });
            return prompt;
        }) ?? throw new Exception("Something went wrong... :(");
    }
    private SelectionPrompt<string> GetDocumentTypeGroups()
    {
        return UI.Execute("Fetching document types...", () =>
        {
            var prompt = UI.NewPrompt<string>("Document Type Groups");
            _host.App.Core.DocumentTypeGroups
            .ToList()
            .ForEach(g =>
            {
                var item = prompt.AddChoice(g.Name);
                g.DocumentTypes.ToList()
                .ForEach(d =>
                {
                    item.AddChild(d.Name);
                });
            });
            return prompt;
        }) ?? throw new Exception("Something went wrong... :(");
    }
}


public enum FileOptions
{
    File_Path,
    Search_Directory,
    Back
}
public enum DocumentImportOptions
{
    Pick_DocumentType,
    Add_Keywords,
    Add_File,
    Import,
    Clear,
    Back
}

internal struct FileItem
{
    public string FileName;
    public string FullPath;
    public override string ToString()
    {
        return FileName;
    }
}