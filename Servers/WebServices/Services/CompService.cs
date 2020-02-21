using Interfaces;
using Models;

namespace Services
{
    public class CompService : ICompService
    {
        public CompServiceModel TestCompServiceModel(CompServiceModel customModel)
        {
            return customModel;
        }
    }


}
