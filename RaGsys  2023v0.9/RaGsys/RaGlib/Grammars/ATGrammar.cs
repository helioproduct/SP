using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections;
using RaGlib.Core;
using RaGlib;

namespace RaGlib.Grammars
{
    public class ATGrammar : Grammar
    {
        public List<Symbol_Operation> OP { set; get; } = null;

        private Grammar G;
        private Stack<Tuple<int, int>> Stack;
        public string OutputConfigure = "";

        Dictionary<string, string> NumbersOP = new Dictionary<string, string>();
        Dictionary<string, string> NumbersT = new Dictionary<string, string>();
        Dictionary<string, string> Numbers = new Dictionary<string, string>();
        Dictionary<string, Dictionary<Symbol, string>> H = new Dictionary<string, Dictionary<Symbol, string>>();
        Dictionary<int, int> SizeRule = new Dictionary<int, int>();

        public List<AttrProduction> Rules { set; get; } = new List<AttrProduction>();
        public ATGrammar() { }

        public ATGrammar(List<Symbol> V, List<Symbol> T, List<Symbol_Operation> OP, Symbol S0)
        {
            this.OP = OP;
            this.V = V;
            this.T = T;
            this.S0 = S0;
        }

        public void LLParser(Grammar grammar)
        {
            this.G = grammar;
            Stack = new Stack<Tuple<int, int>>();

            Console.Write("\n");
            Console.Write("\n");

            Console.WriteLine("Управляющая таблица: ");
            Console.Write("{0, 15}", "");
            foreach (var termSymbol in G.T)
            {
                Console.Write("{0, 15}", termSymbol.symbol);
            }
            Console.Write("\n");

            int count = 0;

            string numV = "";
            ComputeFirstFollowAT();

            for (int i = 0; i < grammar.V.Count; ++i)
            {
                H.Add((string)grammar.V[i].symbol, new Dictionary<Symbol, string>());

                var rules = getRules(grammar.V[i]);

                foreach (var rule in rules)
                {
                    count++;
                    string s = count.ToString();
                    s += ".1";

                    var currFirstSet = FirstAT(rule.RHS);
                    foreach (var firstSymbol in currFirstSet)
                    {
                        if (firstSymbol != "")
                        {
                            H[rule.LHS.symbol].Add(firstSymbol, s);
                        }
                        else
                        {
                            HashSet<Symbol> currFollowSet = FollowAT(rule.LHS);
                            foreach (var currFollowSymb in currFollowSet)
                            {
                                string currFollowSymbFix = (currFollowSymb == "") ? "eps" : currFollowSymb.symbol;
                                H[rule.LHS.symbol].Add(currFollowSymbFix, s);
                            }
                        }
                    }
                }
            }
            numV = "";

            for (int i = 0; i < grammar.V.Count; ++i)
            {
                count = 0;
                for (int j = 0; j < Rules.Count; ++j)
                {
                    ++count;
                    Production currRule = (Production)Rules[j];

                    if (i == 0)
                    {
                        int size = currRule.RHS.Count;
                        SizeRule.Add(count, size);
                    }
                    for (int index = 0; index < currRule.RHS.Count; ++index)
                    {
                        if (currRule.RHS[index].symbol == grammar.V[i].symbol)
                        {
                            numV += count.ToString() + ".";
                            numV += (index + 1).ToString() + "|";
                        }
                    }
                }
                int x = numV.Length - 1;
                if (numV.Length != 0)
                {
                    numV = numV.Remove(x);
                }
                else
                {
                    numV = "";
                }

                Numbers.Add(grammar.V[i].symbol, numV);
                numV = "";
            }


            string numT = "";
            for (int i = 0; i < T.Count; ++i)
            {
                count = 0;
                for (int j = 0; j < Rules.Count; ++j)
                {
                    count++;
                    Production currRule = (Production)Rules[j];
                    for (int index = 0; index < currRule.RHS.Count; ++index)
                    {
                        if (currRule.RHS[index].symbol == grammar.T[i].symbol)
                        {
                            numT += count.ToString() + ".";
                            numT += (index + 1).ToString() + "|";
                        }
                    }
                }
                int x = numT.Length - 1;
                if (numT.Length != 0)
                {
                    numT = numT.Remove(x);
                }
                else
                {
                    numT = "";
                }

                NumbersT.Add(T[i].symbol, numT);
                numT = "";
            }

            string numOP = "";
            for (int i = 0; i < OP.Count; ++i)
            {
                count = 0;
                for (int j = 0; j < Rules.Count; ++j)
                {
                    ++count;
                    Production currRule = (Production)Rules[j];
                    for (int index = 0; index < currRule.RHS.Count; ++index)
                    {
                        if (currRule.RHS[index].symbol == OP[i].symbol)
                        {
                            numOP += count.ToString() + ".";
                            numOP += (index + 1).ToString() + "|";
                        }
                    }
                }
                int x = numOP.Length - 1;
                if (numOP.Length != 0)
                {
                    numOP = numOP.Remove(x);
                }
                else
                {
                    numOP = "";
                }

                NumbersOP.Add(OP[i].symbol, numOP);
                numOP = "";
            }
            foreach (var t in T)
            {
                H.Add((string)t.symbol, new Dictionary<Symbol, string>());
                H[t.symbol].Add(t.symbol, "ВЫБРОС");
            }
            H.Add((string)"|", new Dictionary<Symbol, string>());
            H["|"].Add("eps", "ДОПУСК");


            for (int i = 1; i < V.Count; i++)
            {
                Console.Write("{0, 15}", Numbers[V[i].symbol]);

                foreach (var t in grammar.T)
                {
                    Console.Write("{0, 15}", (H[V[i].symbol].ContainsKey(t) ? H[grammar.V[i].symbol][t] : ""));
                }
                Console.Write("\n");
            }
            foreach (var t in T)
            {
                if (t.symbol == "eps")
                {
                    break;
                }
                Console.Write("{0, 15}", NumbersT[t.symbol]);

                foreach (var term in grammar.T)
                {
                    Console.Write("{0, 15}", (H[t.symbol].ContainsKey(term) ? H[t.symbol][term.symbol] : ""));
                }
                Console.Write("\n");
            }

            Console.Write("{0, 15}", "|");
            foreach (var term in grammar.T)
            {
                Console.Write("{0, 15}", (H["|"].ContainsKey(term) ? H["|"][term.symbol] : ""));
            }
            Console.Write("\n");

            var opf = new List<Symbol_Operation>();
            foreach (var op in OP)
            {
                H.Add(op.symbol, new Dictionary<Symbol, string>());
                foreach (var t in grammar.T)
                {
                    H[op.symbol].Add(t.symbol, "ВЫДАЧА");
                }
                Console.Write("{0, 15}", NumbersOP[op.symbol]);
                Console.Write("                              ВЫДАЧА(");
                op.print();
                Console.Write(")");
                Console.Write("\n");
            }
        }

        public bool DMPAutomate(List<Symbol> input)
        {
            var tuple0 = Tuple.Create(1, 1);
            Stack.Push(tuple0);

            int i = 0;
            Symbol currInputSymbol = input[0];
            Tuple<int, int> currStackSymbol;
            do
            {
                Console.Write("      (");
                for (int k = 0; k < Stack.Count; ++k)
                {
                    Tuple<int, int>[] tmp = new Tuple<int, int>[Stack.Count];
                    Stack.CopyTo(tmp, 0);
                    Console.Write(tmp[k].Item1 + "." + tmp[k].Item2);
                    if (k < Stack.Count - 1)
                    {
                        Console.Write(";");
                    }
                }
                Console.Write(", ");

                for (int j = i; j < input.Count(); ++j)
                {
                    if (input[j].attr != null)
                    {
                        Console.Write(input[j].symbol + "." + input[j].attr[0].symbol);
                        j++;
                    }
                    else
                    {
                        Console.Write(input[j].symbol);
                    }
                }
                Console.Write(", ");
                Console.Write(OutputConfigure);
                Console.WriteLine(")\n");

                bool flag = false;
                currStackSymbol = Stack.Pop();
                string symbolStack = currStackSymbol.Item1.ToString() + "." + currStackSymbol.Item2.ToString();

                int first = currStackSymbol.Item1;
                int second = currStackSymbol.Item2;
                string temp = "";

                second++;
                var curtuple = Tuple.Create(first, second);

                if (second > SizeRule[first])
                {
                    if (Rules[first - 1].RHS[second - 2].attr != null)
                    {
                        temp = Rules[first - 1].RHS[second - 2].attr[0].symbol;
                    }
                    flag = true;
                }

                foreach (var t in T)
                {
                    if (NumbersT[t.symbol].IndexOf(symbolStack) >= 0)
                    {
                        if (H[t.symbol][currInputSymbol.symbol] == "ВЫБРОС")
                        {
                            if (i != input.Count() && input[i].attr == null)
                            {
                                if (!flag)
                                    Stack.Push(curtuple);
                                if (i != input.Count())
                                {
                                    ++i;
                                }
                            }
                            else
                            {
                                if (!flag)
                                    Stack.Push(curtuple);
                                if (i == input.Count())
                                {
                                    continue;
                                }
                                string leftAttr = "";
                                int COUNTER = 0;

                                for (int j = 0; j < Rules[first - 1].F.Count; ++j)
                                {
                                    if (Rules[first - 1].F[j].RH[0].symbol == Rules[first - 1].RHS[second - 2].attr[0].symbol)
                                    {
                                        leftAttr = Rules[first - 1].F[j].LH[0].symbol;
                                    }

                                    if (Rules[first - 1].F[j].RH[0].symbol == "COUNTER")
                                    {
                                        string searchAttr = Rules[first - 1].F[j].LH[0].symbol;

                                        for (int index = 0; index < Rules[first - 1].RHS.Count; ++index)
                                        {
                                            if (Rules[first - 1].RHS[index].attr != null)
                                            {
                                                for (int k = 0; k < Rules[first - 1].RHS[index].attr.Count; ++k)
                                                {
                                                    if (Rules[first - 1].RHS[index].attr[k].symbol == searchAttr)
                                                    {
                                                        Rules[first - 1].RHS[index].attr[k].symbol = COUNTER.ToString();
                                                        COUNTER++;
                                                    }
                                                }

                                            }

                                        }

                                    }
                                }
                                for (int j = 0; j < Rules[first - 1].RHS.Count; ++j)
                                {
                                    if (Rules[first - 1].RHS[j].attr != null)
                                    {
                                        for (int k = 0; k < Rules[first - 1].RHS[j].attr.Count; ++k)
                                        {
                                            if (Rules[first - 1].RHS[j].attr[k].symbol == leftAttr)
                                            {
                                                Rules[first - 1].RHS[j].attr[k].symbol = input[i].attr[0].symbol;
                                            }
                                        }

                                    }

                                }

                                if (i <= input.Count() - 2)
                                {
                                    i = i + 2;
                                }
                            }
                            currInputSymbol = (i == input.Count()) ? "eps" : input[i];
                        }
                    }
                }
                foreach (var v in V)
                {
                    if (Numbers[v.symbol].IndexOf(symbolStack) >= 0)
                    {
                        if (H[v.symbol].ContainsKey(currInputSymbol.symbol))
                        {
                            if (!flag)
                                Stack.Push(curtuple);

                            string s = H[v.symbol][currInputSymbol.symbol];
                            int newFirst = s[0] - '0';
                            int newSecond = s[2] - '0';

                            var newTuple = Tuple.Create(newFirst, newSecond);
                            Stack.Push(newTuple);

                            if (Rules[newFirst - 1].LHS.attr != null)
                            {
                                for (int j = 0; j < Rules[newFirst - 1].F.Count; ++j)
                                {
                                    if (Rules[newFirst - 1].F[j].RH[0].symbol == Rules[newFirst - 1].LHS.attr[0].symbol)
                                    {
                                        for (int index = 0; index < Rules[newFirst - 1].RHS.Count; ++index)
                                        {
                                            if (Rules[newFirst - 1].RHS[index].attr != null)
                                            {
                                                for (int k = 0; k < Rules[newFirst - 1].RHS[index].attr.Count; ++k)
                                                {
                                                    if (Rules[newFirst - 1].RHS[index].attr[k].symbol == Rules[newFirst - 1].F[j].LH[0].symbol)
                                                    {
                                                        Rules[newFirst - 1].RHS[index].attr[k].symbol = temp;
                                                    }
                                                }

                                            }

                                        }
                                    }
                                }
                            }

                        }
                    }
                }

                foreach (var op in OP)
                {
                    if (NumbersOP[op.symbol].IndexOf(symbolStack) >= 0)
                    {
                        if (H[op.symbol][currInputSymbol.symbol] == "ВЫДАЧА")
                        {
                            OutputConfigure += op.symbol;
                            for (int j = 0; j < op.attr.Count; ++j)
                            {
                                OutputConfigure += Rules[first - 1].RHS[second - 2].attr[j].symbol;
                            }

                            if (!flag)
                                Stack.Push(curtuple);
                        }
                    }
                }

            } while (Stack.Count() > 0);

            Console.Write("      (");
            for (int k = 0; k < Stack.Count; ++k)
            {
                Tuple<int, int>[] tmp = new Tuple<int, int>[Stack.Count];
                Stack.CopyTo(tmp, 0);
                Console.Write(tmp[k].Item1 + "." + tmp[k].Item2);
                if (k < Stack.Count - 1)
                {
                    Console.Write(";");
                }
            }
            Console.Write(", ");

            for (int j = i; j < input.Count(); ++j)
            {
                Console.Write(input[j]);
            }
            Console.Write(", ");
            Console.Write(OutputConfigure);
            Console.WriteLine(")\n");

            if (i != input.Count())
            {
                return false;
            }

            return true;
        }
        public void Addrule(Symbol LeftNoTerm, List<Symbol> Right)
        {
            this.Rules.Add(new AttrProduction(LeftNoTerm, Right));
        }

        /// Добавление правила
        public void Addrule(Symbol LeftNoTerm,
                List<Symbol> Right,
                List<AttrFunction> F)
        {
            this.Rules.Add(new AttrProduction(LeftNoTerm, Right, F));
        }

        private Dictionary<Symbol, HashSet<Symbol>> FirstSet =
    new Dictionary<Symbol, HashSet<Symbol>>();

        private Dictionary<Symbol, HashSet<Symbol>> FollowSet =
          new Dictionary<Symbol, HashSet<Symbol>>();
        public void ComputeFirstFollowAT()
        {
            ComputeFirstSetsAT();
            ComputeFollowSetsAT();
        }

        private void ComputeFirstSetsAT()
        {
            FirstSet.Clear();
            foreach (var term in T)
                FirstSet[term] = new HashSet<Symbol>() { term }; // FIRST[c] = {c}
            FirstSet[Symbol.Epsilon] = new HashSet<Symbol>() { Symbol.Epsilon }; // для единообразия
            foreach (Symbol noTerm in V)
                FirstSet[noTerm] = new HashSet<Symbol>(); // First[X] = empty set
            foreach (var op in OP)
                FirstSet[op] = new HashSet<Symbol> { op };
            bool changes = true;
            while (changes)
            {
                changes = false;
                foreach (var rule in Rules)
                {
                    // Для каждого правила X-> Y0Y1…Yn
                    var X = rule.LHS;
                    foreach (var Y in rule.RHS)
                    {
                        foreach (var curFirstSymb in FirstSet[Y])
                        {
                            if (FirstSet[X].Add(curFirstSymb)) // Добавить а в FirstSets[X]
                            {
                                changes = true;
                            }
                        }
                        if (!FirstSet[Y].Contains(Symbol.Epsilon))
                        {
                            break;
                        }
                    }
                }
            } // пока вносятся изменения
        }

        public HashSet<Symbol> FirstAT(Symbol X) { return FirstSet[X]; }

        public HashSet<Symbol> FirstAT(List<Symbol> X)
        {
            var result = new HashSet<Symbol>();
            foreach (Symbol Y in X)
            {
                foreach (Symbol curFirstSymb in FirstSet[Y])
                {
                    result.Add(curFirstSymb);
                }
                if (!FirstSet[Y].Contains(Symbol.Epsilon))
                {
                    break;
                }
            }
            return result;
        }

        private void ComputeFollowSetsAT()
        {
            foreach (Symbol noTerm in V)
                FollowSet[noTerm] = new HashSet<Symbol>();


            FollowSet[S0].Add(Symbol.Sentinel);
            bool changes = true;
            while (changes)
            {
                changes = false;
                foreach (Production rule in Rules)
                {
                    // Для каждого правила X-> Y0Y1…Yn
                    for (int indexOfSymbol = 0; indexOfSymbol < rule.RHS.Count; ++indexOfSymbol)
                    {
                        Symbol curSymbol = rule.RHS[indexOfSymbol];
                        if (T.Contains(curSymbol) || OP.Contains(curSymbol) || curSymbol == Symbol.Epsilon)
                        {
                            continue;
                        }
                        if (indexOfSymbol == rule.RHS.Count - 1)
                        {
                            foreach (Symbol curFollowSymbol in FollowSet[rule.LHS])
                            {
                                if (FollowSet[curSymbol].Add(curFollowSymbol))
                                {
                                    changes = true;
                                }
                            }
                        }
                        else
                        {
                            var curFirst = FirstAT(rule.RHS[indexOfSymbol + 1]);
                            bool epsFound = false;
                            foreach (Symbol curFirstSymbol in curFirst)
                            {
                                if (curFirstSymbol != Symbol.Epsilon)
                                {
                                    if (FollowSet[rule.RHS[indexOfSymbol]].Add(curFirstSymbol))
                                    {
                                        changes = true;
                                    }
                                }
                                else
                                {
                                    epsFound = true;
                                }
                            }
                            if (epsFound)
                            {
                                foreach (Symbol curFollowSymbol in FollowSet[rule.LHS])
                                {
                                    if (FollowSet[rule.RHS[indexOfSymbol]].Add(curFollowSymbol))
                                    {
                                        changes = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public HashSet<Symbol> FollowAT(Symbol X)
        {
            if (FollowSet.ContainsKey(X)) return FollowSet[X];
            return new HashSet<Symbol>();
        }

        // Генерация атрибутов для входной грамматики Воронов
        public void NewAT(List<Symbol> A, List<Symbol> Top, List<Symbol> L)
        {
            int i = 0;
            foreach (var p in Rules)
            {
                var F = new List<AttrFunction>();
                var AF = new List<Symbol>();

                Symbol B = p.LHS;
                B.attr = new List<Symbol>() { A[i] };
                ++i;

                foreach (var x in p.RHS)
                {
                    if (T.Contains(x))
                    {
                        if (L.Contains(x))
                        {
                            x.attr = new List<Symbol>() { A[i] };
                            ++i;
                            AF.Add(x.attr[0]);
                        }
                        else
                        {
                            if (Top.Contains(x))
                            {
                                AF.Add(x);
                            }
                        }
                    }
                    else
                    {
                        x.attr = new List<Symbol>() { A[i] };
                        ++i;
                        AF.Add(x.attr[0]);
                    }

                }
                F.Add(new AttrFunction(B.attr, AF));
                p.F = F;
                i = 0;
            }

        } // end NewAT 

        //### Тимофеев перевод цикла с С в Python 15.2        
        // AT-grammar for concret example 
        public void ATG_C_Py(List<Symbol> A, List<Symbol> Sign)
        {// символы атрибутов {a,n,s ...}, знаки операций {+, =, <}
            int i = 0;
            List<Symbol> TmpList = new List<Symbol>();// для нетерминалов из (1) 
            Symbol IdVal = null;
            foreach (var p in Rules)
            {
                var F = new List<AttrFunction>(); // вводимые атрибуты 
                var AF = new List<Symbol>(); // символ которому принадлежат вводимые атрибуты 

                var B = p.LHS;
                Symbol x1 = null, x2 = null;
                B.attr = new List<Symbol>();
                bool f = false;
                foreach (var x in p.RHS)
                {
                    if (T.Contains(x))
                    { // если символ терминальный
                        if (Sign.Contains(x))
                        {// если он является знаком 
                            B.attr = new List<Symbol>() { A[i] };
                            ++i;
                            int ind = p.RHS.IndexOf(x); // то определяем его индекс в списке RHS
                            x2 = p.RHS.ElementAt(ind + 1);// берем символ справа от знака 
                            x1 = p.RHS.ElementAt(ind - 1);// и слева 
                            if (x.symbol == "=")
                            {// если равно то синтезируем атрибуты 
                                f = false;
                                IdVal = p.LHS;
                                x1.attr = new List<Symbol>() { A[i] };
                                ++i;
                                x2.attr = new List<Symbol>() { A[i] };
                                ++i;
                                AF.Add(x1.attr[0]);
                            }
                            else
                            { // иначе синтезируем атрибут у константы а у переменной пораждаем из LHS (1)
                                TmpList.Add(p.LHS);
                                B.attr = new List<Symbol>() { A[i] };
                                ++i;
                                f = true;
                                x1.attr = new List<Symbol>() { A[i] };
                                ++i;
                                AF.Add(x1.attr[0]);
                                x2.attr = new List<Symbol>() { A[i] };
                                ++i;
                            }

                        }
                    }
                    else if (V.Contains(x) && x.symbol != "B")
                    {// если это нетерминал
                        f = false;
                        B.attr = new List<Symbol>();
                        ++i;
                        x.attr = new List<Symbol>();
                        ++i;
                        AF = new List<Symbol>();
                    }
                }

                if (f)
                {// если пораждаем от LHS
                    if (B.attr.Count > 0 && x1.attr.Count > 0)
                    {
                        f = false;
                        F.Add(new AttrFunction(x1.attr, B.attr));
                    }
                }
                else
                {// если пораждаем от RHS
                    if (B.attr.Count > 0 && AF.Count > 0)
                    {
                        F.Add(new AttrFunction(B.attr, AF));
                    }
                }

                p.F = F;
                i = 0;
            }// присвоены атрибуты нетерминалам 

            foreach (var p in Rules)
            { // идем по правилам чтобы найти эл-т из списка совпадающий с эл-том правила

                foreach (var x in p.RHS)
                { // идем по эл-там правила RHS
                    if (x.symbol == IdVal.symbol)
                    {
                        ++i;
                        x.attr = new List<Symbol>() { A[4] };
                        IdVal = x;
                    }
                }
            }
            i = 0;
            foreach (var p in Rules)
            { // идем по правилам чтобы найти эл-т из списка TmpList совпадающий с эл-том правила 

                foreach (var x in p.RHS)
                {   // идем по эл-там правила RHS
                    AttrFunction F1 = null;
                    Symbol tmpX = null;
                    foreach (var item in TmpList)
                    {   // идем по списку нетерминалов которые нужно соединить с D и сравниваем с эл-тами из RHS
                        if (x.symbol == item.symbol)
                        {
                            ++i;
                            x.attr = new List<Symbol>() { A[i] };
                            tmpX = x;
                        }
                    }
                    if (tmpX != null)
                    {
                        F1 = new AttrFunction(tmpX.attr, IdVal.attr);
                        tmpX = null;
                    }
                    if (F1 != null)
                    {
                        p.F.Add(F1);
                    }
                }
            }
        } // end ATG_C_Py 
        public void ATG_C_Py1(List<Symbol> A, List<Symbol> Sign)
        {// символы атрибутов {a,n,s ...}, знаки операций {+, =, <}
            int i = 0;
            List<Symbol> TmpList = new List<Symbol>();// для нетерминалов из (1) 
            Symbol IdVal = null;
            foreach (var p in Rules)
            {
                var F = new List<AttrFunction>(); // вводимые атрибуты 
                var AF = new List<Symbol>(); // символ которому принадлежат вводимые атрибуты 

                var B = p.LHS;
                Symbol x1 = null, x2 = null;
                B.attr = new List<Symbol>();
                bool f = false;
                foreach (var x in p.RHS)
                {
                    if (T.Contains(x))
                    { // если символ терминальный
                        if (Sign.Contains(x))
                        {// если он является знаком 
                            B.attr = new List<Symbol>() { A[i] };
                            ++i;
                            int ind = p.RHS.IndexOf(x); // то определяем его индекс в списке RHS
                            x2 = p.RHS.ElementAt(ind + 1);// берем символ справа от знака 
                            x1 = p.RHS.ElementAt(ind - 1);// и слева 
                            if (x.symbol == "=")
                            {// если равно то синтезируем атрибуты 
                                f = false;
                                IdVal = p.LHS;
                                x1.attr = new List<Symbol>() { A[i] };
                                ++i;
                                x2.attr = new List<Symbol>() { A[i] };
                                ++i;
                                AF.Add(x1.attr[0]);
                            }
                            else
                            { // иначе синтезируем атрибут у константы а у переменной пораждаем из LHS (1)
                                TmpList.Add(p.LHS);
                                B.attr = new List<Symbol>() { A[i] };
                                ++i;
                                f = true;
                                x1.attr = new List<Symbol>() { A[i] };
                                ++i;
                                AF.Add(x1.attr[0]);
                                x2.attr = new List<Symbol>() { A[i] };
                                ++i;
                            }

                        }
                    }
                    else if (V.Contains(x) && x.symbol != "B")
                    {// если это нетерминал
                        f = false;
                        B.attr = new List<Symbol>();
                        ++i;
                        x.attr = new List<Symbol>();
                        ++i;
                        AF = new List<Symbol>();
                    }
                }

                if (f)
                {// если пораждаем от LHS
                    if (B.attr.Count > 0 && x1.attr.Count > 0)
                    {
                        f = false;
                        F.Add(new AttrFunction(x1.attr, B.attr));
                    }
                }
                else
                {// если пораждаем от RHS
                    if (B.attr.Count > 0 && AF.Count > 0)
                    {
                        F.Add(new AttrFunction(B.attr, AF));
                    }
                }

                p.F = F;
                i = 0;
            }// присвоены атрибуты нетерминалам 

            foreach (var p in Rules)
            { // идем по правилам чтобы найти эл-т из списка совпадающий с эл-том правила

                foreach (var x in p.RHS)
                { // идем по эл-там правила RHS
                    if (x.symbol == IdVal.symbol)
                    {
                        ++i;
                        x.attr = new List<Symbol>() { A[4] };
                        IdVal = x;
                    }
                }
            }
            i = 0;
            foreach (var p in Rules)
            { // идем по правилам чтобы найти эл-т из списка TmpList совпадающий с эл-том правила 

                foreach (var x in p.RHS)
                {   // идем по эл-там правила RHS
                    AttrFunction F1 = null;
                    Symbol tmpX = null;
                    foreach (var item in TmpList)
                    {   // идем по списку нетерминалов которые нужно соединить с D и сравниваем с эл-тами из RHS
                        if (x.symbol == item.symbol)
                        {
                            ++i;
                            x.attr = new List<Symbol>() { A[i] };
                            tmpX = x;
                        }
                    }
                    if (tmpX != null)
                    {
                        F1 = new AttrFunction(tmpX.attr, IdVal.attr);
                        tmpX = null;
                    }
                    if (F1 != null)
                    {
                        p.F.Add(F1);
                    }
                }
            }
        } // end ATG_C_Py 

        //##

        /// Печать грамматики
        public void Print1()
        {
            Console.Write("\nAT-Grammar G = (V, T, OP, P, S)");
            Console.Write("\nV = { "); //нетерминальные символы
            for (int i = 0; i < V.Count; ++i)
            {
                V[i].print();
                if (i != V.Count - 1)
                    Console.Write(", ");
            }
            Console.Write(" },");
            Console.Write("\nT = { "); //терминальные
            for (int i = 0; i < T.Count; ++i)
            {
                T[i].print();
                if (i != T.Count - 1)
                    Console.Write(", ");
            }
            Console.Write(" },");

            var opf = new List<Symbol_Operation>(); //счётчик операционных символов, у которых есть атрибуты
            Console.Write("\nOP = { "); //операционные
            foreach (var op in OP)
            {
                op.print();
                if (op.function != null)
                    opf.Add(op);
                Console.Write(", ");
            }
            Console.Write(" },");

            Console.Write("\nS = ");
            S0.print();

            //печать правил атрибутов операционных символов
            if (opf.Count != 0)
            {
                Console.Write("\nOperation Symbols Rules:\n");
                foreach (var op in opf)
                {
                    op.print();
                    Console.Write("\n");
                }
            }
            //печать правил грамматики
            if (Rules.Count != 0)
            {
                Console.Write("\nGrammar Rules:\n");
                for (int i = 0; i < Rules.Count; ++i)
                {
                    Console.Write("\n");
                    Rules[i].print();
                    Console.Write("\n");
                }
            }
            foreach (var p in Rules)
            {
                foreach (var x in p.RHS)
                {
                    if (x.symbol.Length > 2 && (x.symbol.Substring(0, 2) == "{'"))
                    {
                        Console.WriteLine(x.symbol.Substring(2, x.symbol.Length - 4));
                    }
                }
            }
        }
        public void Print()
        {
            Console.Write("\nAT-Grammar G = (V, T, OP, P, S)");
            Console.Write("\nV = { "); //нетерминальные символы
            for (int i = 0; i < V.Count; ++i)
            {
                V[i].print();
                if (i != V.Count - 1)
                    Console.Write(", ");
            }
            Console.Write(" },");
            Console.Write("\nT = { "); //терминальные
            for (int i = 0; i < T.Count; ++i)
            {
                T[i].print();
                if (i != T.Count - 1)
                    Console.Write(", ");
            }
            Console.Write(" },");

            var opf = new List<Symbol_Operation>(); //счётчик операционных символов, у которых есть атрибуты
            Console.Write("\nOP = { "); //операционные
            foreach (var op in OP)
            {
                op.print();
                if (op.function != null)
                    opf.Add(op);
                Console.Write(", ");
            }
            Console.Write(" },");

            Console.Write("\nS = ");
            S0.print();

            //печать правил атрибутов операционных символов
            if (opf.Count != 0)
            {
                Console.Write("\nOperation Symbols Rules:\n");
                foreach (var op in opf)
                {
                    op.print();
                    Console.Write("\n");
                }
            }
            //печать правил грамматики
            if (Rules.Count != 0)
            {
                Console.Write("\nGrammar Rules:\n");
                for (int i = 0; i < Rules.Count; ++i)
                {
                    Console.Write("\n");
                    Rules[i].print();
                    Console.Write("\n");
                }
            }
        }
        public List<Production> getRules(Symbol noTermSymbol)
        {
            List<Production> result = new List<Production>();
            for (int i = 0; i < Rules.Count; ++i)
            {
                Production currRule = (Production)Rules[i];
                if (currRule.LHS.symbol == noTermSymbol)
                {
                    result.Add(currRule);
                }
            }
            return result;
        }
        private bool IsOper(string s)
        {
            return s == "+" || s == "-" || s == "*" || s == "/";
        }
        public void transform()
        {
            Console.WriteLine("\nPress Enter to start\n");
            Console.ReadLine();
            for (int i = 0; i < Rules.Count; ++i)
            {
                for (int j = 0; j < Rules[i].F.Count; ++j)
                { //обработка j-го атрибутного правила i-го правила грамматики
                    string NewOpS = "";
                    var atrs = new List<Symbol>();
                    var atrs_l = new List<Symbol>();
                    for (int k = 0; k < Rules[i].F[j].RH.Count; ++k)
                    { //проверка наличия функции в правой чаcnи правила
                        if (IsOper(Rules[i].F[j].RH[k].symbol))
                        {
                            NewOpS += Rules[i].F[j].RH[k]; //создание имени для нового оперционного символа
                        }
                        else
                        {
                            atrs.Add(new Symbol(Rules[i].F[j].RH[k] + "'")); //создание дублирующих символов для правил A <- a, но в формате a' <- a.
                            atrs_l.Add(Rules[i].F[j].RH[k]); //список атрибутов, входящих в функцию
                        }
                    }
                    if ((NewOpS.Count()) == 0) // проверка, что нет функций в правй части правила
                        continue;
                    NewOpS += i.ToString() + j.ToString(); //создание уникального имени операционного символа
                    atrs.Add(new Symbol(atrs[0] + "_ans")); // добавление атрибута для результата функции

                    this.OP.Add(new Symbol_Operation("{" + NewOpS + "}", atrs, new List<Symbol>() { new Symbol(atrs[0] + "_ans") },
                        Rules[i].F[j].RH)); // добавление операционного символа с атрибутами и атрибутным правилом
                                            //  Console.WriteLine("####### before ##########");
                    for (int k = 0; k < atrs.Count - 1; ++k)
                    { //добавление копирующих правил a' <- a  !
                      //Было: Rules[i].F.Add(new AttrFunction(new List<Symbol>() { atrs[k] }, new List<Symbol>() { atrs_l[k] }));
                      //Стало:          
                        Rules[i].F.Add(new AttrFunction(new List<Symbol>() { atrs_l[k] }, new List<Symbol>() { atrs[k] }));
                        // BUG при создании копирующих правил для вычисления атрибутов левая и правая часть при записи менялись местами,
                        //из-за чего возникала неправильная печать и добавлялись некоректные правила.
                    }
                    Rules[i].F.Add(new AttrFunction(new List<Symbol>(Rules[i].F[j].LH), new List<Symbol>() { new Symbol(atrs[0] + "_ans") }));
                    //добавление правила z1, ... , zm <- p, где p - результат функции операционного символа
                    Rules[i].F.RemoveAt(j); //удаление правила с функцией в правой части
                    j -= 1;
                    for (int k = Rules[i].RHS.Count - 1; k >= 0; --k)
                    {
                        //поиск самой левой позииции для вставки операционного символа,
                        //начиная с самой правой позиции
                        int k1;
                        if (Rules[i].RHS[k].attr == null) //проверка того, что есть атрибуты у к-го символа правой
                                                          //части правила грамматики
                            continue;
                        for (k1 = 0; k1 < Rules[i].RHS[k].attr.Count; ++k1)
                        { //проверка, что у к-го символа нет атрибута, который есть у операционного символа,
                          //если он есть, то дальше мы не двигаемся и вставляем операционный символ перед ним, инче идём дальше до конца
                            if (atrs_l.Contains(Rules[i].RHS[k].attr[k1]))
                                break;
                        }
                        if (k1 < Rules[i].RHS[k].attr.Count)
                        { //нашли такой символ, справа от которого вставляем операционный
                            Rules[i].RHS.Insert(k + 1, new Symbol("{" + NewOpS + "}", atrs));
                            break;
                        }
                        if (k == 0)
                        { //дошли до конца правила и не нашли символа с хотя бы одним атрибутом, совпадающим с атрибутами операционного символа. Такого быть не должно, т.к. это означает, что атрибутные правила содержат атрибуты, отсутствующие у правила грамматики
                            Rules[i].RHS.Insert(k, new Symbol("{" + NewOpS + "}", atrs));
                            break;
                        }
                    }
                }
                //поиск лишних атрибутов в правилах типа
                // a1, ... , am <- k
                // b1, ..., k, ..., bn <- g
                // и замена на b1, ..., a1, ... , am, ..., bn <- g с удалением правила a1, ... , am <- k
                for (int r = 0; r < Rules[i].F.Count; ++r)
                {
                    bool deleted = false;
                    for (int l = r + 1; l < Rules[i].F.Count; ++l)
                    {
                        if (Rules[i].F[l].LH.Contains(Rules[i].F[r].RH[0]))
                        {
                            Rules[i].F[l].LH.Remove(Rules[i].F[r].RH[0]);
                            deleted = true;
                            foreach (var s in (Rules[i].F[r].LH))
                                Rules[i].F[l].LH.Add(s);
                        }
                    }
                    if (deleted)
                    {
                        Rules[i].F.RemoveAt(r);
                        r -= 1;
                    }
                }
                Console.WriteLine("\nChange for " + (i + 1).ToString() + "th rule\n");
                Rules[i].print();
                Console.ReadLine();
            }
        }

    } // and AGrammar

}

