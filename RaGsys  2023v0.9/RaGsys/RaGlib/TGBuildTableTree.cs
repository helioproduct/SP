using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using RaGlib.Core;
using RaGlib.Grammars;

namespace RaGlib {
    public class TGBuildTableTree{
        public GrammarTranslation G;
        public Stack<Symbol> Stack;
        public DataTable Table;
        public _1dTreeVertices Tree;
        public string AppliedRules = "";
        public TGBuildTableTree(GrammarTranslation grammar){
            this.G = grammar;
            BuildControlTable();
        }
        private void BuildControlTable(){
            Table = new DataTable("ControlTable");
            Stack = new Stack<Symbol>();
            Table.Columns.Add(new DataColumn("SYMBOL", typeof(Symbol)));
            Console.WriteLine("Создадим таблицу. Сначала создадим по столбцу для каждого из этих терминалов: ");
            foreach (var termSymbol in G.T){
                Console.Write(termSymbol.symbol);
                Console.Write(", ");
                Table.Columns.Add(new DataColumn(termSymbol.symbol, typeof(TableElem)));
            }
            G.ComputeFirstFollow();
            for (int i = 0; i < G.V.Count; i++) // Рассмотрим последовательно все нетерминалы
            {
                DataRow workRow = Table.NewRow(); //Новая строка
                workRow["SYMBOL"] = G.V[i];

                Console.Write("Рассмотрим нетерминал ");
                Console.Write((G.V[i].symbol));
                Console.Write("\n");

                var rules = GetRules(G.V[i]);
                // Получим все правила, соответствующие текущему нетерминалу

                foreach (var rule in rules){
                    List<Symbol> newRHS = new List<Symbol>();
                    foreach(Symbol rhs in rule.RHS){
                        if (!(rhs is Symbol_Operation)){
                            newRHS.Add(rhs);
                        }
                    }
                    var currFirstSet = G.First(newRHS);
                    foreach (var firstSymbol in currFirstSet){
                        if (firstSymbol != ""){
                            Console.Write("   Первый символ правила ");
                            Console.Write(rule.LHS);
                            Console.Write(" -> ");
                            for (int j = 0; j < rule.RHS.Count; j++){
                                Console.Write(rule.RHS[j]);
                            }
                            Console.Write(" - ");
                            Console.WriteLine(firstSymbol);
                            workRow[firstSymbol.symbol] = rule;
                            Console.Write("   Это правило заносим в таблицу на пересечении строки нетерминала ");
                            Console.Write(rule.LHS);
                            Console.Write(" и столбца терминала ");
                            Console.WriteLine(firstSymbol);
                            Console.Write("\n");
                        }
                        else{
                            HashSet<Symbol> currFollowSet = G.Follow(rule.LHS);
                            foreach (var currFollowSymb in currFollowSet){
                                string currFollowSymbFix = (currFollowSymb == "") ? "ε" : currFollowSymb.symbol;
                                workRow[currFollowSymbFix] = rule;
                            }
                        }
                    }
                }
                Table.Rows.Add(workRow);
            }
            foreach (var termSymbol in G.T){
                DataRow workRow = Table.NewRow();
                if (termSymbol=="ε"){
                    workRow["SYMBOL"]=new Symbol("⊥");
                    workRow[termSymbol.symbol]=new Command("ДОПУСК");
                }
                else{
                    workRow["SYMBOL"]=termSymbol;
                    workRow[termSymbol.symbol]=new Command("ВЫБРОС");
                }
                Table.Rows.Add(workRow);
            }
            foreach (var opSymbol in G.OP){
                DataRow workRow = Table.NewRow();
                workRow["SYMBOL"]=opSymbol;
                foreach (var termSymbol in G.T){
                    workRow[termSymbol.symbol]=new Command("ВЫДАЧА", opSymbol);
                }
                Table.Rows.Add(workRow);
            }
        }
        private List<Production> GetRules(Symbol noTermSymbol){
            List<Production> result = new List<Production>();
            for (int i = 0; i < G.P.Count; ++i){
                Production currRule = (Production)G.P[i];
                if (currRule.LHS.symbol == noTermSymbol){
                    result.Add(currRule);
                }
            }
            return result;
        }
        public DataTable GetControlTable(){
            return Table;
        }
        public void BuildAndReturnTree(List<Symbol> input){
            Tree = new _1dTreeVertices(G.S0);
            Stack.Push("⊥"); // маркер дна стека
            Stack.Push(G.S0);
            int i = 0;
            Symbol currInputSymbol = input[i];
            do{
                var currStackSymbol = Stack.Peek();
                DataRow custRows;
                DataRow[] tempRows = Table.Select("CONVERT(SYMBOL, System.String) = '" + currStackSymbol.ToString().Replace(@"'", @"''") + "'", null);
                if (tempRows.Count()>1 && (currStackSymbol is Symbol_Operation)){
                    custRows=tempRows[1];
                }
                else{
                    custRows=tempRows[0];
                }
                if (custRows[currInputSymbol.ToString()] is Command){
                    Command currentProduction = (Command) custRows[currInputSymbol.ToString()];
                    if (currentProduction.CommandString=="ВЫБРОС"){
                        if (i != input.Count()){
                            ++i;
                        }
                        currInputSymbol = (i == input.Count()) ? "ε" : input[i].ToString();
                    }
                    Stack.Pop();
                }
                else{
                    if (!custRows.IsNull(currInputSymbol.ToString())) // в клетке[вершина стека, распознанный символ] таблицы разбора существует правило
                    {
                        //  извлечь из стека элемент и занести в стек все терминалы и нетерминалы найденного в таблице правила в стек в порядке обратном порядку их следования в правиле
                        Production currentProduction = (Production) custRows[currInputSymbol.ToString()];
                        Stack.Pop();
                        List<Symbol> RHS_Without_T = new List<Symbol>();
                        foreach (var symbol in currentProduction.RHS){
                            if (symbol is Symbol_Operation || G.V.Contains(symbol) || symbol=="ε"){
                                RHS_Without_T.Add(symbol);
                            }
                        }
                        Tree.AddNodeToTree(currentProduction.LHS, RHS_Without_T);
                        List<Symbol> RHS = currentProduction.RHS;
                        RHS.Reverse();
                        foreach (var chainSymbol in RHS) {
                            if (chainSymbol != "ε"){
                                Stack.Push(chainSymbol);
                            }
                        }
                        AppliedRules += (currentProduction.Id);
                    }
                    else{
                        Console.WriteLine(currInputSymbol+" "+currStackSymbol);
                        Console.WriteLine("Ошибка: отсутствует правило в таблице");
                        Environment.Exit(-1);
                    }
                }
            } while (Stack.Peek() != "⊥"); // вершина стека не равна концу входной последовательности
            if (i != input.Count()){
                Console.WriteLine("Ошибка: не все символы строки были обработаны");
                Environment.Exit(-1);
            }
            Console.WriteLine("Построенное дерево разбора:");
            Tree.PrintTree();
            Console.WriteLine("Порядок примененных правил:");
            Console.WriteLine(AppliedRules);
            Console.WriteLine("Представление дерева в виде массива вершин:");
            Tree.PrintTreeArray();
        }
        
    }
}