using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IT.PostManager.Infra.TelegraphConnect;
using IT.PostManager.WebUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IT.PostManager.WebUI.Controllers
{
    public class OperationController : Controller
    {
        private readonly SignInManager<IdentityUser> _manager;
        private readonly ILogger<OperationController> _logger;
        private readonly ITelegraphClient _telegraphClient;

        public OperationController(SignInManager<IdentityUser> manager, ILogger<OperationController> logger, ITelegraphClient telegraphClient)
        {
            _manager = manager;
            _logger = logger;
            _telegraphClient = telegraphClient;
        }

        #region Get Methods

        public async Task<IActionResult> Pages(int offset = 0)
        {
            if (!_manager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            return View(new PagesListViewModel{TelegraphContents = await _telegraphClient.GetPageListFromTelegraph(offset), Offset = offset});
        }
        
        public async Task<IActionResult> ModifyPage(string hashpath)
        {
            if (!_manager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            var telegraphContents = await _telegraphClient.GetPage(GetStringFromBase64(hashpath));
            var innerData = _telegraphClient.GetPageDataForPage(telegraphContents);
            if (innerData != null && innerData.Disabled) return EnqueueError("The requested page was already deleted.", RedirectToAction("Pages"));
            return View(new ModifyPageViewModel{TelegraphContents = telegraphContents, InnerData = innerData, HashPath = hashpath});
        }
        
        public async Task<IActionResult> PageDate(string hashpath)
        {
            if (!_manager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            var clientResult = await _telegraphClient.GetPageDataForPath(GetStringFromBase64(hashpath));
            return View(new PageDateViewModel{CurrentDateSet = clientResult?.PostDate ?? DateTimeOffset.Now, HashPath = hashpath});
        }

        public async Task<IActionResult> GetPostDataAjax(string hashpath)
        {
            if (!_manager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            var clientResult = await _telegraphClient.GetPageDataForPath(GetStringFromBase64(hashpath));
            if (clientResult != null)
            {
                var postDate = clientResult.PostDate.Date.ToString("yyyy-MM-dd");
                return Json(new {date = postDate, disabled = clientResult.Disabled});
            }

            return StatusCode(500);
        }

        #endregion

        #region Post Methods

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StorePageDate(DateTimeOffset date, string hashpath)
        {
            if (await _telegraphClient.StoreDateForPath(date, GetStringFromBase64(hashpath)))
            {
                return EnqueueSuccess("Changes saved successfully", RedirectToAction("Pages"));
            }
            return EnqueueError("An error has occurred. Changes were not saved.", RedirectToAction("Pages"));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePage(string hashpath)
        {
            if (await _telegraphClient.DisableForPath(GetStringFromBase64(hashpath)))
            {
                return EnqueueSuccess("Page deleted successfully", RedirectToAction("Pages"));
            }
            return EnqueueError("An error has occurred. Changes were not saved.", RedirectToAction("Pages"));
        }
        
        public async Task<IActionResult> UpdatePageContent(string hashpath, string content)
        {
            if (await _telegraphClient.UpdateForPath(GetStringFromBase64(hashpath), content))
            {
                return EnqueueSuccess("Page updated successfully", RedirectToAction("Pages"));
            }
            return EnqueueError("An error has occurred. Changes were not saved.", RedirectToAction("Pages"));
        }

        #endregion
        
        #region Private Methods
        private static string GetStringFromBase64(string hash)
        {
            var base64EncodedBytes = Convert.FromBase64String(hash);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        
        private IActionResult EnqueueSuccess(string message, IActionResult actionResult)
        {
            TempData["successMessage"] = message;
            return actionResult;
        }
        
        private IActionResult EnqueueError(string message, IActionResult actionResult)
        {
            TempData["error"] = message;
            return actionResult;
        }
        #endregion

        
    }
}