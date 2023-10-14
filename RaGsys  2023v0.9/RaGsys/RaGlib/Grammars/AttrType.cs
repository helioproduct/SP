using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaGlib.Core;
using RaGlib;

namespace RaGlib.Grammars
{
    public class AttrType   // ��������� Type - ��������� ������������� � ����������� ���������
    {
        public List<Symbol> Syn = new List<Symbol>();   // ��������� ������������� ���������
        public List<Symbol> Inh = new List<Symbol>();   // ��������� ����������� ���������

        public AttrType() { }

        public AttrType(List<Symbol> Syn, List<Symbol> Inh)
        {
            this.Syn = Syn;
            this.Inh = Inh;
        }
    }   //������� ������� �8�-206�-21
}
