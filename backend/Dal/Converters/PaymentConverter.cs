using Dal.Models;
using Dto;
using System.Linq;
using System.Collections.Generic;

namespace Dal.Converters
{
    public static class PaymentConverter
    {
        // הפיכת ישות מה-DB ל-DTO (בשביל GetAllAsync ברפוזיטורי)
        public static PaymentDto ToPaymentDto(Payment entity)
        {
            if (entity == null) return null;
            return new PaymentDto
            {
                PaymentId = entity.PaymentId,
                ChildId = entity.ChildId,
                Amount = entity.Amount,
                Status = entity.Status,
                PaymentDate = entity.PaymentDate
            };
        }

        // הפיכת DTO לישות DB (בשביל שמירה)
        public static Payment ToPaymentEntity(PaymentDto dto)
        {
            if (dto == null) return null;
            return new Payment
            {
                PaymentId = dto.PaymentId,
                ChildId = dto.ChildId,
                Amount = dto.Amount,
                Status = dto.Status,
                PaymentDate = dto.PaymentDate
            };
        }
    }
}