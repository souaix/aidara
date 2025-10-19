// Application/Services/Customers/CustomerServiceQuestionnaireService.cs
using Backend.Application.Ports;
using Backend.Application.Shared;
using Backend.Application.ViewModels.Services;

namespace Backend.Application.Services.Customers;

public class CustomerServiceQuestionnaireService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly ICustomerServiceQuestionnaireRepo _questionnaireRepo;

    public CustomerServiceQuestionnaireService(
        IUnitOfWorkFactory uowFactory,
        ICustomerServiceQuestionnaireRepo questionnaireRepo)
    {
        _uowFactory = uowFactory;
        _questionnaireRepo = questionnaireRepo;
    }

    public async Task SubmitAsync(Guid userId, CustomerServiceQuestionnaireDto dto, CancellationToken ct)
    {
        await using var uow = await _uowFactory.BeginAsync(withTransaction: true, ct);

        try
        {
            await _questionnaireRepo.SubmitAsync(uow.Connection, uow.Transaction, userId, dto, ct);
            await uow.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync(ct);
            Console.WriteLine($"❗CustomerServiceQuestionnaireService Submit Failed: {ex.Message}");
            throw;
        }
    }
}
