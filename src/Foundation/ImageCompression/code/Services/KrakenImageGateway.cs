using System;
using System.Collections.Generic;
using System.IO;
using Kraken.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RestSharp;       
using Sitecore.Data.Items;
using Sitecore.Foundation.ImageCompression.Model;
using Sitecore.Resources.Media;

namespace Sitecore.Foundation.ImageCompression.Services
{
    /// <summary>
    /// Call 
    /// </summary>
    public class KrakenImageGateway : TinyPngApiGateway, IImageCompressionService
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

        public override string CompressImage(Item currentItem)
        {
            var uploadedImage = SendToKrakenForCompression(currentItem);
            if (uploadedImage == null)
                return null;
            DownloadImage(currentItem, uploadedImage);
            return uploadedImage.KrakedUrl;
        }

        public OptimizeWaitResult SendToKrakenForCompression(Item currentItem)
        {
            var client = new RestClient("https://api.kraken.io/v1/upload");
            var request = CreateUploadRequest(currentItem, client);
            client.Timeout = 300000;

            try
            {
                var response = client.Execute<OptimizeWaitResult>(request);
                var content = response.Content;

                if(response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    Diagnostics.Log.Info($"Image Upload failed {response.StatusCode} | response content: {response.Content}", this);
                    ShouldContinue = false;
                    return null;
                }

                if (response.Data != null && string.IsNullOrEmpty(response.Data.KrakedUrl))
                    return null;

                Diagnostics.Log.Info($"Image Uploaded to Tiny PNG {response.Data.KrakedUrl}", this);

                return response.Data;
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(REMOTE_ERROR, ex);
                RecordError(currentItem, ex.Message);
            }
            return null;
        }

        public new RestRequest CreateUploadRequest(Item currentItem, RestClient client)
        {
            var ApiKey = "53293608b43b69c7f31f1fb98ea1b830";
            var ApiSecret = "a6c4a3d7e2d3afb687335a425187e0cd5e9483bc";
            //apiRequest.Body.Dev = false;
            //var isSet = apiRequest.Body is IOptimizeSetWaitRequest || apiRequest.Body is IOptimizeSetUploadWaitRequest;

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

                    //var requestJson = $"{{\"auth\":{{\"api_key\": \"{ApiKey}\", \"api_secret\": \"{ApiSecret}\"}}, \"wait\":true," +
                    //    "\"lossy\": true,"+
                    //    "\"webp\": true"+
                    //    $"}}";

                    var requestJson = JsonConvert.SerializeObject(BuildRequestObject(), _serializerSettings);

                    //var json = JsonConvert.SerializeObject(apiRequest.Body, _serializerSettings);
                    request.AddParameter("data", requestJson);
                    //request.AddFile("data", requestJson, "application/json");
                    request.AddFile("upload", imageBytes, currentItem.Name, currentItem.Name);
                    request.AddHeader(Name, Value);
                }
            }
            return request;
        }
        
        public UserRequest BuildRequestObject()
        {
            var requestObj = new UserRequest();
            requestObj.Authentication.ApiKey = "53293608b43b69c7f31f1fb98ea1b830";
            requestObj.Authentication.ApiSecret = "a6c4a3d7e2d3afb687335a425187e0cd5e9483bc";
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

        public new string DownloadImage(MediaItem currentItem, OptimizeWaitResult img)
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
                UpdateImageInformation(currentItem, sizeBefore, sizeAfter, ImageCompressionConstants.Messages.OPTIMISED_BY_KRAKEN);
            }
            catch (Exception ex)
            {
                Diagnostics.Log.Error(TIMY_CONNETION_ERROR, ex);
                RecordError(currentItem, ex.Message);
            }
            return "API ISSUE";
        }

        protected void UpdateImageFile(MediaItem currentItem, byte[] responseData)
        {
            currentItem.BeginEdit();
            Media media = MediaManager.GetMedia(currentItem);
            Stream stream = new MemoryStream(responseData);
            try
            {
                media.SetStream(stream, currentItem.Extension);
            }
            catch (Exception ex) {
                return;
            } // Sitecore may not be ready to accept this sort of image, but it still works
            currentItem.EndEdit();
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