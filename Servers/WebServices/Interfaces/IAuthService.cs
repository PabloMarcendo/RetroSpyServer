using System.ServiceModel;
using Models;

namespace Interfaces
{
    [ServiceContract]
    public interface IAuthService
    {
        [OperationContract]
        AuthModel TestAuthServiceModel(AuthModel inputModel);
    }
}