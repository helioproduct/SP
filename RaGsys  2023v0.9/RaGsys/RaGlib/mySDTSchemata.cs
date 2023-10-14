using System;
using System.Collections.Generic;
using RaGlib.Core;

namespace RaGlib {
    public class SDTProduction: Production {

        public List<Symbol> TranslateProd;
        public SDTProduction(Symbol S0, List<Symbol> RHSin, List<Symbol> RHSout): base(S0, RHSin)
        {
            TranslateProd = RHSout;
        }
    }
    public class mySDTSchemata
    {
        public List<Symbol> V;
        public List<Symbol> Sigma;
        public List<Symbol> Delta;
        public List<SDTProduction> Productions;
        public Symbol S0;

        public mySDTSchemata(List<Symbol> NTS, List<Symbol> IPTS, List<Symbol> OPTS, Symbol FS)
        {
            V = NTS;
            Sigma = IPTS;
            Delta = OPTS;
            if (!V.Contains(FS))
                throw new Exception($"��������� ������ {FS} �� ����������� ��������� ������������");
            this.S0 = FS;
            Productions = new List<SDTProduction>();
        }

        public void addRule(Symbol NotTerminal, List<Symbol> Chain1, List<Symbol> Chain2)
        {
            if (!V.Contains(NotTerminal))
                throw new Exception($"� ������� {NotTerminal} --- ({Utility.convert(Chain1)}, {Utility.convert(Chain2)}) ���������� {NotTerminal} �� ����������� ��������� ������������");

            foreach (Symbol symbol in Chain1)
            {
                if (!V.Contains(new Symbol(symbol.symbol.Split('_')[0])) && !Sigma.Contains(symbol))
                    throw new Exception($"� ������� {NotTerminal} --- ({Utility.convert(Chain1)}, {Utility.convert(Chain2)}) ������ {symbol} �� ����������� ��������� ������������ ��� �������� ��������");
            }

            foreach (Symbol symbol in Chain2)
            {
                if (!V.Contains(new Symbol(symbol.symbol.Split('_')[0])) && !Delta.Contains(symbol))
                    throw new Exception($"� ������� {NotTerminal} --- ({Utility.convert(Chain1)}, {Utility.convert(Chain2)}) ������ {symbol} �� ����������� ��������� ������������ ��� ��������� ��������");
            }

            Productions.Add(new SDTProduction(NotTerminal, Chain1, Chain2));
        }

        public void debugSDTS()
        {
            Console.WriteLine("\n");
            Console.WriteLine($"�������������� �������: {Utility.convert(V)}");
            Console.WriteLine($"������� �������: {Utility.convert(Sigma)}");
            Console.WriteLine($"�������� �������: {Utility.convert(Delta)}");
            Console.WriteLine($"��������� ������: {S0}");
            Console.WriteLine();

            for (int i = 0; i < Productions.Count; i++)
            {
                Console.WriteLine($"{i+1}. {Productions[i].LHS} --- ({Utility.convert(Productions[i].RHS)}, {Utility.convert(Productions[i].TranslateProd)})");
            }
        }

        public void TranslateByRules(List<int> RulesNumbers)
        {

            var Chain1 = new List<Symbol>() { S0 };
            var Chain2 = new List<Symbol>() { S0 };
            Console.Write("\n");
            Console.WriteLine($"����� � ������ ��-����� �� ��������: {Utility.convertInt(RulesNumbers)}");
            Console.Write($"({Utility.convert(Chain1)}, {Utility.convert(Chain2)})");

            foreach (var rule in RulesNumbers)
            {
                // ������� ����� ���������� �� ������� rule-1 � �������, ����� ������ ����� �� ����������� � ��������� ������ ����
                int real_rule = rule - 1;

                if (real_rule >= Productions.Count || real_rule < 0)
                    throw new Exception($"������������ ������� � ������� {real_rule}. ����� �� ��������");

                var NT = Productions[real_rule].LHS;
                var l = new List<Symbol>(Productions[real_rule].RHS);
                var r = new List<Symbol>(Productions[real_rule].TranslateProd);

                // ����� ������� �����������
                int index1 = -1;
                int index2 = -1;
                {
                    for (int i = 0; i < Chain1.Count; i++)
                    {
                        Symbol chain = Chain1[i];
                        if (chain.symbol == NT.symbol || chain.symbol.Split('_')[0] == NT.symbol)
                        {
                            index1 = i;
                            NT = chain;
                            break;
                        }
                    }

                    for (int i = 0; i < Chain2.Count; i++)
                    {
                        Symbol chain = Chain2[i];
                        if (chain.symbol == NT.symbol || chain.symbol.Split('_')[0] == NT.symbol)
                        {
                            index2 = i;
                            break;
                        }
                    }
                }

                Chain1.RemoveAt(index1);

                var ll = l;
                var rr = r;
                for (int i = 0; i < ll.Count; i++)
                {
                    var symbol = ll[i];
                    if (!Sigma.Contains(symbol)) { // ���� ��������
                        int max = 0;
                        foreach (var ss in Chain1)
                        {
                            if (ss.symbol.Split('_')[0] == symbol.symbol.Split('_')[0])
                                if (!V.Contains(ss))
                                    if (max < Convert.ToInt32(ss.symbol.Split('_')[1]))
                                        max = Convert.ToInt32(ss.symbol.Split('_')[1]);
                        }
                        max++;
                        Chain1.Insert(index1, new Symbol(symbol.symbol.Split('_')[0] + "_" + max.ToString()));
                        rr[rr.IndexOf(symbol)] = new Symbol(symbol.symbol.Split('_')[0] + "_" + max.ToString());

                    } else {
                        Chain1.Insert(index1, symbol);
                    }
                    index1++;
                }

                Chain2.RemoveAt(index2);
                Chain2.InsertRange(index2, rr);

                Console.Write($" => {rule}\n({Utility.convert(Chain1)}, {Utility.convert(Chain2)})");
            }

            Console.WriteLine();
        }

        private bool Recursive_descent_parser(List<Symbol> current_chain, List<Symbol> real_chain, int depth, ref List<int> answer)
        {
            // answer - ����� �������� ������� �������, ������� ���� �����
            // depth - ������������ ���������� ������, ������� �� ����� ��������� ��� ��������� ������
            // real_chain - �������, ���� ������� ����� �����
            // current_chain - �������, ���������� �� ������ ����. ���������� ����� ���������� �������

            if (depth == 0) 
            {
                // ���� ������ ������ ��������� �� �����, �� � ������� ��������, �� ����� ������� � ���������� answer
                if (Utility.IsSameArrayList(current_chain, real_chain)) 
                    return true;

                return false; // ����� ����� �������� �������� ������� ������, ���� �������� �����������, ����� ������� �� ����������� �����, ��������� ���������
            }

            
            if (Utility.IsSameArrayList(current_chain, real_chain)) // ���� �� ������ �������, �� ����� �������
            { 
                return true;
            } 
            else // ����� ���� ����������, � �������� ����� ��������� �������
            {
                var NT = new Symbol("");
                foreach (var symbol in current_chain) // ������� ������ ����������, � �������� ����� ��������� �������
                {
                    if (V.Contains(new Symbol(symbol.symbol.Split('_')[0])))
                    {
                        NT = new Symbol(symbol.symbol.Split('_')[0]);
                        break;
                    }
                }

                for (int i = 0; i < Productions.Count; i++)
                {
                    var rule = Productions[i];
                    if (rule.LHS.symbol == NT.symbol) // ��� ������� ������� �����������
                    {
                        // ��������� ���

                        var next_chain = new List<Symbol>(current_chain);
                        int index = next_chain.IndexOf(NT);
                        next_chain.RemoveAt(index);

                        var r = new List<Symbol>(rule.RHS);
                        for (int j = 0; j < r.Count; j++)
                        {
                            r[j] = new Symbol(r[j].symbol.Split('_')[0]);
                        }
                        next_chain.InsertRange(index, r);

                        if (Recursive_descent_parser(next_chain, real_chain, depth-1, ref answer))
                        {
                            answer.Add(i+1);
                            return true;
                        }
                    }
                }

                return false;
            }

        }

        public List<int> Derivation(List<Symbol> left_chain)
        {
            var s = new List<int>();

            for (int i = 2; i < 8; i++)
            {
                bool flag = Recursive_descent_parser(new List<Symbol>() { S0 }, left_chain, i, ref s);
                if (flag)
                {
                    break;
                }
                else
                {
                    s.Clear();
                }
            }

            s.Reverse();
            return s;
        }

        public void Translate(List<Symbol> left_chain)
        {
            Console.WriteLine();
            Console.Write("������� ������: ");
            Console.WriteLine(Utility.convert(left_chain));

            var s = Derivation(left_chain);

            Console.Write("Ÿ ����� �� ������� ����������: ");
            foreach (int rule in s)
            {
                Console.Write(rule);
                Console.Write(" ");
            }
            Console.WriteLine();

            TranslateByRules(s);
        }
    }
}