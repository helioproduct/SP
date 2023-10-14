using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RaGlib.Core;
using System.Text;
using System.Threading.Tasks;
using RaGlib.Grammars;
using RaGlib;
using System.Reflection;

namespace RaGlib {
    
    public class AttributeTreeWithDependencies
    {
        public List<Vertex> tree; //Упорядоченное по слоям множество элементов дерева
        public List<List<int>> links; //Ссылки между вершинами дерева
        public List<List<int>> boundaries; //Границы покрытия каждого правила
        public List<Vertex> attributes; //Упорядоченное по слоям множество атрибутов элементов дерева
        public List<List<int>> attrLinks; //Ссылки между атрибутами элементов дерева
        public List<int> owners; //Связь между элементами дерева и их атрибутами с помощью индексации, в виде owners[i] - индекс из tree, который соответствует i-ому атрибуту 
        public List<int> order; //Порядок обхода дерева, который мы получим с помощью топологической сортировки
        public List<int> numRules; //Упорядоченный список используемых для построения дерева правил
        public AttributeGrammar grammar; //Атрибутная грамматика

        //Конструктор класса
        public AttributeTreeWithDependencies(AttributeGrammar grammar, List<int> numRules)
        {
            tree = new List<Vertex>();
            links = new List<List<int>>();
            boundaries = new List<List<int>>();
            attributes = new List<Vertex>();
            attrLinks = new List<List<int>>();
            owners = new List<int>();
            order = new List<int>();
            this.numRules = numRules;
            this.grammar = grammar;
        }


        //Алгоритм построения дерева по слоям в виде упорядоченного множества
        public void BuildTree()
        {
            tree = new List<Vertex>() { new Vertex(grammar.S) };
            links = new List<List<int>>();
            boundaries = new List<List<int>>();
            int lastIndex = 0; //Левая граница поиска
            //Для каждого правила ищем символ, совпадающий с левой частью правила и поэлементно добавляем соответсвующую правую часть в дерево
            for (int i = 0; i < numRules.Count; i++)
            {
                for (int j = lastIndex; j < tree.Count; j++)
                {
                    if (grammar.R[numRules[i] - 1][0].LHS.symbol == tree[j].Symbol.symbol)
                    {
                        lastIndex = j + 1;
                        for (int z = 0; z < grammar.R[numRules[i] - 1][0].RHS.Count; z++)
                        {
                            tree.Add(new Vertex(grammar.R[numRules[i] - 1][0].RHS[z]));
                            links.Add(new List<int>() { j, tree.Count - 1 });
                            tree[j].Add(tree[tree.Count - 1]);
                        }
                        //Запись границ покрытия каждого правила, нужны для дальнейшего поиска
                        boundaries.Add(new List<int>() { j, tree.Count - 1 });
                        break;
                    }
                }
            }
        }

        //Алгоритм преобразования атрибутной грамматики в дерево зависимостей вычислений в виде упорядоченного множества
        public void BuildTreeWithDependencies()
        {
            attributes = new List<Vertex>();
            attrLinks = new List<List<int>>();
            owners = new List<int>();
            bool found; //Флаг для нахождения элемента
            int lastIndex;
            //Добавление всех атрибутов в элементы дерева
            for (int i = 0; i < tree.Count; i++)
            {
                found = false;
                for (int j = 0; j < grammar.V.Count; j++)
                {
                    if (tree[i].Symbol.symbol == grammar.V[j].symbol)
                    {
                        found = true;
                        tree[i].Symbol.attr = grammar.V[j].attr;
                        break;
                    }
                }
                if (!found)
                {
                    for (int j = 0; j < grammar.T.Count; j++)
                    {
                        if (tree[i].Symbol.symbol == grammar.T[j].symbol)
                        {
                            found = true;
                            tree[i].Symbol.attr = grammar.T[j].attr;
                            break;
                        }
                    }
                }
            }
            Symbol hA; //Вспомогательный массив
            int temp; //Вспомогательная переменная
            //Добавление всех атрибутов элементов дерева в виде упорядоченного множества в attributes,
            //а также параллельное заполнение соответствующих индексов в owners 
            for (int i = 0; i < tree.Count; i++)
            {
                hA = tree[i].Symbol;
                if (hA.attr == null)
                {
                    hA.attr = new List<Symbol>();
                }
                for (int j = 0; j < hA.attr.Count; j++)
                {
                    owners.Add(i);
                    attributes.Add(new Vertex(new Symbol(hA.symbol + "." + hA.attr[j].symbol)));
                }
            }
            //Установление атрибутных ссылок
            //Для каждого правила мы проходим по всем соответствующим семантическим правилам
            //и устанавливаем ссылки в виде пар индексов, в которых правый элемент ссылается на левый элемент пары
            for (int i = 0; i < numRules.Count; i++)
            {
                owners.Reverse();
                temp = attributes.Count - owners.IndexOf(boundaries[i][1]) - 1;// Самое правое вхождение правой границы покрытия i-го правила
                owners.Reverse();
                for (int j = 1; j < grammar.R[numRules[i] - 1].Count; j++)
                {
                    lastIndex = -1;
                    for (int k = owners.IndexOf(boundaries[i][0]); k < Math.Min(temp + 1, attributes.Count); k++)
                    {
                        if (grammar.R[numRules[i] - 1][j].LHS.symbol == attributes[k].Symbol.symbol)
                        {
                            lastIndex = k;
                            break;
                        }
                    }
                    for (int k = 0; k < grammar.R[numRules[i] - 1][j].RHS.Count; k++)
                    {
                        for (int z = Math.Min(temp, attributes.Count - 1); z >= owners.IndexOf(boundaries[i][0]); z--)
                        {
                            if (grammar.R[numRules[i] - 1][j].RHS[k].symbol == attributes[z].Symbol.symbol)
                            {
                                attributes[z].Add(new Vertex(attributes[lastIndex].Symbol));
                                attrLinks.Add(new List<int>() { z, lastIndex });
                                break;
                            }
                        }
                    }
                }
            }
            attrLinks.Sort((left, right) => left[0].CompareTo(right[0]));
        }

        //Топологическая сортировка
        //1. Выбираем атрибут, не имеющий ссылающихся на него атрибутов.
        //2. Убираем атрибут и все исходящие от него ссылки из рассмотрения.
        //3. Повторяем пока из рассмотрения не уйдут все элементы дерева

        public void TopologicalSort()
        {
            List<List<int>> copyAttrLinks = new List<List<int>>();
            order = new List<int>();
            List<bool> check = new List<bool>();
            bool found = false;
            int lastIndex;
            for (int i = 0; i < attrLinks.Count; i++)
            {
                copyAttrLinks.Add(new List<int> { attrLinks[i][0], attrLinks[i][1] });
            }
            for (int i = 0; i < attributes.Count; i++)
            {
                check.Add(false);
            }
            while (copyAttrLinks.Count > 0)
            {
                for (int i = 0; i < attributes.Count; i++)
                {
                    if (check[i])
                    {
                        continue;
                    }
                    found = false;
                    for (int j = 0; j < copyAttrLinks.Count; j++)
                    {
                        if (copyAttrLinks[j][1] == i)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        check[i] = true;
                        order.Add(i);
                        lastIndex = 0;
                        while (lastIndex < copyAttrLinks.Count)
                        {
                            if (copyAttrLinks[lastIndex][0] == i)
                            {
                                copyAttrLinks.RemoveAt(lastIndex);
                                continue;
                            }
                            lastIndex++;
                        }
                    }
                }
            }
            order.Add(0); //Самым последним всегда будет первый символ
        }

        //Вывод дерева, как упорядоченного множества его вершин
        public string PrintTree()
        {
            string result = "Упорядоченное множество вершин дерева, между которыми мы определяем зависимости:\n";
            string[] helpers = new string[2];
            string helper;
            int lastIndex;
            helpers[0] = "Вершины: ";
            helpers[1] = "Индексы: ";
            for (int i = 0; i < tree.Count; i++)
            {
                helper = tree[i].Symbol.symbol + " ";
                lastIndex = helper.Length;
                helpers[0] += helper;
                helper = i.ToString();
                while (helper.Length < lastIndex)
                {
                    helper += " ";
                }
                helpers[1] += helper;
            }
            result += helpers[0] + "\n";
            result += helpers[1] + "\n";
            return result;
        }
        //Преобразование ссылок между атрибутами в ссылки между вершинами дерева
        public string PrintConvertedLinks()
        {
            string result = "Зависимости между вершинами дерева:\n";
            for (int i = 0; i < attrLinks.Count; i++)
            {
                result += tree[owners[attrLinks[i][0]]].Symbol.symbol + " -> " + tree[owners[attrLinks[i][1]]].Symbol.symbol + " (" + owners[attrLinks[i][0]].ToString() + " -> " + owners[attrLinks[i][1]] + ")\n";
            }
            return result;
        }

        //Вывод атрибутов с индексами
        public string PrintAttributesWithIndexes()
        {
            string result = "Упорядоченное множество атрибутов, между которыми мы определяем зависимости:\n";
            string[] helpers = new string[2];
            helpers[0] = "Атрибуты: ";
            helpers[1] = "Индексы:  ";
            string helper;
            int lastIndex;
            for (int i = 0; i < attributes.Count; i++)
            {
                helper = attributes[i].Symbol.symbol + " ";
                lastIndex = helper.Length;
                helpers[0] += helper;
                helper = i.ToString();
                while (helper.Length < lastIndex)
                {
                    helper += " ";
                }
                helpers[1] += helper;
            }
            result += helpers[0] + "\n";
            result += helpers[1] + "\n";
            return result;
        }

        //Вывод атрибутных ссылок
        public string PrintAttrLinks()
        {
            int lastIndex = 0;
            string result = "Зависимости между атрибутами:\n";
            for (int i = 0; i < attributes.Count; i++)
            {
                foreach (Vertex v in attributes[i].Next)
                {
                    result += attributes[i].Symbol.symbol + " -> " + v.Symbol.symbol + " (" + attrLinks[lastIndex][0].ToString() + " -> " + attrLinks[lastIndex][1].ToString() + ")\n";
                    lastIndex++;
                }
            }
            return result;
        }

        //Вывод порядка обхода, полученного при помощи топологической сортировки
        public string PrintOrder()
        {
            string result = "Порядок обхода дерева зависимостей, полученный с помощью топологической сортировки: ";
            for (int i = 0; i < order.Count - 1; i++)
            {
                result += order[i].ToString() + ", ";
            }
            result += order[order.Count - 1];
            result += "\n";
            return result;
        }

        public string PrintTreeASCII()
        {
            string result = "Графический вывод дерева:\n";
            string helper;
            string helper2;
            string helper3;
            List<int> treeLevel = new List<int>() {0};
            List<int> curPos;
            List<int> lastPos;
            List<int> curLevel;
            List<int> lastLevel;
            List<int> parents = new List<int>() {-1};
            int maxLevel = 0;
            int maxStringLength = tree.Count - 1;
            int levelLength;
            for(int i = 0; i < tree.Count; i++)
            {
                maxStringLength += tree[i].Symbol.symbol.Length;
                foreach(Vertex v in tree[i].Next)
                {
                    treeLevel.Add(treeLevel[i] + 1);
                    parents.Add(i);
                    maxLevel = Math.Max(maxLevel, treeLevel[i] + 1);
                }
            }
            //Построение первого уровня
            maxStringLength = ((maxStringLength + tree[0].Symbol.symbol.Length) % 2 == 0) ? maxStringLength : maxStringLength + 1;
            helper = new string(' ', (int)(maxStringLength / 2) - (int)(tree[0].Symbol.symbol.Length / 2));
            lastPos = new List<int>() {helper.Length};
            lastLevel = new List<int>() {0};
            helper += tree[0].Symbol.symbol;
            helper += new string(' ', maxStringLength - helper.Length);
            result += helper + "\n";
            for(int i = 1; i <= maxLevel; i++)
            {
                levelLength = 0;
                curLevel = new List<int>();
                curPos = new List<int>();
                helper = "";
                //Находим все элементы текущего уровня
                for(int j = 0; j < treeLevel.Count; j++)
                {
                    if (treeLevel[j] == i)
                    {
                        curLevel.Add(j);
                        levelLength += tree[j].Symbol.symbol.Length;
                    }
                }
                //Составляем вывод следующего уровня
                helper += new string(' ', (int)((maxStringLength - levelLength) / (curLevel.Count + 1)));
                for (int j = 0; j < curLevel.Count; j++)
                {
                    curPos.Add(helper.Length);
                    helper += tree[curLevel[j]].Symbol.symbol;
                    helper += new string(' ', (int)((maxStringLength - levelLength) / (curLevel.Count + 1)));
                }
                //Соединяем следующий уровень с предыдущим
                helper2 = new string(' ', maxStringLength);
                helper3 = new string(' ', maxStringLength);
                int left, right;
                for(int j = 0; j < curLevel.Count; j++)
                {
                    left = Math.Min(curPos[j], lastPos[lastLevel.IndexOf(parents[curLevel[j]])]);
                    right = Math.Max(curPos[j], lastPos[lastLevel.IndexOf(parents[curLevel[j]])]);
                    if(left == right)
                    {
                        helper2 = new StringBuilder(helper2) { [left] = '|' }.ToString();
                        helper3 = new StringBuilder(helper3) { [left] = '|' }.ToString();
                    }
                    else if(left == curPos[j])
                    {
                        helper2 = new StringBuilder(helper2) { [left] = '/' }.ToString();
                        for (int z = left + 1; z < right - 1; z++)
                        {
                            helper3 = new StringBuilder(helper3) { [z] = '_' }.ToString();
                        }
                        helper3 = new StringBuilder(helper3) { [right - 1] = '/' }.ToString();
                    }
                    else if(right == curPos[j])
                    {
                        helper3 = new StringBuilder(helper3) { [left + 1] = (char)92 }.ToString(); // (char)92 = '\'
                        for (int z = left + 2; z < right; z++)
                        {
                            helper3 = new StringBuilder(helper3) { [z] = '_' }.ToString();
                        }
                        helper2 = new StringBuilder(helper2) { [right] = (char)92 }.ToString();
                    }
                }
                lastLevel = curLevel;
                lastPos = curPos;
                result += helper3 + "\n";
                result += helper2 + "\n";
                result += helper + "\n";
            }
            return result;
        }
    }
}