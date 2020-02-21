using Interfaces;
using Models;

namespace Services
{
    public class MotdService : IMotdService
    {
        public MotdModel TestMotdServiceModel(MotdModel customModel)
        {
            return customModel;
        }
    }


}
