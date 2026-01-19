using Dto;
using System.Threading.Tasks;

namespace Bo.Interfaces
{
    // חובה להגדיר כ-interface ולא כ-class
    // חובה להוסיף public כדי שפרויקטים אחרים (כמו ה-API) יוכלו לראות אותו
    public interface IPaymentService
    {
        Task<(bool Success, string Message, string ConfirmationCode)> ProcessPaymentAsync(PaymentRequestDTO request);
    }
}