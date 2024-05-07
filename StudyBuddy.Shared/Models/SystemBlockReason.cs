using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using StudyBuddy.Shared.DTOs.systemBlockDtos;

namespace StudyBuddy.Shared.Models
{
    public class SystemBlockReason
    {
        [Key] public Guid Id { get; set; }
        [Required] public string? Reason { get; set; }

        public static SystemBlockReason fromDto(SystemBlockReasonRequest request)
        {
            return new() {
                Reason = request.Reason,
                Id = Guid.NewGuid()
            };
        }

        public static SystemBlockReasonResponse toDto(SystemBlockReason systemBlockReason)
        {
            return new()
            {
                Id = systemBlockReason.Id,
                Reason = systemBlockReason.Reason
            };
        }
    }
}
