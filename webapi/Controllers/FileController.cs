using Microsoft.AspNetCore.Mvc;
using vaultApp.Services;
using vaultApp.Database;
using vaultApp.Repositories;
using vaultApp.Models;
using System.ComponentModel;

namespace webapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        [HttpGet()]
        public ActionResult Status()
        {
            return Ok(new { success = true, message = "File Controller is active" });
        }
        [HttpPost("File")]
        public ActionResult<object> Upload([FromBody] fileRequest file)
        {
            try
            {
                if (file == null)
                {
                    return BadRequest(new { success = false, error = "No file uploaded" });
                }

                // Check if file.file is null
                if (file.filePath == null || file.filePath.Length == 0)
                {
                    return BadRequest(new { success = false, error = "File data is missing" });
                }
                // Call the file upload service
                string uploadSuccess = FileService.Upload(file.filePath, file.parentId);

                if (uploadSuccess != null)
                {
                    var currentUserId = Redis.GetCurrentUserId();
                    if (string.IsNullOrEmpty(currentUserId))
                    {
                        return BadRequest(new { success = false, error = "User ID not found" });
                    }
                    var fileInfo = FileRepo.GetById(uploadSuccess, currentUserId);
                    return Ok(new
                    {
                        success = true,
                        message = "File uploaded successfully",
                        data = new
                        {
                            name = fileInfo?.Name,
                            fileId = fileInfo?.Id,
                            type = fileInfo?.Type,
                            parentId = fileInfo?.ParentId,
                        }
                    });
                }
                else
                {
                    return BadRequest(new { success = false, error = "File upload failed" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public ActionResult<object> GetById([FromRoute] string id)
        {
            try
            {
                var currentUserId = Redis.GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest(new { success = false, error = "User ID not found" });
                }

                var fileInfo = FileRepo.GetById(id, currentUserId);
                if (fileInfo == null)
                {
                    return NotFound(new { success = false, error = "File not found" });
                }
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        name = fileInfo.Name,
                        fileId = fileInfo.Id,
                        type = fileInfo.Type,
                        parentId = fileInfo.ParentId,
                        size = fileInfo.Size,
                        createdAt = fileInfo.UploadTime
                    }

                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = ex.Message });
            }
        }


        [HttpGet("{id}/data")]
        public ActionResult DownloadFile([FromRoute] string id)
        {
            try
            {
                var loggedInUser = Redis.GetCurrentUserId();
                if (loggedInUser == null)
                {
                    return Unauthorized("No user in session, Try to log in.");
                }
                var fileInfo = FileRepo.GetById(id, loggedInUser);
                if (fileInfo == null)
                {
                    return NotFound("File not found.");
                }
                if (string.IsNullOrEmpty(fileInfo.Path))
                {
                    return NotFound("File path is missing.");
                }
                if (fileInfo.UserId != loggedInUser && fileInfo.Visibility != "public")
                {
                    return Unauthorized("File is not visible.");
                }
                var filePath = fileInfo.Path;
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return File(fileStream, "application/pdf", fileInfo.Name);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("{id}/metadata")]
        public ActionResult<object> GetFileMetadata([FromRoute] string id)
        {
            try
            {
                var loggedInUser = Redis.GetCurrentUserId();
                if (loggedInUser == null)
                {
                    return Unauthorized("No user in session, Try to log in.");
                }
                var fileInfo = FileRepo.GetById(id, loggedInUser);
                if (fileInfo == null)
                {
                    return NotFound("File not found.");
                }
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        name = fileInfo.Name,
                        fileId = fileInfo.Id,
                        type = fileInfo.Type,
                        parentId = fileInfo.ParentId,
                        size = fileInfo.Size,
                        createdAt = fileInfo.UploadTime
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("{id}/thumbnails")]
        public ActionResult GetImgThumbnail([FromRoute] string id)
        {
            try
            {
                var loggedInUser = Redis.GetCurrentUserId();
                if (loggedInUser == null)
                {
                    return Unauthorized("No user in session, Try to log in.");
                }
                var fileInfo = FileRepo.GetById(id, loggedInUser);
                if (fileInfo == null)
                {
                    return NotFound("File not found.");
                }
                if (fileInfo.Type != "image")
                {
                    return BadRequest("File is not an image.");
                }
                if (fileInfo.Path == null)
                {
                    return NotFound("File path is missing.");
                }
                // Build thumbnail path correctly
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.Name);
                var thumbnailPath = Path.Combine(
                    Path.GetDirectoryName(fileInfo.Path)!.Replace("uploads", "thumbnails"),
                    $"{fileNameWithoutExt}.jpg"
                );

                if (!System.IO.File.Exists(thumbnailPath))
                {
                    return NotFound("Thumbnail not found.");
                }
                // display the image for preview without downloading
                var thumbnailStream = new FileStream(thumbnailPath, FileMode.Open, FileAccess.Read);
                return File(thumbnailStream, "image/jpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = ex.Message });
            }
        }

        [HttpPut("{id}/publish")]
        public ActionResult<object> PublishFile([FromRoute] string id)
        {
            try
            {
                var published = FileService.Publish(id);
                if (published)
                {
                    return Ok(new { success = true, message = "File published successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, error = "File publish failed" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = ex.Message });
            }
        }

        [HttpPut("{id}/unpublish")]
        public ActionResult<object> unpublishFile([FromRoute] string id)
        {
            try
            {
                var unpublished = FileService.Unpublish(id);
                if (unpublished)
                {
                    return Ok(new { success = true, message = "File unpublished successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, error = "File unpublish failed" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, error = ex.Message });
            }
        }
    }
}


public class fileRequest
{
    public required string filePath { get; set; }
    public string? parentId { get; set; } = null;
}