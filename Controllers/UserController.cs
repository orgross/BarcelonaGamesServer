using BarcelonaGamesServer.Exceptions;
using BarcelonaGamesServer.Models.User;
using BarcelonaGamesServer.Services.Data;
using BarcelonaGamesServer.Services.Users;
using Microsoft.EntityFrameworkCore;

namespace BarcelonaGamesServer.Controllers
{
    public class UserController
    {
        private readonly UsersService _userService;

        public UserController(UsersService userService)
        {
            _userService = userService;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _userService.GetUsersAsync().ToListAsync();
        }

        public async Task<User> GetOneUserAsync(string id)
        {
            return await _userService.User.FindAsync(id) ?? throw new UserNotFoundException("User not found.");
        }

        public async Task<User> CreateUserAsync(User newUser)
        {
            var existingUser = await _userService.GetUsersAsync().AnyAsync(u => u.Email == newUser.Email);
            if (existingUser)
            {
                throw new UserAlreadyExistsException("User already exists.");
            }

            _userService.Users.Add(newUser);
            await _userService.SaveChangesAsync();
            return newUser; // Or return a DTO instead of the domain model
        }

        public async Task<User> EditUserAsync(string id, User updatedUser)
        {
            var user = await _userService.Users.FindAsync(id);
            if (user == null)
            {
                throw new UserNotFoundException("User not found.");
            }

            // Update user properties here
            // user.Property = updatedUser.Property;

            await _userService.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _userService.Users.FindAsync(id);
            if (user == null)
            {
                throw new UserNotFoundException("User not found.");
            }

            _userService.Users.Remove(user);
            await _userService.SaveChangesAsync();
        }
    }
}
