namespace CampanhasAPI.Application.Dtos.Responses;

public sealed record LoginResponse(string Token, DateTimeOffset ExpiraEm, string Role);
