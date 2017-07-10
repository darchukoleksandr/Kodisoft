using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class SourceTag
    {
        [Required]
        public int SourceId { get; set; }
        public Source Source { get; set; }
        [Required]
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
