using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.ViewModels.Services;

public class ServiceCategoryVm
{
    public string CategoryId { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ServiceSubcategoryVm> Subcategories { get; set; } = new();
}

public class ServiceSubcategoryVm
{
    public string SubcategoryId { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ServiceItemVm> Items { get; set; } = new();
}

public class ServiceItemVm
{
    public string ItemId { get; set; } = "";
    public string Name { get; set; } = "";
}

