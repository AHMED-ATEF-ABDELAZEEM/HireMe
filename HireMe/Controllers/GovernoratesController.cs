using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HireMe.Services;
using Microsoft.AspNetCore.Mvc;

namespace HireMe.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GovernoratesController : ControllerBase
    {
        private readonly IGovernorateService _governorateService;

        public GovernoratesController(IGovernorateService governorateService)
        {
            _governorateService = governorateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGovernorates()
        {
            var governorates = await _governorateService.GetAllGovernoratesAsync();
            return Ok(governorates);
        }
    }
}