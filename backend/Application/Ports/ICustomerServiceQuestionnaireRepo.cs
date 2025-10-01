public interface ICustomerServiceQuestionnaireRepo
{
    Task SubmitAsync(Guid userId, CustomerServiceQuestionnaireDto dto, CancellationToken ct);
}
