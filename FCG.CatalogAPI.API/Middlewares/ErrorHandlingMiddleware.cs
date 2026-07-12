using System.Text.Json;

namespace FCG.CatalogAPI.API.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado durante o processamento da requisição.");

            var (statusCode, mensagem, detalhe) = ex switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, ex.Message, ex.StackTrace),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, ex.Message, ex.StackTrace),
                KeyNotFoundException => (StatusCodes.Status404NotFound, ex.Message, ex.StackTrace),
                _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro interno no servidor.", ex.StackTrace)
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var erroResponse = new ErroResponse
            {
                StatusCode = statusCode,
                Mensagem = mensagem,
                Detalhe = detalhe
            };

            var json = JsonSerializer.Serialize(erroResponse);
            await context.Response.WriteAsync(json);
        }
    }
}
