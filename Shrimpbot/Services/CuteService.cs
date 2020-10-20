using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Shrimpbot.Services.Database;

namespace Shrimpbot.Services
{
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
        public static CuteImage GetImage(ImageSource source = ImageSource.Curated, ImageType type = ImageType.All) => source switch
        {
            ImageSource.Curated => GetImageFromImageFolder(type),
            ImageSource.Online => GetImageFromOnline(type),
            ImageSource.LegacyImages => GetImageFromLegacyImageFolder(type),
            _ => GetImageFromImageFolder(type)
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
            image.Source = json.SelectToken($"$[{index}].source").ToString();
            image.Path = json.SelectToken($"$[{index}].file_url").ToString();
            return image;
        }
        /// <summary>
        /// Gets a random image from the images folder.
        /// </summary>
        /// <param name="type">The type of image to search for</param>
        /// <returns>A <see cref="CuteImage"/> representing the returned image.</returns>
        public static CuteImage GetImageFromImageFolder(ImageType type)
        {
            var imagePaths = new List<DatabaseImage>();

            if (type == ImageType.All || type == ImageType.Anime) imagePaths.AddRange(DatabaseManager.GetImages(ImageType.Anime));
            if (type == ImageType.All || type == ImageType.Anime || type == ImageType.Catgirls) imagePaths.AddRange(DatabaseManager.GetImages(ImageType.Catgirls));

            int number = Rng.Next(0, imagePaths.Count);
            var image = imagePaths[number].Image;

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
            return image;
        }
        public static CuteImage SearchImageBoard(string tags)
        {
            var image = new CuteImage();
            int index = Rng.Next(1, 101);
            var json = JArray.Parse(client.GetStringAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&tags={tags}").Result);
            image.Uploader = json.SelectToken($"$[{index}].owner").ToString();
            image.Source = json.SelectToken($"$[{index}].source").ToString();
            image.Path = json.SelectToken($"$[{index}].file_url").ToString();
            return image;
        }
    }
    public class CuteImage
    {
        public string Path { get; set; }
        public string Creator { get; set; } = string.Empty;
        public string Uploader { get; set; } = string.Empty;
        public string Source { get; set; } = "Unknown source :(";
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
