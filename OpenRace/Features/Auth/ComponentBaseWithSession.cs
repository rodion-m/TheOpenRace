using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace OpenRace.Features.Auth
{
    public class ComponentBaseWithSession : ComponentBase
    {
        [Inject]
        protected SessionService SessionService { get; set; } = null!;

        protected Account Account => SessionService.Current;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await SessionService.LoadFromLocalStorage();
                StateHasChanged();
                if (SessionService.IsAuthorized())
                {
                    await OnAuthorizedAsync();
                }
            }
        }

        protected virtual Task OnAuthorizedAsync()
        {
            return Task.CompletedTask;
        }

    }
}