namespace ModelDownloader.Types
{
    public class ModelsaberSearch
    {
        public ModelsaberSearchType ModelType = ModelsaberSearchType.All;
        public ModelsaberSearchSort ModelSort = ModelsaberSearchSort.Newest;
        public string Search = "";
        public int Page = 0;

        public readonly int PageLength = 18;

        public ModelsaberSearch(ModelsaberSearchType type = ModelsaberSearchType.All, int page = 0, ModelsaberSearchSort sort = ModelsaberSearchSort.Newest, string search = "")
        {
            ModelType = type;
            Page = page;
            ModelSort = sort;
            Search = search;
        }
    }

    public enum ModelsaberSearchType
    {
        All = 0,
        Saber = 1,
        Bloq = 2,
        Platform = 3,
        Avatar = 4
    }
    public enum ModelsaberSearchSort
    {
        Newest = 0,
        Oldest = 1,
        Name = 2,
        Author = 3,
    }
}
