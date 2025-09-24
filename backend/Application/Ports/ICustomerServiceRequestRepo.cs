using System.Threading;
using System.Threading.Tasks;

public interface ICustomerServiceRequestRepo
{
    Task InsertAsync(CustomerServiceRequestDto dto, CancellationToken ct);
}
