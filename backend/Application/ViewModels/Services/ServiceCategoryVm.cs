using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.ViewModels.Services;

public class ServiceCategoryVm
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = "";
    public List<ServiceSubcategoryVm> Subcategories { get; set; } = new();
}

public class ServiceSubcategoryVm
{
    public Guid SubcategoryId { get; set; }
    public string Name { get; set; } = "";
    public List<ServiceItemVm> Items { get; set; } = new();
}

public class ServiceItemVm
{
    public Guid ItemId { get; set; }
    public string Name { get; set; } = "";
}
