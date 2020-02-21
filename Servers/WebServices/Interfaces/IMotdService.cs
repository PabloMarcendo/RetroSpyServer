using System.ServiceModel;
using Models;

namespace Interfaces
{
    [ServiceContract]
    public interface IMotdService
    {
        [OperationContract]
        MotdModel TestMotdServiceModel(MotdModel inputModel);
    }
}