using Core.Exceptions;
using Serilog;

namespace StockMarketSolution.Middleware
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<Exception> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        public ExceptionHandlingMiddleware(
            ILogger<Exception> logger,
            IDiagnosticContext diagnosticContext
        )
        {
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (FinnhubAccessDeniedException ex)
            {
                LogException(ex);
                ;

                throw;
            }
            catch (Exception ex)
            {
                LogException(ex);

                throw;
            }
        }

        private void LogException(Exception ex)
        {
            if (ex.InnerException != null)
            {
                if (ex.InnerException.InnerException != null)
                {
                    _logger.LogError(
                        "{ExceptionType} {ExceptionMessage}",
                        ex.InnerException.InnerException.GetType().ToString(),
                        ex.InnerException.InnerException.Message
                    );

                    _diagnosticContext.Set(
                        "Exception",
                        $"{ex.InnerException.InnerException.GetType().ToString()}, {ex.InnerException.InnerException.Message}, {ex.InnerException.InnerException.StackTrace}"
                    );
                }
                else
                {
                    _logger.LogError(
                        "{ExceptionType} {ExceptionMessage}",
                        ex.InnerException.GetType().ToString(),
                        ex.InnerException.Message
                    );

                    _diagnosticContext.Set(
                        "Exception",
                        $"{ex.InnerException.GetType().ToString()}, {ex.InnerException.Message}, {ex.InnerException.StackTrace}"
                    );
                }
            }
            else
            {
                _logger.LogError(
                    "{ExceptionType} {ExceptionMessage}",
                    ex.GetType().ToString(),
                    ex.Message
                );

                _diagnosticContext.Set(
                    "Exception",
                    $"{ex.GetType()}, {ex.Message}, {ex.StackTrace}"
                );
            }
        }
    }

    public static class ExceptionHandlingMiddlewareExtension
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(
            this IApplicationBuilder builder
        )
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
