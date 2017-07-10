using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region UserCollection -> Source (many-to-many)

            builder.Entity<UserCollectionSource>()
                .HasKey(t => new { t.SourceId, t.UserCollectionId });

            builder.Entity<UserCollectionSource>()
                .HasOne(pt => pt.UserCollection)
                .WithMany(p => p.UserCollectionSources)
                .HasForeignKey(pt => pt.UserCollectionId);

            builder.Entity<UserCollectionSource>()
                .HasOne(pt => pt.Source)
                .WithMany(t => t.UserCollectionSources)
                .HasForeignKey(pt => pt.SourceId);

            #endregion
            #region Source -> Tag (many-to-many)

            builder.Entity<SourceTag>()
                .HasKey(t => new { t.SourceId, t.TagId });

            builder.Entity<SourceTag>()
                .HasOne(pt => pt.Source)
                .WithMany(p => p.SourceTags)
                .HasForeignKey(pt => pt.SourceId);

            builder.Entity<SourceTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.SourceTags)
                .HasForeignKey(pt => pt.TagId);

            #endregion
            #region UserRead (composite key)

            builder.Entity<UserRead>()
                .HasKey(a => new { a.ApplicationUserId, a.ArticleId });

            #endregion

        }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<UserCollection> UserCollections { get; set; }
        public DbSet<UserCollectionSource> UserCollectionSources { get; set; }
        public DbSet<SourceTag> SourceTags { get; set; }
        public DbSet<UserRead> UserReads { get; set; }
    }
}
