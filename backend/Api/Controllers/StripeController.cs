using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Threading;

[ApiController]
[Route("api/[controller]")]
public class StripeController : ControllerBase
{
    public StripeController()
    {
        // StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
        // 如果你在 Program.cs 已經設定，可以不用在這裡重設
    }

    // 建立一個 SetupIntent 並回傳 client_secret 給前端
    [HttpPost("create-setup-intent")]
    public ActionResult CreateSetupIntent()
    {
        // 1) 設定 API key（在真實專案請在 Program.cs 或 DI 設定）
        StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
            ?? "sk_test_51R6D8iCEyd5yyrHJzXBxxxRmHrwo32tLa2hHpXA4GQ9eFBVVpxRcbFkwrWmf3YSEWLZ0VvAVZJkFcygJ7TCuTXpm00AckSQnJN"; // 測試用 fallback（建議不要放在程式碼）

        var options = new SetupIntentCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            // 若你要直接綁定到 Customer，先建立/指定 customer id:
            // Customer = "cus_XXXX",
            Usage = "off_session" // 或 "on_session" 視業務需求
        };
        var service = new SetupIntentService();
        var setupIntent = service.Create(options);

        return Ok(new { clientSecret = setupIntent.ClientSecret });
    }

    // （選用）當前端成功後，你會拿到 payment_method id，若需要在後端把它 attach 到 customer：
    [HttpPost("attach-payment-method")]
    public ActionResult AttachPaymentMethod([FromBody] AttachPayload p)
    {
        StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? "sk_test_51R6D8iCEyd5yyrHJzXBxxxRmHrwo32tLa2hHpXA4GQ9eFBVVpxRcbFkwrWmf3YSEWLZ0VvAVZJkFcygJ7TCuTXpm00AckSQnJN";

        if (string.IsNullOrEmpty(p.CustomerId) || string.IsNullOrEmpty(p.PaymentMethodId))
            return BadRequest("customerId / paymentMethodId required");

        var service = new PaymentMethodService();
        // attach payment method to customer
        var pm = service.Attach(p.PaymentMethodId, new PaymentMethodAttachOptions { Customer = p.CustomerId });

        // 也可以在這紀錄到你 DB: user ↔ payment_method id
        return Ok(pm);
    }

    public record AttachPayload(string CustomerId, string PaymentMethodId);
}
