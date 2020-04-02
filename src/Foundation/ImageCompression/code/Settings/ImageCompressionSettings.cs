using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Foundation.ImageCompression;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.ImageCompression.Settings
{
    public static class ImageCompressionSettings
    {
        public static string GetApiEndpoint()
        {
            return GetGlobalSettingsItem().Fields[ImageCompressionConstants.GlobalSettings.Fields.EndpointUrl].Value;
        }

        public static string GetApiEndpointKey()
        {
            return GetGlobalSettingsItem().Fields[ImageCompressionConstants.GlobalSettings.Fields.EndpointKey].Value;
        }

        public static string GetApiSecret()
        {
            return GetGlobalSettingsItem().Fields[ImageCompressionConstants.GlobalSettings.Fields.EndpointSecret].Value;
        }

        public static string GetConversionApiEndpoint()
        {
            return GetGlobalSettingsItem().Fields[ImageCompressionConstants.GlobalSettings.Fields.ConvertEndpointUrl].Value;
        }

        public static string GetConversionApiEndpointKey()
        {
            return GetGlobalSettingsItem().Fields[ImageCompressionConstants.GlobalSettings.Fields.ConvertEndpointKey].Value;
        }

        public static string GetConversionApiSecret()
        {
            return GetGlobalSettingsItem().Fields[ImageCompressionConstants.GlobalSettings.Fields.ConvertEndpointSecret].Value;
        }

        public static string GetConversionInformationField()
        {
            return GetGlobalSettingsItem().Fields[ImageCompressionConstants.GlobalSettings.Fields.ImageConversionInformationField].Value;
        }

        public static string GetInformationField()
        {
            return GetGlobalSettingsItem().Fields[ImageCompressionConstants.GlobalSettings.Fields.ImageCompressionInformationField].Value;
        }

        public static bool IsImageCompressionEnabled()
        {
            var item = GetGlobalSettingsItem();
            return item.Fields[ImageCompressionConstants.GlobalSettings.Fields.ImageCompressionEnabled].Value == "1";
        }

        public static bool IsImageConversionEnabled()
        {
            var item = GetGlobalSettingsItem();
            return item.Fields[ImageCompressionConstants.GlobalSettings.Fields.ImageConversionEnabled].Value == "1";
        }

        public static bool IsImageCompressionButtonEnabled()
        {
            if (!IsImageCompressionEnabled()) return false;
            var item = GetGlobalSettingsItem();
            return item.Fields[ImageCompressionConstants.GlobalSettings.Fields.ImageCompressionButtonEnabled].Value == "1";
        }

        public static bool IsImageConversionButtonEnabled()
        {
            if (!IsImageConversionEnabled()) return false;
            var item = GetGlobalSettingsItem();
            return item.Fields[ImageCompressionConstants.GlobalSettings.Fields.ImageConversionButtonEnabled].Value == "1";
        }

        public static bool IsImageCompressionScheduledTaskEnabled()
        {
            if (!IsImageCompressionEnabled()) return false;
            var item = GetGlobalSettingsItem();
            return item.Fields[ImageCompressionConstants.GlobalSettings.Fields.ImageCompressionScheduledTaskEnabled].Value == "1";
        }

        public static bool IsImageConversionScheduledTaskEnabled()
        {
            if (!IsImageConversionEnabled()) return false;
            var item = GetGlobalSettingsItem();
            return item.Fields[ImageCompressionConstants.GlobalSettings.Fields.ImageConversionScheduledTaskEnabled].Value == "1";
        }

        public static bool IsImage(Item item)
        {
            return item.InheritsFrom(ImageCompressionConstants.TemplateIDs.ImageTemplateId);
        }

        private static Item GetGlobalSettingsItem()
        {
            return GetMasterDatabase().GetItem(ImageCompressionConstants.GlobalSettings.ImageCompressionGlobalSettingsId);
        }

        private static Database GetMasterDatabase()
        {
            return Database.GetDatabase(ImageCompressionConstants.GlobalSettings.Database.Master);
        }

    }
}