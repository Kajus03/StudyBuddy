﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudyBuddy.API.Services.SystemBlockService;
using StudyBuddy.Shared.DTOs.systemBlockDtos;

namespace StudyBuddy.API.Controllers.AdminController
{
    // All of these endpoints return status 500 on error because 500 is the most beautiful error code after 418
    [Route("api/v1/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ISystemBlockService _systemBlockService;

        public AdminController(ISystemBlockService systemBlockService)
        {
            _systemBlockService = systemBlockService;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSystemBlock(Guid id)
        {
            return Ok(await _systemBlockService.GetSystemBlockAsync(id));
        }

        [HttpGet]
        public async Task<IActionResult> GetSystemBlocks()
        {
            return Ok(await _systemBlockService.GetSystemBlocksAsync());
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> RemoveSystemBlock(Guid id)
        {
            return Ok(await _systemBlockService.RemoveSystemBlockAsync(id));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSystemBlock(SystemBlockRequest request)
        {
            return Ok(await _systemBlockService.CreateSystemBlockAsync(request));
        }
    }
}
