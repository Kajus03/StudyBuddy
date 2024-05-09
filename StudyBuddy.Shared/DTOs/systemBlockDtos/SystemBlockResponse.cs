using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.Shared.DTOs.systemBlockDtos
{
    public class SystemBlockResponse
    {
        [Required] public Guid Id { get; set; }
        [Required] public UserId BlockedUserId { get; set; }
        [Required] public SystemBlockReasonResponse? SystemBlockReason { get; set; }
    }
}
