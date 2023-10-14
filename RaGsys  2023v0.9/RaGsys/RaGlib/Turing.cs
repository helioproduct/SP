using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using RaGlib.Core;
using RaGlib.Automata;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

namespace RaGlib
{
    public class Tape
    {
        public string chainString { get; set; }
        public int head{ get; set; }
    }
    public class Turing:FSAutomate
    {
        private delegate Tape operation(Tape tape,string value);
        private Dictionary<Symbol_Operation, operation> OpDictionary = new Dictionary<Symbol_Operation, operation>();
        public List<Symbol_Operation> SigmaOP { set; get; } = null;
        //private List<Char> Transportation { get; } =new  List<char>{ 'R', 'L', 'B' };
        public Turing(List<Symbol> Q, List<Symbol> Sigma, List<Symbol_Operation> SigmaOP, List<Symbol> F, Symbol q0)
        {
            this.Q = Q;
            this.Sigma = Sigma;
            this.SigmaOP = SigmaOP;
            this.Q0 = q0;
            this.F = F;
            this.Delta = new List<DeltaQSigma>();
            operation OP = new operation(C);
            OpDictionary.Add(new Symbol_Operation("{C}"), OP);
           
            OP = new operation(R);
            OpDictionary.Add(new Symbol_Operation("{R}"), OP);
            OP = new operation(L);
            OpDictionary.Add(new Symbol_Operation("{L}"), OP);
            OP = new operation(B);
            OpDictionary.Add(new Symbol_Operation("{B}"), OP);


        }
        public static Tape R(Tape tape, string value)
        {
            Tape res = new Tape();
            
            res.chainString = tape.chainString;
            res.head = tape.head + 1;
            
            return res;
        }
        public static Tape B(Tape tape, string value)
        {

            return tape;
        }
        public static Tape L(Tape tape, string value)
        {
            Tape res = new Tape();

            res.chainString = tape.chainString;
            res.head = tape.head - 1;

            return res;
        }
        public void AddRule(string state, string term, string opSymbol, string var,string nextState)
        {
            this.Delta.Add(new DeltaQSigma(state, term, new List<Symbol> { new Symbol(nextState), new Symbol_Operation(opSymbol),new Symbol(var) }));
        }
        private static Tape C(Tape tape, string value)
        {
            string inStr = tape.chainString;
            inStr = tape.chainString.Remove(tape.head, 1);

            inStr = inStr.Insert(tape.head, value);
            Tape res = new Tape() { chainString = inStr,head = tape.head};
            //Console.WriteLine("change "+value+" ");
            return res;
        }
        public new void Execute(string chineSymbol)
        {
            Tape tape = new Tape() { chainString = chineSymbol, head = 0 };
            var currState = this.Q0;
            int flag = 0;
            int i = 0;
            Console.WriteLine("Len: "+ chineSymbol.Length);
            Console.WriteLine("Tape: " + chineSymbol);
            while (!F.Contains(currState))
            {
                flag = 0;
                foreach (var d in this.Delta)
                {

                    if (d.LHSQ == currState &&tape.head<tape.chainString.Length &&d.LHSS == tape.chainString.Substring(tape.head, 1))
                    {
                        currState = d.RHSQ[0].symbol;
                        //Console.WriteLine("Current state: "+ currState);
                        if (d.RHSQ.Count > 1)
                        {

                            operation OP = OpDictionary[new Symbol_Operation(d.RHSQ[1].symbol)];
                            
                            tape = OP(tape,d.RHSQ[2].symbol);
                            
                        }
                        flag = 1;
                        break;
                    }
                }
                if (flag == 0) break;
            } // end for

            
            Console.WriteLine("Result: " + tape.chainString);
        } // end Execute
    }
}
