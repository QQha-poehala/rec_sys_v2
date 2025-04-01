namespace Rec_backend.Contracts;

public record RequestLogDto(
     Guid Id,
     DateTime CreatedAt,
     object InputData,
     string Result1,
     string Result2,
     string Result3
     );

public record PagedResult(
     int TotalCount,
     int Page,
     List<RequestLogDto> Items
     );