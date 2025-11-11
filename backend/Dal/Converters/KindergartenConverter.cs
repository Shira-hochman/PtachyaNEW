using Dal.Models;
using Dto;
using System.Collections.Generic;
using System.Linq;

namespace Dal.Converters
{
    public static class KindergartenConverter
    {
        // המרה מיישות (Entity) ל-DTO
        public static Dto.KindergartenDto TokindergartenDto(Dal.Models.Kindergarten kindergarten)
        {
            return new Dto.KindergartenDto
            {
                KindergartenId = kindergarten.KindergartenId,
                Code = kindergarten.Code,
                Name = kindergarten.Name 
            };
        }

        public static Dal.Models.Kindergarten TokindergartenEntity(Dto.KindergartenDto dto)
        {
            return new Dal.Models.Kindergarten
            {
                KindergartenId = dto.KindergartenId,
                Code = dto.Code,
                Name = dto.Name,
                Address=dto.Address
            };
        }

        public static List<KindergartenDto> TokindergartenDtoList(IEnumerable<Kindergarten> kindergarten)
        {
            return kindergarten.Select(TokindergartenDto).ToList();
        }
    }
}