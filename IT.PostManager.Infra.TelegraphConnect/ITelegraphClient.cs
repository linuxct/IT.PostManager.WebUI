using System;
using System.Threading.Tasks;
using IT.PostManager.Core.Contracts;
using Telegraph.Net.Models;

namespace IT.PostManager.Infra.TelegraphConnect
{
    public interface ITelegraphClient
    {
        public Task<PageList> GetPageListFromTelegraph(int offset);
        public Task<Page> GetPage(string path);
        public Task<TelegraphPostInnerDataDto> GetPageDataForPath(string path);
        public TelegraphPostInnerDataDto GetPageDataForPage(Page page);
        public Task<bool> StoreDateForPath(DateTimeOffset date, string path);
        public Task<bool> DisableForPath(string path);
        public Task<bool> UpdateForPath(string getStringFromBase64, string content);
    }
}