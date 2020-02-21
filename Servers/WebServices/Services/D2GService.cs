using Interfaces;
using Models;

namespace Services
{
    public class D2GService : ID2GService
    {
        public D2GModel TestD2GServiceModel(D2GModel customModel)
        {
            return customModel;
        }
    }


}
