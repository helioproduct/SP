using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using RaGlib.Core;
using RaGlib.Automata;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;


namespace RaGlib {
    public class FSAutomateWithOpSymbols : FSAutomate
    {
        private Dictionary<Symbol_Operation, Delegate> D;
        public List<Symbol_Operation> SigmaOP { set; get; } = null;

        public FSAutomateWithOpSymbols(
            List<Symbol> Q,
            List<Symbol> Sigma,
            List<Symbol_Operation> SigmaOP,
            List<Symbol> F,
            string q0,
            Dictionary<Symbol_Operation, Delegate> D = null) : base(Q, Sigma, F, q0)
        {
            if(D is null)
            {
                D = new Dictionary<Symbol_Operation, Delegate>();
            }

            this.SigmaOP = SigmaOP;
            this.D = D;
            this.Delta = new List<DeltaOPRule>();
        }
        

        public new void AddRule(string state, string opSymbol, string nextState)
        {
            this.Delta.Add(new DeltaOPRule(state, null, new List<Symbol> { new Symbol(nextState) }, opSymbol));
        }

        public void AddRule(string state, string term, string opSymbol, string nextState)
        {
            this.Delta.Add(new DeltaOPRule(state, term, new List<Symbol> { new Symbol(nextState) }, opSymbol));
        }

        private DeltaOPRule FindDeltaRuleByState(Symbol state)
        {
            foreach (DeltaOPRule rule in this.Delta)
            {
                if (rule.LHSQ == state)
                {
                    return rule;
                }
            }

            return null;
        }

        public new void Execute(string inputChain)
        {
            Symbol currentState = this.Q0;
            string chain = inputChain;
            Delegate operation;
            DeltaOPRule rule;

            while (!F.Contains(currentState))
            {
                rule = this.FindDeltaRuleByState(currentState);

                if (rule == null)
                {
                    Console.WriteLine($"Ошибка : не удалось найти дельта-правило по состоянию {currentState}");
                    return;
                }

                if (!D.TryGetValue(rule.OperationSymbol, out operation))
                {
                    Console.WriteLine($"Ошибка : не удалось найти операцию по операционному символу {rule.OperationSymbol}");
                    return;
                }

                (bool, string) result = (false, "");

                if (rule.OperationSymbol.Parameters.Count == 0)
                    result = ((Func<string, (bool, string)>)operation)(chain);

                else if (rule.OperationSymbol.Parameters.Count == 1)
                    result = ((Func<string, int, (bool, string)>)operation)(chain, rule.OperationSymbol.Parameters[0]);

                else if (rule.OperationSymbol.Parameters.Count == 2)
                    result = ((Func<string, int, int, (bool, string)>)operation)(chain, rule.OperationSymbol.Parameters[0], rule.OperationSymbol.Parameters[1]);


                if (!result.Item1)
                {
                    Console.WriteLine("Ошибка");
                    return;
                }

                chain = result.Item2;
                currentState = rule.RHSQ[0];
            }

            Console.WriteLine("Строка обработана");
        }
    }
}