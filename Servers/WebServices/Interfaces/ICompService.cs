using System.ServiceModel;
using Models;

namespace Interfaces
{
    [ServiceContract]
    public interface ICompService
    {
        [OperationContract]
        CompServiceModel TestCompServiceModel(CompServiceModel inputModel);
    }
}