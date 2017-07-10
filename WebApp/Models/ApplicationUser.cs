using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApp.Models
{
    /// <summary>
    /// User model with identity support.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// User <see cref="WebApp.Models.UserCollection"/> list.
        /// </summary>
        public ICollection<UserCollection> UserCollections { get; set; } = new List<UserCollection>();
    }
}
