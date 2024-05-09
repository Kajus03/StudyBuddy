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
        public Guid BlockedUserId { get; set; }
        public required SystemBlockReasonRequest SystemBlockReason { get; set; }
    }
}
