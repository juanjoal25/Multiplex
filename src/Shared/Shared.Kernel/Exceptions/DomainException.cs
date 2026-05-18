namespace Shared.Kernel.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string code, string message) : base(message) => Code = code;
    public DomainException(string message) : this("domain.error", message) { }
}

public sealed class InvariantViolationException : DomainException
{
    public InvariantViolationException(string message) : base("invariant.violation", message) { }
}

public sealed class PreconditionFailedException : DomainException
{
    public PreconditionFailedException(string message) : base("precondition.failed", message) { }
}

public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base("conflict", message) { }
}
