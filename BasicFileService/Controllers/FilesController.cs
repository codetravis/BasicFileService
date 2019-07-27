using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace BasicFileService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private IConfiguration config;

        public FilesController(IConfiguration appConfig)
        {
            config = appConfig;
        }

        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Upload()
        {
            try
            {
                IFormFile file = Request.Form.Files[0];
                string pathToSave = config.GetSection("FileStorageTarget").Value;

                if(file.Length > 0)
                {
                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                    string fullPath = Path.Combine(pathToSave, fileName);

                    using (FileStream stream = new FileStream(fullPath, FileMode.Create) )
                    {
                        file.CopyTo(stream);
                    }

                    return Ok(new { fullPath });
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public JsonResult ListFiles()
        {
            string storagePath = config.GetSection("FileStorageTarget").Value;
            IEnumerable<string> fileList = Directory.GetFiles(storagePath).Select(curFile => Path.GetFileName(curFile)).ToArray();

            return new JsonResult(fileList);
        }

        [HttpGet("folders")]
        public JsonResult ListDirectories()
        {
            string storagePath = config.GetSection("FileStorageTarget").Value;
            IEnumerable<string> folderList = Directory.GetDirectories(storagePath).Select(curFolder => Path.GetFileName(curFolder)).ToArray();

            return new JsonResult(folderList);
        }
    }
}