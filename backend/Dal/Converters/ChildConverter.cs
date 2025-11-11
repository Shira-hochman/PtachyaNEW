using System;
using System.Collections.Generic;
using System.Linq;
using Dal.Models; // שימוש ב-Dal.Models
using Dto; // שימוש ב-Dto

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
                //LastName = child.FullName,
                SchoolYear = child.SchoolYear,
                FormLink = child.FormLink,
                Phone = child.Phone,
                Email = child.Email
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
                //FullName = childDto.FullName,
                SchoolYear = childDto.SchoolYear,
                FormLink = childDto.FormLink,
                Phone = childDto.Phone,
                Email = childDto.Email
            };
        }

       
        public static Child ToChildEntity(ParentChildImportDto importDto, int kindergartenId, int paymentId, string phone, string email)
        {
 
            return new Child
            {
                IdNumber = importDto.IdNumber,
                //FullName = importDto.FullName,
                BirthDate = importDto.BirthDate!.Value,
                SchoolYear = importDto.SchoolYear,
                FormLink = importDto.FormLink,
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
            //existingChild.FullName = childDto.FullName;
            existingChild.SchoolYear = childDto.SchoolYear;
            existingChild.FormLink = childDto.FormLink;
            existingChild.Phone = childDto.Phone;
            existingChild.Email = childDto.Email;
        }
    }
}