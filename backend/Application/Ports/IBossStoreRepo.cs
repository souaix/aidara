using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.ViewModels.Services;
namespace Backend.Application.Ports
{
    public interface IBossStoreRepo
    {
        Task<List<StoreVm>> GetStoresByRegionAsync(Guid itemId, string cityId, string? districtId, CancellationToken ct);
        Task<StoreDetailVm?> GetStoreDetailAsync(Guid userId, CancellationToken ct);
    }

}
