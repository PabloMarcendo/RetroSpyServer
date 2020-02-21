using Interfaces;
using Models;

namespace Services
{
    public class SakeService : ISakeService
    {
        public SakeModel TestSakeServiceModel(SakeModel customModel)
        {
            return customModel;
        }
    }


}
