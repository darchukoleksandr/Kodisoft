using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    /// <summary>
    /// Model that represents an source feeds.
    /// </summary>
    public class Source
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of the <see cref="WebApp.Models.Source"/>.
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Url that lead to home page of the <see cref="WebApp.Models.Source"/>.
        /// </summary>
        [Required]
        [DataType(DataType.Url)]
        public string UrlHome { get; set; }
        /// <summary>
        /// Url that lead to rss feed of the <see cref="WebApp.Models.Source"/>.
        /// </summary>
        [Required]
        [DataType(DataType.Url)]
        public string UrlRss { get; set; }
        /// <summary>
        /// Many-to-many relationship navigation property with <see cref="WebApp.Models.Tag"/>.
        /// </summary>
        public ICollection<SourceTag> SourceTags { get; set; } = new List<SourceTag>();
        /// <summary>
        /// Many-to-many relationship navigation property with <see cref="WebApp.Models.UserCollection"/>.
        /// </summary>
        public ICollection<UserCollectionSource> UserCollectionSources { get; set; } = new List<UserCollectionSource>();
    }
    public class SourceViewModel
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of the <see cref="WebApp.Models.SourceViewModel"/>.
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Url that lead to home page of the <see cref="WebApp.Models.SourceViewModel"/>.
        /// </summary>
        [Required]
        [DataType(DataType.Url)]
        public string UrlHome { get; set; }
        /// <summary>
        /// Url that lead to rss feed of the <see cref="WebApp.Models.SourceViewModel"/>.
        /// </summary>
        [Required]
        [DataType(DataType.Url)]
        public string UrlRss { get; set; }
        /// <summary>
        /// Tags view model list of the <see cref="WebApp.Models.Source"/>.
        /// </summary>
        public ICollection<TagViewModel> Tags { get; set; } = new List<TagViewModel>();
    }
}
