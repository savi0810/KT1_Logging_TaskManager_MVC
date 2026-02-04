using KT1_Logging_TaskManager_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace KT1_Logging_TaskManager_MVC
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<CurrentTasks> CurrentTasks { get; set; }
        public DbSet<CompletedTasks> CompletedTasks { get; set; }
        public DbSet<DeletedTasks> DeletedTasks { get; set; }
        public DbSet<OverdueTasks> OverdueTasks { get; set; }

    }
}
