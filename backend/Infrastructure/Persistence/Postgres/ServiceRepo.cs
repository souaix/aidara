// Infrastructure/Persistence/Postgres/ServiceRepo.cs
using Backend.Application.Ports;
using Backend.Application.ViewModels.Services;
using Dapper;
using Npgsql;

public class ServiceRepo : IServiceRepo
{
    private readonly NpgsqlDataSource _ds;

    public ServiceRepo(NpgsqlDataSource ds)
    {
        _ds = ds;
    }

    public async Task<List<ServiceCategoryVm>> GetAllCategoriesAsync(CancellationToken ct)
    {
        const string sql = @"
            SELECT c.category_id, c.name AS category_name, c.sort_order,
                   s.subcategory_id, s.name AS subcategory_name, s.sort_order AS sub_sort,
                   i.item_id, i.name AS item_name, i.sort_order AS item_sort
            FROM service_category c
            LEFT JOIN service_subcategory s ON c.category_id = s.category_id
            LEFT JOIN service_item i ON s.subcategory_id = i.subcategory_id
            ORDER BY c.sort_order, s.sort_order, i.sort_order;
        ";

        using var conn = await _ds.OpenConnectionAsync(ct);
        var rows = await conn.QueryAsync(sql);

        var lookup = new Dictionary<Guid, ServiceCategoryVm>();

        foreach (var row in rows)
        {
            Guid catId = row.category_id;
            if (!lookup.TryGetValue(catId, out var catVm))
            {
                catVm = new ServiceCategoryVm
                {
                    CategoryId = catId,
                    Name = row.category_name
                };
                lookup.Add(catId, catVm);
            }

            if (row.subcategory_id != null)
            {
                var subVm = catVm.Subcategories.FirstOrDefault(x => x.SubcategoryId == row.subcategory_id);
                if (subVm == null)
                {
                    subVm = new ServiceSubcategoryVm
                    {
                        SubcategoryId = row.subcategory_id,
                        Name = row.subcategory_name
                    };
                    catVm.Subcategories.Add(subVm);
                }

                if (row.item_id != null)
                {
                    subVm.Items.Add(new ServiceItemVm
                    {
                        ItemId = row.item_id,
                        Name = row.item_name
                    });
                }
            }
        }

        return lookup.Values.ToList();
    }
}
