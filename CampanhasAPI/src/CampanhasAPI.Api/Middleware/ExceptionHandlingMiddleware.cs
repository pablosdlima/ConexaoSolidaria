using CampanhasAPI.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CampanhasAPI.Api.Middleware;

/// <summary>
/// AppException → o status que ela declara, com a mensagem (ProblemDetails).
/// Qualquer outra → 500 sem vazar detalhes internos.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            logger.LogWarning("{Tipo} em {Metodo} {Rota}: {Mensagem}",
                ex.GetType().Name, context.Request.Method, context.Request.Path, ex.Message);
            await Responder(context, ex.StatusCode, ex.Titulo, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Excecao nao tratada em {Metodo} {Rota}", context.Request.Method, context.Request.Path);
            await Responder(context, StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu um erro interno.");
        }
    }

    private static Task Responder(HttpContext context, int status, string titulo, string detalhe)
    {
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = detalhe,
            Instance = context.Request.Path,
            Extensions = { ["traceId"] = context.TraceIdentifier }
        });
    }
}
