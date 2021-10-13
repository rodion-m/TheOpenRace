using System;

namespace OpenRace
{
    public class AppException : ApplicationException
    {
        public AppException(string message, Exception? innerException = null) 
            : base(message, innerException)
        {}
    }
}