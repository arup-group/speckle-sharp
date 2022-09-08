using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Speckle.Core.Credentials;
using SpeckleConnectionManagerUI.Models;
using static SpeckleConnectionManagerUI.Services.RefreshTokenAction;

namespace SpeckleConnectionManagerUI.Services
{
    public class Sqlite
    {
        public static List<Speckle.Core.Credentials.Account> GetData()
        {
            List<Speckle.Core.Credentials.Account?> entries = new List<Speckle.Core.Credentials.Account>();

            using (SqliteConnection db =
                new SqliteConnection($"Filename={Constants.DatabasePath}"))
            {
                db.Open();

                var createTableCommand = db.CreateCommand();
                createTableCommand.CommandText =
                    @"
                    CREATE TABLE IF NOT EXISTS objects (hash varchar, content varchar);
                ";
                createTableCommand.ExecuteNonQuery();

                if (ContainsDuplicateAccounts(db))
                {
                    RemoveDuplicateAccounts(db);
                }

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT content from objects", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    var row = query.GetString(0);
                    entries.Add(JsonSerializer.Deserialize<Speckle.Core.Credentials.Account>(row));
                }

                db.Close();
            }

            return entries;
        }

        /// <summary>
        /// Checks accounts database for multiple accounts with the same hash. It does not check equality of account content.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private static bool ContainsDuplicateAccounts(SqliteConnection db)
        {
            // Returns count of accounts with duplicated hash
            var selectCmd = new SqliteCommand
              ("SELECT COUNT(*) FROM (SELECT hash FROM objects GROUP BY hash HAVING COUNT(*) > 1)", db);

            var count = 0;

            using (var query = selectCmd.ExecuteReader())
            {
              while (query.Read())
              {
                count = query.GetInt16(0);
              }
            }

            return count > 0;
        }

        private static void RemoveDuplicateAccounts(SqliteConnection db)
        {
            var command = new SqliteCommand("", db);

            // Hashes are duplicated but content is not
            if (ContentUnique(db))
            {
                // Creates blank table with schema of objects table
                command.CommandText = "CREATE TABLE objects_new AS SELECT * FROM objects WHERE 0";
                command.ExecuteNonQuery();

                var updatedAccounts = GetUpdatedAccounts(db);

                foreach (var acc in updatedAccounts)
                {
                    string jsonString = JsonSerializer.Serialize(acc);

                    command.CommandText =
                    @"
                            INSERT INTO objects_new(hash, content)
                            VALUES (@hash, @content)
                          ";

                    command.Parameters.AddWithValue("@hash", acc.id);
                    command.Parameters.AddWithValue("@content", jsonString);
                    command.ExecuteNonQuery();
                }
            }

            // Both hashes and content are duplicated, remove duplicate entries from database
            else
            {
                command.CommandText = "CREATE TABLE objects_new AS SELECT hash, content FROM objects GROUP BY hash, content";
                command.ExecuteNonQuery();
            }

            command.CommandText = "DROP TABLE objects";
            command.ExecuteNonQuery();

            command.CommandText = "ALTER TABLE objects_new RENAME TO objects";
            command.ExecuteNonQuery();

            command.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS index_objects_hash ON objects(hash)";
            command.ExecuteNonQuery();
    }

        private static bool ContentUnique(SqliteConnection db)
        {
            // Returns true if duplicate hashes exist with different (unique) content, else false
            var command = new SqliteCommand
                  ("SELECT ((SELECT COUNT(*) FROM (SELECT * FROM objects GROUP BY hash, content)) > (SELECT COUNT(*) FROM (SELECT * FROM objects GROUP BY hash)))", db);

            bool isUniqueContent = true;

            using (var query = command.ExecuteReader())
            {
              while (query.Read())
              {
                isUniqueContent = query.GetBoolean(0);
              }
            }

            return isUniqueContent;
        }

        public static List<Account> GetUpdatedAccounts(SqliteConnection connection)
        {
            HttpClient client = new();
            SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT * FROM objects GROUP BY hash", connection);

            SqliteDataReader query = selectCommand.ExecuteReader();

            var entries = new List<Row>();

            while (query.Read())
            {
              object[] objs = new object[3];
              var row = query.GetValues(objs);

              entries.Add(new Row
              {
                hash = objs[0].ToString(),
                content = JsonSerializer.Deserialize<Speckle.Core.Credentials.Account>(objs[1].ToString())
              });
            }

            var updatedAccounts = new List<Account>();


            foreach (var entry in entries)
            {
              var content = entry.content;
              var isDefault = entry.content.isDefault;
              var url = content.serverInfo.url;
              Console.WriteLine($"Auth token: {content.token}");
              client.DefaultRequestHeaders.Add("Authorization", $"Bearer {content.token}");

              HttpResponseMessage response;
              try
              {
                response = client.PostAsJsonAsync($"{url}/auth/token", new
                {
                  appId = "sdm",
                  appSecret = "sdm",
                  refreshToken = content.refreshToken,
                }).Result;
              }
              catch
              {
                client.DefaultRequestHeaders.Remove("Authorization");
                continue;
              }
              Console.WriteLine(response.StatusCode);
              if (response.StatusCode != HttpStatusCode.OK)
              {
                continue;
              }
              var tokens = response.Content.ReadFromJsonAsync<Tokens>().Result;
              content.token = tokens.token;
              content.refreshToken = tokens.refreshToken;

              Console.WriteLine(tokens.token);

              HttpResponseMessage info;
              try
              {
                info = client.PostAsJsonAsync($"{url}/graphql", new
                {
                  query = "{\n  user {\n    id\n    name\n    email\n    company\n    avatar\n} serverInfo {\n name \n company \n canonicalUrl \n }\n}\n"
                }).Result;
              }
              catch
              {
                client.DefaultRequestHeaders.Remove("Authorization");
                continue;
              }

              client.DefaultRequestHeaders.Remove("Authorization");
              Console.WriteLine(response.StatusCode);
              if (response.StatusCode != HttpStatusCode.OK)
              {
                continue;
              }

              var infoContent = info.Content.ReadFromJsonAsync<InfoData>().Result;

              if (infoContent == null) return null;

              var serverInfo = infoContent.data.serverInfo;
              serverInfo.url = url;

              var userInfo = infoContent.data.user;

              var updateContent = new Speckle.Core.Credentials.Account()
              {
                token = tokens.token,
                refreshToken = tokens.refreshToken,
                isDefault = isDefault,
                serverInfo = serverInfo,
                userInfo = userInfo
              };

              updatedAccounts.Add(updateContent);

              
            };

            return updatedAccounts;
          }

        public static void DeleteAuthData()
        {
            using (SqliteConnection db =
                new SqliteConnection($"Filename={Constants.DatabasePath}"))
            {
                db.Open();
                var truncateTableCommand = db.CreateCommand();
                truncateTableCommand.CommandText =
                    @"
                    DELETE FROM objects;
                ";
                truncateTableCommand.ExecuteNonQuery();

            }
        }

        public static void SetDefaultServer(string serverUrl, bool isDefault)
        {
            using (SqliteConnection db =
                new SqliteConnection($"Filename={Constants.DatabasePath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT * from objects WHERE instr(content, '{serverUrl}') > 0", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    var objs = new object[3];
                    query.GetValues(objs);
                    var hash = objs[0].ToString();
                    var storedContent = JsonSerializer.Deserialize<Speckle.Core.Credentials.Account>(objs[1].ToString());

                    // If the url is already stored update otherwise create a new entry.
                    if (storedContent != null)
                    {
                        var updateCommand = db.CreateCommand();
                        updateCommand.CommandText =
                            @"
                        UPDATE objects
                        SET content = @content
                        WHERE hash = @hash
                    ";

                        storedContent.isDefault = isDefault;

                        updateCommand.Parameters.AddWithValue("@hash", hash);
                        updateCommand.Parameters.AddWithValue("@content", JsonSerializer.Serialize(storedContent));
                        updateCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void RemoveServer(string serverUrl)
        {
            using (SqliteConnection db =
                new SqliteConnection($"Filename={Constants.DatabasePath}"))
            {
                db.Open();

                SqliteCommand deleteCommand = new SqliteCommand($"DELETE from objects WHERE instr(content, '{serverUrl}') > 0", db);

                deleteCommand.ExecuteNonQuery();
            }
        }
    }
}