﻿using APIConnector.Model;
using Google.Api.Gax.ResourceNames;
using Google.Api.Gax;
using Google.Cloud.Logging.V2;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace APIConnector.GoogleCloud
{
    public class GoogleAPIHelper
    {


        public static async Task<GoogleAPIOauth2TokenInfo> GetTokenInfo(string API_url, ExternalAPIPathConfig path_config, string access_token) {
            string full_url = RESTAPIConnector.ConstructRESTPathURL(API_url, path_config.path);

            var response_message = await RESTAPIConnector.CallRestAPIAsync(new APIConnector.Model.RESTAPIInputModel
            {
                URL = full_url,
                ContentType = "application/json",
                APIType = RESTAPIType.GET,
                Parameters = "",
                AccessToken = access_token
            });

            var response = JsonSerializer.Deserialize<GoogleAPIOauth2TokenInfo>(response_message)!;

            return response;
        }

        public static async Task<GoogleAPIOAuth2UserInfo> GetUserInfo(string API_url, ExternalAPIPathConfig path_config, string access_token) {
            string full_url = RESTAPIConnector.ConstructRESTPathURL(API_url, path_config.path);

            var response_message = await RESTAPIConnector.CallRestAPIAsync(new APIConnector.Model.RESTAPIInputModel
            {
                URL = full_url,
                ContentType = "application/json",
                APIType = RESTAPIType.GET,
                Parameters = "",
                AccessToken = access_token
            });

            var response = JsonSerializer.Deserialize<GoogleAPIOAuth2UserInfo>(response_message)!;

            return response;
        }

        // Code below copied and modified from: https://cloud.google.com/storage/docs/samples/storage-generate-signed-url-v4
        public static async Task<string> GenerateV4SignedReadUrl(UrlSigner urlSigner, string bucketName, string objectName, int duration_in_seconds = 300)
        {
            /*
            string serialized_credential_json = JsonSerializer.Serialize(key_obj);

            string full_scope_url = RESTAPIConnector.ConstructRESTPathURL(scope_API_url, scope_path_config.path);

            var scopes = new string[] { full_scope_url };
            GoogleCredential service_account_credential = GoogleCredential.FromJson(serialized_credential_json).CreateScoped(scopes);
            UrlSigner urlSigner = UrlSigner.FromCredential(service_account_credential);
            */

            // V4 is the default signing version.
            string url = await urlSigner.SignAsync(bucketName, objectName, TimeSpan.FromSeconds(duration_in_seconds), HttpMethod.Get, SigningVersion.V4);
            //Console.WriteLine("Generated GET signed URL:");
            //Console.WriteLine(url);
            //Console.WriteLine("You can use this URL with any user agent, for example:");
            //Console.WriteLine($"curl '{url}'");
            return url;
        }

        public static IEnumerable<Google.Apis.Storage.v1.Data.Object> ListFilesFromStorage(StorageClient storageClient, string bucketName, string prefix, string delimiter = "/")
        {
            // MatchGlob to exclude directory itself reference from: https://stackoverflow.com/questions/54170935/how-to-list-only-files-not-folder-in-gcs
            var options = new ListObjectsOptions { Delimiter = delimiter, MatchGlob = "**[^/]" };
            var storageObjects = storageClient.ListObjects(bucketName, prefix, options);
            return storageObjects;
        }

        public static async Task CopyFileInStorage(StorageClient storageClient, string sourceBucketName, string sourceObjectName, string destinationBucketName, string destinationObjectName)
        {
            //MemoryStream stream = new MemoryStream();
            await storageClient.CopyObjectAsync(sourceBucketName, sourceObjectName, destinationBucketName, destinationObjectName);
            //return stream;
        }

        public static async Task UploadFileToStorage(StorageClient storageClient, string bucketName, string objectName, MemoryStream memoryStream)
        {
            /*
            try
            {
                if (overwrite)
                {
                    var existing_obj = await storage.GetObjectAsync(bucketName, objectName);
                    throw 
                }
            }
            catch (GoogleApiException ex) {
                if (ex.Error.Code != 404) throw ex;
            }
            */
            var obj = await storageClient.UploadObjectAsync(bucketName, objectName, "application/octet-stream", memoryStream);
        }


        public static async Task<IEnumerable<LogEntry>> ListTaskLogs(LoggingServiceV2Client loggingClient, IList<string> projectIDs) {
            

            ListLogEntriesRequest request = new ListLogEntriesRequest
            {
                Filter = "severity=INFO AND labels.log_type=task-log AND timestamp>=\"2024-07-29T01:00:00Z\"",
                OrderBy = "timestamp desc",
                ResourceNamesAsProjectNames = { projectIDs.Select(id => ProjectName.FromProject(id)) }
            };

            // Make the request
            PagedEnumerable<ListLogEntriesResponse, LogEntry> response = loggingClient.ListLogEntries(request);

            /*
            // Iterate over all response items, lazily performing RPCs as required
            foreach (LogEntry item in response)
            {
                // Do something with each item
                Console.WriteLine(item);
            }

            // Or iterate over pages (of server-defined size), performing one RPC per page
            foreach (ListLogEntriesResponse page in response.AsRawResponses())
            {
                // Do something with each page of items
                Console.WriteLine("A page of results:");
                foreach (LogEntry item in page)
                {
                    // Do something with each item
                    Console.WriteLine(item);
                }
            }

            // Or retrieve a single page of known size (unless it's the final page), performing as many RPCs as required
            int pageSize = 10;
            Page<LogEntry> singlePage = response.ReadPage(pageSize);
            // Do something with the page of items
            Console.WriteLine($"A page of {pageSize} results (unless it's the final page):");
            foreach (LogEntry item in singlePage)
            {
                // Do something with each item
                Console.WriteLine(item);
            }
            // Store the pageToken, for when the next page is required.
            string nextPageToken = singlePage.NextPageToken;
            */

            return response.ToList();
        }
    }
}
