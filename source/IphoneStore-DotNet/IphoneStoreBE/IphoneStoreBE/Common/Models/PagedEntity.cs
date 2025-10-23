namespace IphoneStoreBE.Common.Models
{
    public class PagedEntity<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        public PagedEntity()
        {
        }

        public PagedEntity(IEnumerable<T> items, int currentPage, int pageSize)
        {
            var itemList = items.ToList();
            TotalItems = itemList.Count;
            PageSize = pageSize;
            PageIndex = currentPage;
            TotalPages = (int)Math.Ceiling(TotalItems / (double)pageSize);

            Items = itemList
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            HasPreviousPage = PageIndex > 1;
            HasNextPage = PageIndex < TotalPages;
        }
    }
}
