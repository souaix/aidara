// Infrastructure/Persistence/Mock/MockServiceRepo.cs
using System.Data;
using Backend.Application.Ports.Service;
using Backend.Application.ViewModels.Services;

namespace Backend.Infrastructure.Persistence.Mock;

public sealed class MockServiceRepo : IServiceRepo
{
    public Task<List<ServiceCategoryVm>> GetAllCategoriesAsync(
        IDbConnection conn,
        IDbTransaction? tx,
        string lang,
        CancellationToken ct)
    {
        // 模擬語言切換
        bool isZh = lang.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
        bool isZhCn = lang.Equals("zh-cn", StringComparison.OrdinalIgnoreCase);

        string clean = isZhCn ? "清洁服务" : (isZh ? "清潔服務" : "Cleaning Services");
        string design = isZhCn ? "设计装修" : (isZh ? "設計裝潢" : "Design & Interior");
        string repair = isZhCn ? "维修服务" : (isZh ? "修繕服務" : "Repair & Maintenance");

        var list = new List<ServiceCategoryVm>
        {
            new()
            {
                CategoryId = "CLEANING",
                Name = clean,
                Subcategories = new()
                {
                    new()
                    {
                        SubcategoryId = "HOME_CLEAN",
                        Name = isZhCn ? "家庭清洁" : (isZh ? "居家清潔" : "Home Cleaning"),
                        Items = new()
                        {
                            new() { ItemId = "CLEAN01", Name = isZhCn ? "地板打蜡" : (isZh ? "地板打蠟" : "Floor Polishing") },
                            new() { ItemId = "CLEAN02", Name = isZhCn ? "厨房清洁" : (isZh ? "廚房清潔" : "Kitchen Cleaning") }
                        }
                    },
                    new()
                    {
                        SubcategoryId = "OFFICE_CLEAN",
                        Name = isZhCn ? "办公室清洁" : (isZh ? "辦公清潔" : "Office Cleaning"),
                        Items = new()
                        {
                            new() { ItemId = "CLEAN03", Name = isZhCn ? "玻璃清洁" : (isZh ? "玻璃清潔" : "Window Cleaning") }
                        }
                    }
                }
            },
            new()
            {
                CategoryId = "DESIGN",
                Name = design,
                Subcategories = new()
                {
                    new()
                    {
                        SubcategoryId = "INTERIOR",
                        Name = isZhCn ? "室内设计" : (isZh ? "室內設計" : "Interior Design"),
                        Items = new()
                        {
                            new() { ItemId = "DES01", Name = isZhCn ? "空间设计" : (isZh ? "空間設計" : "Space Design") }
                        }
                    }
                }
            },
            new()
            {
                CategoryId = "REPAIR",
                Name = repair,
                Subcategories = new()
                {
                    new()
                    {
                        SubcategoryId = "PLUMBING",
                        Name = isZhCn ? "管道维修" : (isZh ? "水管維修" : "Plumbing"),
                        Items = new()
                        {
                            new() { ItemId = "REP01", Name = isZhCn ? "管道修复" : (isZh ? "水管修復" : "Pipe Fixing") }
                        }
                    },
                    new()
                    {
                        SubcategoryId = "ELECTRIC",
                        Name = isZhCn ? "电力维修" : (isZh ? "電力維修" : "Electrical"),
                        Items = new()
                        {
                            new() { ItemId = "REP02", Name = isZhCn ? "电线检修" : (isZh ? "電線檢修" : "Wiring Check") }
                        }
                    }
                }
            }
        };

        return Task.FromResult(list);
    }
}
