using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tamro.Controllers.DTOs;
using Tamro.Models;

namespace Tamro.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RosterDbContext _dbContext;

        public UsersController(RosterDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// GET (Read all) /users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerator<UserDTO>>> GetAll()
        {
            var users = await _dbContext.User.ToArrayAsync();
            return Ok(users.Select(s => s.ToDTO()));
        }

        /// <summary>
        /// GET (Read) /user/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDTO>> Get(int id)
        {
            var user = await _dbContext.User.FindAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user.ToDTO());
        }

        /// <summary>
        /// POST (Create) /user
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserDTO>> Create([FromBody] UserDTO userDto)
        {
            if (string.IsNullOrEmpty(userDto.Name))
                return BadRequest();

            var existingUser = await _dbContext.User.FindAsync(userDto.Id);
            if (existingUser != null)
                return Conflict();

            var userToAdd = userDto.ToModel();//@class);
            _dbContext.User.Add(userToAdd);
            await _dbContext.SaveChangesAsync();
            var updatedUserDto = userToAdd.ToDTO();

            return CreatedAtAction(nameof(Get), new {id = userDto.Id}, updatedUserDto);
        }

        /// <summary>
        /// DELETE /user/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDTO>> Delete(int id)
        {
            var user = await _dbContext.User.FindAsync(id);
            if (user == null)
                return NotFound();

            _dbContext.User.Remove(user);
            await _dbContext.SaveChangesAsync();
            return Ok(user.ToDTO());
        }

        /// <summary>
        /// PUT (Update) a User by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UserDTO userDto)
        {
            if (userDto.Id != id || string.IsNullOrEmpty(userDto.Name))
                return BadRequest();

            var user = await _dbContext.User.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Update(userDto);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }

    public static class UserExtensions
    {
        public static User ToModel(this UserDTO userDto)
        {
            return new User
            {
                Id = userDto.Id,
                Name = userDto.Name,
                Surname = userDto.Surname,
            };
        }

        public static void Update(this User userToUpdate, UserDTO userDto)
        {
            if (userDto.Id != userToUpdate.Id) throw new NotSupportedException();
            userToUpdate.Name = userDto.Name;
            userToUpdate.Surname = userDto.Surname;
        }

        public static UserDTO ToDTO(this User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname
            };
        }
    }
}
