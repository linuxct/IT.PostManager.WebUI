using IT.PostManager.Core.Contracts;
using Telegraph.Net.Models;

namespace IT.PostManager.Core.Logic
{
    public interface ICoreLogicService
    {
        public TelegraphPostInnerDataDto ExtractInnerDataFromPage(Page page);
    }
}