using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Controllers
{
    /// <summary>
    /// Controller for user subscription manipulation.
    /// </summary>
    [Route("api/subscriptions")]
    public class SubscriptionController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="dbContext"><see cref="ApplicationDbContext"/> object.</param>
        public SubscriptionController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        /// <summary>
        /// Creates a subscription for logged user.
        /// </summary>
        /// <param name="collectionId"><see cref="WebApp.Models.UserCollection"/> primary key.</param>
        /// <param name="sourceId"><see cref="WebApp.Models.Source"/> primary key.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddSourceToCollectionForLoggedUser(int collectionId, int sourceId)
        {
            var loggedUser = await _dbContext.Users
                .Where(user => user.UserName == User.Identity.Name)
                .Include(user => user.UserCollections)
                .FirstOrDefaultAsync();

            var resultCollection = loggedUser.UserCollections
                .FirstOrDefault(collection => collection.Id == collectionId);
            if (resultCollection == null)
            {
                return NotFound($"Logged user collection with specified id was not found ({collectionId})!");
            }

            var subscriptionSource = await _dbContext.Sources.FirstOrDefaultAsync(source => source.Id == sourceId);
            if (subscriptionSource == null)
            {
                return NotFound($"Source with specified id was not found ({sourceId})!");
            }

            if (_dbContext.UserCollectionSources.Any(
                source => source.UserCollectionId == collectionId && source.SourceId == sourceId))
            {
                return BadRequest("Subscription with same id already defined!");
            }

            resultCollection.UserCollectionSources.Add(new UserCollectionSource
            {
                SourceId = sourceId,
                UserCollectionId = collectionId
            });

            await _dbContext.SaveChangesAsync();
            return Ok();
        }
        /// <summary>
        /// Delete a subscription for logged user.
        /// </summary>
        /// <param name="collectionId"><see cref="WebApp.Models.UserCollection"/> primary key.</param>
        /// <param name="sourceId"><see cref="WebApp.Models.Source"/> primary key.</param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveSourceFromCollectionForLoggedUser(int collectionId, int sourceId)
        {
            var subscriptionSource = await _dbContext.UserCollectionSources.FirstOrDefaultAsync(
                source => source.SourceId == sourceId && source.UserCollectionId == collectionId);

            if (subscriptionSource == null)
            {
                return NotFound("Subscription with specified id was not found!");
            }

            _dbContext.UserCollectionSources.Remove(subscriptionSource);

            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}