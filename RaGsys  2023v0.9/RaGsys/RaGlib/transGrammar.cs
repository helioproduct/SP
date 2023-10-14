﻿using System;
using System.Collections.Generic;
using RaGlib.Core;
using RaGlib.Grammars;

namespace RaGlib {
    public class TransGrammar : Grammar
    {
        public List<Symbol_Operation> SigmaA { set; get;} = null;

        public TransGrammar()
        {
            this.V = null;
            this.T = null;           
            this.P = new List<Production>();
            this.S0 = null;
        }
        public TransGrammar(List<Symbol> V, List<Symbol> SigmaI, List<Symbol_Operation> SigmaA, string S)
        {
            this.V = V;
            this.T = SigmaI;
            this.SigmaA = SigmaA;
            this.P = new List<Production>();
            this.S0 = S;
        }

        public TransGrammar(List<Symbol> V, List<Symbol> SigmaI, List<Symbol_Operation> SigmaA, List<Production> P, string S)
        {
            this.V = V;
            this.T = SigmaI;
            this.SigmaA = SigmaA;
            this.P = P;
            this.S0 = S;
        }
    
    public void AddRule(Symbol lhs, List<Symbol> rhs)
        {
            Production tmp = new Production(lhs, rhs);
            P.Add(tmp);
        }


        public void PrintTransGrammar()
        {
            Console.WriteLine("Результирующая транслирующая грамматика:");
            Console.Write("V = { ");
            foreach (Symbol s in V)
            {
                Console.Write(s.symbol + " ");
            }
            Console.Write("}\n");

            Console.Write("SigmaI = { ");
            foreach (Symbol s in T)
            {
                Console.Write(s.symbol + " ");
            }
            Console.Write("}\n");

            Console.Write("SigmaA = { ");
            foreach (Symbol s in SigmaA)
            {
                Console.Write(s.symbol + " ");
            }
            Console.Write("}\n");

            Console.WriteLine("S0 = " + S0);

            Console.WriteLine("P = {");
            foreach (Production p in P)
            {
                Console.Write(p.LHS.symbol + " -> ");
                foreach (Symbol s in p.RHS)
                {
                    if (s is Symbol_Operation)
                    {
                        Console.Write("{" + s.symbol + "}");
                    }
                    else
                    {
                        Console.Write(s.symbol);
                    }
                }
                Console.Write("\n");
            }
            Console.WriteLine("}");
        }

        //преобразовать строку в список операционных и обычных символов
        public List<Symbol> transformInSymbols(string inputChain)
        {
            bool isOPSymbol = false;
            List<Symbol> result = new List<Symbol>();
            foreach (char c in inputChain)
            {
                if (c == '{')
                {
                    isOPSymbol = true;
                } else if (c == '}')
                {
                    isOPSymbol = false;
                } else
                {
                    if (isOPSymbol)
                    {
                        result.Add(new Symbol_Operation(c.ToString()));
                    } else
                    {
                        result.Add(new Symbol(c.ToString()));
                    }
                }
            }
            return result;
        }

        public bool isOPSymbol(string symbol)
        {
            foreach (Symbol_Operation s in SigmaA)
            {
                if (s.symbol == symbol)
                {
                    return true;
                }
            }
            return false;
        }

        //конструктор для построения транслирующей грамматики из КС-грамматики
        public TransGrammar(Grammar CFGrammar, string inputChain, string transChain)
        {
            ConverterInTransGrammar converter = new ConverterInTransGrammar(CFGrammar.T, CFGrammar.V, CFGrammar.P, CFGrammar.S0.symbol);
                converter.Construct();
                TransGrammar result = new TransGrammar();
                result = converter.ConvertInTransGrammar(CFGrammar, inputChain, transChain);
                this.T = result.T;
                this.V = result.V;
                this.P = result.P;
                this.S0 = result.S0;
            }

            public override void Inference() {}
        }


        public class ConverterInTransGrammar : SLRGrammar {
            public ConverterInTransGrammar(List<Symbol> T, List<Symbol> V, List<Production> P, string S0) : base(T, V, P, S0) { }


        public TransGrammar ConvertInTransGrammar(Grammar CFGrammar, string input, string transChain)
        {
            Console.WriteLine("\nПреобразование в транслирующую грамматику...");
            Console.WriteLine("Цепочка для КС-грамматики: " + input);
            Console.WriteLine("Транслирующая цепочка: " + transChain);
            Console.Write("\n");

            int shift = 0;
            while (P[shift].LHS.symbol == "S'")
            {
                    ++shift;
            }

            TransGrammar result = new TransGrammar(CFGrammar.V, CFGrammar.T, new List<Symbol_Operation>(), CFGrammar.P, CFGrammar.S0.symbol);

            string w = input + "$";
            Stack<int> st = new Stack<int>();
            st.Push(0);
            int i = 0;
            Stack<int> res = new Stack<int>();
            bool accepted = false;
            bool error = false;
            do
            {
                char a = w[i];
                PairSymbInt curCondition = new PairSymbInt(a, st.Peek());
                PairSymbInt tableCondition = null;
                if (!M.TryGetValue(curCondition, out tableCondition))
                {
                    error = true;
                    break;
                }
                switch (tableCondition.First.symbol)
                {
                    // Accept - Принятие
                    case "A":
                        accepted = true;
                        break;
                    // Shift - Перенос
                    case "S":
                        st.Push(tableCondition.Second);
                        ++i;
                        break;
                    // Reduction - Свёртка
                    case "R":
                        int rulePos = tableCondition.Second;
                        Production rule = P[rulePos];
                        for (int j = 0; j < rule.RHS.Count; ++j)
                        {
                            st.Pop();
                        }

                        if (P[rulePos].RHS.Count == 1 && isNoTerm(P[rulePos].RHS[0].symbol))
                        {
                            Console.WriteLine("свертка по цепному правилу");
                            Console.Write(P[rulePos].LHS.symbol + " -> ");
                            foreach (Symbol s in P[rulePos].RHS)
                            {
                                if (s is Symbol_Operation)
                                {
                                    Console.Write("{" + s.symbol + "}");
                                }
                                else
                                {
                                    Console.Write(s.symbol);
                                }
                            }
                            Console.Write("\n");

                            Console.Write("остаток транслирующей цепочки: ");
                            Console.Write(transChain);
                            Console.Write("\n\n");
                        } 
                        else if (result.P[rulePos - shift].RHS.Count == 0)
                        {
                                Console.WriteLine("свертка по эпсилон-правилу");
                                Console.Write(P[rulePos].LHS.symbol + " -> ");
                                foreach (Symbol s in P[rulePos].RHS)
                                {
                                    if (s is Symbol_Operation)
                                    {
                                        Console.Write("{" + s.symbol + "}");
                                    }
                                    else
                                    {
                                        Console.Write(s.symbol);
                                    }
                                }
                                Console.Write("\n");

                                Console.Write("остаток транслирующей цепочки: ");
                                Console.Write(transChain);
                                Console.Write("\n\n");
                            }
                        else if (result.P[rulePos - shift].RHS.Count == P[rulePos].RHS.Count)
                        {
                            result.P[rulePos - shift].RHS.Add(new Symbol_Operation(transChain[0].ToString()));
                            transChain = transChain.Substring(1);

                            Console.WriteLine("свертка по правилу");
                            Console.Write(P[rulePos].LHS.symbol + " -> ");
                            foreach (Symbol s in P[rulePos].RHS)
                            {
                                if (s is Symbol_Operation)
                                {
                                    Console.Write("{" + s.symbol + "}");
                                }
                                else
                                {
                                    Console.Write(s.symbol);
                                }
                            }
                            Console.Write("\n");

                            Console.WriteLine("соответствующее правило изменено:");
                            Console.Write(result.P[rulePos - shift].LHS.symbol + " -> ");
                            foreach (Symbol s in result.P[rulePos - shift].RHS)
                            {
                                if (s is Symbol_Operation)
                                {
                                    Console.Write("{" + s.symbol + "}");
                                }
                                else
                                {
                                    Console.Write(s.symbol);
                                }
                            }
                            Console.Write("\n");

                            Console.Write("остаток транслирующей цепочки: ");
                            Console.Write(transChain);
                            Console.Write("\n\n");
                            } 
                        else
                        {
                            Console.WriteLine("свертка по правилу");
                            Console.Write(P[rulePos].LHS.symbol + " -> ");
                            foreach (Symbol s in P[rulePos].RHS)
                            {
                                if (s is Symbol_Operation)
                                {
                                    Console.Write("{" + s.symbol + "}");
                                }
                                else
                                {
                                    Console.Write(s.symbol);
                                }
                            }
                            Console.Write("\n");
                            Console.WriteLine("Правило свертки уже изменено");
                            transChain = transChain.Substring(1);
                            Console.Write("\n");
                        }

                        curCondition = new PairSymbInt(rule.LHS.ToString(), st.Peek());
                        tableCondition = M[curCondition];
                        st.Push(tableCondition.Second);
                        res.Push(rulePos);
                        break;
                    default:
                        error = true;
                        break;
                }
            }
            while (!accepted && !error);

            if (accepted)
            {
                Console.WriteLine("Результирующая транслирующая грамматика:");
                Console.Write("V = { ");
                foreach (Symbol s in result.V)
                {
                        Console.Write(s.symbol + " ");
                }
                Console.Write("}\n");

                Console.Write("SigmaI = { ");
                foreach (Symbol s in result.T)
                {
                        Console.Write(s.symbol + " ");
                }
                Console.Write("}\n");

                Console.Write("SigmaA = { ");
                foreach (Symbol s in result.SigmaA)
                {
                    Console.Write(s.symbol + " ");
                }
                Console.Write("}\n");

                Console.WriteLine("S0 = " + result.S0);

                Console.WriteLine("P = {" );
                foreach (Production p in result.P)
                {
                    Console.Write(p.LHS.symbol + " -> ");
                    foreach (Symbol s in p.RHS)
                    {
                        if (s is Symbol_Operation)
                        {
                            Console.Write("{" + s.symbol + "}");
                        }
                        else
                        {
                            Console.Write(s.symbol);
                        }
                    }
                    Console.Write("\n");
                }
                Console.WriteLine("}");
                return result;
            }
            else
            {
                Console.WriteLine("Строка отвергнута, LR(1)-разбор завершился неудачей");
                return result;
            }
        }
}
}