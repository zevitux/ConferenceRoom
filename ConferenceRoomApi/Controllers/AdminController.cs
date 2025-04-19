using ConferenceRoomApi.Models;
using ConferenceRoomApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRoomApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AdminController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("users/{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userRepository.GetUserById(id);
            if(user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.Id)
                return BadRequest("User ID mismatch");

            try
            {
                var updatedUser = await _userRepository.UpdateAsync(user);
                return Ok(updatedUser);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("User not found");
            }
        }

        [HttpPost("users")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            var newUser = await _userRepository.AddAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
        }
    }
}
