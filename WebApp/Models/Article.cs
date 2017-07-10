using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    /// <summary>
    /// Model that represents an article received via feeds <see cref="WebApp.Models.Source"/>.
    /// </summary>
    public class Article
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// <see cref="WebApp.Models.Source"/>
        /// </summary>
        public Source Source { get; set; }
        /// <summary>
        /// <see cref="WebApp.Models.Source"/> foreign key.
        /// </summary>
        [Required]
        public int SourceId { get; set; }
        /// <summary>
        /// Direct link to article source.
        /// </summary>
        [Required]
        [DataType(DataType.Url)]
        public string Link { get; set; }
        /// <summary>
        /// Title of the article.
        /// </summary>
        [Required]
        public string Title { get; set; }
        /// <summary>
        /// Content of the article. May be as in HTML and in plaint text format.
        /// </summary>
        [Required]
        [DataType(DataType.Html)]
        public string Content { get; set; }
        /// <summary>
        /// Publish date of the article
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; }
    }
    /// <summary>
    /// View model for <see cref="WebApp.Models.Article"/> that not contains <see cref="WebApp.Models.Source"/> navigation property.
    /// </summary>
    public class ArticleViewModel
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// <see cref="WebApp.Models.Source"/> foreign key.
        /// </summary>
        [Required]
        public int SourceId { get; set; }
        /// <summary>
        /// Direct link to article source.
        /// </summary>
        [Required]
        [DataType(DataType.Url)]
        public string Link { get; set; }
        /// <summary>
        /// Title of the article.
        /// </summary>
        [Required]
        public string Title { get; set; }
        /// <summary>
        /// Content of the article. May be as in HTML and in plaint text format.
        /// </summary>
        [Required]
        [DataType(DataType.Html)]
        public string Content { get; set; }
        /// <summary>
        /// Publish date of the article
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; }
    }


}
