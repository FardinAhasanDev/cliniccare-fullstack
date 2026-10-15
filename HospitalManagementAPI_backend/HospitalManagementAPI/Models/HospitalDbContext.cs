using Microsoft.EntityFrameworkCore;

namespace HospitalManagementAPI.Models
{
    public class HospitalDbContext : DbContext
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> opp) : base(opp)
        { }
        public virtual DbSet<Doctor> Doctors { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<DoctorSpecialization> DoctorSpecializations { get; set; }
    }
}
