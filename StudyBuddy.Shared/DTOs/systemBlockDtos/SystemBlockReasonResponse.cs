﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyBuddy.Shared.DTOs.systemBlockDtos
{
    public class SystemBlockReasonResponse
    {
        [Required] public Guid Id { get; set; }
        [Required] public string? Reason { get; set; }
    }
}
