using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace OpenRace.Features.Auth
{
    public class SessionService
    {
        private const string SessionKey = "session";
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        private readonly ILogger<SessionService> _logger;

        public SessionService(ProtectedLocalStorage protectedLocalStorage, ILogger<SessionService> logger)
        {
            _protectedLocalStorage = protectedLocalStorage;
            _logger = logger;
        }

        public async Task<Session> Get()
        {
            try
            {
                var result = await _protectedLocalStorage.GetAsync<Session>(SessionKey);
                return result.Value ?? Session.Empty;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while deserializing a session");
                return Session.Empty;
            }
        }
        public ValueTask Set(Session session)
        {
            return _protectedLocalStorage.SetAsync(SessionKey, session);
        }

        public ValueTask Reset() => Set(Session.Empty);
    }
}