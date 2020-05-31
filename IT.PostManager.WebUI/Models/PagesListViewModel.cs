using Telegraph.Net.Models;

namespace IT.PostManager.WebUI.Models
{
    public class PagesListViewModel
    {
        public PageList TelegraphContents { get; set; }
        public int Offset { get; set; }
        public readonly int DefaultPageSize = 10;
    }
}