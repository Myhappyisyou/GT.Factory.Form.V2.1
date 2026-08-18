using GT.Factory.Common.Enums;
using GT.Factory.Station.DriverForm.Aynettek;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station.DriverForm.Login
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
