using System;

namespace OpenRace.Exceptions
{
    public class AppException : ApplicationException
    {
        public AppException(string message, Exception? innerException = null) 
            : base(message, innerException)
        {}
    }
}