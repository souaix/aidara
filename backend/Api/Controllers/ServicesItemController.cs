// Backend.Api/Controllers/ServicesItemController.cs
using Backend.Application.Ports;
using Backend.Application.Shared;
using Backend.Application.ViewModels.Services;
using Backend.Infrastructure.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesItemController : ControllerBase
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IServiceRepo _serviceRepo;

        public ServicesItemController(IUnitOfWorkFactory uowFactory, IServiceRepo serviceRepo)
        {
            _uowFactory = uowFactory;
            _serviceRepo = serviceRepo;
        }

        /// <summary>
        /// 取得所有服務分類 (含中分類與服務項目)
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<List<ServiceCategoryVm>>> GetCategories(CancellationToken ct)
        {
            Console.WriteLine("START");
            // 取得語系
            var lang = LanguageHelper.DetectLanguage(HttpContext);

            Console.WriteLine("LANG:"+lang);
            // 開啟 UoW (查詢不需交易)
            await using var uow = await _uowFactory.BeginAsync(withTransaction: false, ct);
            Console.WriteLine("UOW DONE");
            // 查詢資料
            var categories = await _serviceRepo.GetAllCategoriesAsync(uow.Connection, uow.Transaction, lang, ct);
            Console.WriteLine("CAT:"+ categories);
            return Ok(categories);
        }
    }
}
