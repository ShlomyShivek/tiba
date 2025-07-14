using System;

namespace Tiba.Rest.Exceptions;

public class UnauthorizedException:Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
