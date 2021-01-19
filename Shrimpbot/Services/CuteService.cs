using Discord.Commands;
using Newtonsoft.Json.Linq;
using Shrimpbot.Services.Database;
using Shrimpbot.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Shrimpbot.Services
{
    /// <summary>
    /// Manages image commands and integrations with image boards.
    /// </summary>
    public class CuteService
    {
        public static HttpClient client = new HttpClient();
        public static Random Rng = new Random();
        static CuteService()
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"ShrimpBot/17.0 (https://github.com/Royce551/ShrimpBot)");
        }
        /// <summary>
        /// Gets a random image from the specified source.
        /// </summary>
        /// <param name="source">The source to get the image from.</param>
        /// <param name="type">The type of image to search for</param>
        /// <returns>A <see cref="CuteImage"/> representing the returned image.</returns>
        public static CuteImage GetImage(DatabaseManager manager, ImageSource source = ImageSource.Curated, ImageType type = ImageType.All) => source switch
        {
            ImageSource.Curated => GetImageFromImageFolder(type, manager),
            ImageSource.Online => GetImageFromOnline(type),
            ImageSource.LegacyImages => GetImageFromLegacyImageFolder(type),
            _ => GetImageFromImageFolder(type, manager)
        };
        /// <summary>
        /// Gets a random image from an image board.
        /// </summary>
        /// <param name="type">The type of image to search for</param>
        /// <returns>A <see cref="CuteImage"/> representing the returned image.</returns>
        public static CuteImage GetImageFromOnline(ImageType type)
        {
            var image = new CuteImage();
            int index = Rng.Next(1, 101);
            string tags = type switch
            {
                ImageType.Catgirls => "rating:safe 1girl cat_ears cat_tail",
                ImageType.Anime => "rating:safe -photo",
                _ => "rating:safe"
            };
            var json = JArray.Parse(client.GetStringAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&tags={tags}").Result);
            image.Uploader = json.SelectToken($"$[{index}].owner").ToString();
            image.ImageSource = json.SelectToken($"$[{index}].source").ToString();
            image.FileSource = ImageSource.Online;
            image.Path = json.SelectToken($"$[{index}].file_url").ToString();
            return image;
        }
        /// <summary>
        /// Gets a random image from the images folder.
        /// </summary>
        /// <param name="type">The type of image to search for</param>
        /// <returns>A <see cref="CuteImage"/> representing the returned image.</returns>
        public static CuteImage GetImageFromImageFolder(ImageType type, DatabaseManager manager)
        {
            var cuteImages = new List<FolderCuteImage>();
            var imagePaths = Directory.EnumerateFiles("Images", "*.*", SearchOption.AllDirectories).Where(x => Path.GetExtension(x) != ".info");
            foreach (var path in imagePaths)
            {
                var infoPath = Path.Combine(Path.GetDirectoryName(path), $"{Path.GetFileNameWithoutExtension(path)}.info");
                if (File.Exists(infoPath))
                {
                    var fields = File.ReadAllText(infoPath).Split(';');
                    cuteImages.Add(new FolderCuteImage
                    {
                        Path = path,
                        Type = ParseImageType(fields[1]),
                        Creator = fields[2],
                        ImageSource = fields[3],
                        FileSource = ImageSource.Curated
                    });
                } 
            }
            var selectedImages = new List<FolderCuteImage>();
            if (type == ImageType.All || type == ImageType.Anime) selectedImages.AddRange(cuteImages.Where(x => x.Type == ImageType.Anime).ToList());
            if (type == ImageType.All || type == ImageType.Anime || type == ImageType.Catgirls) selectedImages.AddRange(cuteImages.Where(x => x.Type == ImageType.Catgirls).ToList());

            int number = Rng.Next(0, selectedImages.Count - 1);
            var image = selectedImages[number];
            return image;
        }
        /// <summary>
        /// Gets a random Shrimpbot Legacy/Shrimpbot Python Rewrite image.
        /// </summary>
        /// <param name="type">The type of image to search for.</param>
        /// <returns>A <see cref="CuteImage"/> representing the returned image.</returns>
        public static CuteImage GetImageFromLegacyImageFolder(ImageType type)
        {
            var image = new CuteImage();
            int minNumber = type switch
            {
                ImageType.All => 1,
                ImageType.Catgirls => 16,
                ImageType.Anime => 1,
                _ => 1
            };
            int maxNumber = 19; // Always 19
            int number = Rng.Next(minNumber, maxNumber + 1);
            image.Path = Path.Combine(Directory.GetCurrentDirectory(), "LegacyImages", $"cute{number}.png");
            image.Creator = $"Legacy image #{number}";
            image.Uploader = $"Squid Grill";
            image.FileSource = ImageSource.LegacyImages;
            return image;
        }
        public static CuteImage SearchImageBoard(string tags)
        {
            var image = new CuteImage();
            int index = Rng.Next(1, 101);
            var json = JArray.Parse(client.GetStringAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&tags={tags}").Result);
            image.Uploader = json.SelectToken($"$[{index}].owner").ToString();
            image.ImageSource = json.SelectToken($"$[{index}].source").ToString();
            image.FileSource = ImageSource.Online;
            image.Path = json.SelectToken($"$[{index}].file_url").ToString();
            return image;
        }
        public static ImageType ParseImageType(string type) => type.ToLower() switch
        {
            "anime" => ImageType.Anime,
            "catgirls" => ImageType.Catgirls,
            "all" => ImageType.All,
            _ => ImageType.All
        };
        public static ImageSource ParseImageSource(string source) => source.ToLower() switch
        {
            "curated" => ImageSource.Curated,
            "legacy" => ImageSource.LegacyImages,
            "online" => ImageSource.Online,
            _ => ImageSource.Curated
        };
    }
    public class CuteImage
    {
        public string Path { get; set; }
        public string Creator { get; set; } = string.Empty;
        public string Uploader { get; set; } = string.Empty;
        public string ImageSource { get; set; } = "Unknown source :(";
        public ImageSource FileSource { get; set; }
        public async void SendEmbed(SocketCommandContext context)
        {
            var embedBuilder = MessagingUtils.GetShrimpbotEmbedBuilder();
            var builder = new StringBuilder();
            if (FileSource == Services.ImageSource.Online) // Involves URLs
            {
                if (!string.IsNullOrEmpty(Creator)) builder.AppendLine($"Creator: {Creator}");
                if (!string.IsNullOrEmpty(Uploader)) builder.AppendLine($"Uploaded by {Uploader}");
                builder.AppendLine($"Source: {ImageSource}");

                embedBuilder.ImageUrl = Path;
                embedBuilder.Url = Path;
                embedBuilder.WithDescription(builder.ToString());
                var embed = embedBuilder.Build();
                await context.Channel.SendMessageAsync(embed: embed);
            }
            else // Involves local files
            {
                if (!string.IsNullOrEmpty(Creator)) builder.AppendLine($"Creator: {Creator}");
                if (!string.IsNullOrEmpty(Uploader)) builder.AppendLine($"Uploaded by {Uploader}");
                builder.AppendLine($"Source: {ImageSource}");

                string path = System.IO.Path.GetFileName(Path);
                embedBuilder.ImageUrl = $"attachment://{path}";
                embedBuilder.WithDescription(builder.ToString());
                var embed = embedBuilder.Build();
                await context.Channel.SendFileAsync(new FileStream(Path, FileMode.Open), path, embed: embed);
            }
        }
    }
    public class FolderCuteImage : CuteImage
    {
        public ImageType Type { get; init; }
    }
    public enum ImageType
    {
        All,
        Anime,
        Catgirls
    }
    public enum ImageSource
    {
        Curated,
        LegacyImages,
        Online
    }
}
