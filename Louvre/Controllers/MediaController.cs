using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Louvre.Shared.Core;
using Progbiz.DapperEntity;
using Louvre.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Louvre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {

        private readonly IDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public MediaController(IDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        [HttpPost("get-file-name")]
        public async Task<IActionResult> Get(MediaIDModel model)
        {
            var fileName = await _dbContext.GetAsync<string>($@"SELECT  FileName
                FROM  Medias
                Where MediaID=@MediaID", new { model.MediaID });
            return Ok(new FileNameModel() { FileName = fileName });
        }

        [DisableRequestSizeLimit]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(FileUploadAPIModel model)
        {
            if (model.Content != null)
            {
                model.FolderName = "gallery/" + model.FolderName;
                try
                {
                    if (model.Content.Length > 0)
                    {
                        await DeleteExistingFileAsync(model.MediaID);

                        if (!Directory.Exists(Path.Combine("wwwroot", model.FolderName)))
                        {
                            Directory.CreateDirectory(Path.Combine("wwwroot", model.FolderName));
                        }
                        var fileName = Guid.NewGuid().ToString("N");

                        fileName = $"{fileName}.{model.Extension}";
                        string path = Path.Combine(_env.ContentRootPath, "wwwroot", model.FolderName, fileName);

                        var fs = System.IO.File.Create(path);
                        fs.Write(model.Content, 0, model.Content.Length);
                        fs.Close();

                        Media media = new Media()
                        {
                            ContentType = model.ContentType,
                            FileName = "/" + model.FolderName + "/" + fileName,
                            ContentLength = model.Content.Length,
                            Extension = model.Extension,
                            MediaID = model.MediaID
                        };
                        model.MediaID = await _dbContext.SaveAsync(media);
                    }
                    else
                    {
                        return BadRequest(new APIBaseResponse() { Status = false, Message = "File not found" });
                    }
                }
                catch //(Exception err)
                {
                    return BadRequest(new APIBaseResponse() { Status = false, Message = "Oops..Something went wrong!!" });
                }
            }
            return Ok(new MediaIDModel() { MediaID = model.MediaID.Value });
        }

        private async Task DeleteExistingFileAsync(int? mediaId)
        {
            if (mediaId != null)
            {
                try
                {
                    var m = await _dbContext.GetAsync<Media>(Convert.ToInt32(mediaId));
                    string deleteFilePath = Path.Combine(_env.ContentRootPath, "wwwroot");
                    deleteFilePath += m.FileName;
                    System.IO.File.Delete(deleteFilePath);
                }
                catch { }
            }
        }
    }
}
