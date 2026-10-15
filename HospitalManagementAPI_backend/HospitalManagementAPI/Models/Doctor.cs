using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementAPI.Models
{

    public class Doctor
    {
        public int DoctorId { get; set; }

        [Required, StringLength(150)]
        public string DoctorName { get; set; } = null!;
        public bool IsActive { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime JoinDate { get; set; }

        public string? ImageUrl { get; set; }
        public string? ImageName { get; set; }

        public virtual ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();
    }

    public class Department
    {
        public int DepartmentId { get; set; }

        [Required, StringLength(100)]
        public string DepartmentName { get; set; } = null!; 

        public virtual ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();
    }

    public class DoctorSpecialization
    {
        public int DoctorSpecializationId { get; set; }
        public int DoctorId { get; set; }
        public int DepartmentId { get; set; }
        public int YearsOfExperience { get; set; }
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Department Department { get; set; } = null!;
    }
}
