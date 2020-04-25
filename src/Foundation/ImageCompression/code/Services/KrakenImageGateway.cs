using System;
using System.Collections.Generic;
using System.IO;
using Kraken.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RestSharp;
using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.ImageCompression.Settings;
using Sitecore.Resources.Media;

namespace Sitecore.Foundation.ImageCompression.Services
{
    /// <summary>
    /// Call 
    /// </summary>
    public class KrakenImageGateway : IImageNextGenFormatService
    {
        private const string REMOTE_ERROR = "Kraken Remote API caused an error";
        private const string TIMY_CONNETION_ERROR = "Could not download Image from Kraken";
        private const string LOCATON_RESPONSE = "Location";
        private const string COMPRESSION_COUNT = "Compression-Count";
        private const string Value = "multipart/form-data";
        private const string Name = "Content-Type";

        public bool ShouldContinue { get; set; }

        public KrakenImageGateway()
        {
            ShouldContinue = true;
        }

        public string ConvertImage(Item currentItem)
        {
            var uploadedImage = SendToKrakenForCompression(currentItem);
            if (uploadedImage == null)
                return null;
            DownloadImage(currentItem, uploadedImage);
            return uploadedImage.KrakedUrl;
        }

        public OptimizeWaitResult SendToKrakenForCompression(Item currentItem)
        {
            var client = new RestClient(ImageCompressionSettings.GetConversionApiEndpoint());
            var request = CreateUploadRequest(currentItem, client);
            client.Timeout = 300000;

            try
            {
                var response = client.Execute<OptimizeWaitResult>(request);
                var content = response.Content;

                if(response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    Diagnostics.Log.Info(message: $"Image Upload failed {response.StatusCode} | response content: {response.Content}", owner: this);
                    ShouldContinue = false;
                    return null;
                }

                if (response.Data != null && string.IsNullOrEmpty(response.Data.KrakedUrl))
                    return null;

                Diagnostics.Log.Info(message: $"Image Uploaded to Tiny PNG {response.Data.KrakedUrl}", owner: this);

                return response.Data;
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(REMOTE_ERROR, ex);
                TinyPngApiGateway.RecordError(currentItem, ex.Message);
            }
            return null;
        }

        public new RestRequest CreateUploadRequest(Item currentItem, RestClient client)
        {
            var _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCaseExceptDictionaryKeysResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = true } },
                NullValueHandling = NullValueHandling.Ignore
            };
                       
            var request = new RestRequest(Method.POST);
            request.Parameters.Clear();
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");

            if (currentItem != null)
            {
                var _mediaItem = new MediaItem(currentItem);

                if (_mediaItem != null)
                {
                    var imageBytes = ReadMediaStream(_mediaItem);
                    var requestJson = JsonConvert.SerializeObject(BuildRequestObject(), _serializerSettings);
                    request.AddParameter("data", requestJson); 
                    request.AddFile("upload", imageBytes, currentItem.Name, currentItem.Name);
                    request.AddHeader(Name, Value);
                }
            }
            return request;
        }
        
        public UserRequest BuildRequestObject()
        {
            var ApiKey = ImageCompressionSettings.GetConversionApiEndpointKey();
            var ApiSecret = ImageCompressionSettings.GetConversionApiSecret();

            var requestObj = new UserRequest();
            requestObj.Authentication.ApiKey = ApiKey;
            requestObj.Authentication.ApiSecret = ApiSecret;
            requestObj.WebP = true;
            requestObj.Lossy = true;
            requestObj.Wait = true;
            return requestObj;
        }

        public byte[] ReadMediaStream(MediaItem _currentMedia)
        {
            Stream stream = _currentMedia.GetMediaStream();
            long fileSize = stream.Length;
            byte[] buffer = new byte[(int)fileSize];
            stream.Read(buffer, 0, (int)stream.Length);
            stream.Close();

            return buffer;
        }

        public string DownloadImage(MediaItem currentItem, OptimizeWaitResult img)
        {
            var client = new RestClient(img.KrakedUrl);

            var request = new RestRequest(Method.GET);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            client.Timeout = 300000;

            try
            {
                byte [] responseData = client.DownloadData(request);
                string sizeBefore = currentItem.InnerItem.Fields["Size"].Value;
                UpdateImageFile(currentItem, responseData);
                string sizeAfter = responseData.Length.ToString();
                TinyPngApiGateway.UpdateImageInformation(currentItem, sizeBefore, sizeAfter, ImageCompressionConstants.Messages.OPTIMISED_BY_KRAKEN, ImageCompressionSettings.GetConversionInformationField());
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(TIMY_CONNETION_ERROR, ex);
                TinyPngApiGateway.RecordError(currentItem, ex.Message);
            }
            return "API ISSUE";
        }

        protected void UpdateImageFile(MediaItem currentItem, byte[] responseData)
        {
            Media media = MediaManager.GetMedia(currentItem);

            Stream stream = new MemoryStream(responseData);
            try
            {
                if (!currentItem.InnerItem.InheritsFrom(ImageCompressionConstants.TemplateIDs.RelatedImageTemplateId))
                {
                    InjectTheNeededTemplate(currentItem);
                }

                Data.Fields.LinkField relatedField = media.MediaData.MediaItem.InnerItem.Fields[ImageCompressionConstants.ImageFields.RELATED_IMAGE_FIELD];
                if (relatedField != null && relatedField.TargetItem != null)
                {
                    var myMedia = new Data.Items.MediaItem(relatedField.TargetItem);
                    var hiddenWebPMedia = new Media(new MediaData(myMedia));
                    hiddenWebPMedia.SetStream(stream, currentItem.Extension);
                }
                else
                {
                    if (stream != null)
                    {
                        CreateNewMediaItemWithStreamAndLink(stream, currentItem, relatedField, media);
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            } // Sitecore may not be ready to accept this sort of image, but it still works despite this exception
            
        }

        private void InjectTheNeededTemplate(MediaItem currentItem)
        {
            using (new SecurityModel.SecurityDisabler())
            {
                Sitecore.Data.Items.TemplateItem baseImageTemplate = currentItem.InnerItem.Template;
                baseImageTemplate.InnerItem.Editing.BeginEdit();
                baseImageTemplate.InnerItem["__Base Template"] = $"{baseImageTemplate.InnerItem.Fields["__Base Template"].Value}|{ImageCompressionConstants.TemplateIDs.RelatedImageTemplateId}";
                baseImageTemplate.InnerItem.Editing.EndEdit();
            }
        }

        private void CreateNewMediaItemWithStreamAndLink(Stream stream, MediaItem currentItem, LinkField relatedField, Media originalMedia)
        {
            var mediaCreator = new MediaCreator();
            var options = new MediaCreatorOptions
            {
                Versioned = false,
                IncludeExtensionInItemName = false,
                Database = Factory.GetDatabase(ImageCompressionConstants.GlobalSettings.Database.Master),
                Destination = $"/sitecore/media library{currentItem.MediaPath}webp"
            };

            using (new SecurityModel.SecurityDisabler())
            {
                var newHiddenMedia = mediaCreator.CreateFromStream(stream, currentItem.Name + ".webp", options);

                newHiddenMedia.Editing.BeginEdit();
                newHiddenMedia.Appearance.Hidden = true;
                newHiddenMedia.Editing.EndEdit();

                currentItem.BeginEdit();
                relatedField.Value = string.Empty; //https://sitecore.stackexchange.com/questions/4104/how-to-programatically-update-a-general-link-field-in-a-custom-user-profille
                new LinkField(originalMedia.MediaData.MediaItem.InnerItem.Fields[ImageCompressionConstants.ImageFields.RELATED_IMAGE_FIELD])
                {
                    LinkType = "internal",
                    TargetID = newHiddenMedia.ID
                };
                currentItem.EndEdit();
            }
        }

        internal class CamelCaseExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
        {
            protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
            {
                var contract = base.CreateDictionaryContract(objectType);
                contract.DictionaryKeyResolver = propertyName => propertyName;
                return contract;
            }
        }

        internal JsonSerializerSettings ConfigureSerialization()
        {
            var jsonSerialization = new JsonSerializerSettings
            {
                ContractResolver = new CamelCaseExceptDictionaryKeysResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = true } },
                NullValueHandling = NullValueHandling.Ignore
            };
            return jsonSerialization;
        }
    }
}