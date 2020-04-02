namespace Sitecore.Foundation.ImageCompression.Services
{
    public interface IImageNextGenFormatService
    {
        string ConvertImage(Data.Items.Item currentItem);
        bool ShouldContinue { get; set; }
    }
}