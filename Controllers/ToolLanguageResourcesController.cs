using EmployeeProfileManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeProfileManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolLanguageResourcesController : ControllerBase
    {
        private readonly EmployeeContext _context;

        public ToolLanguageResourcesController(EmployeeContext context)
        {
            _context = context;
        }

        // GET: api/ToolLanguageResources
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToolLanguageResourceDTO>>> GetToolLanguageResources()
        {
            var toolLanguageResources = await _context.ToolLanguageResources.ToListAsync();
            var toolLanguageResourceDTOs = toolLanguageResources.Select(tlr => MapToToolLanguageResourceDTO(tlr)).ToList();
            return Ok(toolLanguageResourceDTOs);
        }

        private ToolLanguageResourceDTO MapToToolLanguageResourceDTO(ToolLanguageResource toolLanguageResource)
        {
            return new ToolLanguageResourceDTO
            {
                ToolLanguageResourceId = toolLanguageResource.ToolLanguageResourceId,
                PositionResourceId = toolLanguageResource.PositionResourceId,
                Name = toolLanguageResource.Name
            };
        }
    }
}
