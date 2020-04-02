using Sitecore.Data;

namespace Sitecore.Foundation.ImageCompression
{
    public struct ImageCompressionConstants
    {
        public struct Messages
        {
            public static readonly string OPTIMISED_BY = "Optimised by TinyPng";
            public static readonly string OPTIMISED_BY_KRAKEN = "Optimised by Kraken.io";
        }

        public struct TemplateIDs
        {
            public static readonly ID ImageCompressionPageTemplateId = new ID("{AFB9A38D-7ECA-4C13-848C-F6A5DF08C11E}");
            public static readonly ID ImageTemplateId = new ID("{F1828A2C-7E5D-4BBD-98CA-320474871548}");
            public static readonly ID UnversionedJpegImageTemplateId = new ID("{DAF085E8-602E-43A6-8299-038FF171349F}");
            public static readonly ID UnversionedImageTemplateId = new ID("{F1828A2C-7E5D-4BBD-98CA-320474871548}");
            public static readonly ID VersionedJpegImageTemplateId = new ID("{C97BA923-8009-4858-BDD5-D8BE5FCCECF7}");
            public static readonly ID VersionedImageTemplateId = new ID("{EB3FB96C-D56B-4AC9-97F8-F07B24BB9BF7}");
        }
    
        public struct GlobalSettings
        {
            public static readonly ID ImageCompressionGlobalSettingsId = new ID("{E25A860E-B060-4AAB-9E6B-91546EFB08CC}");

            public struct Fields
            {
                public static readonly string ImageCompressionEnabled = "ImageCompressionEnabled";
                public static readonly string ImageCompressionButtonEnabled = "ImageCompressionButtonEnabled";
                public static readonly string ImageCompressionScheduledTaskEnabled = "ImageCompressionScheduledTaskEnabled";
                public static readonly string EndpointUrl = "EndpointURL";
                public static readonly string EndpointKey = "EndpointKey";
                public static readonly string EndpointSecret = "EndpointSecret";
                public static readonly string ImageCompressionInformationField = "ImageCompressionInformationField";

                public static readonly string ImageConversionEnabled = "ImageConversionEnabled";
                public static readonly string ImageConversionButtonEnabled = "ImageConversionButtonEnabled";
                public static readonly string ImageConversionScheduledTaskEnabled = "ImageConversionScheduledTaskEnabled";
                public static readonly string ConvertEndpointUrl = "ConvertEndpointURL";
                public static readonly string ConvertEndpointKey = "ConvertEndpointKey";
                public static readonly string ConvertEndpointSecret = "ConvertEndpointSecret";
                public static readonly string ImageConversionInformationField = "ImageConversionInformationField";
            }

            public struct Database
            {
                public static readonly string Master = "master";
            }

            public struct Index
            {
                public static readonly string Master = "sitecore_master_index";
            }
        }
    }
}