using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rec_backend.Contracts;
using Rec_backend.Data;
using Rec_backend.Models;
using Rec_backend.Services;

namespace Rec_backend.Controllers
{
    [ApiController]

    public class RecController : ControllerBase
    {
        private readonly RecommendationService _recommendationService;
        private readonly StudyDirectionService _studyDirectionService;
        private readonly OnnxMapper _onnxMapper;
        private readonly RecDbContext _db;
        private readonly AdminService _admin;

        public RecController(RecDbContext db, RecommendationService recommendationService, OnnxMapper onnxMapper, StudyDirectionService studyDirectionService, AdminService admin)
        {
            _db = db;
            _recommendationService = recommendationService;
            _onnxMapper = onnxMapper;
            _studyDirectionService = studyDirectionService;
            _admin = admin;
        }
        [HttpPost]
        [Route("recommendation")]
        public async Task<IActionResult> GetRecommendation([FromBody] GetQuestionnaireRequest request, CancellationToken ct)
        {
            try
            {
                // Converting a query to a float array for ML models
                var mlInput = _onnxMapper.ToMlInput(request);
                // Get recommendations 
                var recommendationNames = _recommendationService.GetRecommendations(mlInput);
                if (recommendationNames.Count < 3)
                    return BadRequest("Модель вернула недостаточно рекомендаций");
                //  Get code
                var result = recommendationNames
                .Select(name => new
                {
                    Name = name,
                    Code = _studyDirectionService.GetDirectionCode(name) ?? "NOT_FOUND"
                })
                .ToList();

                var requestLog = new RequestLog(
                    JsonSerializer.Serialize(request), // Input 
                    result.ElementAtOrDefault(0)?.Name ?? "null", // result 1
                    result.ElementAtOrDefault(1)?.Name ?? "null", // result 2
                    result.ElementAtOrDefault(2)?.Name ?? "null"); // result 3

                await _db.RequestLogs.AddAsync(requestLog, ct);
                await _db.SaveChangesAsync(ct);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Произошла ошибка: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("auth")]
        public IActionResult GetUser([FromBody] GetUserRequest request, CancellationToken ct)
        {
            if ( _admin.GetAdmin(request.login, request.password))
                return Ok(new { Message = "Authentication successful" });
            return Unauthorized(new { Message = "Invalid login or password" });
        }

        [HttpGet]
        [Route("get_data_for_table")]
        public async Task<IActionResult> GetDataForTable(
            [FromQuery] int page = 1,
            CancellationToken ct = default)
        {
            try
            {
                if (page < 1) page = 1;
                int pageSize = 10;
                var query = _db.RequestLogs.AsQueryable();

                var totalCount = await query.CountAsync(ct);

                var items = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new RequestLogDto(
                        x.Id,
                        x.CreatedAt,
                        FormatInputData(x.InputData),
                        x.Result1,
                        x.Result2,
                        x.Result3))
                    .ToListAsync(ct);

                var result = new PagedResult(
                    totalCount,
                    page,
                    items);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Произошла ошибка при получении данных",
                    Error = ex.Message
                });
            }
        }
        private static object FormatInputData(string inputData)
        {
            var jsonData = JsonSerializer.Deserialize<JsonElement>(inputData);
            return ProcessJsonElement(jsonData);
        }
        // Parse json
        private static object? ProcessJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject()
                    .ToDictionary(
                        prop => prop.Name,
                        prop => ProcessJsonElement(prop.Value)),

                JsonValueKind.Array => element.EnumerateArray()
                    .Select(ProcessJsonElement)
                    .ToList(),

                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetInt32(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.ToString()
            };
        }
        [HttpGet]
        [Route("getfile")]
        public async Task<IActionResult> GetFileCSV(CancellationToken ct)
        {
            try
            {
                var records = await _db.RequestLogs
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync(ct);

                if (!records.Any())
                    return NotFound("Нет данных для экспорта");

                // Create CSV string
                var csvBuilder = new StringBuilder();

                // Headers CSV
                csvBuilder.AppendLine("ID,CreatedAt,InputData,Result1,Result2,Result3");
                foreach (var record in records)
                {
                    var escapedInputData = EscapeCsvValue(record.InputData ?? string.Empty);

                    csvBuilder.AppendLine(
                        $"{record.Id}," +
                        $"{record.CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                        $"{escapedInputData}," +
                        $"{EscapeCsvValue(record.Result1 ?? string.Empty)}," +
                        $"{EscapeCsvValue(record.Result2 ?? string.Empty)}," +
                        $"{EscapeCsvValue(record.Result3 ?? string.Empty)}");
                }
                var memoryStream = new MemoryStream();
                var writer = new StreamWriter(memoryStream, Encoding.UTF8);
                await writer.WriteAsync(csvBuilder.ToString());
                await writer.FlushAsync();
                memoryStream.Position = 0;
                return File(memoryStream, "text/csv", $"export_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
            catch
            {
                return StatusCode(500, "Произошла ошибка при генерации CSV файла");
            }
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
