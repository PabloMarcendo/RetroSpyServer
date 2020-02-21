using Interfaces;
using Models;

namespace Services
{
    public class AuthService : IAuthService
    {
        public AuthModel TestAuthServiceModel(AuthModel customModel)
        {
            return customModel;
        }
    }
}
