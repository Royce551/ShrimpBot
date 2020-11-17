namespace Shrimpbot.Services.Database
{
    public class DatabaseImage
    {
        /// <summary>
        /// To simplify things, the file path is used as the ID.
        /// </summary>
        public string DatabaseImageId { get; set; }
        /// <summary>
        /// The image type of the image, used for searching purposes.
        /// </summary>
        public ImageType Type { get; set; }
        /// <summary>
        /// The actual image.
        /// </summary>
        public CuteImage Image { get; set; }
    }
}
