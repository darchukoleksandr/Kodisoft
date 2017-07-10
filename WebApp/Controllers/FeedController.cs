using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebApp.Data;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    [Route("api/feed")]
    public class FeedController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _dbContext;
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="dbContext"><see cref="ApplicationDbContext"/> object.</param>
        /// <param name="memoryCache"><see cref="IMemoryCache"/> object.</param>
        /// <param name="sourceSaver"><see cref="ISourceSaver"/> objcet.</param>
        public FeedController(ApplicationDbContext dbContext, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _cache = memoryCache;
        }
        /// <summary>
        /// Get unreaded articles for all collections of logged user. Supports caching.
        /// </summary>
        /// <remarks>
        /// Probably need to add pagination support. (Not sure how to do this with caching)
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUnreadedArticlesFromAllCollections()
        {
            var loggedUser = await _dbContext.Users
                .Where(user => user.UserName == User.Identity.Name)
                .FirstAsync();

            var sources = await _dbContext.Sources.FromSql(@"SELECT DISTINCT S.Id, S.Name, S.urlHome, S.UrlRss FROM Sources AS S
                                        JOIN UserCollectionSources AS UCS ON S.Id = UCS.SourceId
                                        JOIN UserCollections AS UC ON UC.Id = UCS.UserCollectionId
                                        WHERE UC.ApplicationUserId = @p0
                                        ", loggedUser.Id).ToArrayAsync();

            List<ArticleViewModel> result = new List<ArticleViewModel>();

            foreach (Source source in sources)
            {
                IEnumerable<ArticleViewModel> articles;
                if (_cache.TryGetValue($"cachedUserArticles_{loggedUser.UserName}_{source.Id}", out articles)){

                    result.AddRange(articles);
                    continue;
                }
                articles = await _dbContext.Articles.FromSql(@"SELECT A.Id, Content, Link, PublishDate, SourceId, Title 
                        FROM Articles AS A 
                        LEFT JOIN UserReads AS R ON A.Id = R.ArticleId
                        WHERE (R.ApplicationUserId != @p0 OR R.ApplicationUserId IS NULL)
                        AND A.SourceId = @p1", loggedUser.Id, source.Id)
                    .Select(article => new ArticleViewModel
                    {
                        Id = article.Id,
                        SourceId = article.SourceId,
                        Link = article.Link,
                        Title = article.Title,
                        Content = article.Content,
                        PublishDate = article.PublishDate
                    }).ToArrayAsync();
                result.AddRange(articles);
                _cache.Set($"cachedUserArticles_{loggedUser.UserName}_{source.Id}", articles, TimeSpan.FromMinutes(5));
            }


            return Ok(result);
        }
        /// <summary>
        /// Get unreaded articles for specified collection of logged user. Supports caching.
        /// </summary>
        /// <remarks>
        /// Probably need to add pagination support. (Not sure how to do this with caching)
        /// </remarks>
        /// <param name="collectionName"><see cref="WebApp.Models.UserCollection"/> name.</param>
        /// <returns><see cref="IEnumerable{ArticleViewModel}"/> of unreaded articles.</returns>
        [HttpGet("{*collectionName}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<ArticleViewModel>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUnreadedArticlesInCollection(string collectionName)
        {
            var loggedUser = await _dbContext.Users
                .Where(user => user.UserName == User.Identity.Name)
                .Include(user => user.UserCollections)
                .FirstAsync();

            var userCollection = loggedUser.UserCollections.FirstOrDefault(collection => collection.Name == collectionName);
            if (userCollection == null)
            {
                return NotFound($"Collection with specified name was not found ({collectionName})!");
            }

            var subscriptions = await _dbContext.UserCollectionSources.Where(source => source.UserCollectionId == userCollection.Id).ToArrayAsync();

            var result = new List<ArticleViewModel>();
            foreach (var subscription in subscriptions)
            {
                IEnumerable<ArticleViewModel> articles;
                // Trying to retrieve cached articles
                if (_cache.TryGetValue($"cachedUserArticles_{loggedUser.UserName}_{subscription.SourceId}", out articles))
                {
                    result.AddRange(articles);
                    continue;
                }
                // Otherwise retrieving articles from database
                articles = await _dbContext.Articles.FromSql(@"SELECT A.Id, Content, Link, PublishDate, SourceId, Title 
                        FROM Articles AS A 
                        LEFT JOIN UserReads AS R ON A.Id = R.ArticleId
                        WHERE (R.ApplicationUserId != @p0 OR R.ApplicationUserId IS NULL)
                        AND A.SourceId = @p1", loggedUser.Id, subscription.SourceId)
                    .Select(article => new ArticleViewModel
                    {
                        Id = article.Id,
                        SourceId = article.SourceId,
                        Link = article.Link,
                        Title = article.Title,
                        Content = article.Content,
                        PublishDate = article.PublishDate
                    }).ToArrayAsync();
                result.AddRange(articles);
                _cache.Set($"cachedUserArticles_{loggedUser.UserName}_{subscription.SourceId}", articles, TimeSpan.FromMinutes(5));
            }

            return Ok(result);
        }
        /// <summary>
        /// Marks a specified <see cref="WebApp.Models.Article"/> for logged user as read.
        /// </summary>
        /// <param name="articleId"><see cref="WebApp.Models.Article"/> primary key.</param>
        /// <returns></returns>
        [HttpPost("markAsRead")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MarkAsRead(int articleId)
        {
            var loggedUser = await _dbContext.Users
                .Where(user => user.UserName == User.Identity.Name)
                .FirstOrDefaultAsync();

            if (!_dbContext.Articles.Any(article => article.Id == articleId))
            {
                return NotFound($"Article with specified id was not found ({articleId})!");
            }

            await _dbContext.UserReads.AddAsync(new UserRead
            {
                ArticleId = articleId,
                ApplicationUserId = loggedUser.Id
            });
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}