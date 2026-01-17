using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dto;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;


namespace Bo.Services
{
   

    public interface IPaymentService
    {
        Task<(bool Success, string Message, string ConfirmationCode)> ProcessPaymentAsync(PaymentRequestDTO request);
    }

    public class KesherPaymentService : IPaymentService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public KesherPaymentService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<(bool Success, string Message, string ConfirmationCode)> ProcessPaymentAsync(PaymentRequestDTO request)
        {
            var client = _httpClientFactory.CreateClient();

            // משיכת פרטי ההתחברות מהקונפיגורציה
            var terminal = _config["KesherSettings:TerminalId"];
            var user = _config["KesherSettings:UserName"];
            var pass = _config["KesherSettings:Password"];
            var url = _config["KesherSettings:ApiUrl"];

            // בניית הבקשה לפי הפרוטוקול של "קשר"
            var paymentBody = new
            {
                Terminal = terminal,
                UserName = user,
                Password = pass,
                Amount = request.Amount,
                CardNumber = request.CardNumber,
                Expiry = request.Expiry.Replace("/", ""), // הסרת / במידה וקיים
                CVV = request.Cvv,
                Id = request.HolderId,
                CustomerName = request.HolderName
            };

            var json = JsonSerializer.Serialize(paymentBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // כאן מנתחים את התשובה של קשר (בדרך כלל Success או קוד 000)
                    // לדוגמה:
                    return (true, "התשלום בוצע בהצלחה", "123456");
                }

                return (false, "סירוב מחברת האשראי או שגיאת תקשורת", null);
            }
            catch (Exception ex)
            {
                // לוג שגיאה למערכת
                return (false, "שגיאה פנימית בתהליך הסליקה", null);
            }
        }
    }
}
