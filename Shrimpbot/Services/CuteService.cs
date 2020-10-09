using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Shrimpbot.Services
{
    public class CuteService
    {
        /// <summary>
        /// Gets a random image from the specified source.
        /// </summary>
        /// <param name="source">The source to get the image from.</param>
        /// <param name="type">The type of image to search for</param>
        /// <returns>A <see cref="CuteImage"/> representing the returned image.</returns>
        public static CuteImage GetImage(ImageSource source = ImageSource.LocalImages, ImageType type = ImageType.All) => source switch
        {
            ImageSource.LocalImages => GetImageFromImageFolder(type),
            ImageSource.Online => GetImageFromOnline(type),
            ImageSource.LegacyImages => GetImageFromLegacyImageFolder(type),
            _ => GetImageFromImageFolder(type)
        };
        /// <summary>
        /// Gets a random image an image board.
        /// </summary>
        /// <param name="type">The type of image to search for</param>
        /// <returns>A <see cref="CuteImage"/> representing the returned image.</returns>
        public static CuteImage GetImageFromOnline(ImageType type)
        {
            var image = new CuteImage();
            return image;
        }
        /// <summary>
        /// Gets a random image from the images folder.
        /// </summary>
        /// <param name="type">The type of image to search for</param>
        /// <returns>A <see cref="CuteImage"/> representing the returned image.</returns>
        public static CuteImage GetImageFromImageFolder(ImageType type)
        {
            var image = new CuteImage();
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
            int number = new Random().Next(minNumber, maxNumber + 1);
            image.Path = Path.Combine(Directory.GetCurrentDirectory(), "LegacyImages", $"cute{number}.png");
            image.Creator = $"Legacy image #{number}";
            image.Source = "I dunno";
            return image;
        }
    }
    public class CuteImage
    {
        public string Path { get; set; }
        public string Creator { get; set; }
        public string Source { get; set; }
    }
    public enum ImageType
    {
        All,
        Anime,
        Catgirls
    }
    public enum ImageSource
    {
        LocalImages,
        LegacyImages,
        Online
    }
}
