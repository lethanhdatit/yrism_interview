using Microsoft.EntityFrameworkCore;

namespace EmployeeProfileManagement.Models
{
    public class EmployeeContext : DbContext
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<ToolLanguage> ToolLanguages { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<PositionResource> PositionResources { get; set; }
        public DbSet<ToolLanguageResource> ToolLanguageResources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PositionResource>()
                .HasMany(pr => pr.ToolLanguageResources)
                .WithOne(tlr => tlr.PositionResource)
                .HasForeignKey(tlr => tlr.PositionResourceId);
        }
    }

    public class Employee
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public List<Position> Positions { get; set; }
    }

    public class Position
    {
        public int PositionId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public List<ToolLanguage> ToolLanguages { get; set; }
        public int EmployeeId { get; set; }
    }

    public class ToolLanguage
    {
        public int ToolLanguageId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public string Description { get; set; }
        public List<Image> Images { get; set; }
        public int PositionId { get; set; }
    }

    public class Image
    {
        public int ImageId { get; set; }
        public byte[] Data { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public int DisplayOrder { get; set; }
        public int ToolLanguageId { get; set; }
    }

    public class PositionResource
    {
        public int PositionResourceId { get; set; }
        public string Name { get; set; }
        public List<ToolLanguageResource> ToolLanguageResources { get; set; }
    }

    public class ToolLanguageResource
    {
        public int ToolLanguageResourceId { get; set; }
        public string Name { get; set; }
        public int PositionResourceId { get; set; }
        public PositionResource PositionResource { get; set; }
    }

    public static class DbInitializer
    {
        public static void Initialize(EmployeeContext context)
        {
            context.Database.EnsureCreated();

            // Look for any PositionResources
            if (!context.PositionResources.Any())
            {
                var positionResources = new PositionResource[]
                {
                    new PositionResource { Name = "Frontend" },
                    new PositionResource { Name = "Backend" },
                    new PositionResource { Name = "Designer" }
                };
                foreach (var pr in positionResources)
                {
                    context.PositionResources.Add(pr);
                }
                context.SaveChanges();

                var toolLanguageResources = new ToolLanguageResource[]
                {
                    // Frontend Tools
                    new ToolLanguageResource { Name = "Javascript", PositionResourceId = positionResources[0].PositionResourceId },
                    new ToolLanguageResource { Name = "ReactJS", PositionResourceId = positionResources[0].PositionResourceId },
                    new ToolLanguageResource { Name = "VueJS", PositionResourceId = positionResources[0].PositionResourceId },
                    new ToolLanguageResource { Name = "AngularJS", PositionResourceId = positionResources[0].PositionResourceId },
                    new ToolLanguageResource { Name = "Jquery", PositionResourceId = positionResources[0].PositionResourceId },

                    // Backend Tools
                    new ToolLanguageResource { Name = "PHP", PositionResourceId = positionResources[1].PositionResourceId },
                    new ToolLanguageResource { Name = "Python", PositionResourceId = positionResources[1].PositionResourceId },
                    new ToolLanguageResource { Name = "Ruby", PositionResourceId = positionResources[1].PositionResourceId },
                    new ToolLanguageResource { Name = "Java", PositionResourceId = positionResources[1].PositionResourceId },
                    new ToolLanguageResource { Name = "Nodejs", PositionResourceId = positionResources[1].PositionResourceId },
                    new ToolLanguageResource { Name = "C", PositionResourceId = positionResources[1].PositionResourceId },
                    new ToolLanguageResource { Name = "C++", PositionResourceId = positionResources[1].PositionResourceId },
                    new ToolLanguageResource { Name = ".NET", PositionResourceId = positionResources[1].PositionResourceId },

                    // Designer Tools
                    new ToolLanguageResource { Name = "Adobe XD", PositionResourceId = positionResources[2].PositionResourceId },
                    new ToolLanguageResource { Name = "Figma", PositionResourceId = positionResources[2].PositionResourceId },
                    new ToolLanguageResource { Name = "Illustrator", PositionResourceId = positionResources[2].PositionResourceId },
                    new ToolLanguageResource { Name = "InvisionStudio", PositionResourceId = positionResources[2].PositionResourceId },
                    new ToolLanguageResource { Name = "Photoshop", PositionResourceId = positionResources[2].PositionResourceId },
                    new ToolLanguageResource { Name = "Sketch", PositionResourceId = positionResources[2].PositionResourceId }
                };
                foreach (var tlr in toolLanguageResources)
                {
                    context.ToolLanguageResources.Add(tlr);
                }
                context.SaveChanges();
            }
        }
    }
}
