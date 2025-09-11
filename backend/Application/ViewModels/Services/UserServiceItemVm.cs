using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.ViewModels.Services;

public class UserServiceItemVm
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = "";
    public Guid SubcategoryId { get; set; }
    public string SubcategoryName { get; set; } = "";
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
}

