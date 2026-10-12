using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CoffeeNChillFunctions.DTOs;

namespace CoffeeNChillFunctions
{
    public class StaffDocumentFunctions
    {
        private readonly ILogger<StaffDocumentFunctions> _logger;
        private const string ContainerName = "staff-docs";

        public StaffDocumentFunctions(ILogger<StaffDocumentFunctions> logger)
        {
            _logger = logger;
        }

        private async Task<BlobContainerClient> GetContainerClientAsync()
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "UseDevelopmentStorage=true";
            var serviceClient = new BlobServiceClient(connectionString);
            var containerClient = serviceClient.GetBlobContainerClient(ContainerName);
            await containerClient.CreateIfNotExistsAsync();
            return containerClient;
        }

        [Function("UploadStaffDocument")]
        public async Task<IActionResult> UploadStaffDocument(
     [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequest req)
        {
            _logger.LogInformation("Processing staff document upload request.");

            if (req == null)
            {
                return new BadRequestObjectResult(new { error = "Request is null." });
            }

            string fileName = req.Query["fileName"].ToString();
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = $"doc_{Guid.NewGuid():N}.dat";
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    await req.Body.CopyToAsync(ms);

                    if (ms.Length == 0)
                    {
                        return new BadRequestObjectResult(new { error = "Request body stream is empty." });
                    }

                    ms.Position = 0;

                    var containerClient = await GetContainerClientAsync();
                    var blobClient = containerClient.GetBlobClient(fileName);

                    await blobClient.UploadAsync(ms, overwrite: true);

                    var dto = new StaffDocumentDto
                    {
                        FileName = fileName,
                        SizeInBytes = ms.Length,
                        ContentType = req.ContentType ?? "application/octet-stream",
                        LastModified = DateTimeOffset.UtcNow
                    };

                    return new OkObjectResult(dto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document to Blob storage.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [Function("ListStaffDocuments")]
        public async Task<IActionResult> ListStaffDocuments(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest req)
        {
            _logger.LogInformation("Fetching staff document list.");

            try
            {
                var containerClient = await GetContainerClientAsync();
                var documents = new List<StaffDocumentDto>();

                await foreach (BlobItem blob in containerClient.GetBlobsAsync())
                {
                    documents.Add(new StaffDocumentDto
                    {
                        FileName = blob.Name,
                        SizeInBytes = blob.Properties.ContentLength ?? 0,
                        ContentType = blob.Properties.ContentType ?? "application/octet-stream",
                        LastModified = blob.Properties.LastModified
                    });
                }

                return new OkObjectResult(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing staff documents.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [Function("DownloadStaffDocument")]
        public async Task<IActionResult> DownloadStaffDocument(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequest req,
            string fileName)
        {
            _logger.LogInformation("Downloading document: {FileName}", fileName);

            try
            {
                var containerClient = await GetContainerClientAsync();
                var blobClient = containerClient.GetBlobClient(fileName);

                if (!await blobClient.ExistsAsync())
                {
                    return new NotFoundObjectResult(new { error = "Document not found." });
                }

                var stream = await blobClient.OpenReadAsync();
                var properties = await blobClient.GetPropertiesAsync();

                return new FileStreamResult(stream, properties.Value.ContentType ?? "application/octet-stream")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document {FileName}.", fileName);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}