using System.ComponentModel.DataAnnotations;

namespace CAC.Core.Jobs
{
    public sealed record JobTriggerOptions
    {
        public const string ConfigKey = "JobTrigger";
        
        [Required]
        public string JobName { get; set; } = string.Empty;
    }
}
