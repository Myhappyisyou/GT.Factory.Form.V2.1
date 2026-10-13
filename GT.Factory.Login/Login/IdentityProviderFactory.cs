using GT.Factory.Common.Enums;
using GT.Factory.RFID.Aynettek;
using System.Collections.Generic;

namespace GT.Factory.Login.Login
{
    public static class IdentityProviderFactory
    {
        public static List<IIdentityProvider> CreateProviders(
            LoginConfig config,
            LoginService loginService,
            IRfidService rfidService)
        {
            var providers = new List<IIdentityProvider>();

            foreach (var type in config.IdentityTypes)
            {
                switch (type)
                {
                    case IdentityType.AccountPassword:
                        providers.Add(new AccountIdentityProvider(loginService, false));
                        break;

                    case IdentityType.Card:
                        providers.Add(new CardIdentityProvider(loginService, rfidService, false));
                        break;

                    case IdentityType.RoleSelect:
                        providers.Add(new RoleIdentityProvider(config.Roles, loginService));
                        break;
                }
            }

            return providers;
        }
    }
}
