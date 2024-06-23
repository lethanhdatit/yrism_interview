using EmployeeProfileManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeProfileManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionResourcesController : ControllerBase
    {
        private readonly EmployeeContext _context;

        public PositionResourcesController(EmployeeContext context)
        {
            _context = context;
        }

        // GET: api/PositionResources
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PositionResourceDTO>>> GetPositionResources()
        {
            var positionResources = await _context.PositionResources
                .Include(pr => pr.ToolLanguageResources)
                .ToListAsync();

            var positionResourceDTOs = positionResources.Select(pr => MapToPositionResourceDTO(pr)).ToList();
            return Ok(positionResourceDTOs);
        }

        private PositionResourceDTO MapToPositionResourceDTO(PositionResource positionResource)
        {
            return new PositionResourceDTO
            {
                PositionResourceId = positionResource.PositionResourceId,
                Name = positionResource.Name,
                ToolLanguageResources = positionResource.ToolLanguageResources.Select(tlr => new ToolLanguageResourceDTO
                {
                    ToolLanguageResourceId = tlr.ToolLanguageResourceId,
                    PositionResourceId = tlr.PositionResourceId,
                    Name = tlr.Name
                }).ToList()
            };
        }
    }
}
