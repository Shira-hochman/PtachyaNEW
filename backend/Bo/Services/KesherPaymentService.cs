using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dto;
using Microsoft.Extensions.Configuration;
using Bo.Interfaces;

namespace Bo.Services
{
    public class KesherPaymentService : IPaymentService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public KesherPaymentService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;

            // Force TLS 1.2 for Kesher
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        }

        /// <summary>
        /// Process payment using Kesher API
        /// </summary>
        /// <param name="request">Payment request with amount, card details, number of payments</param>
        /// <returns>Success flag, message, confirmation code</returns>
        public async Task<(bool Success, string Message, string ConfirmationCode)>
            ProcessPaymentAsync(PaymentRequestDTO request)
        {
            var client = _httpClientFactory.CreateClient();
            var url = _config["KesherSettings:ApiUrl"];

            // --- קביעת סוג התשלום לפי מספר תשלומים ---
            // CreditType:
            // 1 – אשראי רגיל
            // 3 – חיוב מיידי
            // 8 – תשלומים
            // 10 – הוראת קבע
            int creditType = request.NumPayments > 1 ? 8 : 3; // ברירת מחדל: חיוב מיידי
            string paramJ = request.NumPayments > 1 ? "J4" : "J5";

            int totalAmount = (int)Math.Round(request.Amount * 100);
            int numPayments = request.NumPayments > 0 ? request.NumPayments : 1;
            int firstPayment = totalAmount;

            if (creditType == 8 && numPayments > 1)
            {
                int basePayment = totalAmount / numPayments;
                int remainder = totalAmount - (basePayment * numPayments);
                firstPayment = basePayment + remainder;
            }

            var tranObj = new
            {
                CreditNum = request.CardNumber,
                Expiry = request.Expiry,
                Total = totalAmount,
                Currency = 1,
                CreditType = creditType,
                ParamJ = paramJ,
                TransactionType = "debit",
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Mail = request.Email,
                ProjectNumber = "00001",
                UniqNum = Guid.NewGuid().ToString("N")[..8],
                NumPayment = creditType == 8 ? numPayments - 1 : (int?)null,
                FirstPayment = creditType == 8 ? firstPayment : (int?)null
            };


            var body = new
            {
                Json = new
                {
                    userName = _config["KesherSettings:UserName"],
                    password = _config["KesherSettings:Password"],
                    func = "SendTransaction",
                    format = "json",
                    tran = tranObj
                },
                format = "json"
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return (false, $"Kesher HTTP Error {(int)response.StatusCode}: {responseString}", null);

                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                if (!root.TryGetProperty("RequestResult", out var result))
                    return (false, "תגובה לא תקינה מ־Kesher (חסר RequestResult)", null);

                bool success = result.GetProperty("Status").GetBoolean();
                string description = result.GetProperty("Description").GetString();
                string okNum = root.TryGetProperty("OKNum", out var ok) ? ok.GetString() : null;

                return (success, description, okNum);
            }
            catch (Exception ex)
            {
                return (false, "שגיאת תקשורת: " + ex.Message, null);
            }
        }
    }
}
