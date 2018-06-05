namespace Server.Models
{
    public sealed class SearchParams
    {
        public SearchRequest Request { get; set; }
        public int? Take { get; set; }
        public int? Page { get; set; }
        public SearchSortRequest[] Sort { get; set; }
    }
}
