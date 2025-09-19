using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.User;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AccountController(
            NorviguetDbContext context,
            IMapper mapper,
            BlobServiceClient blobServiceClient,
            IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _blobServiceClient = blobServiceClient;
            _containerName = configuration["AzureStorage:ContainerName"]!;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetAccount()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            var result = _mapper.Map<UserDto>(user);
            // Agregar la URL de la imagen al resultado si existe
            if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                // Si ImageUrl es una URL, extrae el nombre del archivo
                var fileName = Uri.IsWellFormedUriString(user.ImageUrl, UriKind.Absolute)
                    ? Path.GetFileName(new Uri(user.ImageUrl).LocalPath)
                    : user.ImageUrl;

                    result.ImageUrl = GetBlobSasUrl(fileName);
            }

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAccount([FromForm] UpdateUserAccountDto updateUserDto, [FromForm] IFormFile? file)
        {
            // Obtener el id del usuario autenticado desde los claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            // Solo actualiza el nombre (no la imagen)
            _mapper.Map(updateUserDto, user);

            // Si hay imagen, subirla y asignar la URL devuelta
            if (file != null)
            {
                if (file.Length == 0)
                    return BadRequest("No se recibió ningún archivo.");

                var fileName = $"user_{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                try
                {
                    using var stream = file.OpenReadStream();
                    var imageUrl = await UploadAsync(fileName, stream, file.ContentType);

                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        return StatusCode(500, "No se pudo subir la imagen al almacenamiento.");
                    }

                    // Eliminar imagen anterior si existe
                    if (!string.IsNullOrEmpty(user.ImageUrl))
                    {
                        // Si ImageUrl es una URL, extrae el nombre del archivo
                        var oldFileName = Uri.IsWellFormedUriString(user.ImageUrl, UriKind.Absolute)
                            ? Path.GetFileName(new Uri(user.ImageUrl).LocalPath)
                            : user.ImageUrl; // Ya es solo el nombre del archivo
                        await DeleteAsync(oldFileName);
                    }

                    // Asigna la URL devuelta por Azure Blob Storage
                    user.ImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error al subir la imagen: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            return Ok(_mapper.Map<UserDto>(user));
        }

        private string GetBlobSasUrl(string fileName)
        {
            // Lógica para generar la URL SAS del blob
            var blobClient = _blobServiceClient.GetBlobContainerClient(_containerName).GetBlobClient(fileName);
            var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1)); // URL SAS válida por 1 hora
            return sasUri.ToString();
        }

        private async Task<string> UploadAsync(string fileName, Stream stream, string contentType)
        {
            var blobClient = _blobServiceClient.GetBlobContainerClient(_containerName).GetBlobClient(fileName);
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString();
        }

        private async Task DeleteAsync(string fileName)
        {
            var blobClient = _blobServiceClient.GetBlobContainerClient(_containerName).GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
