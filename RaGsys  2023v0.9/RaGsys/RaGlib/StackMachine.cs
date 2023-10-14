using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaGlib
{
    struct Delta
    {
        public string curState;
        public List<string> curTerms;
        public string nextState;
        public List<string> nextTerms;
    }

    public class ThreeStackAutomat
    {
        private List<string> Sigma;
        private List<string> Gamma;
        private List<string> Q;
        private string s;
        private List<string> T;
        private string z;
        private List<Delta> rules;

        public List<string> sigma
        {
            get { return Sigma; }
        }

        public List<string> gamma
        {
            get { return Gamma; }
        }


        public ThreeStackAutomat(List<string> _Sigma, List<string> _Gamma, List<string> _Q, string _s, List<string> _T, string _z)
        {
            Sigma = _Sigma;
            Gamma = _Gamma;
            Q = _Q;
            s = _s;
            T = _T;
            z = _z;
            rules = new List<Delta>();
        }

        public void AddRule(string state, List<string> terms, string nextState, List<string> nextTerms)
        {
            Delta delta;
            delta.curState = state;
            delta.curTerms = terms;
            delta.nextState = nextState;
            delta.nextTerms = nextTerms;
            rules.Add(delta);
        }

        public List<Stack<string>> Execute(List<string> input)
        {
            List<Stack<string>> stacks = new List<Stack<string>>();
            for (int i = 0; i < 3; i++)
            {
                stacks.Add(new Stack<string>());
                stacks[i].Push(z);
            }
            string state = s;
            int pos = 0;
            bool changed = true;
            while (changed)
            {
                changed = false;
                string sym = "";
                if (pos < input.Count)
                    sym = input[pos];
                for (int i = 0; i < rules.Count; i++)
                {
                    if (rules[i].curState != state)
                        continue;
                    if (rules[i].curTerms[0] != sym)
                        continue;
                    bool ok = true;
                    for (int j = 1; j < 4; j++)
                    {
                        if (rules[i].curTerms[j] != stacks[j - 1].Peek() && rules[i].curTerms[j].Length > 0)
                            ok = false;
                    }
                    if (!ok)
                        continue;
                    state = rules[i].nextState;
                    for (int j = 0; j < 3; j++)
                    {
                        if (rules[i].curTerms[j + 1].Length > 0)
                            stacks[j].Pop();
                        if (rules[i].nextTerms[j].Length > 0)
                            stacks[j].Push(rules[i].nextTerms[j]);
                    }
                    pos++;
                    changed = true;
                    break;
                }
            }
            for (int i = 0; i < T.Count; i++)
            {
                if (T[i] == state)
                    return stacks;
            }
            return null;
        }
    }

    public struct Pair
    {
        public string state;
        public List<string> chain;

        public Pair(string _state, List<string> _chain)
        {
            state = _state;
            chain = _chain;
        }

        public override int GetHashCode()
        {
            return state.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Pair item = (Pair)obj;
            if (item.state != state)
                return false;
            if (item.chain.Count != chain.Count)
                return false;
            for (int i = 0; i < chain.Count; i++)
            {
                if (item.chain[i] != chain[i])
                    return false;
            }
            return true;
        }
    }

    public class StackMachine
    {
        private List<string> Sigma;
        private List<string> Gamma;
        private List<string> Q;
        private string s;
        private List<string> T;
        private string z;
        private List<Delta> rules;
        private Dictionary<Pair, Pair> P;

        public List<string> sigma
        {
            get { return Sigma; }
        }

        public List<string> gamma
        {
            get { return Gamma; }
        }


        public StackMachine(List<string> _Sigma, List<string> _Gamma, List<string> _Q, string _s, List<string> _T, string _z)
        {
            Sigma = _Sigma;
            Gamma = _Gamma;
            Q = _Q;
            s = _s;
            T = _T;
            z = _z;
            rules = new List<Delta>();
            P = new Dictionary<Pair, Pair>();
        }

        public void AddRule(string state, List<string> terms, string argState, List<string> argTerms)
        {
            Delta delta;
            delta.curState = state;
            delta.curTerms = terms;
            delta.nextState = argState;
            delta.nextTerms = argTerms;
            rules.Add(delta);
        }

        public void AddFunction(Pair cur, Pair next)
        {
            P.Add(cur, next);
        }

        public Stack<string> Execute(List<string> input)
        {
            List<string> stack = new List<string>();
            stack.Add(z);
            string state = s;
            int pos = 0;
            bool changed = true;
            while (changed)
            {
                changed = false;
                string sym = "";
                if (pos < input.Count)
                    sym = input[pos];
                for (int i = 0; i < rules.Count; i++)
                {
                    if (rules[i].curState != state)
                        continue;
                    if (rules[i].curTerms[0] != sym)
                        continue;
                    bool ok = true;
                    for (int j = 1; j < rules[i].curTerms.Count && ok; j++)
                    {
                        if (stack.Count - j < 0)
                            ok = false;
                        if (stack[stack.Count - j] != rules[i].curTerms[j])
                            ok = false;
                    }
                    if (!ok)
                        continue;
                    Pair key = new Pair(rules[i].nextState, rules[i].nextTerms);
                    if (P.ContainsKey(key))
                    {
                        state = P[key].state;
                        var func = P[key];
                        stack.RemoveRange(stack.Count - rules[i].curTerms.Count + 1, rules[i].curTerms.Count - 1);
                        for (int j = P[key].chain.Count - 1; j >= 0; j--)
                            stack.Add(P[key].chain[j]);
                    }
                    else
                        return null;
                    pos++;
                    changed = true;
                    break;
                }
            }
            for (int i = 0; i < T.Count; i++)
            {
                if (T[i] == state)
                {
                    Stack<string> res = new Stack<string>();
                    for (int j = 0; j < stack.Count; j++)
                        res.Push(stack[j]);
                    return res;
                }
            }
            return null;
        }
    }
}
