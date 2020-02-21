using System.ServiceModel;
using Models;

namespace Interfaces
{
    [ServiceContract]
    public interface ID2GService
    {
        [OperationContract]
        D2GModel TestD2GServiceModel(D2GModel inputModel);
    }
}