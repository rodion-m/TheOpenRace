using System;
using JetBrains.Annotations;

namespace OpenRace
{
    public class RegistrationNumbersEndedException : AppException
    {
        public RegistrationNumbersEndedException(int distance, Exception? innerException = null) 
            : base($"Места на дистанцию {distance} закончились", innerException)
        {
        }
    }
}