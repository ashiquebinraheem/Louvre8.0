namespace Louvre.Shared.Models
{
    //   public class PagedList<T> 
    //   {
    //	public int PageIndex { get; set; }
    //	public int PageSize { get; set; }
    //	public int TotalCount { get; set; }
    //	public int TotalPages { get; set; }

    //	public bool HasPreviousPage => PageIndex > 1;
    //	public bool HasNextPage => PageIndex < TotalPages;
    //	public List<T> Data { get; set; }

    //	public PagedList(IEnumerable<T> items, int count, int pageIndex = 0, int pageSize = int.MaxValue)
    //	{
    //		PageSize = pageSize;
    //		PageIndex = pageIndex <= 1 ? 1 : pageIndex;

    //		TotalCount = count;
    //		TotalPages = count / pageSize;
    //		if (count % pageSize > 0)
    //		{
    //			TotalPages++;
    //		}
    //		//AddRange(items);
    //		Data = items.ToList();
    //	}
    //}

    //public class PagedListSearchPostModel
    //{
    //	public PagedListSearchPostModel()
    //	{
    //		PageIndex = 1;
    //		PageSize = 10;
    //		OrderByFieldName = "1";
    //		SearchOperator = "like";
    //	}

    //	public int PageIndex { get; set; }
    //	public int PageSize { get; set; }
    //	public int TotalPages { get; set; }

    //	public string? SearchColumnName { get; set; }
    //	public string? SearchString { get; set; }
    //	public string? OrderByFieldName { get; set; }
    //       public string? SearchOperator { get; set; }
    //       public string? WhereCondition { get; set; }
    //   }


    public class SearchByViewModel
    {
        public SearchByViewModel(string columnName, string displayName, string width = "0px", bool haveSearchOption = true)
        {
            ColumnName = columnName;
            DisplayName = displayName;
            HaveSearchOption = haveSearchOption;
            Width = width;
        }
        public string? ColumnName { get; set; }
        public string? DisplayName { get; set; }
        public bool HaveSearchOption { get; set; }
        public string? Width { get; set; }
    }

}
