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
            if (firstRender)
            {
                _session = await SessionService.Get();
                StateHasChanged();
            }
            await base.OnAfterRenderAsync(firstRender);
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