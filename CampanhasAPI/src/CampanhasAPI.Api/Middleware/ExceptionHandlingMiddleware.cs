using CampanhasAPI.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CampanhasAPI.Api.Middleware;

/// <summary>
/// Converte exceções de domínio no status HTTP correto (ProblemDetails).
/// Qualquer outra vira 500 sem vazar detalhes internos.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (status, titulo) = ex switch
            {
                DomainException      => (StatusCodes.Status400BadRequest, "Regra de negocio violada"),
                NotFoundException    => (StatusCodes.Status404NotFound, "Nao encontrado"),
                ConflitoException    => (StatusCodes.Status409Conflict, "Conflito"),
                AutenticacaoException => (StatusCodes.Status401Unauthorized, "Nao autenticado"),
                _                    => (StatusCodes.Status500InternalServerError, "Erro interno")
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Excecao nao tratada em {Metodo} {Rota}", context.Request.Method, context.Request.Path);
            else
                logger.LogWarning("{Tipo} em {Metodo} {Rota}: {Mensagem}", ex.GetType().Name, context.Request.Method, context.Request.Path, ex.Message);

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = status == 500 ? "Ocorreu um erro interno." : ex.Message,
                Instance = context.Request.Path,
                Extensions = { ["traceId"] = context.TraceIdentifier }
            });
        }
    }
}
