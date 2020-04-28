namespace PawnMake
{
    public class MakeFile
    {
        public string ProjectName { get; set; } = "project";
        public BuildFolder[] BuildFolders { get; set; }
        public string[] IncludeFolders { get; set; }
        public string[] Files { get; set; }
        public string[] Run { get; set; }
        public string Args { get; set; }
    }
}
