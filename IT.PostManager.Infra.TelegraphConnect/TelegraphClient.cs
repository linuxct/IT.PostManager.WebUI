using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IT.PostManager.Core.Contracts;
using IT.PostManager.Core.Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegraph.Net;
using Telegraph.Net.Models;

namespace IT.PostManager.Infra.TelegraphConnect
{
    public class TelegraphClient : ITelegraphClient
    {
        private readonly Telegraph.Net.TelegraphClient _client;
        private readonly ITokenClient _secureClient;
        private readonly ILogger<TelegraphClient> _logger;
        private readonly ICoreLogicService _coreLogicService;
        private readonly IConfiguration _configuration;

        public TelegraphClient(ILogger<TelegraphClient> logger, ICoreLogicService coreLogicService, IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new Telegraph.Net.TelegraphClient();
            _secureClient = _client.GetTokenClient(_configuration.GetSection("TelegraphApiKey").Value);
            _logger = logger;
            _coreLogicService = coreLogicService;
        }

        public async Task<PageList> GetPageListFromTelegraph(int offset)
        {
            try
            {
                var result = await _secureClient.GetPageListAsync(offset: offset, limit: 10);
                return result;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("FLOOD_WAIT_"))
                {
                    _logger.LogError("Error while retrieving the posts from the API {0}, waiting 12 seconds.",
                        e.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(12));
                }
            }

            return null;
        }
        
        public async Task<Page> GetPage(string path)
        {
            var page = await _client.GetPageAsync(path, returnContent: true);
            return page;
        }

        public async Task<TelegraphPostInnerDataDto> GetPageDataForPath(string path)
        {
            var page = await _client.GetPageAsync(path, returnContent: true);
            var result = _coreLogicService.ExtractInnerDataFromPage(page);
            return result;
        }
        
        public TelegraphPostInnerDataDto GetPageDataForPage(Page page)
        {
            var result = _coreLogicService.ExtractInnerDataFromPage(page);
            return result;
        }

        public async Task<bool> StoreDateForPath(DateTimeOffset date, string path)
        {
            var page = await _client.GetPageAsync(path, returnContent: true);
            var pageNodeList = page.Content;
            var result = _coreLogicService.ExtractInnerDataFromPage(page) ?? new TelegraphPostInnerDataDto();
            result.PostDate = date;
            var serializedResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            
            if (pageNodeList.Any(node => node.Tag == "code"))
            {
                pageNodeList.RemoveAll(x => x.Tag == "code");
            }

            var pageTitle = page.Title.ToTitleWithDate(date);
            if (pageTitle.Length >= 256)
            {
                pageTitle = pageTitle.Substring(0, 255);
            }
            
            pageNodeList.Add(new NodeElement("code", null, serializedResult));
            var editedPage = await _secureClient.EditPageAsync(
                page.Path,
                pageTitle,
                pageNodeList.ToArray(),
                page.AuthorName,
                page.AuthorUrl,
                true
            );
            
            return editedPage.Content.Any(x=>x.Tag=="code" && x.Children.Any(y=>y.Attributes["value"] == serializedResult));
        }
        
        public async Task<bool> UpdateForPath(string path, string content)
        {
            var page = await _client.GetPageAsync(path, returnContent: true);
            var pageNodeList = page.Content;
            var innerData = _coreLogicService.ExtractInnerDataFromPage(page) ?? new TelegraphPostInnerDataDto();
            var serializedInnerData = JsonSerializer.Serialize(innerData,  new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var updatedContent = content.Split("\r\n").ToList();
            updatedContent.RemoveAll(string.IsNullOrWhiteSpace);
            var result = new List<NodeElement>();
            foreach (var paragraph in updatedContent)
            {
                result.Add(paragraph);
                result.Add(new NodeElement("br", null));
            }
            
            if (result.Any(node => node.Tag == "code"))
            {
                result.RemoveAll(x => x.Tag == "code");
            }
            result.Add(new NodeElement("code", null, serializedInnerData));
            
            var editedPage = await _secureClient.EditPageAsync(
                page.Path,
                page.Title,
                result.ToArray(),
                page.AuthorName,
                page.AuthorUrl,
                true
            );
            
            return editedPage.Content.Any(x=>x.Tag=="code" && x.Children.Any(y=>y.Attributes["value"] == serializedInnerData));
        }
        
        public async Task<bool> DisableForPath(string path)
        {
            var page = await _client.GetPageAsync(path, returnContent: true);
            var pageNodeList = page.Content;
            var result = _coreLogicService.ExtractInnerDataFromPage(page) ?? new TelegraphPostInnerDataDto();
            result.Disabled = true;
            var serializedResult = JsonSerializer.Serialize(result,  new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            
            if (pageNodeList.Any(node => node.Tag == "code"))
            {
                pageNodeList.RemoveAll(x => x.Tag == "code");
            }
            pageNodeList.Add(new NodeElement("code", null, serializedResult));
            var editedPage = await _secureClient.EditPageAsync(
                page.Path,
                page.Title,
                pageNodeList.ToArray(),
                page.AuthorName,
                page.AuthorUrl,
                true
            );
            
            return editedPage.Content.Any(x=>x.Tag=="code" && x.Children.Any(y=>y.Attributes["value"] == serializedResult));
        }

    }
}