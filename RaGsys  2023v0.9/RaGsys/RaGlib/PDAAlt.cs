using RaGlib.Automata;
using RaGlib.Core;
using System;
using System.Collections.Generic;
using static System.Formats.Asn1.AsnWriter;

namespace RaGlib
{
    // Класс для создания списка альтернативных правил 
    public class DeltaConfig
    {
        int countOfAlt;
        public List<DeltaQSigmaGamma> listOfDeltaRule;
        public DeltaConfig()
        {
            listOfDeltaRule = new List<DeltaQSigmaGamma>();
            countOfAlt = 0;
        }

        public void addRule(string LHSQ, string LHSS, string LHSZ, List<Symbol> RHSQ, List<Symbol> RHSZ)
        {
            listOfDeltaRule.Add(new DeltaQSigmaGamma(LHSQ, LHSS, LHSZ, RHSQ, RHSZ));
            countOfAlt++;
        }
    }

    // МП-автомат с альтернативами
    public class PDAAlt : PDA
    {

        public PDAAlt(
            List<Symbol> Q,
            List<Symbol> Sigma,
            List<Symbol> Gamma,
            Symbol Q0,
            Symbol z0,
            List<Symbol> F) :
            base(Q, Sigma, Gamma, Q0, z0, F)
        {

        }

        // Метод для создания дельта правил из дельта конфигурации
        public void addDeltaRule(DeltaConfig configuration)
        {
            foreach (var rule in configuration.listOfDeltaRule)
            {
                this.Delta.Add(rule);
            }
        }

        // Создать делата правило
        public override void addDeltaRule(string LHSQ, string LHSS, string LHSZ, List<Symbol> RHSQ, List<Symbol> RHSZ)
        {
            this.Delta.Add(new DeltaQSigmaGamma(LHSQ, LHSS, LHSZ, RHSQ, RHSZ));
        }

        // Найти все дельта правила с указанной левой частью
        public List<DeltaQSigmaGamma> findAllDelta(Symbol z)
        {
            var deltaList = new List<DeltaQSigmaGamma>();
            foreach (var delta in this.Delta)
                if (delta.LHSZ.symbol.Equals(z.symbol)) deltaList.Add(delta);
            return deltaList;
        }

        // Запуск разбора (В основе лежит поиск в ширину BFS)
        public override bool Execute(string str)
        {
            currState = this.Q0;
            int i = 0;
            str = str + "ε";
            DeltaQSigmaGamma delta = null;
            Queue<Tuple<DeltaQSigmaGamma, Stack<Symbol>, int>> q = new Queue<Tuple<DeltaQSigmaGamma, Stack<Symbol>, int>>();
            //  Задаем начальное состояние
            List<DeltaQSigmaGamma> start = findAllDelta(Z.Peek());
            foreach (var v in start)
            {
                q.Enqueue(new Tuple<DeltaQSigmaGamma, Stack<Symbol>, int>(v, new Stack<Symbol>(Z), i));
            }
            // Запускаем разбор
            while (q.Count != 0)
            {

                Tuple<DeltaQSigmaGamma, Stack<Symbol>, int> state = q.Dequeue();
                delta = state.Item1;
                Stack<Symbol> store = new Stack<Symbol>(state.Item2);
                int j = state.Item3;
                Console.WriteLine("Current string: " + str.Substring(j));
                Console.WriteLine(" step 1");
                delta.Debug();
                bool next = true;
                if (!delta.LHSS.symbol.Equals("ε"))
                {
                    for (; j < str.Length;)
                    {

                        if (str[j].Equals("ε")) return true;
                        Console.WriteLine(" step 2 " + str[j] + "  " + delta.LHSS);
                        if (str[j].ToString().Equals(delta.LHSS.symbol))
                        {
                            Console.WriteLine(" step 3 " + str[j]);
                            if (delta.RHSZ[0].symbol.Equals("ε"))
                                store.Pop();
                            else
                                store.Push(delta.RHSZ[0]);
                        } else
                        {
                            next = false;
                        }
                        j++;
                        if (str[j] == 'ε') return true;
                        break;
                    }
                }
                else
                {
                    store.Pop();
                    delta.RHSZ.Reverse();
                    foreach (var symbol in delta.RHSZ)
                        store.Push(symbol);
                    delta.RHSZ.Reverse();
                }
                if(next)
                {
                    List<DeltaQSigmaGamma> vertex = findAllDelta(store.Peek());
                    foreach (var v in vertex)
                    {
                        q.Enqueue(new Tuple<DeltaQSigmaGamma, Stack<Symbol>, int>(v, new Stack<Symbol>(store), j));
                    }
                } else
                {
                    Console.WriteLine("[X] Fail branch");
                }
            }
            return false;
        }
    }
}

