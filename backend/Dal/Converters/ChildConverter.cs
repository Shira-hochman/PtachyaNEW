using System;
using System.Collections.Generic;
using System.Linq;
using Dal.Models;
using Dto;

namespace Dal_Repository.ModelsConverters
{
    public static class ChildConverter
    {
        public static ChildDto ToChildDto(Child child)
        {
            return new ChildDto
            {
                ChildId = child.ChildId,
                KindergartenId = child.KindergartenId,
                IdNumber = child.IdNumber,
                BirthDate = child.BirthDate,
                LastName = child.lastName,
                FirstName = child.FirstName,
                SchoolYear = child.SchoolYear,
                Phone = child.Phone,
                Email = child.Email,

                // שליפת נתונים מתוך טבלת הגנים המקושרת
                KindergartenName = child.Kindergarten?.Name,
                // ⭐️ שורות חדשות שצריך להוסיף ⭐️
                KindergartenAddress = child.Kindergarten?.Address ?? "",
               
            };
        }

        public static Child ToChildEntity(ChildDto childDto)
        {
            return new Child
            {
                ChildId = childDto.ChildId,
                KindergartenId = childDto.KindergartenId,
                IdNumber = childDto.IdNumber,
                BirthDate = childDto.BirthDate,
                FirstName = childDto.FirstName,
                lastName = childDto.LastName,
                SchoolYear = childDto.SchoolYear,
                Phone = childDto.Phone,
                Email = childDto.Email
                // כתובת וסמל מוסד לא נשמרים בטבלת ילד, אלא בטבלת גן, לכן לא מעדכנים אותם כאן
            };
        }

        public static Child ToChildEntity(ParentChildImportDto importDto, int kindergartenId, int paymentId, string phone, string email)
        {
            return new Child
            {
                IdNumber = importDto.IdNumber,
                BirthDate = importDto.BirthDate!.Value,
                FirstName = importDto.FirstName,
                lastName = importDto.LastName,
                SchoolYear = importDto.SchoolYear,
                KindergartenId = kindergartenId,
                Phone = phone,
                Email = email
            };
        }

        public static List<ChildDto> ToChildDtoList(List<Child> children)
        {
            return children.Select(ToChildDto).ToList();
        }

        public static void UpdateChildEntity(Child existingChild, ChildDto childDto)
        {
            existingChild.KindergartenId = childDto.KindergartenId;
            existingChild.IdNumber = childDto.IdNumber;
            existingChild.BirthDate = childDto.BirthDate;
            existingChild.FirstName = childDto.FirstName;
            existingChild.lastName = childDto.LastName;
            existingChild.SchoolYear = childDto.SchoolYear;
            existingChild.Phone = childDto.Phone;
            existingChild.Email = childDto.Email;
        }
    }
}