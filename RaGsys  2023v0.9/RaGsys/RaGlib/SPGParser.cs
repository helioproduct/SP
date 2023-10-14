using System;
using System.Collections.Generic;
using RaGlib.Core;
using System.Linq;

namespace RaGlib
{
    // Курсовая работа студента группы М8О-210Б-21 Катин Иван 
    public class SPGParser
    {
        private Grammar grammar;
        private Dictionary<Symbol, HashSet<Symbol>> LHSForAllNonTerms;
        private Dictionary<Symbol, HashSet<Symbol>> RHSForAllNonTerms;

        public SPGParser()
        {
            grammar = new Grammar(new List<Symbol>(), new List<Symbol>(), new List<Production>(), "S");
            LHSForAllNonTerms = new Dictionary<Symbol, HashSet<Symbol>>();
            RHSForAllNonTerms = new Dictionary<Symbol, HashSet<Symbol>>();
        }

        public SPGParser(Grammar grammar)
        {
            this.grammar = grammar;
            LHSForAllNonTerms = new Dictionary<Symbol, HashSet<Symbol>>();
            RHSForAllNonTerms = new Dictionary<Symbol, HashSet<Symbol>>();
        }
        // Пример
        public void Example()
        {

            grammar = new Grammar(new List<Symbol>() { "a", "c", "g", "d", "f", "x", "b", "m" },
                new List<Symbol>() { "S", "A", "B", "D", "C", "R", "N" },
                "S"
            );
            grammar.AddRule("S", new List<Symbol>() { "a", "A", "b" });
            grammar.AddRule("A", new List<Symbol>() { "c", "D", "C", "D" });
            grammar.AddRule("A", new List<Symbol>() { "x", "N" });
            grammar.AddRule("B", new List<Symbol>() { "c", "D" });
            grammar.AddRule("D", new List<Symbol>() { "g" });
            grammar.AddRule("C", new List<Symbol>() { "d", "R" });
            grammar.AddRule("R", new List<Symbol>() { "f", "B" });
            grammar.AddRule("N", new List<Symbol>() { "m", "N" });
            grammar.AddRule("N", new List<Symbol>() { "m" });
            Execute();
        }

        public HashSet<Symbol> LHS(Symbol x) // Нахождения множества, задаваемого функцией LHS для нетерминала X
        {
            if (!grammar.V.Contains(x)) return null;
            if (LHSForAllNonTerms.ContainsKey(x))
            {
                return LHSForAllNonTerms[x];
            }

            LHSForAllNonTerms[x] = new HashSet<Symbol>();
            foreach (var production in grammar.P)
            {
                var rhs = production.RHS;
                if (production.LHS == x.symbol)
                {
                    if (!LHSForAllNonTerms[x].Contains(rhs[0]))
                    {
                        LHSForAllNonTerms[x].Add(rhs[0]);
                        if (grammar.V.Contains(rhs[0]))
                        {
                            var newResult = LHS(rhs[0]);
                            foreach (var y in newResult)
                            {
                                LHSForAllNonTerms[x].Add(y);
                            }
                        }
                    }
                }
            }

            return LHSForAllNonTerms[x];
        }

        public HashSet<Symbol> RHS(Symbol x) // Нахождения множества, задаваемого функцией RHS для нетерминала X
        {
            if (!grammar.V.Contains(x)) return null;
            if (RHSForAllNonTerms.ContainsKey(x))
            {
                return RHSForAllNonTerms[x];
            }

            RHSForAllNonTerms[x] = new HashSet<Symbol>();
            foreach (var production in grammar.P)
            {
                var rhs = production.RHS;
                if (production.LHS == x.symbol)
                {
                    if (!RHSForAllNonTerms[x].Contains(rhs[^1]))
                    {
                        RHSForAllNonTerms[x].Add(rhs[^1]);
                        if (grammar.V.Contains(rhs[^1]))
                        {
                            var newResult = RHS(rhs[^1]);
                            foreach (var y in newResult)
                            {
                                RHSForAllNonTerms[x].Add(y);
                            }
                        }
                    }
                }
            }

            return RHSForAllNonTerms[x];
        }

        public void Execute()
        {
            var markerStack = new Symbol("Д");
            var endChain = new Symbol("$");
            var start = grammar.S0;

            // Нахождение матрицы предшествования M
            var matrixM = new Dictionary<Symbol, Dictionary<Symbol, string>>();
            matrixM[markerStack] = new Dictionary<Symbol, string>();

            foreach (var x in LHS(start))
            {
                matrixM[markerStack][x] = "<";
            }

            foreach (var x in RHS(start))
            {
                matrixM.TryAdd(x, new Dictionary<Symbol, string>());
                matrixM[x][endChain] = ">";
            }

            foreach (var p in grammar.P)
            {
                var rhs = p.RHS;
                if (rhs.Count == 1) continue;
                for (int i = 0; i < rhs.Count; ++i)
                {
                    if (i != 0)
                    {
                        matrixM.TryAdd(rhs[i - 1], new Dictionary<Symbol, string>());
                        matrixM[rhs[i - 1]][rhs[i]] = "=";

                        if (grammar.V.Contains(rhs[i - 1]))
                        {
                            foreach (var y in RHS(rhs[i - 1]))
                            {
                                matrixM.TryAdd(y, new Dictionary<Symbol, string>());
                                matrixM[y][rhs[i]] = ">";

                                if (grammar.V.Contains(rhs[i]))
                                {
                                    foreach (var x in LHS(rhs[i]))
                                    {
                                        matrixM[y][x] = ">";
                                    }
                                }
                            }
                        }

                        if (grammar.V.Contains(rhs[i]))
                        {
                            foreach (var y in LHS(rhs[i]))
                            {
                                matrixM.TryAdd(rhs[i - 1], new Dictionary<Symbol, string>());
                                matrixM[rhs[i - 1]][y] = "<";
                            }
                        }
                    }
                }
            }

            Console.WriteLine("\nПолученная матрица предшествования M: ");
            Console.Write("    ");
            Console.Write("\n");

            List<Symbol> alphabet = new List<Symbol>();

            alphabet.AddRange(grammar.V);
            alphabet.AddRange(grammar.T);
            alphabet.Add(endChain);
            alphabet.Add(markerStack);

            Console.Write("{0,4}", "M");
            foreach (var t in alphabet)
            {
                if (t != markerStack)
                {
                    Console.Write("{0, 4}", t.symbol);
                }
            }

            Console.WriteLine();
            
            foreach (var v in alphabet)
            {
                if (v == endChain)
                {
                    continue;
                }

                Console.Write("{0, 4}", v);
                foreach (var t in alphabet)
                {
                    Console.Write("{0, 4}", (matrixM.ContainsKey(v) && matrixM[v].ContainsKey(t) ? matrixM[v][t] : ""));
                }

                Console.Write("\n");
            }

            Console.Write("\nДля продолжения нажмите <Enter>");
            Console.ReadLine();

            //Проверка цепочки алгоритм перенос-свертка.   
            for (;;)
            {
                Console.Write("\nВведите цепочку: ");
                string input = Console.In.ReadLine();
                
                input += "$";
                var result = new List<int>();
                var stack = new List<Symbol>();
                stack.Add(markerStack);
                bool good = false;
                
                for (int i = 0; i < input.Length; )
                {
                    
                    var inputSymbol = new Symbol(input[i].ToString());
                    
                    if (stack[^1] == start && inputSymbol == endChain)
                    {
                        good = true;
                        break;
                    }
                    
                    if (!matrixM.ContainsKey(stack[^1]) ||
                        !matrixM[stack[^1]].ContainsKey(inputSymbol))
                    {
                        break;
                    }
                    
                    if (matrixM[stack[^1]][inputSymbol] == "<" || matrixM[stack[^1]][inputSymbol] == "=")
                    {
                        stack.Add(inputSymbol);
                        i++;
                    }
                    else
                    {
                        Production ruleForReduce = null;
                        
                        //выделение основы 
                        List <Symbol> baseRule = new List<Symbol>();
                        int firstBaseIndex = stack.Count - 1;
                        baseRule.Add(stack[firstBaseIndex]);
                        while (firstBaseIndex > 0 && matrixM[stack[firstBaseIndex-1]][stack[firstBaseIndex]] == "=")
                        {
                            baseRule.Insert(0, stack[firstBaseIndex-1]);
                            firstBaseIndex--;
                        }
                        
                        foreach (var p in grammar.P)
                        {
                            if (p.RHS.Count != baseRule.Count)
                            {
                                continue;
                            }

                            bool isTrueRule = true;
                            
                            for (int j = 0; j < p.RHS.Count; j++)
                            {
                                if (p.RHS[j] != baseRule[j])
                                {
                                    isTrueRule = false;
                                    break;
                                }
                            }

                            if (isTrueRule)
                            {
                                ruleForReduce = p;
                                break;
                            }
                        }

                        if (ruleForReduce == null)
                        {
                            break;
                        }
                        
                        result.Add(ruleForReduce.Id);
                        stack.RemoveRange(stack.Count - ruleForReduce.RHS.Count, ruleForReduce.RHS.Count());
                        stack.Add(ruleForReduce.LHS);
                    }

                    if (i == input.Length)
                    {
                        i--;
                    }
                }

                if (good)
                {
                    Console.WriteLine("Входная цепочка " + input.Substring(0, input.Length-1) + " принадлежит грамматики простого предшествования.");
                    Console.Write("Результат правого вывода: ");
                    
                    foreach (var rule in result)
                    {
                        Console.Write(rule.ToString() + " ");
                    }

                    Console.Write("\n");
                }
                else
                {
                    Console.WriteLine("Входная цепочка не распознана.");
                }


                Console.Write("Вы хотите продолжить (Y/N)? - ");
                string answer = Console.In.ReadLine();
                
                if (answer != "Y")
                {
                    break;
                }
            }
        }
    }
}