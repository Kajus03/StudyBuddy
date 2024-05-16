using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudyBuddy.Shared.DTOs.systemBlockDtos;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.Shared.Models
{
    public class SystemBlock
    {
        [Key] public Guid Id { get; set; }
        [Required] public UserId BlockedUserId { get; set; }
        [Required] public Guid SystemBlockReasonId { get; set; }
        public DateTime? BlockedUntil { get; set; }
        public User? BlockedUser { get; set; }
        public SystemBlockReason? SystemBlockReason { get; set; }

        public static SystemBlock fromDto(SystemBlockRequest request)
        {
            SystemBlockReason reason = SystemBlockReason.fromDto(request.SystemBlockReason);

            return new()
            {
                Id = Guid.NewGuid(),
                BlockedUserId = UserId.From(request.BlockedUserId),
                SystemBlockReasonId = reason.Id,
                SystemBlockReason = reason,
                BlockedUntil = request.BlockedUntil
            };
        }

        public static SystemBlockResponse toDto(SystemBlock block)
        {
            return new()
            {
                Id = block.Id,
                BlockedUserId = block.BlockedUserId,
                BlockedUntil = block.BlockedUntil,
                SystemBlockReason = block.SystemBlockReason != null ? SystemBlockReason.toDto(block.SystemBlockReason) : null
            };
        }
    }
}
