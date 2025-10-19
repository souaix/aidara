// Infrastructure/Persistence/Postgres/ServiceRepo.cs
using System.Data;
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Dapper;

namespace Backend.Infrastructure.Persistence.Postgres;

public sealed class ServiceRepo : IServiceRepo
{
    public async Task<List<ServiceCategoryVm>> GetAllCategoriesAsync(
        IDbConnection conn,
        IDbTransaction? tx,
        string lang,
        CancellationToken ct)
    {
        const string sql = """
            SELECT 
                category_id,
                category_name,
                subcategory_id,
                subcategory_name,
                item_id,
                item_name,
                category_sort,
                subcategory_sort,
                item_sort
            FROM vw_service_hierarchy(@lang)
            ORDER BY category_sort, subcategory_sort, item_sort;
        """;

        var rows = await conn.QueryAsync(
            new CommandDefinition(sql, new { lang }, tx, cancellationToken: ct));

        // ⚙️ 建立分層結構
        var lookup = new Dictionary<string, ServiceCategoryVm>();

        foreach (var row in rows)
        {
            string catId = row.category_id;
            if (!lookup.TryGetValue(catId, out var catVm))
            {
                catVm = new ServiceCategoryVm
                {
                    CategoryId = catId,
                    Name = row.category_name ?? "",
                    Subcategories = new List<ServiceSubcategoryVm>()
                };
                lookup.Add(catId, catVm);
            }

            if (row.subcategory_id != null)
            {
                var subVm = catVm.Subcategories
                    .FirstOrDefault(x => x.SubcategoryId == (string)row.subcategory_id);
                if (subVm == null)
                {
                    subVm = new ServiceSubcategoryVm
                    {
                        SubcategoryId = row.subcategory_id,
                        Name = row.subcategory_name ?? "",
                        Items = new List<ServiceItemVm>()
                    };
                    catVm.Subcategories.Add(subVm);
                }

                if (row.item_id != null)
                {
                    subVm.Items.Add(new ServiceItemVm
                    {
                        ItemId = row.item_id,
                        Name = row.item_name ?? ""
                    });
                }
            }
        }

        return lookup.Values.ToList();
    }
}
