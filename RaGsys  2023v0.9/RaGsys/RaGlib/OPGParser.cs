using System;
using System.Collections.Generic;
using RaGlib.Core;
using System.Linq;
using Microsoft.Win32.SafeHandles;

namespace RaGlib
{
    // Курсовая работа студента группы М8О-207Б-21 Мусаелян Ярослав 
    public class OPGParser
    {
        private Grammar grammar;
        private Dictionary<Symbol, HashSet<Symbol>> LHSForAllNonTerms;
        private Dictionary<Symbol, HashSet<Symbol>> RHSForAllNonTerms;
        private Dictionary<Symbol, HashSet<Symbol>> LHSForTerminal;
        private Dictionary<Symbol, HashSet<Symbol>> RHSForTerminal;

        public OPGParser()
        {
            grammar = new Grammar(new List<Symbol>(), new List<Symbol>(), new List<Production>(), "S");
            LHSForAllNonTerms = new Dictionary<Symbol, HashSet<Symbol>>();
            RHSForAllNonTerms = new Dictionary<Symbol, HashSet<Symbol>>();
            LHSForTerminal = new Dictionary<Symbol, HashSet<Symbol>>();
            RHSForTerminal = new Dictionary<Symbol, HashSet<Symbol>>();
        }

        public OPGParser(Grammar grammar)
        {
            this.grammar = grammar;
            LHSForAllNonTerms = new Dictionary<Symbol, HashSet<Symbol>>();
            RHSForAllNonTerms = new Dictionary<Symbol, HashSet<Symbol>>();
            LHSForTerminal = new Dictionary<Symbol, HashSet<Symbol>>();
            RHSForTerminal = new Dictionary<Symbol, HashSet<Symbol>>();
        }
        // Пример
        public void Example()
        {

            grammar = new Grammar(new List<Symbol>() { "-", "&", "+", "(", ")", "a" },
                new List<Symbol>() { "S", "T", "E", "F" },
                "S"
            );
            grammar.AddRule("S", new List<Symbol>() { "S", "-", "T" });
            grammar.AddRule("S", new List<Symbol>() { "T" });
            grammar.AddRule("T", new List<Symbol>() { "T", "&", "E" });
            grammar.AddRule("T", new List<Symbol>() { "E" });
            grammar.AddRule("E", new List<Symbol>() { "+", "E" });
            grammar.AddRule("E", new List<Symbol>() { "F" });
            grammar.AddRule("F", new List<Symbol>() { "(", "E", ")" });
            grammar.AddRule("F", new List<Symbol>() { "a" });
            Execute_Example("a-a");
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

        public HashSet<Symbol> LHS_t(Symbol x) // Нахождения множества, задаваемого функцией LHS_t для нетерминала X
        {
            if (!grammar.V.Contains(x)) return null;
            if (LHSForTerminal.ContainsKey(x))
            {
                return LHSForTerminal[x];
            }

            LHSForTerminal[x] = new HashSet<Symbol>();
            foreach (var production in grammar.P)
            {
                var rhs = production.RHS;
                if (production.LHS == x.symbol)
                {
                    if (grammar.T.Contains(rhs[0]) || (rhs.Count != 1 && grammar.T.Contains(rhs[1]) && grammar.V.Contains(rhs[0])))
                    {
                        var lhs = production.LHS;
                        if (grammar.T.Contains(rhs[0]))
                        {
                            LHSForTerminal[lhs].Add(rhs[0]);
                        }
                        else
                        {
                            LHSForTerminal[lhs].Add(rhs[1]);
                        }
                    }
                }
            }
            foreach (var y in LHS(x))
            {
                if (grammar.V.Contains(y) && y != x)
                {
                    foreach (var z in LHS_t(y))
                    {
                        LHSForTerminal[x].Add(z);
                    }
                }
            }
            return LHSForTerminal[x];

        }

        public HashSet<Symbol> RHS_t(Symbol x) // Нахождения множества, задаваемого функцией RHS_t для нетерминала X
        {

            if (!grammar.V.Contains(x)) return null;
            if (RHSForTerminal.ContainsKey(x))
            {
                return RHSForTerminal[x];
            }

            RHSForTerminal[x] = new HashSet<Symbol>();
            foreach (var production in grammar.P)
            {
                var rhs = production.RHS;
                if (production.LHS == x.symbol)
                {
                    if (grammar.T.Contains(rhs[^1]) || (rhs.Count != 1 && grammar.T.Contains(rhs[^2]) && grammar.V.Contains(rhs[^1])))
                    {
                        var lhs = production.LHS;
                        if (grammar.T.Contains(rhs[^1]))
                        {
                            RHSForTerminal[lhs].Add(rhs[^1]);
                        }
                        else
                        {
                            RHSForTerminal[lhs].Add(rhs[^2]);
                        }
                    }

                }
            }
            foreach (var y in RHS(x))
            {
                if (grammar.V.Contains(y) && y != x)
                {
                    foreach (var z in RHS_t(y))
                    {
                        RHSForTerminal[x].Add(z);
                    }
                }
            }
            return RHSForTerminal[x];
        }

        public void Execute()
        {


            var markerStack = new Symbol("Д");
            var endChain = new Symbol("$");
            var start = grammar.S0;

            // Нахождение матрицы предшествования M
            var matrixM = new Dictionary<Symbol, Dictionary<Symbol, string>>();
            matrixM[markerStack] = new Dictionary<Symbol, string>();

            foreach (var x in LHS_t(start))
            {
                matrixM[markerStack][x] = "<";
            }

            foreach (var x in RHS_t(start))
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
                        if (grammar.T.Contains(rhs[i - 1]) && grammar.T.Contains(rhs[i]))
                        {
                            matrixM[rhs[i - 1]][rhs[i]] = "=";
                        }
                        if (i + 1 != rhs.Count)
                        {
                            if (grammar.T.Contains(rhs[i - 1]) && grammar.T.Contains(rhs[i + 1]) && grammar.V.Contains(rhs[i]))
                            {
                                matrixM[rhs[i - 1]][rhs[i + 1]] = "=";
                            }
                        }
                        if (grammar.V.Contains(rhs[i - 1]) && grammar.T.Contains(rhs[i]))
                        {
                            foreach (var y in RHS_t(rhs[i - 1]))
                            {
                                matrixM.TryAdd(y, new Dictionary<Symbol, string>());
                                matrixM[y][rhs[i]] = ">";
                            }
                        }
                        if (grammar.V.Contains(rhs[i]) && grammar.T.Contains(rhs[i - 1]))
                        {
                            foreach (var y in LHS_t(rhs[i]))
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

            for (int p = 0; p < grammar.P.Count; p++) // замена разных нетерминалов в правилах одним нетерминалом
            {
                grammar.P[p].LHS = start;
                for (int i = 0; i < grammar.P[p].RHS.Count; i++)
                {
                    if (grammar.V.Contains(grammar.P[p].RHS[i]))
                    {
                        grammar.P[p].RHS[i] = start;
                    }
                }
            }

            for (; ; )
            {
                Console.Write("\nВведите цепочку: ");
                string input = Console.In.ReadLine();

                input += "$";
                var result = new List<int>();
                var stack = new List<Symbol>();
                stack.Add(markerStack);
                bool good = false;

                for (int i = 0; i < input.Length;)
                {
                    var inputSymbol = new Symbol(input[i].ToString());

                    if (stack[^1] == start && inputSymbol == endChain && stack.Count == 2)
                    {
                        good = true;
                        break;
                    }
                    int UpTerminal = stack.Count - 1;
                    while (UpTerminal > 0 && grammar.V.Contains(stack[UpTerminal])) //нахождение верхнего терминала
                    {
                        UpTerminal--;
                    }
                    if (!matrixM.ContainsKey(stack[UpTerminal]) ||
                        !matrixM[stack[UpTerminal]].ContainsKey(inputSymbol))
                    {
                        break;
                    }

                    if (matrixM[stack[UpTerminal]][inputSymbol] == "<" || matrixM[stack[UpTerminal]][inputSymbol] == "=")
                    {
                        stack.Add(inputSymbol);
                        i++;
                    }
                    else
                    {
                        Production ruleForReduce = null;

                        //выделение основы 
                        List<Symbol> baseRule = new List<Symbol>();
                        int firstBaseIndex = UpTerminal;
                        int secondBaseIndex = UpTerminal - 1;
                        while (secondBaseIndex != 0 && grammar.V.Contains(stack[secondBaseIndex]))
                        {
                            secondBaseIndex--;
                        }
                        if (secondBaseIndex == 0 || matrixM[stack[secondBaseIndex]][stack[firstBaseIndex]] != "=")
                        {
                            baseRule.Add(stack[firstBaseIndex]);
                        }
                        else if (matrixM[stack[secondBaseIndex]][stack[firstBaseIndex]] == "=")
                        {
                            baseRule.Add(stack[firstBaseIndex]);
                            while (secondBaseIndex > 0 && matrixM[stack[secondBaseIndex]][stack[firstBaseIndex]] == "=")
                            {
                                firstBaseIndex = secondBaseIndex;
                                secondBaseIndex--;
                                while (secondBaseIndex > 0 && grammar.V.Contains(stack[secondBaseIndex]))
                                {
                                    secondBaseIndex--;
                                }
                            }
                        }
                        int cnt = UpTerminal + 1;
                        while (cnt != stack.Count)
                        {
                            baseRule.Add(stack[cnt]);
                            cnt++;
                        }
                        cnt = UpTerminal - 1;
                        while (cnt != firstBaseIndex - 1)
                        {
                            baseRule.Insert(0, stack[cnt]);
                            cnt--;
                        }
                        if (grammar.V.Contains(stack[cnt]))
                        {
                            baseRule.Insert(0, stack[cnt]);
                        }
                        UpTerminal = secondBaseIndex;

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
                    Console.WriteLine("Входная цепочка " + input.Substring(0, input.Length - 1) + " принадлежит грамматики операторного предшествования.");
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
        public void Execute_Example(string example)
        {


            var markerStack = new Symbol("Д");
            var endChain = new Symbol("$");
            var start = grammar.S0;

            // Нахождение матрицы предшествования M
            var matrixM = new Dictionary<Symbol, Dictionary<Symbol, string>>();
            matrixM[markerStack] = new Dictionary<Symbol, string>();

            foreach (var x in LHS_t(start))
            {
                matrixM[markerStack][x] = "<";
            }

            foreach (var x in RHS_t(start))
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
                        if (grammar.T.Contains(rhs[i - 1]) && grammar.T.Contains(rhs[i]))
                        {
                            matrixM[rhs[i - 1]][rhs[i]] = "=";
                        }
                        if (i + 1 != rhs.Count)
                        {
                            if (grammar.T.Contains(rhs[i - 1]) && grammar.T.Contains(rhs[i + 1]) && grammar.V.Contains(rhs[i]))
                            {
                                matrixM[rhs[i - 1]][rhs[i + 1]] = "=";
                            }
                        }
                        if (grammar.V.Contains(rhs[i - 1]) && grammar.T.Contains(rhs[i]))
                        {
                            foreach (var y in RHS_t(rhs[i - 1]))
                            {
                                matrixM.TryAdd(y, new Dictionary<Symbol, string>());
                                matrixM[y][rhs[i]] = ">";
                            }
                        }
                        if (grammar.V.Contains(rhs[i]) && grammar.T.Contains(rhs[i - 1]))
                        {
                            foreach (var y in LHS_t(rhs[i]))
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

            for (int p = 0; p < grammar.P.Count; p++) // замена разных нетерминалов в правилах одним нетерминалом
            {
                grammar.P[p].LHS = start;
                for (int i = 0; i < grammar.P[p].RHS.Count; i++)
                {
                    if (grammar.V.Contains(grammar.P[p].RHS[i]))
                    {
                        grammar.P[p].RHS[i] = start;
                    }
                }
            }

            for (; ; )
            {
                Console.Write("\nВведенная цепочка: ");
                Console.WriteLine(example);
                string input = example.ToString();

                input += "$";
                var result = new List<int>();
                var stack = new List<Symbol>();
                stack.Add(markerStack);
                bool good = false;

                for (int i = 0; i < input.Length;)
                {
                    var inputSymbol = new Symbol(input[i].ToString());

                    if (stack[^1] == start && inputSymbol == endChain && stack.Count == 2)
                    {
                        good = true;
                        break;
                    }
                    int UpTerminal = stack.Count - 1;
                    while (UpTerminal > 0 && grammar.V.Contains(stack[UpTerminal])) //нахождение верхнего терминала
                    {
                        UpTerminal--;
                    }
                    if (!matrixM.ContainsKey(stack[UpTerminal]) ||
                        !matrixM[stack[UpTerminal]].ContainsKey(inputSymbol))
                    {
                        break;
                    }

                    if (matrixM[stack[UpTerminal]][inputSymbol] == "<" || matrixM[stack[UpTerminal]][inputSymbol] == "=")
                    {
                        stack.Add(inputSymbol);
                        i++;
                    }
                    else
                    {
                        Production ruleForReduce = null;

                        //выделение основы 
                        List<Symbol> baseRule = new List<Symbol>();
                        int firstBaseIndex = UpTerminal;
                        int secondBaseIndex = UpTerminal - 1;
                        while (secondBaseIndex != 0 && grammar.V.Contains(stack[secondBaseIndex]))
                        {
                            secondBaseIndex--;
                        }
                        if (secondBaseIndex == 0 || matrixM[stack[secondBaseIndex]][stack[firstBaseIndex]] != "=")
                        {
                            baseRule.Add(stack[firstBaseIndex]);
                        }
                        else if (matrixM[stack[secondBaseIndex]][stack[firstBaseIndex]] == "=")
                        {
                            baseRule.Add(stack[firstBaseIndex]);
                            while (secondBaseIndex > 0 && matrixM[stack[secondBaseIndex]][stack[firstBaseIndex]] == "=")
                            {
                                firstBaseIndex = secondBaseIndex;
                                secondBaseIndex--;
                                while (secondBaseIndex > 0 && grammar.V.Contains(stack[secondBaseIndex]))
                                {
                                    secondBaseIndex--;
                                }
                            }
                        }
                        int cnt = UpTerminal + 1;
                        while (cnt != stack.Count)
                        {
                            baseRule.Add(stack[cnt]);
                            cnt++;
                        }
                        cnt = UpTerminal - 1;
                        while (cnt != firstBaseIndex - 1)
                        {
                            baseRule.Insert(0, stack[cnt]);
                            cnt--;
                        }
                        if (grammar.V.Contains(stack[cnt]))
                        {
                            baseRule.Insert(0, stack[cnt]);
                        }
                        UpTerminal = secondBaseIndex;

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
                    Console.WriteLine("Входная цепочка " + input.Substring(0, input.Length - 1) + " принадлежит грамматики операторного предшествования.");
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


                //Console.Write("Вы хотите продолжить (Y/N)? - ");
                string answer = "N";

                if (answer != "Y")
                {
                    break;
                }
            }
        }
    }
}
