using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    /// <summary>
    /// CRUD controller for <see cref="WebApp.Models.Source"/> model.
    /// </summary>
    [Route("api/sources")]
    public class SourceController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISourceSaver _sourceSaver;
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="dbContext"><see cref="ApplicationDbContext"/> object.</param>
        /// <param name="sourceSaver"><see cref="ISourceSaver"/> objcet.</param>
        public SourceController(ApplicationDbContext dbContext, ISourceSaver sourceSaver)
        {
            _dbContext = dbContext;
            _sourceSaver = sourceSaver;
        }
        /// <summary>
        /// Get sources including their popularity (amount of subscriptions). Supports pagination.
        /// </summary>
        /// <param name="page">Current page (default = 1).</param>
        /// <param name="pageSize">Amount of items per page.</param>
        /// <returns><see cref="IEnumerable{SourceViewModel}"/></returns>
        [HttpGet("popularity")]
        [ProducesResponseType(typeof(IEnumerable<SourceViewModel>), 200)]
        public async Task<IActionResult> GetSourcesByPopularity(int page = 1, int pageSize = 5)
        {
            var result = await _dbContext.Sources
                .FromSql(@"SELECT S.Id, S.Name, S.UrlHome, S.UrlRss FROM Sources AS S 
                        LEFT JOIN UserCollectionSources AS C ON S.Id = C.SourceId 
                        GROUP BY Id, Name, UrlHome, UrlRss 
                        ORDER BY Count(C.UserCollectionId) DESC
                        OFFSET @p0 ROWS
                        FETCH NEXT @p1 ROWS ONLY",
                        (page - 1) * pageSize,
                        pageSize
                        ).Select(sources => new SourceViewModel
                        {
                            Id = sources.Id,
                            Name = sources.Name,
                            UrlHome = sources.UrlHome,
                            UrlRss = sources.UrlRss,
                            Tags = _dbContext.Tags.Join(_dbContext.SourceTags.Where(sourceTag => sourceTag.SourceId == sources.Id),
                                tag => tag.Id,
                                sourceTag => sourceTag.TagId,
                                (tag, sourceTag) => new TagViewModel
                                {
                                    Id = tag.Id,
                                    Name = tag.Name
                                })
                                .ToList()
                        }).ToArrayAsync();

            return Ok(result);
        }
        /// <summary>
        /// Get sources. Supports pagination.
        /// </summary>
        /// <param name="page">Current page (default = 1).</param>
        /// <param name="pageSize">Amount of items per page.</param>
        /// <returns><see cref="IEnumerable{SourceViewModel}"/></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SourceViewModel>), 200)]
        public async Task<IActionResult> GetSources(int page = 1, int pageSize = 5)
        {
            var result = await _dbContext.Sources
                .Include(source => source.SourceTags)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(source => new SourceViewModel
                {
                    Id = source.Id,
                    Name = source.Name,
                    UrlHome = source.UrlHome,
                    UrlRss = source.UrlRss,
                    Tags = source.SourceTags
                        .Where(sourceTag => sourceTag.SourceId == source.Id)
                        .Select(sourceTag => new TagViewModel
                        {
                            Id = sourceTag.Tag.Id,
                            Name = sourceTag.Tag.Name
                        }).ToList()
                })
                .ToArrayAsync();

            return Ok(result);
        }
        /// <summary>
        /// Creates new <see cref="WebApp.Models.Source"/>. Before creating downloads feed document and verifies it for supporting.
        /// </summary>
        /// <param name="newSource"><see cref="WebApp.Models.SourceViewModel"/> model.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> NewSource(SourceViewModel newSource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(newSource);
            }

            newSource = FormatSource(newSource);

            if (newSource.UrlHome.Equals(newSource.UrlRss, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Home url and rss url are same! Please provide different urls!");
            }
            if (_dbContext.Sources.Any(source => source.UrlRss == newSource.UrlRss))
            {
                return BadRequest("Source with same rss url is already saved!");
            }
            if (_dbContext.Sources.Any(source => source.UrlHome == newSource.UrlHome))
            {
                return BadRequest("Source with same home url is already saved!");
            }
            if (!await XmlFeedParser.IsFeedUrl(newSource.UrlRss))
            {
                return BadRequest("Source feed url contains unsupported feed type!");
            }

            var result = new Source
            {
                Name = newSource.Name,
                UrlHome = newSource.UrlHome,
                UrlRss = newSource.UrlRss
            };
            //----------------- NEW START
            foreach (var newSourceTag in newSource.Tags)
            {
                var existedTag = _dbContext.Tags.FirstOrDefault(tag => tag.Name == newSourceTag.Name);
                if (existedTag == null)
                {
                    existedTag = new Tag
                    {
                        Name = newSourceTag.Name
                    };
                    _dbContext.Tags.Add(existedTag);
                    var sourceTag1 = new SourceTag
                    {
                        Source = result,
                        Tag = existedTag
                    };
                    _dbContext.SourceTags.Add(sourceTag1);
                    result.SourceTags.Add(sourceTag1);
                    continue;
                }

                var sourceTag2 = new SourceTag
                {
                    Source = result,
                    Tag = existedTag
                };
                _dbContext.SourceTags.Add(sourceTag2);
                result.SourceTags.Add(sourceTag2);
            }
            //------------------ NEW END

            await _dbContext.Sources.AddAsync(result);
            await _dbContext.SaveChangesAsync();

            await _sourceSaver.Save(result);

            return Ok();
        }
        /// <summary>
        /// Removes specified <see cref="WebApp.Models.Source"/>.
        /// </summary>
        /// <param name="sourceId"><see cref="WebApp.Models.Source"/> primary key.</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveSource(int sourceId)
        {
            var sourceToDelete = await _dbContext.Sources.SingleOrDefaultAsync(source => source.Id == sourceId);
            
            if (sourceToDelete == null)
            {
                return NotFound($"Source with specified id was not found ({sourceId})!");
            }

            _dbContext.Remove(sourceToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
        /// <summary>
        /// Delete last symbol from url like https://website.com/feed///.
        /// </summary>
        /// <param name="source"><see cref="WebApp.Models.Source"/> to format</param>
        /// <returns>Formatted <see cref="WebApp.Models.Source"/></returns>
        private SourceViewModel FormatSource(SourceViewModel source)
        {
            while (source.UrlHome[source.UrlHome.Length - 1] == '/')
            {
                source.UrlHome = source.UrlHome.Remove(source.UrlHome.Length - 1, 1);
            }
            while (source.UrlRss[source.UrlRss.Length - 1] == '/')
            {
                source.UrlRss = source.UrlRss.Remove(source.UrlRss.Length - 1, 1);
            }
            return source;
        }



        //[HttpGet("addTag")]
        //public IActionResult AddTagToSubs([FromQuery] int id, [FromQuery] string tagName)
        //{
        //    var loggedUser = _dbContext.Users
        //        .FirstOrDefault(user => user.UserName == User.Identity.Name);

        //    var result = _dbContext.Subscriptions
        //        .FirstOrDefault(subscription => subscription.Id == id);
        //    if (result == null)
        //    {
        //        return NotFound();
        //    }
        //    var tagToAdd = _dbContext.Tags.FirstOrDefault(tag => tag.Name.Equals(tagName));
        //    if (tagToAdd == null)
        //    {
        //        tagToAdd = new Tag
        //        {
        //            Name = tagName
        //        };
        //        _dbContext.Tags.Add(tagToAdd);
        //    }

        //    var subscriptionTag = new SubscriptionTag
        //    {
        //        Tag = tagToAdd,
        //        Subscription = result
        //    };

        //    //TODO DUPLICATE VALUES SQLEXCEPTION!!!
        //    _dbContext.SubscriptionTags.Add(subscriptionTag);

        //    _dbContext.SaveChanges();
        //    return Ok();
        //}
    }
}