using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class UserCollection
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        public ICollection<UserCollectionSource> UserCollectionSources { get; set; } = new List<UserCollectionSource>();
    }

    public class UserCollectionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<SourceViewModel> Sources { get; set; } = new List<SourceViewModel>();
    }
}
