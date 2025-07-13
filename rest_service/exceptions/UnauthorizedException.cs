using System;

namespace tiba.rest.exceptions;

public class UnauthorizedException:Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
