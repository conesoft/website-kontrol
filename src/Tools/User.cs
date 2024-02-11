using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Security.Principal;

namespace Conesoft_Website_Kontrol.Tools;
public class User
{
    private readonly AuthenticationStateProvider state;

    public User(AuthenticationStateProvider state)
    {
        this.state = state;
    }

    public async Task<ClaimsPrincipal> Get() => (await state.GetAuthenticationStateAsync()).User;
    public async Task<IIdentity?> GetIdentity() => (await Get()).Identity;
    public async Task<string?> GetName() => (await Get()).Identity?.Name;
    public async Task<bool> IsInRole(string role) => (await Get()).IsInRole(role);
}