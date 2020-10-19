using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoeBot.Core.Models
{
    public class TradeArgs : EventArgs
    {
        public CustomerInfo TradeIn;
        public bool IsAFK;
        public string log24;
    }
}
