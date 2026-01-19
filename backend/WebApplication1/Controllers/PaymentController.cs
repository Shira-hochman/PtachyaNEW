using Bo.Interfaces; // היכן שהממשק נמצא עכשיו
using Bo.Services;
using Dal.Models;
using Dto;
using Microsoft.AspNetCore.Mvc;
using Dal.Repositories.Interfaces; // ודאי שהשורה הזו קיימת
using Dal.Repositories;            // במידה וצריך

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRepository _repository;

    public PaymentController(IPaymentService paymentService, IPaymentRepository repository)
    {
        _paymentService = paymentService;
        _repository = repository;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ExecutePayment([FromBody] PaymentRequestDTO request)
    {
        // 1. ביצוע הסליקה בפועל מול "קשר"
        var result = await _paymentService.ProcessPaymentAsync(request);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        // 2. שמירת התיעוד ב-DB (המודל ששלחת קודם)
        var paymentRecord = new Payment
        {
            ChildId = request.ChildId,
            Amount = request.Amount,
            Status = "Success",
            PaymentDate = DateTime.Now
            // ניתן להוסיף כאן ConfirmationCode = result.ConfirmationCode
        };

        await _repository.AddAsync(paymentRecord); // תקין - לפי הממשק שלך

        return Ok(new { message = result.Message, authCode = result.ConfirmationCode });
    }
}