using IT.PostManager.Core.Contracts;
using Telegraph.Net.Models;

namespace IT.PostManager.WebUI.Models
{
    public class ModifyPageViewModel
    {
        public Page TelegraphContents { get; set; }
        public TelegraphPostInnerDataDto InnerData { get; set; }
        public string HashPath { get; set; }
    }
}