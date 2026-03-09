using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;


namespace fnPostDataStorage;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function("DataStorage")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("Processando a imagem no storage");

      
        if (!req.Headers.TryGetValue("file-type", out var fileTypeHeader))
        {
            return new BadRequestObjectResult("O cabeçalho 'file-type´ é obrigatório");
        }
        var fileType = fileTypeHeader.ToString();
        var form = await req.ReadFormAsync();
        var file = form.Files["files"];

        if (file == null || file.Length == 0)
        {
            return new BadRequestObjectResult("Nenhum arquivo foi enviado.");
        }

        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        string containerName = fileType;
        BlobClient blobClient = new BlobClient(connectionString, containerName, file.FileName);
        BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);

        await containerClient.CreateIfNotExistsAsync();
        await containerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);
      
        string BlobName = file.FileName;
        var blob = containerClient.GetBlobClient(BlobName);

        using (var stream = file.OpenReadStream())
        {
            await blob.UploadAsync(stream, true);
        }

        _logger.LogInformation($"Arquivo {file.FileName} enviado com sucesso para o container {containerName}.");

        return new OkObjectResult(new
        {
            Message = $"Arquivo {file.FileName} enviado com sucesso para o container {containerName}.",
            BlobUrl = blob.Uri
        });
     
        
       
    }
}