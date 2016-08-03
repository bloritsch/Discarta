using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHaven.DisCarta.PreProcessor.Model
{
    public struct Status
    {
        public string Message;
        public double Current;
        public double Total;
        public double Percent => Current / Total;
    }
}
