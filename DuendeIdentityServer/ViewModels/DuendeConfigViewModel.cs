using Duende.IdentityServer.Test;

namespace DuendeIdentityServer.ViewModels
{
    public class DuendeConfigViewModel
    {
        public IEnumerable<TestUser>? TestUsers => Config.TestUsers;
    }
}
