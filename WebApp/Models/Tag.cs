using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Tag
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public ICollection<SourceTag> SourceTags { get; set; } = new List<SourceTag>();
    }

    public class TagViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
