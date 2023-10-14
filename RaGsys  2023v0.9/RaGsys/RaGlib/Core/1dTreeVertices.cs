using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaGlib.Grammars;
using SDT.Types;

namespace RaGlib.Core
{
    public class _1dTreeVertices
    {
        public Vertex root;
        public List<Vertex> _1dtreevertices; //Упорядоченное по слоям множество элементов дерева
        public List<List<int>> links; //Ссылки между вершинами дерева
        public List<List<int>> boundaries; //Границы покрытия каждого правила
        public List<Vertex> attributes; //Упорядоченное по слоям множество атрибутов элементов дерева
        public List<List<int>> attrLinks; //Ссылки между атрибутами элементов дерева
        public List<int> owners; //Связь между элементами дерева и их атрибутами с помощью индексации, в виде owners[i] - индекс из _1dtreevertices, который соответствует i-ому атрибуту 
        public List<int> order; //Порядок обхода дерева, который мы получим с помощью топологической сортировки
        public List<int> numRules; //Упорядоченный список используемых для построения дерева правил
        public AttributeGrammar grammar; //Атрибутная грамматика
        public ATGrammar gr;
        public List<Symbol> VERTEXS;
        public Symbol[,] LINKS;

        public _1dTreeVertices(Symbol rootSymbol)
        {
            this.root = new Vertex(rootSymbol, false);
            _1dtreevertices = new List<Vertex>();
            _1dtreevertices.Add(this.root);
        }

        //Конструктор класса
        public _1dTreeVertices(AttributeGrammar grammar, List<int> numRules)
        {
            _1dtreevertices = new List<Vertex>();
            links = new List<List<int>>();
            boundaries = new List<List<int>>();
            attributes = new List<Vertex>();
            attrLinks = new List<List<int>>();
            owners = new List<int>();
            order = new List<int>();
            this.numRules = numRules;
            this.grammar = grammar;
        }

        public _1dTreeVertices(ATGrammar gr, List<Symbol> VERTEXS, Symbol[,] LINKS)
        {
            this.gr = gr;
            this.VERTEXS = VERTEXS;
            this.LINKS = LINKS;
        }
        public void AddNodeToTree(Symbol LHS_Symbol, List<Symbol> RHS_Symbols)
        {
            Vertex parentNode = root.FindFirst(root, LHS_Symbol.symbol);
            foreach (Symbol rhs_symbol in RHS_Symbols)
            {
                Vertex currentNode = parentNode.Add(rhs_symbol, rhs_symbol is Symbol_Operation);
                _1dtreevertices.Add(currentNode);
            }
        }
        public void PrintTree()
        {
            root.Print();
        }
        public void PrintTreeArray()
        {
            foreach (var x in _1dtreevertices)
            {
                Console.Write(x.Symbol.ToString() + " ");
            }
            Console.WriteLine();
        }

        //Алгоритм построения дерева по слоям в виде упорядоченного множества
        public void BuildTree()
        {
            _1dtreevertices = new List<Vertex>() { new Vertex(grammar.S) };
            links = new List<List<int>>();
            boundaries = new List<List<int>>();
            int lastIndex = 0; //Левая граница поиска
            //Для каждого правила ищем символ, совпадающий с левой частью правила и поэлементно добавляем соответсвующую правую часть в дерево
            for (int i = 0; i < numRules.Count; i++)
            {
                for (int j = lastIndex; j < _1dtreevertices.Count; j++)
                {
                    if (grammar.R[numRules[i] - 1][0].LHS.symbol == _1dtreevertices[j].Symbol.symbol)
                    {
                        lastIndex = j + 1;
                        for (int z = 0; z < grammar.R[numRules[i] - 1][0].RHS.Count; z++)
                        {
                            _1dtreevertices.Add(new Vertex(grammar.R[numRules[i] - 1][0].RHS[z]));
                            links.Add(new List<int>() { j, _1dtreevertices.Count - 1 });
                            _1dtreevertices[j].Add(_1dtreevertices[_1dtreevertices.Count - 1]);
                        }
                        //Запись границ покрытия каждого правила, нужны для дальнейшего поиска
                        boundaries.Add(new List<int>() { j, _1dtreevertices.Count - 1 });
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
            for (int i = 0; i < _1dtreevertices.Count; i++)
            {
                found = false;
                for (int j = 0; j < grammar.V.Count; j++)
                {
                    if (_1dtreevertices[i].Symbol.symbol == grammar.V[j].symbol)
                    {
                        found = true;
                        _1dtreevertices[i].Symbol.attr = grammar.V[j].attr;
                        break;
                    }
                }
                if (!found)
                {
                    for (int j = 0; j < grammar.T.Count; j++)
                    {
                        if (_1dtreevertices[i].Symbol.symbol == grammar.T[j].symbol)
                        {
                            found = true;
                            _1dtreevertices[i].Symbol.attr = grammar.T[j].attr;
                            break;
                        }
                    }
                }
            }
            Symbol hA; //Вспомогательный массив
            int temp; //Вспомогательная переменная
            //Добавление всех атрибутов элементов дерева в виде упорядоченного множества в attributes,
            //а также параллельное заполнение соответствующих индексов в owners 
            for (int i = 0; i < _1dtreevertices.Count; i++)
            {
                hA = _1dtreevertices[i].Symbol;
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

        // Вывод всех унаследованных атрибутов в дереве
        public string PrintAllInheritedAttributes()
        {
            string result = "Список унаследованных атрибутов с индексами в дереве:\n";
            string attr = "Атрибут: ";
            string indexes = "Индекс:  ";
            // Проверяем все атрибуты слева направо
            for (int i = 0; i < attributes.Count; ++i)
            {
                // Ищем от какого атрибута зависит наш атрибут
                int dependency_index = -1;
                foreach (var dependency in attrLinks)
                {
                    if (dependency[1] == i)
                    {
                        dependency_index = dependency[0];
                        break;
                    }
                }
                // атрибут не от кого не зависит
                if (dependency_index == -1)
                {
                    continue;
                }

                int attr_tree_index = owners[i];
                int dep_tree_index = owners[dependency_index];

                // Ищем их родителя в дереве
                int attr_tree_parent = -1;
                int dep_tree_parent = -1;
                foreach (var link in links)
                {
                    if (link[1] == attr_tree_index)
                    {
                        attr_tree_parent = link[0];
                        break;
                    }
                }
                foreach (var link in links)
                {
                    if (link[1] == dep_tree_index)
                    {
                        dep_tree_parent = link[0];
                        break;
                    }
                }

                // pattern 1: атрибут наследуется между разными нетерминальными символами правой части
                if (attr_tree_parent == dep_tree_parent && attr_tree_parent != -1)
                {
                    // проверка есть ли правило наследования
                    int rule_index = -1;
                    for (var j = 0; j < grammar.R.Count; ++j)
                    {
                        if (grammar.R[j][0].LHS == _1dtreevertices[attr_tree_parent].Symbol)
                        {
                            rule_index = j;
                            for (var k = 1; k < grammar.R[rule_index].Count; ++k)
                            {
                                if (grammar.R[j][k].LHS == attributes[i].Symbol)
                                {
                                    // Нашли правило
                                    attr += attributes[i].Symbol + " ";
                                    indexes += i.ToString();
                                    while (indexes.Length < attr.Length)
                                    {
                                        indexes += " ";
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
                else if (dep_tree_index == attr_tree_parent) // pattern 2: атрибут наследуется из левой части правила в правую 
                {
                    attr += attributes[i].Symbol + " ";
                    indexes += i.ToString();
                    while (indexes.Length < attr.Length)
                    {
                        indexes += " ";
                    }
                }
            }
            return result + attr + "\n" + indexes;
        }

        // Вывод всех синтезированных атрибутов в дереве
        public string PrintAllSynthesizedAttributes()
        {
            string result = "Список синтезированных атрибутов с индексами в дереве:\n";
            string attr = "Атрибут: ";
            string indexes = "Индекс:  ";
            // Проверяем все атрибуты справа налево
            for (int i = attributes.Count - 1; i > -1; --i)
            {
                // Ищем от какого атрибута зависит наш атрибут               
                int dependency_idx = -1;
                foreach (var dependency in attrLinks)
                {
                    if (dependency[1] == i)
                    {
                        dependency_idx = dependency[0]; break;
                    }
                }
                // pattern 1: Если атрибут ни от кого не зависит => он является синтезированным               
                if (dependency_idx == -1)
                {
                    attr += attributes[i].Symbol + " ";
                    indexes += i.ToString();
                    while (indexes.Length < attr.Length)
                    {
                        indexes += " ";
                    }
                    continue;
                }
                int dependent_tree_idx = owners[i];
                // pattern 2: атрибут наследуется из правой части правила в левую или между атрибутами левой части
                for (var j = 0; j < grammar.R.Count; ++j)
                {
                    // Проверка есть ли правило наследования                   
                    if (grammar.R[j][0].LHS == _1dtreevertices[dependent_tree_idx].Symbol)
                    {
                        for (var k = 1; k < grammar.R[j].Count; ++k)
                        {
                            if (grammar.R[j][k].LHS == attributes[i].Symbol)
                            {
                                for (var l = 0; l < grammar.R[j][k].RHS.Count; ++l)
                                {
                                    if (grammar.R[j][k].RHS[l] == attributes[dependency_idx].Symbol)
                                    {
                                        attr += attributes[i].Symbol + " ";
                                        indexes += i.ToString();
                                        while (indexes.Length < attr.Length)
                                        {
                                            indexes += " ";
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
            return result + attr + "\n" + indexes;
        }

        public string PrintAllAttributes()
        {
            string result1 = "Список синтезированных атрибутов с индексами в дереве:\n";
            string attr1 = "Атрибут: ";
            string indexes1 = "Индекс:  ";

            string result2 = "Список унаследованных атрибутов с индексами в дереве:\n";
            string attr2 = "Атрибут: ";
            string indexes2 = "Индекс:  ";
            // Проверяем все атрибуты слева направо
            for (int i = 0; i < attributes.Count; ++i)
            {
                // Ищем от какого атрибута зависит наш атрибут
                int dependency_index = -1;
                foreach (var dependency in attrLinks)
                {
                    if (dependency[1] == i)
                    {
                        dependency_index = dependency[0];
                        break;
                    }
                }

                int dependency_idx = -1;
                foreach (var dependency in attrLinks)
                {
                    if (dependency[1] == i)
                    {
                        dependency_idx = dependency[0]; break;
                    }
                }
                // pattern 1: Если атрибут ни от кого не зависит => он является синтезированным               
                if (dependency_idx == -1)
                {
                    attr1 += attributes[i].Symbol + " ";
                    indexes1 += i.ToString();
                    while (indexes1.Length < attr1.Length)
                    {
                        indexes1 += " ";
                    }
                    continue;
                }

                int attr_tree_index = owners[i];
                int dep_tree_index = owners[dependency_index];

                // Ищем их родителя в дереве
                int attr_tree_parent = -1;
                int dep_tree_parent = -1;
                foreach (var link in links)
                {
                    if (link[1] == attr_tree_index)
                    {
                        attr_tree_parent = link[0];
                        break;
                    }
                }
                foreach (var link in links)
                {
                    if (link[1] == dep_tree_index)
                    {
                        dep_tree_parent = link[0];
                        break;
                    }
                }

                // pattern 1: атрибут наследуется между разными нетерминальными символами правой части
                if (attr_tree_parent == dep_tree_parent && attr_tree_parent != -1)
                {
                    // проверка есть ли правило наследования
                    int rule_index = -1;
                    for (var j = 0; j < grammar.R.Count; ++j)
                    {
                        if (grammar.R[j][0].LHS == _1dtreevertices[attr_tree_parent].Symbol)
                        {
                            rule_index = j;
                            for (var k = 1; k < grammar.R[rule_index].Count; ++k)
                            {
                                if (grammar.R[j][k].LHS == attributes[i].Symbol)
                                {
                                    // Нашли правило
                                    attr2 += attributes[i].Symbol + " ";
                                    indexes2 += i.ToString();
                                    while (indexes2.Length < attr2.Length)
                                    {
                                        indexes2 += " ";
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
                else if (dep_tree_index == attr_tree_parent) // pattern 2: атрибут наследуется из левой части правила в правую 
                {
                    attr2 += attributes[i].Symbol + " ";
                    indexes2 += i.ToString();
                    while (indexes2.Length < attr2.Length)
                    {
                        indexes2 += " ";
                    }
                }

                
                int dependent_tree_idx = owners[i];
                // pattern 2: атрибут наследуется из правой части правила в левую или между атрибутами левой части
                for (var j = 0; j < grammar.R.Count; ++j)
                {
                    // Проверка есть ли правило наследования                   
                    if (grammar.R[j][0].LHS == _1dtreevertices[dependent_tree_idx].Symbol)
                    {
                        for (var k = 1; k < grammar.R[j].Count; ++k)
                        {
                            if (grammar.R[j][k].LHS == attributes[i].Symbol)
                            {
                                for (var l = 0; l < grammar.R[j][k].RHS.Count; ++l)
                                {
                                    if (grammar.R[j][k].RHS[l] == attributes[dependency_idx].Symbol)
                                    {
                                        attr1 += attributes[i].Symbol + " ";
                                        indexes1 += i.ToString();
                                        while (indexes1.Length < attr1.Length)
                                        {
                                            indexes1 += " ";
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
            return result1 + attr1 + "\n" + indexes1 +"\n\n" + result2 + attr2 + "\n" + indexes2;
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
        public string Print_1dtreevertices()
        {
            string result = "Упорядоченное множество вершин дерева, между которыми мы определяем зависимости:\n";
            string[] helpers = new string[2];
            string helper;
            int lastIndex;
            helpers[0] = "Вершины: ";
            helpers[1] = "Индексы: ";
            for (int i = 0; i < _1dtreevertices.Count; i++)
            {
                helper = _1dtreevertices[i].Symbol.symbol + " ";
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
                result += _1dtreevertices[owners[attrLinks[i][0]]].Symbol.symbol + " -> " + _1dtreevertices[owners[attrLinks[i][1]]].Symbol.symbol + " (" + owners[attrLinks[i][0]].ToString() + " -> " + owners[attrLinks[i][1]] + ")\n";
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
            List<int> treeLevel = new List<int>() { 0 };
            List<int> curPos;
            List<int> lastPos;
            List<int> curLevel;
            List<int> lastLevel;
            List<int> parents = new List<int>() { -1 };
            int maxLevel = 0;
            int maxStringLength = _1dtreevertices.Count - 1;
            int levelLength;
            for (int i = 0; i < _1dtreevertices.Count; i++)
            {
                maxStringLength += _1dtreevertices[i].Symbol.symbol.Length;
                foreach (Vertex v in _1dtreevertices[i].Next)
                {
                    treeLevel.Add(treeLevel[i] + 1);
                    parents.Add(i);
                    maxLevel = Math.Max(maxLevel, treeLevel[i] + 1);
                }
            }
            //Построение первого уровня
            maxStringLength = ((maxStringLength + _1dtreevertices[0].Symbol.symbol.Length) % 2 == 0) ? maxStringLength : maxStringLength + 1;
            helper = new string(' ', (int)(maxStringLength / 2) - (int)(_1dtreevertices[0].Symbol.symbol.Length / 2));
            lastPos = new List<int>() { helper.Length };
            lastLevel = new List<int>() { 0 };
            helper += _1dtreevertices[0].Symbol.symbol;
            helper += new string(' ', maxStringLength - helper.Length);
            result += helper + "\n";
            for (int i = 1; i <= maxLevel; i++)
            {
                levelLength = 0;
                curLevel = new List<int>();
                curPos = new List<int>();
                helper = "";
                //Находим все элементы текущего уровня
                for (int j = 0; j < treeLevel.Count; j++)
                {
                    if (treeLevel[j] == i)
                    {
                        curLevel.Add(j);
                        levelLength += _1dtreevertices[j].Symbol.symbol.Length;
                    }
                }
                //Составляем вывод следующего уровня
                helper += new string(' ', (int)((maxStringLength - levelLength) / (curLevel.Count + 1)));
                for (int j = 0; j < curLevel.Count; j++)
                {
                    curPos.Add(helper.Length);
                    helper += _1dtreevertices[curLevel[j]].Symbol.symbol;
                    helper += new string(' ', (int)((maxStringLength - levelLength) / (curLevel.Count + 1)));
                }
                //Соединяем следующий уровень с предыдущим
                helper2 = new string(' ', maxStringLength);
                helper3 = new string(' ', maxStringLength);
                int left, right;
                for (int j = 0; j < curLevel.Count; j++)
                {
                    left = Math.Min(curPos[j], lastPos[lastLevel.IndexOf(parents[curLevel[j]])]);
                    right = Math.Max(curPos[j], lastPos[lastLevel.IndexOf(parents[curLevel[j]])]);
                    if (left == right)
                    {
                        helper2 = new StringBuilder(helper2) { [left] = '|' }.ToString();
                        helper3 = new StringBuilder(helper3) { [left] = '|' }.ToString();
                    }
                    else if (left == curPos[j])
                    {
                        helper2 = new StringBuilder(helper2) { [left] = '/' }.ToString();
                        for (int z = left + 1; z < right - 1; z++)
                        {
                            helper3 = new StringBuilder(helper3) { [z] = '_' }.ToString();
                        }
                        helper3 = new StringBuilder(helper3) { [right - 1] = '/' }.ToString();
                    }
                    else if (right == curPos[j])
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

        public void Calculating()
        {
            List<Symbol> INHERIT_VERTEXS = new List<Symbol>();



            foreach (var vertex in VERTEXS)
            {
                int i = 0;
                int idx = 0;
                int rows = LINKS.GetUpperBound(0) + 1;
                for (int j = 0; j < rows; j++)
                {
                    if (LINKS[j, 1].attr == vertex.attr)
                    {
                        i++;
                        idx = j;
                        INHERIT_VERTEXS.AddRange(LINKS[j, 0].attr);
                    }
                }
                if (i > 1) //inherit attribute
                {
                    foreach (AttrProduction rule in gr.Rules)
                    {
                        if (rule.LHS.symbol == vertex.symbol)
                        {
                            vertex.attr.Clear();
                            if (rule.RHS[1] == "+")
                            {
                                vertex.attr.AddRange(new List<Symbol>() { (Int32.Parse(INHERIT_VERTEXS[0].ToString()) + Int32.Parse(INHERIT_VERTEXS[1].ToString())).ToString() });
                            }
                            else if (rule.RHS[1] == "*")
                            {
                                vertex.attr.AddRange(new List<Symbol>() { (Int32.Parse(INHERIT_VERTEXS[0].ToString()) * Int32.Parse(INHERIT_VERTEXS[1].ToString())).ToString() });
                            }
                        }
                    }
                    INHERIT_VERTEXS.Clear();
                }
                else if (i == 1) //syntesized attribute
                {
                    vertex.attr = LINKS[idx, 0].attr;
                }
            }
        }

        public void PrintVertexs()
        {
            for (int i = 0; i < VERTEXS.Count(); i++)
            {
                Console.WriteLine("{0} {1} {2}\n", i, VERTEXS[i].symbol, VERTEXS[i].attr[0]);
            }
        }

    }

}