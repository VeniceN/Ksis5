using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace File.Controllers
{
    // API-���������� 
    [ApiController]
    [Route("{**filepath}")] // filepath � ��� ������������� ������ �������� ��� ��������� ��������� �����
    public class FilesController : ControllerBase
    {
        // ���� ��� �������� �������� ����� ������
        private readonly string storageRoot;
        private readonly string logFile;
        private readonly ILogger<FilesController> logger;

        // �����������
        public FilesController(IConfiguration config, ILogger<FilesController> logger)
        {
            // ����������� ���� � ���������
            var path = config["FileStorage"] ?? Environment.GetEnvironmentVariable("STORAGE_PATH") ?? "FileStorage";
            storageRoot = Path.Combine(Directory.GetCurrentDirectory(), path);
            logFile = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "log.txt");

            // �������� ����������, ���� ��� �� ����������
            Directory.CreateDirectory(storageRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(logFile)!);

            this.logger = logger;
        }

        // ��������� PUT-�������
        [HttpPut]
        public async Task<IActionResult> UploadFile(string filepath)
        {
            // �������������� URL-���� � ���� �� �����
            string fullPath = Path.Combine(storageRoot, filepath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                // ����������� ����������� ������� � ����
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await Request.Body.CopyToAsync(fileStream);
                }

                Log("PUT", filepath);
                return Ok("���� ��������");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "������ ��� �������� �����");
                return StatusCode(500, "������ �������");
            }
        }

        // ��������� GET-�������
        [HttpGet]
        public IActionResult GetFileOrDirectory(string filepath)
        {
            string fullPath = Path.Combine(storageRoot, filepath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            // ���� ���� ���������� � ������ ����
            if (System.IO.File.Exists(fullPath))
            {
                Log("GET", filepath);
                return PhysicalFile(fullPath, "application/octet-stream");
            }
            // ���� ��� ���������� � ������ ������ ������
            else if (Directory.Exists(fullPath))
            {
                var files = Directory.GetFiles(fullPath).Select(Path.GetFileName).ToArray();
                Log("GET-DIR", filepath);
                return Ok(JsonSerializer.Serialize(files));
            }

            // ���� ������ �� ������� � 404
            return NotFound();
        }

        // ��������� HEAD-�������
        [HttpHead]
        public IActionResult GetFileInfo(string filepath)
        {
            string fullPath = Path.Combine(storageRoot, filepath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            // ���� ���� �� ������ � 404
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            // ��������� ���������� � ����� � ���������� � ��������� ������
            var fileInfo = new FileInfo(fullPath);
            Response.Headers["File-Size"] = fileInfo.Length.ToString();
            Response.Headers["Last-Modified"] = fileInfo.LastWriteTimeUtc.ToString("R");

            Log("HEAD", filepath);
            return Ok();
        }

        // ��������� DELETE-�������
        [HttpDelete]
        public IActionResult DeleteFileOrDirectory(string filepath)
        {
            string fullPath = Path.Combine(storageRoot, filepath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            // �������� �����
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
                Log("DELETE-FILE", filepath);
                return Ok("���� �����");
            }
            // �������� ���������� ����������
            else if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
                Log("DELETE-DIR", filepath);
                return Ok("������� �����");
            }

            // ���� �� ������� � 404
            return NotFound();
        }

        // ����� ����������� ��������
        private void Log(string method, string path)
        {
            string logEntry = $"[{DateTime.UtcNow:u}] {method} {path}";
            System.IO.File.AppendAllText(logFile, logEntry + Environment.NewLine);
            logger.LogInformation(logEntry);
        }
    }
}
