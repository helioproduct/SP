using System;
using System.Collections.Generic;
using System.Linq;
using RaGlib.Core;
using RaGlib.Grammars;

namespace RaGlib
{
    public class ATScheme
    {
        public class ATSProduction : AttrProduction
        {
            public List<Symbol> TranslateProd;
            public List<AttrFunction> TranslateF;

            public ATSProduction(Symbol LHS, List<Symbol> Chain1, List<Symbol> Chain2, List<AttrFunction> F1, List<AttrFunction> F2) : base(LHS, Chain1, F1)
            {
                TranslateF = F2;
                TranslateProd = Chain2;
            }
        }

        public List<Symbol> V; // Множество нетерминалов
        public List<Symbol> OP; // Множество операционных символов
        public List<Symbol> Sigma; // Множество терминалов у входной грамматики
        public List<Symbol> Delta; // Множество терминалов у выходной грамматики
        public List<ATSProduction> Productions;
        public Symbol S0;

        public ATGrammar GrammarInput; // Входная грамматика
        public ATGrammar GrammarOutput; // Выходная грамматика

        public Vertex root; // Дерево вывода
        public ATScheme(List<Symbol> _V, List<Symbol> _OP, List<Symbol> _Sigma, List<Symbol> _Delta, Symbol S)
        {
            V = _V;
            OP = _OP;
            Sigma = _Sigma;
            Delta = _Delta;
            S0 = S;
            Productions = new List<ATSProduction>();
        }

        // Создание схемы по входной и выходной грамматикам
        public ATScheme(ATGrammar _GrammarInput, ATGrammar _GrammarOutput)
        {
            if (_GrammarInput.V != _GrammarOutput.V || _GrammarInput.OP != _GrammarOutput.OP
                    || _GrammarInput.Rules.Count != _GrammarOutput.Rules.Count || _GrammarInput.S0 != _GrammarOutput.S0)
            {
                throw new Exception("На вход СУ-схеме подали некорректные грамматики");
            }
            GrammarInput = _GrammarInput;
            GrammarOutput = _GrammarOutput;
            V = GrammarInput.V;
            Sigma = GrammarInput.T;
            Delta = GrammarOutput.T;
            OP = new List<Symbol>();
            foreach (var sym in _GrammarInput.OP)
            {
                if (sym.attr == null)
                {
                    OP.Add(new Symbol(sym.symbol));
                }
                else
                {
                    OP.Add(new Symbol(sym.symbol, sym.attr));
                }
            }

            S0 = GrammarInput.S0;
            Productions = new List<ATSProduction>();

            for (int i = 0; i < GrammarInput.Rules.Count; ++i)
            {
                if (GrammarInput.Rules[i].LHS != GrammarOutput.Rules[i].LHS)
                {
                    throw new Exception("На вход СУ-схеме подали некорректные грамматики");
                }
                AddNewProduction(GrammarInput.Rules[i].LHS, GrammarInput.Rules[i].RHS, GrammarOutput.Rules[i].RHS,
                        GrammarInput.Rules[i].F, GrammarOutput.Rules[i].F);
            }
        }

        public void BuildInputTree()
        {
            root = new Vertex(S0, false); // root

            var input = Console.ReadLine();
            var words = input.Split(" ");
            foreach (var word in words)
            {
                var pos = Convert.ToInt32(word) - 1;
                if (pos < 0 || pos >= GrammarInput.Rules.Count)   //words.count (92 строка)
                {
                    throw new Exception($"Правило {pos + 1} либо отрицательно, либо превышает общее количество правил");
                }
                var node = root.FindFirst(root, GrammarInput.Rules.ElementAt(pos).LHS.symbol);
                node.Id = pos;
                node.RealAttrs = GrammarInput.Rules.ElementAt(pos).LHS.attr;
                node.F = GrammarInput.Rules.ElementAt(pos).F;
                foreach (var sym in GrammarInput.Rules.ElementAt(pos).RHS)
                {
                    node.Add(sym, OP.Contains(sym));
                }
            }
            var leaves = root.CountLeaves(root);
            Console.WriteLine($"Введите {leaves} начальных атрибутов:");
            input = Console.ReadLine();
            words = input.Split(" ");

            var initialValues = new List<Symbol>();
            foreach (var i in words)
            {
                initialValues.Add(i);
            }

            root.pos = 0;
            root.vals = initialValues;
            root.SetAttr(root);
        }

        public void TreeTransform()
        {
            Transform(root);
            root.pos = 0;
            root.SetAttr(root);
        }
        //Алгоритм 6.1  Преобразование деревьев при помощи СУ-схемы
        public void Transform(Vertex node)
        {
            node.F = Productions.ElementAt(node.Id).TranslateF;
            node.RealAttrs = Productions.ElementAt(node.Id).LHS.attr;
            int len = (node.Symbol.attr == null ? 0 : node.Symbol.attr.Count);
            node.Calculated = new List<int>(new int[len]);

            //Проверка на лист
            if (!node.Next.Any())
            {
                return;
            }

            //Удаление всех терминальных листьев
            var T = new List<Vertex>();
            foreach (var child in node.Next)
            {
                var ch = child.Symbol;
                if (Sigma.Contains(ch.ToString()))
                {
                    T.Add(child);
                }
            }
            foreach (var del in T)
            {
                node.Next.Remove(del);
            }

            //Перестановка соответствующих потомков и добавление новых терминалов
            var Betta = Productions.ElementAt(node.Id).TranslateProd;
            var NewChildren = new LinkedList<Vertex>();

            foreach (var item in Betta)
            {
                if (Delta.Contains(item.symbol))
                {
                    NewChildren.AddLast(new Vertex(item, false));
                    continue;
                }
                foreach (var element in node.Next)
                {
                    if (element.Symbol.symbol == item.symbol)
                    {
                        NewChildren.AddLast(element);
                        node.Next.Remove(element);
                        break;
                    }
                }
            }

            node.Next = NewChildren;

            //Рекурсивно спускаемся и продолжаем алгоритм в потомках
            foreach (var child in node.Next)
            {
                Transform(child);
            }
        }

        public void PrintTree()
        {
            root.Print();
            Console.Write("Крона дерева: ");
            root.PrintCrown(root);
            Console.WriteLine();
        }

        public void AddNewProduction(Symbol LHS, List<Symbol> Chain1, List<Symbol> Chain2, List<AttrFunction> F1, List<AttrFunction> F2)
        {
            Productions.Add(new ATSProduction(LHS, Chain1, Chain2, F1, F2));
        }
    }

}