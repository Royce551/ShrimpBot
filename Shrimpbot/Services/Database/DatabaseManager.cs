using Discord;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;

namespace Shrimpbot.Services.Database
{
    /// <summary>
    /// Provides static methods for interacting with the database.
    /// </summary>
    public class DatabaseManager
    {
        /// <summary>
        /// The directory where the database is located.
        /// </summary>
        public static string DatabaseDirectory;
        private static readonly LiteDatabase Database;
        static DatabaseManager()
        {
            DatabaseDirectory = Path.Combine(Directory.GetCurrentDirectory(),
                                                "Database"
                                                );
            if (!Directory.Exists(DatabaseDirectory)) Directory.CreateDirectory(DatabaseDirectory);
            Database = new LiteDatabase(Path.Combine(DatabaseDirectory, "database.sdb1"));
        }
        /// <summary>
        /// Gets a <see cref="DatabaseUser"/> from the database. Creates a user if they aren't already in the database.
        /// </summary>
        /// <param name="id">The Discord user ID of the user.</param>
        public static DatabaseUser GetUser(ulong id)
        {
            DatabaseUser user;

            user = Database.GetCollection<DatabaseUser>("Users").FindOne(x => id == x.Id);
            if (user is null)
            {
                LoggingService.Log(LogSeverity.Verbose, "Created a user!");
                Database.GetCollection<DatabaseUser>("Users").Insert(new DatabaseUser { Id = id });
                user = Database.GetCollection<DatabaseUser>("Users").FindOne(x => id == x.Id);
            }
            return user;
        }
        /// <summary>
        /// Writes a <see cref="DatabaseUser"/> to the database. Creates a user if they aren't already in the database.
        /// </summary>
        /// <param name="id">The Discord user ID of the user.</param>
        public static void WriteUser(DatabaseUser user)
        {
            if (!Database.GetCollection<DatabaseUser>("Users").Update(user))
            {
                Database.GetCollection<DatabaseUser>("Users").Insert(user);
            }
        }
        /// <summary>
        /// Gets a list of all users in the database.
        /// </summary>
        /// <returns></returns>
        public static List<DatabaseUser> GetAllUsers()
        {
            var z = new List<DatabaseUser>();
            if (Database.GetCollection<DatabaseUser>("Users").Query().ToList().Count == 0)
                Console.WriteLine(z.Count);
            z = Database.GetCollection<DatabaseUser>("Users").Query().ToList();
            return z;
        }
        /// <summary>
        /// Gets a <see cref="DatabaseServer"/> from the database. Creates a server if they aren't already in the database.
        /// </summary>
        /// <param name="id">The Discord server ID of the server.</param>
        public static DatabaseServer GetServer(ulong id)
        {
            DatabaseServer server;

            server = Database.GetCollection<DatabaseServer>("Servers").FindOne(x => id == x.Id);
            if (server is null)
            {
                LoggingService.Log(LogSeverity.Verbose, "Created a server!");
                Database.GetCollection<DatabaseServer>("Servers").Insert(new DatabaseServer { Id = id });
                server = Database.GetCollection<DatabaseServer>("Servers").FindOne(x => id == x.Id);
            }
            return server;
        }
        /// <summary>
        /// Writes a <see cref="DatabaseServer"/> to the database. Creates a server if they aren't already in the database.
        /// </summary>
        /// <param name="id">The Discord server ID of the server.</param>
        public static void WriteServer(DatabaseServer server)
        {
            if (!Database.GetCollection<DatabaseServer>("Servers").Update(server))
            {
                Database.GetCollection<DatabaseServer>("Servers").Insert(server);
            }
        }
        /// <summary>
        /// Gets a list of all servers in the database.
        /// </summary>
        /// <returns></returns>
        public static List<DatabaseServer> GetAllServers()
        {
            var z = new List<DatabaseServer>();
            if (Database.GetCollection<DatabaseServer>("Servers").Query().ToList().Count == 0)
                Console.WriteLine(z.Count);
            z = Database.GetCollection<DatabaseServer>("Servers").Query().ToList();
            return z;
        }
        /// <summary>
        /// Gets a list of images of the selected type.
        /// </summary>
        /// <param name="type">The type of images to get.</param>
        public static List<DatabaseImage> GetImages(ImageType type) =>
            Database.GetCollection<DatabaseImage>("Images").Query()
            .Where(x => x.Type == type)
            .ToList();
        /// <summary>
        /// Creates a <see cref="DatabaseImage"./>
        /// </summary>
        /// <param name="path">The path of the image. Ideally this would be a relative path.</param>
        /// <param name="type">The type of the image</param>
        /// <param name="image">The <see cref="CuteImage"/> that represents this image.</param>
        public static void CreateImage(string path, ImageType type, CuteImage image) =>
            Database.GetCollection<DatabaseImage>("Images").Insert(new DatabaseImage
            {
                DatabaseImageId = path,
                Type = type,
                Image = image
            });
        /// <summary>
        /// Updates a <see cref="DatabaseImage"/>.
        /// </summary>
        public static void UpdateImage(DatabaseImage image) =>
            Database.GetCollection<DatabaseImage>("Images").Update(image);

        public static void ExecuteSql(string sql) => Database.Execute(sql);
    }
}
