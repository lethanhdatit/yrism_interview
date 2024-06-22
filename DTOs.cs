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
        public int PositionId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public List<ToolLanguageDTO> ToolLanguages { get; set; }
    }

    public class ToolLanguageDTO
    {
        public int ToolLanguageId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public string Description { get; set; }
        public List<ImageDTO> Images { get; set; }
    }

    public class ImageDTO
    {
        public int ImageId { get; set; }
        public IFormFile Data { get; set; }
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
                .Must(positions => positions.Select(p => p.PositionId).Distinct().Count() == positions.Count)
                .WithMessage("Position cannot be duplicated.");
            RuleForEach(x => x.Positions).SetValidator(new PositionValidator());
        }
    }

    public class PositionValidator : AbstractValidator<PositionDTO>
    {
        public PositionValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Position name is required.");
            RuleFor(x => x.ToolLanguages)
                .NotEmpty().WithMessage("At least one Tool/Language is required.")
                .Must(toolLanguages => toolLanguages.Select(t => t.ToolLanguageId).Distinct().Count() == toolLanguages.Count)
                .WithMessage("Tool/Language cannot be duplicated.");
            RuleForEach(x => x.ToolLanguages).SetValidator(new ToolLanguageValidator());
        }
    }

    public class ToolLanguageValidator : AbstractValidator<ToolLanguageDTO>
    {
        public ToolLanguageValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tool/Language name is required.");
            RuleFor(x => x.From)
                .LessThanOrEqualTo(x => x.To)
                .WithMessage("From year must be less than or equal to To year.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
            RuleForEach(x => x.Images).SetValidator(new ImageValidator());
        }
    }

    public class ImageValidator : AbstractValidator<ImageDTO>
    {
        public ImageValidator()
        {
            RuleFor(x => x.Data.Length).LessThanOrEqualTo(2 * 1024 * 1024)
                .WithMessage("Image size must be less than or equal to 2MB.");
            RuleFor(x => x.Data.ContentType).Must(contentType => contentType.StartsWith("image/"))
                .WithMessage("Invalid image format. Only image files are allowed.");
        }
    }
}
