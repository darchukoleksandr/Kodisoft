using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Route("api/collections")]
    public class CollectionController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="dbContext"><see cref="ApplicationDbContext"/> object.</param>
        public CollectionController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        /// <summary>
        /// Get logged user collections.
        /// </summary>
        /// <returns><see cref="IEnumerable{UserCollectionViewModel}"/></returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<UserCollectionViewModel>), 200)]
        public async Task<IActionResult> GetLoggedUserCollections()
        {
            var loggedUser = await _dbContext.Users
                .Where(user => user.UserName == User.Identity.Name)
                .Include(user => user.UserCollections)
                .FirstAsync();

            var result = loggedUser.UserCollections
                .Select(collection => new UserCollectionViewModel
                {
                    Id = collection.Id,
                    Name = collection.Name,
                    Sources = _dbContext.UserCollectionSources
                        .Where(source => source.UserCollectionId == collection.Id)
                        .Select(source => new SourceViewModel
                        {
                            Id = source.Source.Id,
                            Name = source.Source.Name,
                            UrlHome = source.Source.UrlHome,
                            UrlRss = source.Source.UrlRss,
                        }).ToArray()
                });

            return Ok(result);
        }
        /// <summary>
        /// Create new <see cref="UserCollection"/>.
        /// </summary>
        /// <param name="name">Name for collection.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateCollectionForLoggedUser(string name)
        {
            var loggedUser = await _dbContext.Users
                .Include(user => user.UserCollections)
                .FirstOrDefaultAsync(user => user.UserName == User.Identity.Name);

            if (loggedUser.UserCollections.Any(subscription => subscription.Name == name))
            {
                return BadRequest($"User already have collection with specified name ({name})!");
            }

            var result = new UserCollection
            {
                ApplicationUserId = loggedUser.Id,
                Name = name
            };

            await _dbContext.UserCollections.AddAsync(result);
            await _dbContext.SaveChangesAsync();

            return Ok(result.Id);
        }
        /// <summary>
        /// Remove <see cref="UserCollection"/> by primary key.
        /// </summary>
        /// <param name="collectionId">Primary key of collection.</param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveCollectionForLoggedUser(int collectionId)
        {
            var loggedUser = await _dbContext.Users
                .Include(user => user.UserCollections)
                .FirstOrDefaultAsync(user => user.UserName == User.Identity.Name);

            var collection = loggedUser.UserCollections.FirstOrDefault(subscription => subscription.Id == collectionId);
            if (collection == null)
            {
                return NotFound($"Collection with specified id was not found ({collectionId})!");
            }

            _dbContext.UserCollections.Remove(collection);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }


        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetCollectionById(int id)
        //{
        //    var result = await _dbContext.UserCollections
        //        .FirstOrDefaultAsync(collection => collection.Id == id);

        //    if (result == null)
        //    {
        //        return NotFound($"Collection with specified id was not found ({id})!");
        //    }

        //    return Ok(result);
        //}
    }
}