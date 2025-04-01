using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Rec_backend.Models;

public class RequestLog
{
    public RequestLog(string inputData, string result1, string result2, string result3 )
    {
        CreatedAt = DateTime.UtcNow;
        InputData = inputData;
        Result1 = result1;
        Result2 = result2;
        Result3 = result3;   
    }
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string InputData { get; set; }

    public string Result1 { get; set; }

    public string Result2 { get; set; }

    public string Result3 { get; set; }

}
