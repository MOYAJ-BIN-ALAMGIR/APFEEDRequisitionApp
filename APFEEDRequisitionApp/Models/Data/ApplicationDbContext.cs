using Microsoft.EntityFrameworkCore;
using APFEEDRequisitionApp.Models;

namespace APFEEDRequisitionApp.Models.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Requisition> Requisitions { get; set; }
    }
}
