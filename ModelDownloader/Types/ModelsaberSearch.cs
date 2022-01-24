namespace ModelDownloader.Types
{
    public class ModelSaberSearch
    {
        public ModelsaberSearchType ModelType { get; } = ModelsaberSearchType.All;
        public ModelsaberSearchSort ModelSort { get; } = ModelsaberSearchSort.Newest;
        public string Search { get; } = string.Empty;
        public int Page { get; } = 0;

        public const int PageLength = 18;

        public ModelSaberSearch(ModelsaberSearchType type = ModelsaberSearchType.All, int page = 0, ModelsaberSearchSort sort = ModelsaberSearchSort.Newest, string search = "")
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
        Avatar = 4,
        Wall = 5,
        Effect = 6
    }

    public enum ModelsaberSearchSort
    {
        Newest = 0,
        Oldest = 1,
        Name = 2,
        Author = 3
    }
}