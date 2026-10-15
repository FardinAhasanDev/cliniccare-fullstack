using System.ComponentModel.DataAnnotations;

namespace HospitalManagementAPI.Models.DTOs
{
    public class DoctorDTO
    {
        public int DoctorId { get; set; }

    
        public string DoctorName { get; set; } = null!;

      

        public bool IsActive { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime JoinDate { get; set; }

        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

        public string SpecializationsJson { get; set; } = "[]";
    }

    public class DoctorSpecializationDTO
    {
        public int DepartmentId { get; set; }
        public int YearsOfExperience { get; set; }
    }
}
