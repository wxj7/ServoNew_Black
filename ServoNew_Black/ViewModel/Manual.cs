using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServoNew_Black.ViewModel
{
    public class Mannual : BaseViewModel
    {
        public ServoNewVM ServoNewVM_Ins { get; private set; }
        public Mannual()
        {

            ServoNewVM_Ins = ServoNewVM.Instance;
        }
    }
}
