using Azul.Api.Models.Output;
using Azul.Core.Util;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Azul.Api.Util;

public class AzulExceptionFilterAttribute(ILogger logger) : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is DataNotFoundException)
        {
            logger.LogWarning(context.Exception,
                $"Client asked for a resource that does not exist. Request: {GetRequestUrl(context)}");

            context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Result = new NotFoundResult();
        }
        else if (context.Exception is ApplicationException || context.Exception is InvalidOperationException)
        {
            logger.LogWarning(context.Exception,
                $"Invalid client input caused an exception. Request: {GetRequestUrl(context)}");

            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Result = new JsonResult(new ErrorModel(context.Exception));
        }
        else
        {
            logger.LogError(context.Exception,
                $"An unhandled exception occurred in the application. Request: {GetRequestUrl(context)}");

            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Result = new JsonResult(new ErrorModel(context.Exception));
        }


    }

    private string GetRequestUrl(ExceptionContext context)
    {
        if (context.HttpContext?.Request == null) return string.Empty;
        return $"{context.HttpContext.Request.Method} - {context.HttpContext.Request.GetDisplayUrl()}";
    }
}