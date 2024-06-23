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
        private readonly CdnUploadService _cdnUploadService;
        private const string TemporaryInternalResource = "TemporaryInternalResource";

        public EmployeesController(EmployeeContext context, CdnUploadService cdnUploadUtil)
        {
            _context = context;
            _cdnUploadService = cdnUploadUtil;
        }

        // GET: api/Employees
        [HttpGet]
        public async Task<ActionResult> GetEmployees(
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
            var employeeDTOs = employees.Select(e => MapToEmployeeDTO(e)).ToList();

            return new JsonResult(employeeDTOs)
            {
                StatusCode = 200
            };
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetEmployee(int id)
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

            var employeeDTO = MapToEmployeeDTO(employee);

            return new JsonResult(employeeDTO)
            {
                StatusCode = 200
            };
        }

        // POST: api/Employees
        [HttpPost]
        public async Task<ActionResult> PostEmployee([FromForm] EmployeeDTO employeeDTO)
        {
            var employee = MapToEmployee(employeeDTO);

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return new JsonResult(new { id = employee.EmployeeId })
            {
                StatusCode = 200
            };
        }

        // PUT: api/Employees/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, [FromForm] EmployeeDTO employeeDTO)
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

            UpdateEmployeeFromDTO(employee, employeeDTO);

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
        public async Task<IActionResult> DeleteEmployee(int id)
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

        private EmployeeDTO MapToEmployeeDTO(Employee employee)
        {
            return new EmployeeDTO
            {
                Name = employee.Name,
                Positions = employee.Positions.Select(p => new PositionDTO
                {
                    Id = p.PositionId,
                    PositionResourceId = p.PositionResourceId,
                    DisplayOrder = p.DisplayOrder,
                    ToolLanguages = p.ToolLanguages.Select(t => new ToolLanguageDTO
                    {
                        Id = t.ToolLanguageId,
                        ToolLanguageResourceId = t.ToolLanguageResourceId,
                        DisplayOrder = t.DisplayOrder,
                        From = t.From,
                        To = t.To,
                        Description = t.Description,
                        Images = t.Images.Select(i => new ImageDTO
                        {
                            Id = i.ImageId,
                            CdnUrl = i.CdnUrl,
                            DisplayOrder = i.DisplayOrder
                        }).ToList()
                    }).ToList()
                }).ToList()
            };
        }

        private Employee MapToEmployee(EmployeeDTO employeeDTO)
        {
            return new Employee
            {
                Name = employeeDTO.Name,
                Positions = employeeDTO.Positions.Select(p => new Position
                {
                    PositionResourceId = p.PositionResourceId,
                    DisplayOrder = p.DisplayOrder,
                    ToolLanguages = p.ToolLanguages.Select(t => new ToolLanguage
                    {
                        ToolLanguageResourceId = t.ToolLanguageResourceId,
                        DisplayOrder = t.DisplayOrder,
                        From = t.From,
                        To = t.To,
                        Description = t.Description,
                        Images = t.Images.Select(i => new Image
                        {
                            DisplayOrder = i.DisplayOrder,
                            CdnUrl = i.Data != null ? _cdnUploadService.UploadFileAsync(i.Data, TemporaryInternalResource).Result : null,
                        }).ToList()
                    }).ToList()
                }).ToList()
            };
        }

        private void UpdateEmployeeFromDTO(Employee employee, EmployeeDTO employeeDTO)
        {
            employee.Name = employeeDTO.Name;

            // Remove positions that are not in the DTO
            foreach (var position in employee.Positions.ToList())
            {
                if (employeeDTO.Positions.All(p => p.Id != position.PositionId))
                {
                    _context.Positions.Remove(position);
                }
                else
                {
                    foreach (var toolLanguage in position.ToolLanguages.ToList())
                    {
                        var positionDTO = employeeDTO.Positions.First(p => p.Id == position.PositionId);
                        if (positionDTO.ToolLanguages.All(t => t.Id != toolLanguage.ToolLanguageId))
                        {
                            _context.ToolLanguages.Remove(toolLanguage);
                        }
                        else
                        {
                            foreach (var image in toolLanguage.Images.ToList())
                            {
                                var toolLanguageDTO = positionDTO.ToolLanguages.First(t => t.Id == toolLanguage.ToolLanguageId);
                                if (toolLanguageDTO.Images.All(i => i.Id != image.ImageId))
                                {
                                    _context.Images.Remove(image);
                                }
                            }
                        }
                    }
                }
            }

            // Update Positions
            foreach (var positionDTO in employeeDTO.Positions)
            {
                var position = employee.Positions.FirstOrDefault(p => p.PositionId == positionDTO.Id);
                if (position == null)
                {
                    position = new Position
                    {
                        PositionResourceId = positionDTO.PositionResourceId,
                        DisplayOrder = positionDTO.DisplayOrder,
                        ToolLanguages = positionDTO.ToolLanguages.Select(t => new ToolLanguage
                        {
                            ToolLanguageResourceId = t.ToolLanguageResourceId,
                            DisplayOrder = t.DisplayOrder,
                            From = t.From,
                            To = t.To,
                            Description = t.Description,
                            Images = t.Images.Select(i => new Image
                            {
                                CdnUrl = i.Data != null ? _cdnUploadService.UploadFileAsync(i.Data, TemporaryInternalResource).Result : null,
                                DisplayOrder = i.DisplayOrder
                            }).ToList()
                        }).ToList()
                    };
                    employee.Positions.Add(position);
                }
                else
                {
                    position.PositionResourceId = positionDTO.PositionResourceId;
                    position.DisplayOrder = positionDTO.DisplayOrder;

                    // Update ToolLanguages
                    foreach (var toolLanguageDTO in positionDTO.ToolLanguages)
                    {
                        var toolLanguage = position.ToolLanguages.FirstOrDefault(t => t.ToolLanguageId == toolLanguageDTO.Id);
                        if (toolLanguage == null)
                        {
                            toolLanguage = new ToolLanguage
                            {
                                ToolLanguageResourceId = toolLanguageDTO.ToolLanguageResourceId,
                                DisplayOrder = toolLanguageDTO.DisplayOrder,
                                From = toolLanguageDTO.From,
                                To = toolLanguageDTO.To,
                                Description = toolLanguageDTO.Description,
                                Images = toolLanguageDTO.Images.Select(i => new Image
                                {
                                    CdnUrl = i.Data != null ? _cdnUploadService.UploadFileAsync(i.Data, TemporaryInternalResource).Result : null,
                                    DisplayOrder = i.DisplayOrder,
                                }).ToList()
                            };
                            position.ToolLanguages.Add(toolLanguage);
                        }
                        else
                        {
                            toolLanguage.ToolLanguageResourceId = toolLanguageDTO.ToolLanguageResourceId;
                            toolLanguage.DisplayOrder = toolLanguageDTO.DisplayOrder;
                            toolLanguage.From = toolLanguageDTO.From;
                            toolLanguage.To = toolLanguageDTO.To;
                            toolLanguage.Description = toolLanguageDTO.Description;

                            // Update Images
                            foreach (var imageDTO in toolLanguageDTO.Images)
                            {
                                var image = toolLanguage.Images.FirstOrDefault(i => i.ImageId == imageDTO.Id);
                                if (image == null)
                                {
                                    image = new Image
                                    {
                                        CdnUrl = imageDTO.Data != null ? _cdnUploadService.UploadFileAsync(imageDTO.Data, TemporaryInternalResource).Result : null,
                                        DisplayOrder = imageDTO.DisplayOrder,
                                    };
                                    toolLanguage.Images.Add(image);
                                }
                                else
                                {
                                    if (imageDTO.Data != null)
                                    {
                                        image.CdnUrl = imageDTO.Data != null ? _cdnUploadService.UploadFileAsync(imageDTO.Data, TemporaryInternalResource).Result : null;
                                    }
                                    image.DisplayOrder = imageDTO.DisplayOrder;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
