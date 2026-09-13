using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CoffeeNChillFunctions.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoffeeNChillFunctions
{
    public class StaffDocumentFunctions
    {
        private readonly ILogger _logger;
        private const string ContainerName = "staff-docs";
        private const string ConnectionStringSetting = "AzureWebJobsStorage";

        public StaffDocumentFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StaffDocumentFunctions>();
        }

        private BlobContainerClient GetBlobContainerClient()
        {
            string connectionString = Environment.GetEnvironmentVariable(ConnectionStringSetting)
                ?? "UseDevelopmentStorage=true";
            var containerClient = new BlobContainerClient(connectionString, ContainerName);
            containerClient.CreateIfNotExists();
            return containerClient;
        }

        // 1. POST /api/documents/upload - Upload Staff Document
        [Function("UploadStaffDocument")]
        public async Task<HttpResponseData> UploadStaffDocument(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequestData req)
        {
            _logger.LogInformation("Processing staff document upload request.");

            try
            {
                var containerClient = GetBlobContainerClient();

                // Read header or query parameter for file name
                string fileName = req.Headers.Contains("X-File-Name")
                    ? string.Join("", req.Headers.GetValues("X-File-Name"))
                    : $"doc_{Guid.NewGuid():N}.pdf";

                var blobClient = containerClient.GetBlobClient(fileName);

                using (var stream = req.Body)
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(new
                {
                    message = "File uploaded successfully to staff-docs.",
                    fileName = fileName,
                    blobUri = blobClient.Uri.ToString()
                });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading staff document.");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync($"Error: {ex.Message}");
                return response;
            }
        }

        // 2. GET /api/documents - List All Staff Documents
        [Function("ListStaffDocuments")]
        public async Task<HttpResponseData> ListStaffDocuments(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequestData req)
        {
            _logger.LogInformation("Retrieving all staff documents.");

            try
            {
                var containerClient = GetBlobContainerClient();
                var documentList = new List<StaffDocumentDto>();

                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
                {
                    documentList.Add(new StaffDocumentDto
                    {
                        FileName = blobItem.Name,
                        SizeInBytes = blobItem.Properties.ContentLength ?? 0,
                        ContentType = blobItem.Properties.ContentType ?? "application/octet-stream",
                        LastModified = blobItem.Properties.LastModified
                    });
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(documentList);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing staff documents.");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync($"Error: {ex.Message}");
                return response;
            }
        }

        // 3. GET /api/documents/download/{fileName} - Download Staff Document
        [Function("DownloadStaffDocument")]
        public async Task<HttpResponseData> DownloadStaffDocument(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequestData req,
            string fileName)
        {
            _logger.LogInformation("Streaming download for document: {FileName}", fileName);

            try
            {
                var containerClient = GetBlobContainerClient();
                var blobClient = containerClient.GetBlobClient(fileName);

                if (!await blobClient.ExistsAsync())
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Document '{fileName}' was not found.");
                    return notFoundResponse;
                }

                var downloadInfo = await blobClient.DownloadStreamingAsync();

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", downloadInfo.Value.Details.ContentType ?? "application/octet-stream");
                response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                await downloadInfo.Value.Content.CopyToAsync(response.Body);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading staff document.");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync($"Error: {ex.Message}");
                return response;
            }
        }
    }
}