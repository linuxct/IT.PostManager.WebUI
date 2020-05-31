using System;

namespace IT.PostManager.WebUI.Models
{
    public class PageDateViewModel
    {
        public string PostName { get; set; }
        public string HashPath { get; set; }
        public DateTimeOffset CurrentDateSet { get; set; }
    }
}