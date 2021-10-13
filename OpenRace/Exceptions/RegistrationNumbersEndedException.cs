using System;

namespace OpenRace.Exceptions
{
    public class RegistrationNumbersEndedException : AppException
    {
        public RegistrationNumbersEndedException(int distance, Exception? innerException = null) 
            : base($"Места на дистанцию {distance} закончились", innerException)
        {
        }
    }
}