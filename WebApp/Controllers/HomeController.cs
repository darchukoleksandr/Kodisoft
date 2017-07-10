using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Services;

namespace WebApp.Controllers
{
    /// <summary>
    /// Basic client provider controller.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
//        private readonly Sheluder _sheluder;
        /// <summary>
        /// Public contstructor.
        /// </summary>
        /// <param name="dbContext"><see cref="WebApp.Data.ApplicationDbContext"/> object.</param>
        public HomeController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
//            _sheluder = sheluder;
        }
        /// <summary>
        /// Homepage for client.
        /// </summary>
        /// <returns>Homepage view.</returns>
        [ProducesResponseType(200)]
        public async Task<IActionResult> Index()
        {
            ViewData["sources"] = await _dbContext.Sources.ToArrayAsync();
            if (User.Identity.IsAuthenticated)
            {
                var loggedUser = _dbContext.Users.First(user => user.UserName == User.Identity.Name);
                ViewData["collections"] = await _dbContext.UserCollections
                    .Where(collection => collection.ApplicationUserId == loggedUser.Id).ToArrayAsync();
            }
            return View();
        }
        /// <summary>
        /// Custom error page.
        /// </summary>
        /// <returns>Error view.</returns>
        [ProducesResponseType(200)]
        public IActionResult Error()
        {
            return View();
        }
//        /// <summary>
//        /// Updates timeout of <see cref="Sheluder"/>. NOT WORKING
//        /// </summary>
//        /// <param name="minutes">Timeout size in minutes.</param>
//        /// <returns></returns>
//        [HttpPost("sheluder")]
//        public IActionResult UpdateSheluderTimer(int minutes)
//        {
//            _sheluder.ChangeTimeoutByMinutes(minutes);
//
//            return Ok();
//        }
    }
}
