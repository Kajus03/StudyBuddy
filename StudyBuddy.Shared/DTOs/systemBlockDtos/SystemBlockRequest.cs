using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.Shared.DTOs.systemBlockDtos
{
    public class SystemBlockRequest
    {
        [Required] public Guid BlockedUserId { get; set; }
        public DateTime? BlockedUntil { get; set; }
        [Required] public required SystemBlockReasonRequest SystemBlockReason { get; set; }
    }
}
