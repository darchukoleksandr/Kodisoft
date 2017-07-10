
namespace WebApp.Models
{
    /// <summary>
    /// Model that represents wich articles did user have read.
    /// </summary>
    public class UserRead
    {
        /// <summary>
        /// <see cref="WebApp.Models.Article"/> navigation property.
        /// </summary>
        public Article Article { get; set; }
        /// <summary>
        /// <see cref="WebApp.Models.Article"/> primary key.
        /// </summary>
        public int ArticleId { get; set; }
        /// <summary>
        /// <see cref="WebApp.Models.ApplicationUser"/> navigation property.
        /// </summary>
        public ApplicationUser ApplicationUser { get; set; }
        /// <summary>
        /// <see cref="WebApp.Models.ApplicationUser"/> primary key.
        /// </summary>
        public string ApplicationUserId { get; set; }
    }
}
