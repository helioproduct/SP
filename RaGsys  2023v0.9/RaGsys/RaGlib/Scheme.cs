using System;
using System.Collections.Generic;
using System.Linq;
using RaGlib.Core;
using RaGlib.Grammars;
using System.IO;
using System.Xml.Linq;
using System.Collections;

namespace RaGlib { 
public class Scheme
{
    public class SProduction
    {
        public Symbol LHS;
        public List<Symbol> RHS_IN;
        public List<Symbol> RHS_OUT;
        public static int Count = 0; 
        public int Id; ///< Production number

        public SProduction(Symbol LHS, List<Symbol> RHS_IN, List<Symbol> RHS_OUT)
        {
            this.LHS = LHS;
            this.RHS_IN = RHS_IN;
            this.RHS_OUT = RHS_OUT;
        }

        public void print() {
            LHS.print();
            Console.Write(" -> ");
            for (int i = 0; i < RHS_IN.Count; ++i)
            {
                RHS_IN[i].print();
            }
            Console.Write(",");
            for (int i = 0; i < RHS_OUT.Count; ++i)
            {
                RHS_OUT[i].print();
            }
            Console.Write("\n");
        }
    }

    public List<Symbol> V; // Множество нетерминалов
    public List<Symbol> Sigma; // Множество терминалов у входной грамматики
    public List<Symbol> Delta; // Множество терминалов у выходной грамматики
    public List<SProduction> Productions;
    public Symbol S0;

    public Grammar GrammarInput; // Входная грамматика
    public Grammar GrammarOutput; // Выходная грамматика

    public Scheme(List<Symbol> _V, List<Symbol> _Sigma, List<Symbol> _Delta, Symbol S)
    {
        V = _V;
        Sigma = _Sigma;
        Delta = _Delta;
        S0 = S;
        Productions = new List<SProduction>();
    }

    public bool Equal(List<Symbol> A, List<Symbol> B) {
        if (A.Count != B.Count) {
            return false;
        }
        for (int i = 0; i < A.Count; i++) {
            int f = 0;
            for (int j = 0; j < B.Count; j++) {
                if (A[i].symbol == B[j].symbol) {
                    f = 1;
                    break;
                }
            }
            if (f != 1) {
                return false;
            }
        }
        for (int i = 0; i < B.Count; i++) {
            int f = 0;
            for (int j = 0; j < A.Count; j++) {
                if (B[i].symbol == A[j].symbol) {
                    f = 1;
                    break;
                }
            }
            if (f == 0) {
                return false;
            }
        }
        return true;
    }

    public bool In(Symbol s, List<Symbol> A) {
        for (int i = 0; i < A.Count(); i++) {
            if (s == A[i]) {
                return true;
            }
        }
        return false;
    }

    // Создание схемы по входной и выходной грамматикам
    public Scheme(Grammar _GrammarInput, Grammar _GrammarOutput)
    {
        if(_GrammarInput.P.Count != _GrammarOutput.P.Count || _GrammarInput.S0 != _GrammarOutput.S0 || !Equal(_GrammarInput.V, _GrammarOutput.V)) {
            throw new Exception("На вход СУ-схеме подали некорректные грамматики");
        }
        GrammarInput = _GrammarInput;
        GrammarOutput = _GrammarOutput;
        V = GrammarInput.V;
        Sigma = GrammarInput.T;
        Delta = GrammarOutput.T;

        S0 = GrammarInput.S0;
        Productions = new List<SProduction>();

        for(int i = 0; i < GrammarInput.P.Count; ++i) 
        {
            if(GrammarInput.P[i].LHS != GrammarOutput.P[i].LHS)
            {
                throw new Exception("На вход СУ-схеме подали некорректные грамматики");
            }
            
            List<Symbol> A, B;
            A = new List<Symbol>();
            B = new List<Symbol>();
            for (int j = 0; j < GrammarInput.P[i].RHS.Count; j++) {
                if ( In(GrammarInput.P[i].RHS[j], V) ) {
                    if (In(GrammarInput.P[i].RHS[j], A)) {
                        throw new Exception("На вход СУ-схеме подали некорректные грамматики");
                    }
                    A.Add(GrammarInput.P[i].RHS[j]);
                }
            }
            for (int j = 0; j < GrammarOutput.P[i].RHS.Count; j++) {
                if ( In(GrammarOutput.P[i].RHS[j], V) ) {
                    if (In(GrammarOutput.P[i].RHS[j], B)) {
                        throw new Exception("На вход СУ-схеме подали некорректные грамматики");
                    }
                    B.Add(GrammarOutput.P[i].RHS[j]);
                }
            }
            if ( !Equal(A, B) ) {
                throw new Exception("На вход СУ-схеме подали некорректные грамматики");
            }

            Productions.Add( new SProduction(GrammarInput.P[i].LHS, GrammarInput.P[i].RHS, GrammarOutput.P[i].RHS) );

        }
    }

    public void Print()
    {

        //печать правил грамматики
        if (Productions.Count != 0)
        {
            Console.Write("\nGrammar Rules:\n");
            for (int i = 0; i < Productions.Count; ++i)
            {
                Productions[i].print();
            }
        }
    }

        public _1dTreeVertices LeftInfrence1dTreeVertices;   //Дерево вывода входной грамматики D
        public _1dTreeVertices RightInfrence1dTreeVertices;  //Дерево вывода преобразованное в выходную грамматику D'
        public List<Int32> NUMBER_INFERENCE_RULES;
    public void InputRules() {
        Console.WriteLine("Введите цепочку для входной грамматики, ввод через пробелы, пример 1 2 3 3 2 3 4:"); //1 2 1 3 4 4 3
        var input = Console.ReadLine();
        var words = input.Split(" ");
        NUMBER_INFERENCE_RULES = new List<Int32>();
        foreach (var word in words)
        {
            var pos = Convert.ToInt32(word);
            NUMBER_INFERENCE_RULES.Add(pos);

        }
    }
        public void BuildInputTree()
        {
            var index_rules = 0;
            var index_tree = 0;
            LeftInfrence1dTreeVertices = new _1dTreeVertices(Productions[NUMBER_INFERENCE_RULES[index_rules] - 1].LHS);
            while (true)
            {
                var node = LeftInfrence1dTreeVertices._1dtreevertices[index_tree];
                if (Productions[NUMBER_INFERENCE_RULES[index_rules] - 1].LHS == node.Symbol)
                {
                    LeftInfrence1dTreeVertices.AddNodeToTree(Productions[NUMBER_INFERENCE_RULES[index_rules] - 1].LHS, Productions[NUMBER_INFERENCE_RULES[index_rules] - 1].RHS_IN);
                    index_rules++;
                }
                index_tree++;
                if (index_rules > NUMBER_INFERENCE_RULES.Count - 1)
                {
                    break;
                }
            }
        }
        public void PrintTree12() 
        {
            LeftInfrence1dTreeVertices.PrintTree();
        }
        public void PrintTreeStack(_1dTreeVertices Tree) 
        {
        var SchemeTree = Tree._1dtreevertices;
        string s1 = "";
        string s2 = "";
        var i = 0;
        while (i!=SchemeTree.Count)
        {
            var childs_k =1;
            if (SchemeTree[i].Next.Count>1)
            {
                childs_k =SchemeTree[i].Next.Count;
            }
            s1+= new string(' ',childs_k-1) + SchemeTree[i].Symbol.symbol+ new string(' ', childs_k-1);
            var j =0;
            while(j != SchemeTree[i].Next.Count)
            {
                s2= s2 + SchemeTree[i].Next.ElementAt(j).Symbol.symbol + " ";
                j++;
            }
            if (SchemeTree[i].Next.Count ==0)
            {
                s2+="  ";
            }
            s1=s1 + "|";
            s2=s2.Substring(0,s2.Length-1) + "|";
            i++;
        }
        Console.WriteLine(s1);
        Console.WriteLine(s2);
    }
    private int Count_Terms(Vertex position) 
    {
        if (position.Next.Count==0)
        {
            return 1;
        } else {
            var j =0;
            var count=0;
            while(j != position.Next.Count) {
                count+=Count_Terms(position.Next.ElementAt(j));
                j++;
            }
            return count;
        }
    }
    public void PrintTree(_1dTreeVertices Tree) 
    {
        var OutputTree = Tree._1dtreevertices;
        var CurLevel = 1;
        var NextLevel = 0;
        var i = 0;
        var last_empty = 1;
        var pos=new List<int> {};
        var pos_iter=0;
        string s1= "";
        string s2= "";
        bool Flag = false;
        while (i!=OutputTree.Count)
        {
            bool flag_no_print = false;
            s1+=new string(' ', Count_Terms(OutputTree[i])-1);
            if (OutputTree[i].Next.Count==0) {
                    var ver_new = new Vertex(new Symbol(OutputTree[i].Symbol.symbol), false);
                OutputTree.Insert(last_empty, ver_new);
                OutputTree[i].Add(ver_new);
                flag_no_print = true;
                pos.Add(s1.Length);
            } else {
                foreach (var child in OutputTree[i].Next)
                {
                    pos.Add(s1.Length);
                }
            }
            if (i!=0 && OutputTree[i].Symbol.symbol!=" "){
                if (s1.Length ==pos[pos_iter]){
                    s1+="|";
                } else {
                    if (s1.Length <pos[pos_iter]){
                        s1+="/";
                    } else {
                        s1+="\\";
                    }
                }
                pos_iter++;
            } else if (OutputTree[i].Symbol.symbol==" "){
                s1+=" ";
            }
            
            s1+= new string(' ', Count_Terms(OutputTree[i]));
            if (flag_no_print && Flag==false){
                s2+= new string(' ', Count_Terms(OutputTree[i])-1) + "|" + new string(' ', Count_Terms(OutputTree[i]));
            } else {
                s2+= new string(' ', Count_Terms(OutputTree[i])-1) + OutputTree[i].Symbol.symbol + new string(' ', Count_Terms(OutputTree[i]));
            }
            last_empty+=OutputTree[i].Next.Count;
            NextLevel+=OutputTree[i].Next.Count;
            CurLevel--;
            if (CurLevel==0)
            {
                CurLevel=NextLevel;
                NextLevel=0;
                Console.WriteLine(s1);
                s1="";
                Console.WriteLine(s2);
                s2="";
                var j = OutputTree.Count - Count_Terms(OutputTree[0]);
                if (Flag)
                {
                    return;
                }
                Flag = true;
                while(j!=OutputTree.Count){
                    if (OutputTree[j].Next.Count!=0)
                    {
                        Flag=false;
                    }
                    j++;
                }
            }
            i++;
        }
    }
    public _1dTreeVertices TransformTree;
    public void BuildCopyTree() 
        {
            var index_rules = 0;
            var index_tree = 0;
            TransformTree = new _1dTreeVertices(Productions[NUMBER_INFERENCE_RULES[index_rules] - 1].LHS);
            while (true)
            {
                var node = TransformTree._1dtreevertices[index_tree];
                if (Productions[NUMBER_INFERENCE_RULES[index_rules] - 1].LHS == node.Symbol)
                {
                    TransformTree.AddNodeToTree(Productions[NUMBER_INFERENCE_RULES[index_rules] - 1].LHS, Productions[NUMBER_INFERENCE_RULES[index_rules] - 1].RHS_IN);
                    index_rules++;
                }
                index_tree++;
                if (index_rules > NUMBER_INFERENCE_RULES.Count - 1)
                {
                    break;
                }
            }
        }
 public void Transform(){
        BuildCopyTree(); // строится копия
        var Transform1dTree = TransformTree._1dtreevertices;
            var pos = 0;
        foreach (var node in Transform1dTree)
            {
                node.pos = pos;
                pos++;
            }
        var index_rules = 0;
        while (true) // обход идет по NUMBER_INFERENCE_RULES
        {
            int count = 0;
            for(int u = 0; u <= index_rules; u++)
            {
                if (Productions[NUMBER_INFERENCE_RULES[u]-1].LHS == Productions[NUMBER_INFERENCE_RULES[index_rules]-1].LHS){
                    count++;
                }
            }
            int treeIndex = 0;
            foreach (var X in Transform1dTree) // Тут ищет только первое пока вхождение левой части (крона ищется)
            {
                treeIndex++;
                if (Productions[NUMBER_INFERENCE_RULES[index_rules]-1].LHS == X.Symbol) // крона ли нужная
                {
                    count--;
                    if (count == 0)
                    {
                        break;
                    }
                }
            }
            var node = Transform1dTree[treeIndex - 1]; //найденная крона по индексу
            var Nodes = new List<int>();
            foreach (var child in node.Next) {  //ее дети запомнили
                Nodes.Add(child.pos);
            }
            //node.Next =  new List<Vertex>(); // обнулили
            var NewNodes = new List<Vertex>(); // куда хотим правильный порядок сложить
            foreach (var Y in Productions[NUMBER_INFERENCE_RULES[index_rules]-1].RHS_OUT) // смотрим что хотим на выходе
            {
                if (V.Contains(Y)) // Если нетерминал
                {
                    foreach (var Z in Nodes) // Ищем этот нетерминал среди детей
                    {
                        if (Transform1dTree[Z].Symbol.symbol == Y){ // Вдруг нашли и совпал символ его
                            NewNodes.Add(new Vertex(Transform1dTree[Z])); // добавляем во временный, его собственные ссылки детей должны не поменяться
                            break;
                        }
                    }
                }
                else
                {
                    NewNodes.Add(new Vertex(Y, false)); //Иначе терминал, и у него нет вершины
                }
            }
            foreach (var M in NewNodes) // хочу на место старых добавить все собранное по порядку
            {
                if(Nodes.Count != 0){ 
                    var new_pos = Nodes[0]; // возьми первое доступное место
                    Transform1dTree.RemoveAt(new_pos); //убери оттуда что там
                    Transform1dTree.Insert(new_pos, M); // Вставь сохраненное и которое в нужном порядке сейчас
                    //node.Add_link(new_pos);
                    Nodes.RemoveAt(0); // убери место где можно вставить
                }
            }
            index_rules++; // обход идет по NUMBER_INFERENCE_RULES, идем дальше
            if (index_rules>NUMBER_INFERENCE_RULES.Count-1)
            {
                break;
            }
        
        }
        var index_tree = 0;
        RightInfrence1dTreeVertices = new _1dTreeVertices(Transform1dTree[index_tree].Symbol);
        while (true)
        {
            var Childs = Transform1dTree[index_tree].Next;
            var Nodes = new List<int>();
            foreach (var child in Childs)
            {  //ее дети запомнили
                Nodes.Add(child.pos);
            }
            var sy = new List<Symbol>();
                foreach (var sms in Nodes)
                {  //ее дети запомнили
                    sy.Add(Transform1dTree[sms].Symbol);
                }
                RightInfrence1dTreeVertices.AddNodeToTree(Transform1dTree[index_tree].Symbol, sy);
                    index_tree++;
            if (index_tree>Transform1dTree.Count-1)
            {
                break;
            }
        }

        }
    }
}