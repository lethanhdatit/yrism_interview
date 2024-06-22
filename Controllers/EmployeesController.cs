using AutoMapper;
using EmployeeProfileManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeProfileManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeContext _context;
        private readonly IMapper _mapper;

        public EmployeesController(EmployeeContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetEmployees(
            string search = null, int? pageNumber = null, int? pageSize = null)
        {
            var query = _context.Employees
                .Include(e => e.Positions)
                .ThenInclude(p => p.ToolLanguages)
                .ThenInclude(t => t.Images)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    EF.Functions.ToTsVector("simple", e.Name)
                    .Matches(EF.Functions.PlainToTsQuery("simple", search)) ||
                    EF.Functions.Unaccent(e.Name).Contains(EF.Functions.Unaccent(search)));
            }

            query = query
                .OrderByDescending(e => e.Positions
                    .Sum(p => p.ToolLanguages
                        .Sum(t => t.To - t.From)));

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                query = query
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

            var employees = await query.ToListAsync();
            var employeeDTOs = _mapper.Map<List<EmployeeDTO>>(employees);

            return Ok(employeeDTOs);
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDTO>> GetEmployee([FromRoute] int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Positions)
                .ThenInclude(p => p.ToolLanguages)
                .ThenInclude(t => t.Images)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            var employeeDTO = _mapper.Map<EmployeeDTO>(employee);
            return Ok(employeeDTO);
        }

        // POST: api/Employees
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee([FromForm] EmployeeDTO employeeDTO)
        {
            var employee = _mapper.Map<Employee>(employeeDTO);

            foreach (var position in employee.Positions)
            {
                foreach (var toolLanguage in position.ToolLanguages)
                {
                    toolLanguage.Images = new List<Image>();
                    foreach (var imageDTO in employeeDTO.Positions
                        .First(p => p.PositionId == position.PositionId)
                        .ToolLanguages.First(t => t.ToolLanguageId == toolLanguage.ToolLanguageId)
                        .Images)
                    {
                        using var memoryStream = new MemoryStream();
                        await imageDTO.Data.CopyToAsync(memoryStream);
                        var image = new Image
                        {
                            Data = memoryStream.ToArray(),
                            DisplayOrder = imageDTO.DisplayOrder
                        };
                        toolLanguage.Images.Add(image);
                    }
                }
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, employee);
        }

        // PUT: api/Employees/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee([FromRoute] int id, [FromForm] EmployeeDTO employeeDTO)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var employee = await _context.Employees
                .Include(e => e.Positions)
                .ThenInclude(p => p.ToolLanguages)
                .ThenInclude(t => t.Images)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            _mapper.Map(employeeDTO, employee);

            foreach (var positionDTO in employeeDTO.Positions)
            {
                var position = employee.Positions.FirstOrDefault(p => p.PositionId == positionDTO.PositionId);
                if (position == null)
                {
                    position = _mapper.Map<Position>(positionDTO);
                    employee.Positions.Add(position);
                }
                else
                {
                    _mapper.Map(positionDTO, position);
                }

                foreach (var toolLanguageDTO in positionDTO.ToolLanguages)
                {
                    var toolLanguage = position.ToolLanguages.FirstOrDefault(t => t.ToolLanguageId == toolLanguageDTO.ToolLanguageId);
                    if (toolLanguage == null)
                    {
                        toolLanguage = _mapper.Map<ToolLanguage>(toolLanguageDTO);
                        position.ToolLanguages.Add(toolLanguage);
                    }
                    else
                    {
                        _mapper.Map(toolLanguageDTO, toolLanguage);
                    }

                    toolLanguage.Images ??= new List<Image>();
                    foreach (var imageDTO in toolLanguageDTO.Images)
                    {
                        var image = toolLanguage.Images.FirstOrDefault(i => i.ImageId == imageDTO.ImageId);
                        if (image == null)
                        {
                            using var memoryStream = new MemoryStream();
                            await imageDTO.Data.CopyToAsync(memoryStream);
                            image = new Image
                            {
                                Data = memoryStream.ToArray(),
                                DisplayOrder = imageDTO.DisplayOrder
                            };
                            toolLanguage.Images.Add(image);
                        }
                        else
                        {
                            _mapper.Map(imageDTO, image);
                            using var memoryStream = new MemoryStream();
                            await imageDTO.Data.CopyToAsync(memoryStream);
                            image.Data = memoryStream.ToArray();
                        }
                    }
                }
            }

            foreach (var position in employee.Positions)
            {
                var positionDTO = employeeDTO.Positions.FirstOrDefault(p => p.PositionId == position.PositionId);
                if (positionDTO == null)
                {
                    _context.Positions.Remove(position);
                }
                else
                {
                    foreach (var toolLanguage in position.ToolLanguages)
                    {
                        var toolLanguageDTO = positionDTO.ToolLanguages.FirstOrDefault(t => t.ToolLanguageId == toolLanguage.ToolLanguageId);
                        if (toolLanguageDTO == null)
                        {
                            _context.ToolLanguages.Remove(toolLanguage);
                        }
                        else
                        {
                            foreach (var image in toolLanguage.Images.ToList())
                            {
                                if (toolLanguageDTO.Images.All(i => i.ImageId != image.ImageId))
                                {
                                    _context.Images.Remove(image);
                                }
                            }
                        }
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee([FromRoute] int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}
