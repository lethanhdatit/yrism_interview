using FluentValidation;

namespace EmployeeProfileManagement
{
    public class EmployeeDTO
    {
        public string Name { get; set; }
        public List<PositionDTO> Positions { get; set; }
    }

    public class PositionDTO
    {
        public int Id { get; set; }
        public int PositionResourceId { get; set; }
        public int DisplayOrder { get; set; }
        public List<ToolLanguageDTO> ToolLanguages { get; set; }
    }

    public class ToolLanguageDTO
    {
        public int Id { get; set; }
        public int ToolLanguageResourceId { get; set; }
        public int DisplayOrder { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public string Description { get; set; }
        public List<ImageDTO> Images { get; set; }
    }

    public class ImageDTO
    {
        public int Id { get; set; }
        public IFormFile? Data { get; set; }
        public string? CdnUrl { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class PositionResourceDTO
    {
        public int PositionResourceId { get; set; }
        public string Name { get; set; }
        public List<ToolLanguageResourceDTO> ToolLanguageResources { get; set; }
    }

    public class ToolLanguageResourceDTO
    {
        public int ToolLanguageResourceId { get; set; }
        public int PositionResourceId { get; set; }
        public string Name { get; set; }
    }

    public class EmployeeValidator : AbstractValidator<EmployeeDTO>
    {
        public EmployeeValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Positions)
                .NotEmpty().WithMessage("At least one position is required.")
                .Must(positions => positions.Select(p => p.PositionResourceId).Distinct().Count() == positions.Count)
                .WithMessage("Position cannot be duplicated.");
            RuleForEach(x => x.Positions).SetValidator(new PositionValidator());
        }
    }

    public class PositionValidator : AbstractValidator<PositionDTO>
    {
        public PositionValidator()
        {
            RuleFor(x => x.PositionResourceId).NotEmpty().WithMessage("PositionResourceId is required.");
            RuleFor(x => x.ToolLanguages)
                .NotEmpty().WithMessage("At least one Tool/Language is required.")
                .Must(toolLanguages => toolLanguages.Select(t => t.ToolLanguageResourceId).Distinct().Count() == toolLanguages.Count)
                .WithMessage("Tool/Language cannot be duplicated.");
            RuleForEach(x => x.ToolLanguages).SetValidator(new ToolLanguageValidator());
        }
    }

    public class ToolLanguageValidator : AbstractValidator<ToolLanguageDTO>
    {
        public ToolLanguageValidator()
        {
            RuleFor(x => x.ToolLanguageResourceId).NotEmpty().WithMessage("ToolLanguageResourceId is required.");
            RuleFor(x => x.From)
                .LessThanOrEqualTo(x => x.To)
                .WithMessage("From year must be less than or equal to To year.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
            RuleFor(x => x.Images).NotNull().NotEmpty().WithMessage("Images is required.");
            RuleFor(x => x.Images).ForEach(f => f.Must(d => d.Id >= 0 || (d.Data != null && d.Data.Length > 0)).WithMessage("Please provide either 'ImageId' or 'Data' for each image."));
            RuleForEach(x => x.Images).SetValidator(new ImageValidator());
        }
    }

    public class ImageValidator : AbstractValidator<ImageDTO>
    {
        public ImageValidator()
        {
            When(x => x.Data != null, () =>
            {
                RuleFor(x => x.Data!.Length).LessThanOrEqualTo(2 * 1024 * 1024).WithMessage("Image size must be less than or equal to 2MB.");
                RuleFor(x => x.Data!.ContentType).Must(contentType => contentType.StartsWith("image/"))
                    .WithMessage("Invalid image format. Only image files are allowed.");
            });
        }
    }

    public static class FileConverter
    {
        public static IFormFile? ToFormFile(this byte[] byteArray, string name, string fileName)
        {
            if (byteArray == null || byteArray.Length == 0)
            {
                return null;
            }

            var stream = new MemoryStream(byteArray);
            return new FormFile(stream, 0, byteArray.Length, name, fileName);
        }

        public static async Task<byte[]?> FromFormFile(this IFormFile? file)
        {
            if (file == null)
            {
                return null;
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
