using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagementAPI.Models;
using HospitalManagementAPI.Models.DTOs;
using System.Text.Json.Serialization;

namespace HospitalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly HospitalDbContext _context;
        private readonly IWebHostEnvironment _env;
        public DoctorsController(HospitalDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetAllDoctors()
        {
            var doctors = await _context.Doctors.Include(d => d.DoctorSpecializations)
        .ThenInclude(s => s.Department).AsNoTracking().ToListAsync();
            return Ok(doctors);
        }

        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            var departments = await _context.Departments.ToListAsync();
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Doctor>> GetDoctorById(int id)
        {
            var doctor = await _context.Doctors.Include(d => d.DoctorSpecializations).ThenInclude(s => s.Department).FirstOrDefaultAsync(d => d.DoctorId == id);
            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        [HttpPost]
        //[{"DepartmentId":1,"YearsOfExperience":3},{"DepartmentId":2,"YearsOfExperience":5}]
        public async Task<IActionResult> PostDoctor([FromForm] DoctorDTO dto)
        {
            string fileName = "noimage.png";
            if (dto.ImageFile != null)
            {
                fileName = await SaveImageAsync(dto.ImageFile);
            }
            var doctor = new Doctor
            {
                DoctorName = dto.DoctorName,
                IsActive = dto.IsActive,
                JoinDate = dto.JoinDate,
                ImageName = fileName,
                ImageUrl = "/images/" + fileName
            };
            if (!string.IsNullOrWhiteSpace(dto.SpecializationsJson))
            {
                try
                {
                    string rawJson = dto.SpecializationsJson.Trim();
                    if (rawJson.StartsWith("{") && rawJson.EndsWith("}"))
                    {
                        rawJson = $"[{rawJson}]";
                    }
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };

                    var specializations = System.Text.Json.JsonSerializer.Deserialize<List<DoctorSpecializationDTO>>(rawJson, options);
                    if (specializations != null)
                    {
                        foreach (var item in specializations)
                        {
                            var departmentExists = await _context.Departments.AnyAsync(d => d.DepartmentId == item.DepartmentId);
                            if (!departmentExists)
                            {
                                return BadRequest("Department not exists");
                            }
                            doctor.DoctorSpecializations.Add(new DoctorSpecialization
                            {
                                DepartmentId = item.DepartmentId,
                                YearsOfExperience = item.YearsOfExperience
                            });
                        }
                    }
                }
                catch (System.Text.Json.JsonException ex)
                {
                    return BadRequest($"JSON Parsing Error: {ex.Message}. Received: {dto.SpecializationsJson}");
                }
            }
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return Ok(doctor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDoctor(int id, [FromForm] DoctorDTO dto)
        {
            if (id == 0) return BadRequest();
            if (id != dto.DoctorId) return BadRequest();
            var existingDoctor = await _context.Doctors.Include(d => d.DoctorSpecializations).ThenInclude(s => s.Department).FirstOrDefaultAsync(d => d.DoctorId == id);
            if (existingDoctor == null) return NotFound();
            string oldImageUrl = existingDoctor.ImageUrl;
            string fileName = existingDoctor.ImageName;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                fileName = await SaveImageAsync(dto.ImageFile);
                oldImageUrl = "/images/" + fileName;
                var imagePath = Path.Combine(_env.WebRootPath, "images", existingDoctor.ImageName ?? "");
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            existingDoctor.DoctorId = id;
            existingDoctor.ImageName = fileName;
            existingDoctor.ImageUrl = oldImageUrl;
            existingDoctor.DoctorName = dto.DoctorName;
            existingDoctor.IsActive = dto.IsActive;
            existingDoctor.JoinDate = dto.JoinDate;
            var specializations = existingDoctor.DoctorSpecializations;
            _context.DoctorSpecializations.RemoveRange(specializations);

            if (!string.IsNullOrWhiteSpace(dto.SpecializationsJson))
            {
                try
                {
                    string rawJson = dto.SpecializationsJson.Trim();
                    if (rawJson.StartsWith("{") && rawJson.EndsWith("}"))
                    {
                        rawJson = $"[{rawJson}]";
                    }
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    };

                    var newSpecializations = System.Text.Json.JsonSerializer.Deserialize<List<DoctorSpecializationDTO>>(rawJson, options);
                    if (newSpecializations != null)
                    {
                        foreach (var item in newSpecializations)
                        {
                            var departmentExists = await _context.Departments.AnyAsync(d => d.DepartmentId == item.DepartmentId);
                            if (!departmentExists)
                            {
                                return BadRequest("Department not exists");
                            }
                            existingDoctor.DoctorSpecializations.Add(new DoctorSpecialization
                            {
                                DoctorId = id,
                                DepartmentId = item.DepartmentId,
                                YearsOfExperience = item.YearsOfExperience
                            });
                        }
                    }
                }
                catch (System.Text.Json.JsonException ex)
                {
                    return BadRequest($"JSON Parsing Error: {ex.Message}. Received: {dto.SpecializationsJson}");
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Doctors.Any(d => d.DoctorId == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string uploadDir = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(uploadDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            return fileName;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();
            var imagePath = Path.Combine(_env.WebRootPath, "images", doctor.ImageName ?? "");
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
