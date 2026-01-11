using System;
using System.Collections.Generic;
using System.Text;

namespace REslava.Result;

public class ExceptionError : Reason<ExceptionError>, IError
{
    public Exception Exception { get; }

    public ExceptionError(Exception exception) : base(exception?.Message?? "An exception ocurred")
    {
        ArgumentNullException.ThrowIfNull(exception);
        Exception = exception;
        ExceptionInitialize(exception);
    }
    public ExceptionError(Exception exception, string customMessage) : base(customMessage)
    {
        ArgumentNullException.ThrowIfNull(exception);
        Exception = exception;
        ExceptionInitialize(exception);
    }    

    private void ExceptionInitialize(Exception exception)
    {        
        WithTags("ExceptionType", exception.GetType().Name);

        if (exception.StackTrace is not null)
        {
            WithTags("StackTrace", exception.StackTrace);
        }

        if (exception.InnerException is not null)
        {
            WithTags("InnerException", exception.InnerException.Message);
        }
    }
}
