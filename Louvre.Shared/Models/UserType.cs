namespace Louvre.Shared.Models
{
    public class PagePermissionPostViewModel
    {
        public int PageID { get; set; }
        public int? PagePermissionID { get; set; }
        public string? PageName { get; set; }
        public bool Have { get; set; }
    }
}
