using BarcelonaGamesServer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BarcelonaGamesServer.Services.Data;

namespace BarcelonaGamesServer.Services.GameCardService
{
    public class GameCardService
    {
        private readonly IConfiguration _configuration;

        public GameCardService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GameCard> CreateCardAsync(GameCard newCard)
        {
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                var command = new SqlCommand("INSERT INTO GameCards (GameName, BarcelonaScore, RivalScore, Link, Image, Where) VALUES (@GameName, @BarcelonaScore, @RivalScore, @Link, @Image, @Where); SELECT SCOPE_IDENTITY();", connection);
                command.Parameters.AddWithValue("@GameName", newCard.GameName);
                command.Parameters.AddWithValue("@BarcelonaScore", newCard.BarcelonaScore);
                command.Parameters.AddWithValue("@RivalScore", newCard.RivalScore);
                command.Parameters.AddWithValue("@Link", newCard.Link ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Image", newCard.Image ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Where", newCard.Where ?? (object)DBNull.Value);

                var result = await command.ExecuteScalarAsync();
                newCard.ID = result.ToString();
            }
            return newCard;
        }

        public async Task<List<GameCard>> GetAllCardsAsync()
        {
            var cards = new List<GameCard>();
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                var command = new SqlCommand("SELECT * FROM GameCards", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var card = new GameCard
                        {
                            ID = reader["ID"].ToString(),
                            GameName = reader["GameName"].ToString(),
                            BarcelonaScore = (int)reader["BarcelonaScore"],
                            RivalScore = (int)reader["RivalScore"],
                            Link = reader["Link"].ToString(),
                            Image = reader["Image"].ToString(),
                            Where = reader["Where"].ToString(),
                        };
                        cards.Add(card);
                    }
                }
            }
            return cards;
        }

        public async Task DeleteCardAsync(string cardId)
        {
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                var command = new SqlCommand("DELETE FROM GameCards WHERE ID = @ID", connection);
                command.Parameters.AddWithValue("@ID", cardId);

                var result = await command.ExecuteNonQueryAsync();
                if (result == 0)
                {
                    throw new Exception("Card not found");
                }
            }
        }
        public async Task<List<GameCard>> GetMyCardsAsync(string userId)
        {
            var cards = new List<GameCard>();
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                var command = new SqlCommand("SELECT * FROM GameCards WHERE UserId = @UserId", connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        cards.Add(new GameCard
                        {
                            ID = reader["ID"].ToString(),
                            GameName = reader["GameName"].ToString(),
                            BarcelonaScore = (int)reader["BarcelonaScore"],
                            RivalScore = (int)reader["RivalScore"],
                            // Populate other fields as necessary
                        });
                    }
                }
            }
            return cards;
        }

        public async Task LikeCardAsync(string cardId, string userId) //need to add like table in the sql DB
        {
            // This method assumes the existence of a Likes table to manage likes.
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                // Check if the user already liked the card
                var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Likes WHERE CardId = @CardId AND UserId = @UserId", connection);
                checkCommand.Parameters.AddWithValue("@CardId", cardId);
                checkCommand.Parameters.AddWithValue("@UserId", userId);

                int count = (int)await checkCommand.ExecuteScalarAsync();
                SqlCommand command;
                if (count > 0)
                {
                    // User already liked the card, so unlike it
                    command = new SqlCommand("DELETE FROM Likes WHERE CardId = @CardId AND UserId = @UserId", connection);
                }
                else
                {
                    // Like the card
                    command = new SqlCommand("INSERT INTO Likes (CardId, UserId) VALUES (@CardId, @UserId)", connection);
                }
                command.Parameters.AddWithValue("@CardId", cardId);
                command.Parameters.AddWithValue("@UserId", userId);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> IsOwner(string cardID, string userID)
        {
            using (var connection = SqlService.CreateSqlConnection(_configuration))
            {
                var command = new SqlCommand("SELECT COUNT(*) FROM GameCards WHERE ID = @CardId AND UserId = @UserId", connection);
                command.Parameters.AddWithValue("@CardId", cardID);
                command.Parameters.AddWithValue("@UserId", userID);

                int count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
        }

    }
}
