using Discord;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shrimpbot.Services.Database
{
    public class DatabaseManager
    {
        public static string DatabasePath;
        private static LiteDatabase Database; 
        static DatabaseManager()
        {
            DatabasePath = Path.Combine(Directory.GetCurrentDirectory(),
                                                "Database"
                                                );
            if (!Directory.Exists(DatabasePath)) Directory.CreateDirectory(DatabasePath);
            Database = new LiteDatabase(Path.Combine(DatabasePath, "database.sdb1"));
        }
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
        public static void WriteUser(DatabaseUser user)
        {
            if (!Database.GetCollection<DatabaseUser>("Users").Update(user))
            {
                Database.GetCollection<DatabaseUser>("Users").Insert(user);
            }
        }
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
        public static void WriteServer(DatabaseServer server)
        {
            if (!Database.GetCollection<DatabaseServer>("Servers").Update(server))
            {
                Database.GetCollection<DatabaseServer>("Servers").Insert(server);
            }
        }
        public static List<DatabaseImage> GetImages(ImageType type) => 
            Database.GetCollection<DatabaseImage>("Images").Query()
            .Where(x => x.Type == type)
            .ToList();
        public static void CreateImage(string path, ImageType type, CuteImage image) =>
            Database.GetCollection<DatabaseImage>("Images").Insert(new DatabaseImage
            {
                DatabaseImageId = path,
                Type = type,
                Image = image
            });
        public static void UpdateImage(DatabaseImage image) =>
            Database.GetCollection<DatabaseImage>("Images").Update(image);

        public static void ExecuteSql(string sql) => Database.Execute(sql);
    }
}
