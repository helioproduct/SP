using System;
using System.Collections;
using System.Collections.Generic;

using RaGlib.Core;

namespace RaGlib { 
    public class myHTable
    {
        public List<List<Symbol>> table = new List<List<Symbol>>() { new List<Symbol>(), new List<Symbol>()};

        public myHTable(List<Symbol> InputSigma, List<Symbol> OutputSigma)
        {
            if (InputSigma.Count != OutputSigma.Count)
            {
                Console.WriteLine("����������� ��������� ������� (������� ����������)");
                throw new RankException();
            }

            table = new List<List<Symbol>>() { InputSigma, OutputSigma };
        }

        public void debugHTable()
        {
            var a = table[0];
            var h_a = table[1];
            for (int i = 0; i < a.Count; i++)
            {
                Console.Write(a[i]);
                Console.Write(" --- ");
                Console.WriteLine(h_a[i]);
            }
        }

        public string h(List<Symbol> input)
        {
            string output = "";

            if (input.Count == 1 && input[0].symbol == "")
                return "";

            foreach (Symbol symbol in input) // ��� ������� ������� �� �������� ������������������
            {
                var s = table[0];
                int ss = s.IndexOf(symbol);

                if (ss == -1)
                {
                    Console.WriteLine("������ �� �� ��������\n������� ������� �� ���� ���������");
                    return "";
                }

                output += (table[1])[ss].symbol; // � �������� ����� ���������� ��������������� �������� h(a)
            }

            return output;
        }
    }
}