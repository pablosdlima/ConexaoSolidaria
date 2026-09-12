namespace CampanhasAPI.Application.Dtos.Requests;

/// <summary>RF-5: o doador informa IdCampanha e ValorDoacao. O IdDoador vem do JWT.</summary>
public sealed record DoacaoRequest(Guid IdCampanha, decimal ValorDoacao);
