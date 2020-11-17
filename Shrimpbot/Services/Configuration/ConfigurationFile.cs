using Discord;

namespace Shrimpbot.Services.Configuration
{
    public enum ReleaseStream
    {
        Stable,
        PreRelease
    }
    public class ConfigurationFile
    {
        public string Token { get; set; } = "Put your token here!";
        public string Prefix { get; set; } = "s#";
        public Color AccentColor { get; set; } = Color.Blue;
        public string Name { get; set; } = "ShrimpBot PlusExtraCatgirls";
        public string Version { get; set; } = "17";
        public string CurrencySymbol { get; set; } = "$qui";
        public string Currency { get; set; } = "$quiDollars";
    }
}
