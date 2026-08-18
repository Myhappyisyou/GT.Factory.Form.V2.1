using GT.Factory.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station.DriverForm.Login
{
    public interface ILoginView
    {
        event Action<User> LoginSuccess;

        void Initialize(LoginConfig config);
    }
}
