using BarcelonaGamesServer.Models.User;
using BarcelonaGamesServer.Services.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.Services.Users;
using BarcelonaGamesServer.Utils;
using System.Security.Authentication;
using BarcelonaGamesServer.Exceptions;
using UserAlreadyExistsException = BarcelonaGamesServer.Exceptions.UserAlreadyExistsException;


namespace BarcelonaGamesServer.Services.Users
{
    public class UsersService
    {
        private readonly IConfiguration _configuration;

        public UsersService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<object> CreateUserAsync(Models.User.User newUser)
        {
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                var checkUserCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email", connection);
                checkUserCmd.Parameters.AddWithValue("@Email", newUser.Email);

                int exists = (int)await checkUserCmd.ExecuteScalarAsync();
                if (exists > 0)
                {
                    throw new UserAlreadyExistsException("User with this email already exists.");
                }

                newUser.Password = PasswordHelper.GeneratePassword(newUser.Password);

                var cmd = new SqlCommand("INSERT INTO Users (Name, Email, Password, Address, Phone, IsBusiness, IsAdmin, Image) VALUES (@Name, @Email, @Password, @Address, @Phone, @IsBusiness, @IsAdmin, @Image); SELECT SCOPE_IDENTITY();", connection);
                cmd.Parameters.AddWithValue("@Name", newUser.Name);
                cmd.Parameters.AddWithValue("@Email", newUser.Email);
                cmd.Parameters.AddWithValue("@Password", newUser.Password);
                cmd.Parameters.AddWithValue("@Address", newUser.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", newUser.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsBusiness", newUser.IsBusiness);
                cmd.Parameters.AddWithValue("@IsAdmin", newUser.IsAdmin);
                cmd.Parameters.AddWithValue("@Image", newUser.Image ?? (object)DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                newUser.Id = Convert.ToString(result);
                return new { newUser.Id, newUser.Name, newUser.Email };
            }
        }

        public async Task<List<Models.User.User>> GetUsersAsync()
        {
            var users = new List<Models.User.User>();
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                var cmd = new SqlCommand("SELECT Id, Name, Email, Address, Phone, IsBusiness, IsAdmin, Image FROM Users", connection);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            Id = reader["Id"].ToString(),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader["Address"].ToString(),
                            Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader["Phone"].ToString(),
                            IsBusiness = (bool)reader["IsBusiness"],
                            IsAdmin = (bool)reader["IsAdmin"],
                            Image = reader.IsDBNull(reader.GetOrdinal("Image")) ? null : reader["Image"].ToString(),
                        });
                    }
                }
            }
            return users;
        }

        public async Task<User> GetOneUserAsync(string userId)
        {
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                var cmd = new SqlCommand("SELECT Id, Name, Email, Address, Phone, IsBusiness, IsAdmin, Image FROM Users WHERE Id = @Id", connection);
                cmd.Parameters.AddWithValue("@Id", userId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            Id = reader["Id"].ToString(),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader["Address"].ToString(),
                            Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader["Phone"].ToString(),
                            IsBusiness = (bool)reader["IsBusiness"],
                            IsAdmin = (bool)reader["IsAdmin"],
                            Image = reader.IsDBNull(reader.GetOrdinal("Image")) ? null : reader["Image"].ToString(),
                        };
                    }
                    else
                    {
                        throw new UserNotFoundException($"User with ID {userId} not found.");
                    }
                }
            }
        }

        public async Task DeleteUserAsync(string userId)
        {
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                var cmd = new SqlCommand("DELETE FROM Users WHERE Id = @Id", connection);
                cmd.Parameters.AddWithValue("@Id", userId);

                var result = await cmd.ExecuteNonQueryAsync();
                if (result == 0)
                {
                    throw new UserNotFoundException($"User with ID {userId} not found.");
                }
            }
        }

        public async Task<User> EditUserAsync(string userId, User updatedUser)
        {
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                var cmd = new SqlCommand(@"UPDATE Users SET Name = @Name, Email = @Email, Address = @Address, Phone = @Phone, IsBusiness = @IsBusiness, IsAdmin = @IsAdmin, Image = @Image WHERE Id = @Id", connection);
                cmd.Parameters.AddWithValue("@Id", userId);
                cmd.Parameters.AddWithValue("@Name", updatedUser.Name);
                cmd.Parameters.AddWithValue("@Email", updatedUser.Email);
                cmd.Parameters.AddWithValue("@Address", updatedUser.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", updatedUser.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsBusiness", updatedUser.IsBusiness);
                cmd.Parameters.AddWithValue("@IsAdmin", updatedUser.IsAdmin);
                cmd.Parameters.AddWithValue("@Image", updatedUser.Image ?? (object)DBNull.Value);

                var result = await cmd.ExecuteNonQueryAsync();
                if (result == 0)
                {
                    throw new UserNotFoundException($"User with ID {userId} not found.");
                }

                updatedUser.Password = ""; // Ensure password is not returned
                return updatedUser;
            }
        }

        public async Task<User> LoginAsync(LoginModel loginModel)
        {
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                var cmd = new SqlCommand("SELECT * FROM Users WHERE Email = @Email", connection);
                cmd.Parameters.AddWithValue("@Email", loginModel.Email);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var hashedPassword = reader["Password"].ToString();
                        if (PasswordHelper.VerifyPassword(hashedPassword, loginModel.Password))
                        {
                            return new User
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader["Address"].ToString(),
                                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader["Phone"].ToString(),
                                IsBusiness = (bool)reader["IsBusiness"],
                                IsAdmin = (bool)reader["IsAdmin"],
                                Image = reader.IsDBNull(reader.GetOrdinal("Image")) ? null : reader["Image"].ToString(),
                            };
                        }
                        else
                        {
                            throw new Exception("Incorrect password.");
                        }
                    }
                    else
                    {
                        throw new Exception("User not found.");
                    }
                }
            }
        }

    }
}
