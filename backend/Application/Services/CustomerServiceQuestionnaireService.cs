using Backend.Application.Ports;

public class CustomerServiceQuestionnaireService
{
    private readonly IUnitOfWork _uow;
    public CustomerServiceQuestionnaireService(IUnitOfWork uow) => _uow = uow;

    public async Task SubmitAsync(Guid userId, CustomerServiceQuestionnaireDto dto, CancellationToken ct)
    {
        await _uow.BeginAsync(ct);
        try
        {
            var repo = _uow.CreateCustomerServiceQuestionnaireRepo();
            await repo.SubmitAsync(userId, dto, ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }
}
