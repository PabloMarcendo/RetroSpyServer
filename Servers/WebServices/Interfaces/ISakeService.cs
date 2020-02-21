using System.ServiceModel;
using Models;

namespace Interfaces
{
    [ServiceContract]
    public interface ISakeService
    {
        [OperationContract]
        SakeModel TestSakeServiceModel(SakeModel inputModel);
    }
}