using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace OpenRace.Features.Auth
{
    public class ComponentBaseWithSession : ComponentBase
    {
        [Inject]
        protected SessionService SessionService { get; set; } = null!;

        private Session? _session;
        protected Session Session => _session ?? Session.Empty;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                _session = await SessionService.Get();
                StateHasChanged();
                if (_session.IsAuthorized())
                {
                    await OnAuthorizedAsync();
                }
            }
        }

        protected virtual Task OnAuthorizedAsync()
        {
            return Task.CompletedTask;
        }

        protected async Task SetSession(Session session, bool updateState = true)
        {
            _session = session;
            if(updateState)
                StateHasChanged();
            await SessionService.Set(session);
        }
    }
}