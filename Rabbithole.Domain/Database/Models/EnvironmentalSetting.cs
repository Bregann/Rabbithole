using System.ComponentModel.DataAnnotations;

namespace Rabbithole.Domain.Database.Models
{
    public class EnvironmentalSetting
    {
        [Key]
        public required string Key { get; set; }

        [Required]
        public required string Value { get; set; }
    }
}
