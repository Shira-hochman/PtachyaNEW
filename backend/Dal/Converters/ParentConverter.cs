//using Dal.Models;
//using Dto;
//using System.Collections.Generic;
//using System.Linq;

//namespace Dal.Converters
//{
//    // *** יש לוודא שהמחלקה היא public static כדי להשתמש בה בקלות ***
//    public static class ParentConverter
//    {
//        // המרה מיישות (Entity) ל-DTO
//        public static Dto.ParentDto ToParentDto(Dal.Models.Parent parent)
//        {
//            return new Dto.ParentDto
//            {
//                ParentId = parent.ParentId,
//                FullName = parent.FullName,
//                Phone = parent.Phone,
//                Email = parent.Email
//            };
//        }

//        // המרה מ-DTO ליישות (Entity) - לשימוש כללי
//        public static Dal.Models.Parent ToParentEntity(Dto.ParentDto dto)
//        {
//            return new Dal.Models.Parent
//            {
//                ParentId = dto.ParentId,
//                FullName = dto.FullName,
//                Phone = dto.Phone,
//                Email = dto.Email
//            };
//        }

//        // המרה ישירה מ-ParentChildImportDto לישות Parent (הכרחי לייבוא)
//        public static Dal.Models.Parent ToParentEntity(Dto.ParentChildImportDto importDto)
//        {
//            return new Dal.Models.Parent
//            {
//                FullName = importDto.ParentFullName,
//                Phone = importDto.ParentPhone,
//                Email = importDto.ParentEmail
//            };
//        }

//        // המרה מרשימת Entities ל-DTOs
//        public static List<ParentDto> ToParentDtoList(List<Parent> parents)
//        {
//            // שימוש יעיל ב-LINQ
//            return parents.Select(ToParentDto).ToList();
//        }
//    }
//}