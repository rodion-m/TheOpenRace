using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace OpenRace.Features.Auth
{
    public class SessionService
    {
        private const string SessionKey = "session";
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        private readonly AuthConfig _authConfig;
        private readonly ILogger<SessionService> _logger;
        
        private Account? _session;
        public Account Current => _session ?? Account.Empty;

        public SessionService(
            ProtectedLocalStorage protectedLocalStorage, 
            AuthConfig authConfig,
            ILogger<SessionService> logger)
        {
            _protectedLocalStorage = protectedLocalStorage ?? throw new ArgumentNullException(nameof(protectedLocalStorage));
            _authConfig = authConfig;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task LoadFromLocalStorage()
        {
            try
            {
                var result = await _protectedLocalStorage.GetAsync<Account>(SessionKey);
                _session = result.Value;
            }
            catch (TaskCanceledException e)
            {
                _logger.LogDebug(e, "Session loading is cancelled");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while deserializing a session");
            }
        }
        public void Set(Account account)
        {
            _session = account ?? throw new ArgumentNullException(nameof(account));
        }
        
        public ValueTask Save()
        {
            return _protectedLocalStorage.SetAsync(SessionKey, Current);
        }


        public ValueTask Reset()
        {
            Set(Account.Empty);
            return _protectedLocalStorage.DeleteAsync(SessionKey);
        }

        public bool IsAuthorized() => IsAuthorized(Current);

        public bool IsAuthorized(Account account)
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
            return _authConfig.Users.Any(
                it => it.UserName == account.UserName && it.Password == account.Password);
        }

        public bool IsAdmin() => IsAuthorized() && _authConfig.Admins.Contains(Current.UserName);

        public async ValueTask<bool> Auth(string userName, string password)
        {
            if (userName == null) throw new ArgumentNullException(nameof(userName));
            if (password == null) throw new ArgumentNullException(nameof(password));
            var account = new Account(userName, password);
            if (IsAuthorized(account))
            {
                Set(account);
                await Save();
                return true;
            }

            return false;
        }
    }
}