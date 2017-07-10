using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Route("api/tags")]
    public class TagController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="dbContext"><see cref="ApplicationDbContext"/> object.</param>
        public TagController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        /// <summary>
        /// Get a list of <see cref="Tag"/>.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TagViewModel>), 200)]
        public async Task<IActionResult> GetTags()
        {
            return Ok(await _dbContext.Tags.Select(tag => new TagViewModel
            {
                Id = tag.Id,
                Name = tag.Name
            }).ToArrayAsync());
        }
        /// <summary>
        /// Create a new <see cref="Tag"/>. (Not sure if this is needed. Tags can also be created with creating a source)
        /// </summary>
        /// <param name="name">Name for new tag.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(200)]
        public IActionResult CreateTag(string name)
        {
            if (_dbContext.Tags.Any(tag => tag.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest($"Tag with that name is already defined {name}!");
            }

            var result = new Tag
            {
                Name = name
            };

            _dbContext.Tags.Add(result);
            _dbContext.SaveChanges();

            return Ok();
        }
        /// <summary>
        /// Change tag.
        /// </summary>
        /// <param name="tagToChange"></param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangeTag([FromBody] TagViewModel tagToChange)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(tagToChange);
            }

            var savedTag = await _dbContext.Tags.SingleOrDefaultAsync(tag => tag.Id == tagToChange.Id);
            if (savedTag == null)
            {
                return NotFound($"Tag with specified id was not found ({tagToChange.Id})!");
            }
            savedTag.Name = tagToChange.Name;

            _dbContext.Tags.Update(savedTag);
            _dbContext.SaveChanges();

            return Ok();
        }
    }
}