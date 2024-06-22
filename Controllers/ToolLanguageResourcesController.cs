using AutoMapper;
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
        private readonly IMapper _mapper;

        public ToolLanguageResourcesController(EmployeeContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/ToolLanguageResources
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToolLanguageResourceDTO>>> GetToolLanguageResources()
        {
            var toolLanguageResources = await _context.ToolLanguageResources.ToListAsync();
            var toolLanguageResourceDTOs = _mapper.Map<List<ToolLanguageResourceDTO>>(toolLanguageResources);
            return Ok(toolLanguageResourceDTOs);
        }
    }
}
