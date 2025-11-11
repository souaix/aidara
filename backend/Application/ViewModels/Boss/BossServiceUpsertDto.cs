using Backend.Application.ViewModels.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.ViewModels.Boss;

public class BossServiceUpsertDto
{
    public Guid UserId { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public int MinPrice { get; set; }
    public int MaxPrice { get; set; }

    public List<ServiceAreaDto> ServiceAreas { get; set; } = new();
}

