using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaGlib.Core {
    public class Command : TableElem
    {
        public string CommandString=null;
        public Symbol_Operation OpSymbol;
        public Command(string CommandString){
            this.CommandString = CommandString;
        }
        public Command(string CommandString, Symbol_Operation OpSymbol){
            this.CommandString = CommandString;
            this.OpSymbol = OpSymbol;
        }
    }
}
