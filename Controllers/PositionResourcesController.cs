using AutoMapper;
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
        private readonly IMapper _mapper;

        public PositionResourcesController(EmployeeContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/PositionResources
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PositionResourceDTO>>> GetPositionResources()
        {
            var positionResources = await _context.PositionResources
                .Include(pr => pr.ToolLanguageResources)
                .ToListAsync();

            var positionResourceDTOs = _mapper.Map<List<PositionResourceDTO>>(positionResources);
            return Ok(positionResourceDTOs);
        }
    }
}
