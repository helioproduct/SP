using System;
using System.Collections;
using System.Collections.Generic;
using RaGlib.Core;

namespace RaGlib.SATDMP {


    public class Attribute {
        public string Name { get; }
        public int? Value { get; }

        public Attribute(string Name, int? Value) {
            this.Name = Name;
            this.Value = Value;
        }

        public override string ToString()
        {
            return String.Format("{0}={1}", Name, Value);
        }

    }

    //Filled attr translation grammar symbol
    public class ATSymbol : Symbol {
        public List<Attribute> Attr = new List<Attribute>();
        public bool Operation = false;

        //full constructor
        public ATSymbol(string Symbol, int I, int J, List<Attribute> Attr): base(Symbol, I, J) {
            this.Attr = Attr;
        }

        //symbol with attr
        public ATSymbol(string Symbol, List<Attribute> Attr):base(Symbol)
        {
            this.Attr = Attr;
        }

        //symbol with simple attr
        public ATSymbol(string Symbol, List<string> Attr) : base(Symbol)
        {
            foreach (var s in Attr)
            {
                this.Attr.Add(GetAttribute(s));
            }
        }

        //operating symbol with attrs
        public ATSymbol(string Symbol, List<Attribute> Attr, bool Operation) : base(Symbol)
        {
            this.Attr = Attr;
            this.Operation = Operation;
        }

        //simple constructor of attr elemnt a=v or a in Attr list
        public ATSymbol(string Symbol, List<string> Attr, bool Operation) : base(Symbol)
        {
            foreach(var s in Attr)
            {
                this.Attr.Add(GetAttribute(s));
            }
            this.Operation = Operation;
        }

        //get attr from string attr a=v
        private Attribute GetAttribute(string attr) {
            var x = attr.Split(new char[] { '=' });
            if (x.Length == 3)
            {
                return new Attribute(x[0], Int32.Parse(x[2]));
            }
            else return new Attribute(x[0], null);
        }


        //full string og symbol with attrs and indexes
        public string FullString(bool FilledType = false)
        {
            string s = "";
            if (FilledType)
            {
                s += symbol + production + symbolPosition;
            }
            else if (Operation)
            {
                s += "{" + symbol + "}[";
                foreach (var x in Attr) s += x + ",";
                s += "]";
            }
            else
            {
                s += symbol+ production+ symbolPosition + "[";
                foreach (var x in Attr) s += x + ",";
                s += "]";
            }

            return s;
        }


    }

    //attr grammar rule
    public class ATGrammarProduction:Production {

        public List<Production> AttrsRules { get; } //attrs production rules
        public List<ATSymbol> OP { get; } //operating symbols

        //get full-filled grammar
        public ATGrammarProduction(ATSymbol LHS, List<Symbol> RHS, List<Production> AttrsRules, List<ATSymbol> OP, int Id) : base(LHS, RHS)
        {
            base.Id = Id;
            this.AttrsRules = AttrsRules;
            this.OP = OP;
            foreach(var x in OP) x.Operation = true;
        }

        
    }

    //Модернизированный парсер MyLRParser для АТ-грамматики
    public class SDMPParser
    {
        private Grammar grammar = new Grammar(new List<Symbol>(), new List<Symbol>(), new List<Production>(), "S"); //grammar
        private int Count = 1; //productions count
        private HashSet<Symbol> T = new HashSet<Symbol>(); //set of terminal symbols
        private HashSet<Symbol> V = new HashSet<Symbol>(); //set of non-terminals symbols



        //example
        //        new SATGrammarSymbol("A", new List<string>() { "v" }), //LHS
        //        new List<Symbol>() {
        //            new SATGrammarSymbol("B", new List<string>() {"a"}), //RHS B and attr a
        //            new SATGrammarSymbol("C", new List<string>() { "q" })  //RHS C and attr q
        //        },
        //        new List<SATGrammarSymbol>() {
        //            new SATGrammarSymbol("+", new List<string>() {"l"})  //OP symbol {+} and it`s attr l
        //        },
        //        new List<Production>() { new Production("v", new List<Symbol> { "a" }), new Production("l", new List<Symbol> { "q" }) }, //AttrRules v<-a, l<-q

        public void AddRule(ATSymbol LHS, List<ATSymbol> RHS, List<ATSymbol> OP, List<Production> AttrsRules)
        {
            var rhs = new List<Symbol>();
            foreach (var x in RHS) rhs.Add((Symbol)x);
            this.grammar.AddRule(new ATGrammarProduction(LHS,rhs,AttrsRules,OP,Count));
            Count++;
            V.Add(LHS.symbol);
            foreach (var s in RHS) {
                if (s.symbol.ToLower() != s.symbol) V.Add(s.symbol);
                else T.Add(s.symbol);
            }

            grammar.V = new List<Symbol>(V);
            grammar.T = new List<Symbol>(T);
        }

        public void Info()
        {
            Console.Write("АТ - грамматика :\nАлфавит нетерминальных символов: ");
            foreach (var v in grammar.V) Console.Write(v);
            Console.Write("\nАлфавит терминальных символов: ");
            foreach (var t in grammar.T) Console.Write(t);
            Console.Write("\nПравила:\n");
            foreach (var p in grammar.P)
            { 
                if (p is ATGrammarProduction)
                {
                    ATGrammarProduction b = (ATGrammarProduction)p;
                    Console.Write("(" + p.Id.ToString() + ") " + ((ATSymbol)b.LHS).FullString() + " -> ");
                    foreach (ATSymbol x in b.RHS) Console.Write(x.FullString());
                    foreach (ATSymbol x in b.OP) Console.Write(x.FullString());
                    Console.Write("\n");
                    foreach (var x in b.AttrsRules)
                    {
                        Console.Write(x.LHS.ToString() + " <- ");
                        foreach (var y in x.RHS) Console.Write(y);
                        Console.Write("\n");
                    }
                }
                else {
                    Console.Write("(" + p.Id.ToString() + ") " + p.LHS + " -> ");
                    foreach (var x in p.RHS) Console.Write(x);
                }
                Console.Write("\n");
                
            }
            Console.WriteLine("Начальный нетерминал: " + grammar.S0);
       
        }

        private List<Symbol> OFirst(Symbol occur) // Возвращает множество OFIRST для грам. вхождения
        {
            var result = new List<Symbol>();
            result.Add(occur);
            foreach (var p in grammar.P)
            {
                var rhs = p.RHS;
                if (p.LHS == occur.symbol)
                {
                    if (occur.symbol == rhs[0])
                    {
                        result.Add(new Symbol(rhs[0].symbol, p.Id, 1));
                    }
                    else
                    {
                        var newResult = OFirst(new Symbol(rhs[0].symbol, p.Id, 1));
                        result.AddRange(newResult);
                    }
                }
            }
            return result;
        }
        private string GetVp(List<Symbol> Teta, Dictionary<string, List<Symbol>> M) // Возвращает для множества грам. вхождений их магазинный символ
        {
            string Key = "";
            foreach (var v in Teta)
            {
                Key += v.symbol + v.production.ToString() + v.symbolPosition.ToString();
            }
            if (!M.ContainsKey(Key)) M.Add(Key, Teta);
            return Key;
        }

        public void Execute()
        {
            Console.WriteLine("\nИсходная ");
            Info();
            grammar.EpsDelete();
            var augmentedRHS = new List<Symbol>();
            augmentedRHS.Add(grammar.S0);
            var augmentedRule = new Production(new Symbol("П"), augmentedRHS);
            augmentedRule.Id = 0;
            grammar.P.Insert(0, augmentedRule);
            grammar.V.Add(new Symbol("П"));
            grammar.T.Add(new Symbol("$"));
            var start = new Symbol(grammar.S0.symbol, 0, 1);
            Console.WriteLine("\nПополненная грамматика: ");
            Info();

            // Нахождение матрицы отношения OBLOW и множества грамматических вхождений
            var oblow = new Dictionary<Symbol, Dictionary<Symbol, int>>();
            var allGrammarOccurs = new HashSet<Symbol>();
            foreach (var p in grammar.P)
            {
                var rhs = p.RHS;
                if (p.LHS == "П")
                {
                    var symbol = new Symbol(rhs[0].ToString(), p.Id, 1);
                    var botMarker = new Symbol("^", 0, 0);
                    allGrammarOccurs.Add(botMarker);
                    allGrammarOccurs.Add(symbol);
                    var first = OFirst(symbol);
                    oblow.Add(botMarker, new Dictionary<Symbol, int>());
                    foreach (var occur in first)
                    {
                        oblow[botMarker].Add(occur, 1);
                    }
                }
                else
                {
                    for (int j = 0; j < rhs.Count; ++j)
                    {
                        var current = new Symbol(rhs[j].ToString(), p.Id, j + 1);
                        allGrammarOccurs.Add(current);
                        if (j != rhs.Count - 1)
                        {
                            var first = OFirst(new Symbol(rhs[j + 1].ToString(), p.Id, j + 2));
                            oblow.Add(current, new Dictionary<Symbol, int>());
                            foreach (var occur in first) oblow[current].Add(occur, 1);
                        }
                    }
                }
            }

            Console.WriteLine("\nПолученная матрица отношения OBLOW: ");
            Console.Write("    ");
            foreach (var x in allGrammarOccurs)
            {
                if (x.symbol != "^") Console.Write("{0, 4}", x.symbol + x.production.ToString() + x.symbolPosition.ToString());
            }
            Console.Write("\n");
            foreach (var x in allGrammarOccurs)
            {
                Console.Write("{0, 4}", x.symbol + (x.symbol == "^" ? "" : x.production.ToString() + x.symbolPosition.ToString()));
                foreach (var y in allGrammarOccurs)
                {
                    if (y.symbol == "^") continue;
                    Console.Write("{0, 4}", (oblow.ContainsKey(x) && oblow[x].ContainsKey(y) ? "1" : " "));
                }
                Console.Write("\n");
            }

            // Построение промежуточной матрицы для таблицы переходов g(X)
            var building = new Dictionary<Symbol, Dictionary<Symbol, List<Symbol>>>();
            HashSet<Symbol> nonDeterministic = new HashSet<Symbol>();

            foreach (var x in allGrammarOccurs)
            {
                building.Add(x, new Dictionary<Symbol, List<Symbol>>());
                foreach (var y in allGrammarOccurs)
                {
                    if (y != "^" && oblow.ContainsKey(x) && oblow[x].ContainsKey(y))
                    {
                        if (!building[x].ContainsKey(y.symbol)) building[x].Add(y.symbol, new List<Symbol>());
                        building[x][y.symbol].Add(y);
                        if (building[x][y.symbol].Count > 1)
                        {
                            foreach (var bad in building[x][y.symbol]) { nonDeterministic.Add(bad); }
                        }
                    }
                }
            }

            var Z = new Dictionary<string, Dictionary<Symbol, string>>();
            var Vp = new HashSet<string>();
            var M = new Dictionary<string, List<Symbol>>();

            var alphabet = new List<Symbol>(grammar.T);
            alphabet.AddRange(grammar.V);
            foreach (var x in allGrammarOccurs)
            {
                if (nonDeterministic.Contains(x)) continue;
                var XArray = new List<Symbol>();
                XArray.Add(x);
                string vX = GetVp(XArray, M);
                Vp.Add(vX);
                Z.Add(vX, new Dictionary<Symbol, string>());
                foreach (var t1 in alphabet)
                {
                    if (t1 == "$" || t1 == "П") continue;
                    if (!building.ContainsKey(x) || !building[x].ContainsKey(t1)) continue;
                    string vD = GetVp(building[x][t1], M);
                    Vp.Add(vD);
                    Z[vX].Add(t1, vD);
                    if (building[x][t1].Count > 1 && !Z.ContainsKey(vD)) //добавлен if
                    {
                        Z.Add(vD, new Dictionary<Symbol, string>());
                        foreach (var d in building[x][t1])
                        {
                            foreach (var t2 in alphabet)
                            {
                                if (building.ContainsKey(d) && building[d].ContainsKey(t2))
                                {
                                    Z[vD].Add(t2, GetVp(building[d][t2], M));
                                }
                            }
                        }
                    }
                }
            }

            Console.Write("\nПолученная матрица для функции переходов g(X):\n      ");
            foreach (var t in alphabet)
            {
                if (t != "$" && t != "П") Console.Write("{0, 8}", t.symbol);
            }
            Console.Write("\n");
            foreach (var v in Vp)
            {
                Console.Write("{0, 8}", v);
                foreach (var t in alphabet)
                {
                    if (t == "$" || t == "$") continue;
                    Console.Write("{0, 8}", (Z.ContainsKey(v) && Z[v].ContainsKey(t) ? Z[v][t] : ""));
                }
                Console.Write("\n");
            }



            var H = new Dictionary<string, Dictionary<Symbol, string>>();

            foreach (var v in Vp)
            {
                H.Add(v, new Dictionary<Symbol, string>());
                foreach (var t in grammar.T)
                {
                    var first = (Symbol)M[v][0];
                    if (first.Equals(start) && M[v].Count == 1 && t == "$")
                    {
                        H[v].Add(t, "ДОПУСК");
                        continue;
                    }
                    else if (!first.Equals(start) && first != "^" && M[v].Count == 1 && grammar.P[first.production].RHS.Count == first.symbolPosition)
                    {
                        H[v].Add(t, "СВЕРТКА " + first.production.ToString());
                        continue;
                    }
                    if (t != "$")
                    {
                        bool check = true;
                        foreach (var x in M[v])
                        {
                            if (grammar.P[x.production].RHS.Count == x.symbolPosition) { check = false; }
                        }
                        if (check)
                        {
                            H[v].Add(t, "ПЕРЕНОС");
                            continue;
                        }
                    }
                    if (M[v].Contains(start))
                    {
                        bool check = false;
                        foreach (var x in M[v])
                        {
                            if (!v.Equals(start) && grammar.P[x.production].RHS.Count != x.symbolPosition)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check)
                        {
                            if (t == "$")
                            {
                                H[v].Add(t, "ДОПУСК");
                            }
                            else
                            {
                                H[v].Add(t, "ПЕРЕНОС");
                            }
                        }
                    }
                }
            }

            Console.Write("\nПолученная матрицы для функции действий f(X):\n      ");
            foreach (var t in grammar.T)
            {
                Console.Write("{0, 10}", t.symbol);
            }
            Console.Write("\n");
            foreach (var v in Vp)
            {
                Console.Write("{0, 8}", v);
                foreach (var t in grammar.T)
                {
                    Console.Write("{0, 10}", (H[v].ContainsKey(t) ? H[v][t] : "ОШИБКА"));
                }
                Console.Write("\n");
            }

            for (; ; )
            {
                Console.Write("\nВведите цепочку: ");
                string input = Console.In.ReadLine();
                input += "$[]";
                List<ATSymbol> symbols = ParseLine(input);
                var result = new List<ATSymbol>();
                var VpSymbols = new List<ATSymbol>();
                VpSymbols.Add(new ATSymbol("^", 0, 0, new List<Attribute>()));
                bool good = false;
                Console.WriteLine("\nСостояния магазина: ");
                for (int i = 0; i < symbols.Count; ++i)
                {
                    var inputSymbol = symbols[i];

                   
                    if (!H.ContainsKey(VpSymbols[VpSymbols.Count - 1].FullString(true)) || !H[VpSymbols[VpSymbols.Count - 1].FullString(true)].ContainsKey(inputSymbol))
                    {
                        break;
                    }
                    if (H[VpSymbols[VpSymbols.Count - 1].FullString(true)][inputSymbol] == "ДОПУСК")
                    {
                     
                        good = true;
                        break;
                    }
                    else if (H[VpSymbols[VpSymbols.Count - 1].FullString(true)][inputSymbol] == "ПЕРЕНОС")
                    {
                        if (!Z.ContainsKey(VpSymbols[VpSymbols.Count - 1].FullString(true)) || !Z[VpSymbols[VpSymbols.Count - 1].FullString(true)].ContainsKey(inputSymbol))
                        {
                            break;
                        }

                        var tmp = Z[VpSymbols[VpSymbols.Count - 1].FullString(true)][inputSymbol];
                        int production = Int32.Parse(tmp[1].ToString());
                        int position = Int32.Parse(tmp[2].ToString());
                        var attrs = new List<Attribute>();
                        ATSymbol tmpRule = GetSATGrammarSymbol(production, position);
                        for(int k = 0; k < tmpRule.Attr.Count; k++) {
                            attrs.Add(new Attribute(tmpRule.Attr[k].Name, inputSymbol.Attr[k].Value));
                        }
                        var symbol = new ATSymbol(inputSymbol.symbol, production, position, attrs);
                        VpSymbols.Add(symbol);  
                    }
                    else
                    { 
                        string numberStr = H[VpSymbols[VpSymbols.Count - 1].FullString(true)][inputSymbol].Substring(8);
                        int numberInt = Int32.Parse(numberStr);
                        var range = VpSymbols.GetRange(VpSymbols.Count - grammar.P[numberInt].RHS.Count, grammar.P[numberInt].RHS.Count);
                        var production = (ATGrammarProduction)grammar.P[numberInt];
                        var rules = production.AttrsRules;
                        var attrs = new Dictionary<string, int>();

                        //заносим имеющиеся значения атрибутов
                        foreach(var s in range)
                        {
                            foreach (var a in s.Attr) attrs.Add(a.Name, (int)a.Value);
                        }
                        VpSymbols.RemoveRange(VpSymbols.Count - grammar.P[numberInt].RHS.Count, grammar.P[numberInt].RHS.Count);

                        //считаем атрибуты
                        foreach (var rule in rules) {
                            attrs[rule.LHS.ToString()] = attrs[rule.RHS[0].ToString()];
                        }

                        if (!Z.ContainsKey(VpSymbols[VpSymbols.Count - 1].FullString(true)) || !Z[VpSymbols[VpSymbols.Count - 1].FullString(true)].ContainsKey(grammar.P[numberInt].LHS))
                        {
                            break;
                        }
                        //operating symbol to output
                        var OP = GetOPSymbol(numberInt);
                        var newAttr = new List<Attribute>();
                        foreach (var a in OP.Attr) {
                            newAttr.Add(new Attribute(a.Name, attrs[a.Name]));
                        }
                        result.Add(new ATSymbol(OP.symbol, newAttr, true));

                        //LHS symbol to store
                        newAttr = new List<Attribute>();
                        var LHS = (ATSymbol)grammar.P[numberInt].LHS;
                        foreach (var a in LHS.Attr)
                        {
                            newAttr.Add(new Attribute(a.Name, attrs[a.Name]));
                        }
                        var tmp = Z[VpSymbols[VpSymbols.Count - 1].FullString(true)][grammar.P[numberInt].LHS];
                        int prod = Int32.Parse(tmp[1].ToString());
                        int position = Int32.Parse(tmp[2].ToString());
                        VpSymbols.Add(new ATSymbol(LHS.symbol, prod, position, newAttr));
                        --i;
                    }
                    Console.Write("\n");
                    foreach (var x in VpSymbols)
                    {
                        Console.Write(x.FullString()+" ");
                    }
                    
                }
                if (good)
                {
                    Console.WriteLine("\nВходная цепочка " + input + " распознана.");
                    Console.Write("Результат вывода: ");
                    foreach (var rule in result)
                    {
                        Console.Write(rule.FullString() + " ");
                    }
                    Console.Write("\n");
                }
                else
                {
                    Console.WriteLine("Входная цепочка не распознана.");
                }
                for (; ; )
                {
                    Console.Write("Вы хотите продолжить (да/нет)? - ");
                    string answer = Console.In.ReadLine();
                    if (answer == "нет")
                    {
                        return;
                    }
                    else if (answer == "да")
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

        }

        //line example a[4]c[8]g[3]d[7,7,8]f[1]c[2]g[8]g[9]b[2,4]
        //line example a[4]c[8]g[3]d[7]f[1]c[2]g[8]g[9]b[2]
        //parsing line if symbols with attrs
        public static List<ATSymbol> ParseLine(string line) {
            var list = new List<ATSymbol>();
            var attrs = new List<Attribute>();
            var symbol = "";


            var s = line.Split(new char[] { '[', ']'});
            
            for(int i = 0; i < s.Length-1; i++) {
                if (s[i] == "")
                {
                   
                    list.Add(new ATSymbol(symbol, attrs));
                    symbol = "";
                    attrs = new List<Attribute>();

                }
                else if (Char.IsDigit(s[i][0])) {
                    var s2 = s[i].Split(new char[] { ',' });
                    foreach (var x in s2)
                    {
                        attrs.Add(new Attribute("", Int32.Parse(x)));
                    }
                    list.Add(new ATSymbol(symbol, attrs));
                    symbol = "";
                    attrs = new List<Attribute>();
                }
                else
                {
                    symbol = s[i];
                    if (i == s.Length - 1)
                    {
                        list.Add(new ATSymbol(symbol, attrs));
                        symbol = "";
                        attrs = new List<Attribute>();
                    }
                }
              
            }

            return list;
        }

        //get symbol from production by it`s indexes
        private ATSymbol GetSATGrammarSymbol(int production, int position)
        {
            return (ATSymbol)grammar.P[production].RHS[position - 1];
        }

        //get operating symbol from production by it`s production number
        private ATSymbol GetOPSymbol(int production)
        {
            return (ATSymbol)((ATGrammarProduction)grammar.P[production]).OP[0];
        }


    }
}
