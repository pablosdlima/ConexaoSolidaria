using FluentValidation;

namespace CampanhasAPI.Api.Filters;

/// <summary>
/// Roda o validador FluentValidation do request antes do handler.
/// Falha vira 400 no formato ProblemDetails (RFC 7807), com os erros por campo.
/// </summary>
public class ValidationEndpointFilter<TRequest> : IEndpointFilter where TRequest : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null) return await next(context);

        var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
        if (validator is null) return await next(context);

        var resultado = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (resultado.IsValid) return await next(context);

        var erros = resultado.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return Results.ValidationProblem(erros, title: "Erro de validacao");
    }
}
