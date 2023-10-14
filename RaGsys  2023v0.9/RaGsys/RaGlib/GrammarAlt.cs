using RaGlib.Automata;
using RaGlib.Core;
using RaGlib.Grammars;
using System;
using System.Collections.Generic;

namespace RaGlib
{
    public class GrammarAlt : Grammar
    {
        public GrammarAlt(List<Symbol> T, List<Symbol> V, string S0) : base(T, V, S0)
        {

        }

        public GrammarAlt(List<Symbol> T, List<Symbol> V, List<Production> production, string S0) : base(T, V, S0)
        {

        }

        public GrammarAlt() : base()
        {

        }

        public new PDAAlt Transform()
        {
            var Q = new List<Symbol>() { "q" };
            var Sigma = new List<Symbol>(this.T);
            var F = new List<Symbol>();
            var Gamma = new List<Symbol>(this.V);
            foreach (var t in this.T)
                Gamma.Add(t);
            var Q0 = "q0";
            var Z = new Stack<Symbol>();
            Z.Push(new Symbol("z0"));
            Z.Push(this.S0);

            var Delta = new List<DeltaQSigmaGamma>();
            DeltaQSigmaGamma delta = null;

            var rhsq = new List<Symbol>() { "q" };
            var rhsz = new List<Symbol>() { "ε" };

            foreach (var p in this.P)
            {
                if (!this.V.Contains(p.LHS.symbol))
                {
                    Console.Write($"This grammar cant have rule: {p.LHS.symbol} -> ");
                    foreach (var s in p.RHS)
                    {
                        Console.Write(s.symbol);
                    }
                    Console.Write("\n");
                    return null;
                }
                foreach (var t in p.RHS)
                {
                    if (!this.T.Contains(t) && !this.V.Contains(t))
                    {
                        Console.Write($"This grammar cant have rule: {p.LHS.symbol} -> ");
                        foreach (var s in p.RHS)
                        {
                            Console.Write(s.symbol);
                        }
                        Console.Write("\n");
                        return null;
                    }
                }
            }

            foreach (var p in this.P)
            {
                delta = new DeltaQSigmaGamma("q", "ε", p.LHS.symbol, rhsq, p.RHS);
                Delta.Add(delta);
            }
            foreach (var t in this.T)
            {
                delta = new DeltaQSigmaGamma("q", t.symbol, t.symbol, rhsq, rhsz);
                Delta.Add(delta);
            }

            delta = new DeltaQSigmaGamma("q0", "ε", "z0", new List<Symbol> { "qf" }, new List<Symbol> { "ε" });
            Delta.Add(delta);

            PDAAlt KA = new PDAAlt(Q, Sigma, Gamma, Q0, "z0", F);
            KA.Delta = Delta;
            KA.Z.Push(this.S0);
            return KA;
        }
        public new void AddRule(string LeftNoTerm, List<Symbol> RHS)
        {
            var TmpRHS = new List<Symbol>();
            foreach (var s in RHS)
            {
                if (s.symbol == "|")
                {
                    this.P.Add(new Production(LeftNoTerm, TmpRHS));
                    TmpRHS = new List<Symbol>();
                }
                else
                {
                    TmpRHS.Add(s);
                }
            }
            if (TmpRHS.Count > 0)
            {
                this.P.Add(new Production(LeftNoTerm, TmpRHS));
            }
        }

    }
}