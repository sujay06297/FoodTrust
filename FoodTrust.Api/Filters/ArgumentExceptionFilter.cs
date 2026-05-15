using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FoodTrust.Api.Filters;

public sealed class ArgumentExceptionFilter : IExceptionFilter
{
    /// <summary>
    /// 將參數驗證例外轉換成 HTTP 400 ProblemDetails 回應。
    /// </summary>
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ArgumentException exception)
        {
            return;
        }

        context.Result = new BadRequestObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid request.",
            Detail = exception.Message
        });
        context.ExceptionHandled = true;
    }
}
