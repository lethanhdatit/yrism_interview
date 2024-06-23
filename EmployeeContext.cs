using Microsoft.EntityFrameworkCore;

namespace EmployeeProfileManagement.Models
{
    public class EmployeeContext : DbContext
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Enable lazy loading proxies
            optionsBuilder.UseLazyLoadingProxies();
        }

        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<ToolLanguage> ToolLanguages { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<PositionResource> PositionResources { get; set; }
        public virtual DbSet<ToolLanguageResource> ToolLanguageResources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PositionResource>()
                .HasMany(pr => pr.ToolLanguageResources)
                .WithOne(tlr => tlr.PositionResource)
                .HasForeignKey(tlr => tlr.PositionResourceId);

            modelBuilder.Entity<ToolLanguageResource>()
                .HasMany(tlr => tlr.ToolLanguages)
                .WithOne(tl => tl.ToolLanguageResource)
                .HasForeignKey(tl => tl.ToolLanguageResourceId);

            modelBuilder.Entity<Position>()
                .HasMany(p => p.ToolLanguages)
                .WithOne(tl => tl.Position)
                .HasForeignKey(tl => tl.PositionId);

            modelBuilder.Entity<Position>()
                .HasOne(p => p.PositionResource)
                .WithMany(pr => pr.Positions)
                .HasForeignKey(p => p.PositionResourceId);

            modelBuilder.Entity<ToolLanguage>()
                .HasMany(tl => tl.Images)
                .WithOne(i => i.ToolLanguage)
                .HasForeignKey(i => i.ToolLanguageId);
        }
    }

    public class Employee
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public virtual List<Position> Positions { get; set; }
    }

    public class Position
    {
        public int PositionId { get; set; }
        public int PositionResourceId { get; set; }
        public int DisplayOrder { get; set; }
        public virtual List<ToolLanguage> ToolLanguages { get; set; }
        public virtual PositionResource PositionResource { get; set; }
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }
    }

    public class ToolLanguage
    {
        public int ToolLanguageId { get; set; }
        public int ToolLanguageResourceId { get; set; }
        public int DisplayOrder { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public string Description { get; set; }
        public virtual List<Image> Images { get; set; }
        public virtual ToolLanguageResource ToolLanguageResource { get; set; }
        public int PositionId { get; set; }
        public virtual Position Position { get; set; }
    }

    public class Image
    {
        public int ImageId { get; set; }
        public string CdnUrl { get; set; }
        public int DisplayOrder { get; set; }
        public int ToolLanguageId { get; set; }
        public virtual ToolLanguage ToolLanguage { get; set; }
    }

    public class PositionResource
    {
        public int PositionResourceId { get; set; }
        public string Name { get; set; }
        public virtual List<ToolLanguageResource> ToolLanguageResources { get; set; }
        public virtual List<Position> Positions { get; set; }
    }

    public class ToolLanguageResource
    {
        public int ToolLanguageResourceId { get; set; }
        public string Name { get; set; }
        public int PositionResourceId { get; set; }
        public virtual PositionResource PositionResource { get; set; }
        public virtual List<ToolLanguage> ToolLanguages { get; set; }
    }

    public static class DbInitializer
    {
        public static void Initialize(EmployeeContext context)
        {
            context.Database.EnsureCreated();

            // Create the unaccent extension if it doesn't exist
            context.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS unaccent");

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
