using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Backend.Infrastructure.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesItemController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public ServicesItemController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        /// <summary>
        /// 取得所有服務分類 (含中分類與服務項目)
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<List<ServiceCategoryVm>>> GetCategories(CancellationToken ct)
        {
            string lang = LanguageHelper.DetectLanguage(HttpContext);
            var repo = _uow.CreateServiceRepo();
            var categories = await repo.GetAllCategoriesAsync(lang,ct);
            return Ok(categories);
        }


    }
}
