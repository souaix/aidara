using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.ViewModels.Services
{
    public record StoreVm(
        Guid UserId,
        string DisplayName,
        string AvatarUrl,
        Guid ItemId,
        string ItemName,
        string[] Methods,
        decimal AvgScore,
        string CityId,
        string DistrictId
    );
}
