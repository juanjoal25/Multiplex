using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Kernel.Exceptions;

namespace Infraestructura.Api.Filters;

public sealed class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext ctx)
    {
        var (status, code) = ctx.Exception switch
        {
            ConflictException => (StatusCodes.Status409Conflict, "conflict"),
            PreconditionFailedException => (StatusCodes.Status400BadRequest, "precondition.failed"),
            InvariantViolationException => (StatusCodes.Status422UnprocessableEntity, "invariant.violation"),
            DomainException => (StatusCodes.Status400BadRequest, "domain.error"),
            _ => (0, "")
        };
        if (status == 0) return;
        ctx.Result = new ObjectResult(new ProblemDetails { Title = code, Detail = ctx.Exception.Message, Status = status }) { StatusCode = status };
        ctx.ExceptionHandled = true;
    }
}
