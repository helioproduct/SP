using RaGlib.Automata;
using RaGlib.Core;
using System.Collections.Generic;


namespace RaGlib
{
    internal class DeltaOPRule : DeltaQSigma
    {
        public Symbol_Operation OperationSymbol { get; private set; }

        public DeltaOPRule(Symbol LHSQ, Symbol LHSS, List<Symbol> RHSQ, Symbol_Operation OP) : base(LHSQ, LHSS, RHSQ)
        {
            this.OperationSymbol= OP;
        }
    }
}
