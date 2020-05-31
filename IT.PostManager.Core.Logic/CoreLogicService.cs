using System;
using System.Linq;
using System.Text.Json;
using IT.PostManager.Core.Contracts;
using Telegraph.Net.Models;

namespace IT.PostManager.Core.Logic
{
    public class CoreLogicService : ICoreLogicService
    {
        public TelegraphPostInnerDataDto ExtractInnerDataFromPage(Page page)
        {
            try
            {
                var el = page.Content.Find(e => e.Tag == "code");
                if (el == null) return null;
                if (el.Children.Any(y => y.Attributes["value"] != null))
                {
                    var json = el.Children.First(x => x.Attributes["value"] != null)?.Attributes["value"];
                    return JsonSerializer.Deserialize<TelegraphPostInnerDataDto>(json, new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}