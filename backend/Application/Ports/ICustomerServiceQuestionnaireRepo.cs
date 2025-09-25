public interface ICustomerServiceQuestionnaireRepo
{
    Task SubmitAsync(CustomerServiceQuestionnaireDto dto, CancellationToken ct);
}
