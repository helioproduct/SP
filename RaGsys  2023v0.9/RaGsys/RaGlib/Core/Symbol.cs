﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaGlib.Core {
  public class Symbol { //: ICloneable  {
      public string symbol; ///< Строковое значение/имя символа
      public List<Symbol> attr { set; get;} = null; ///< Множество атрибутов символа
        // $$$

        //&&&

      public int production { set; get;} = 0; // for grammar occur  
      public int symbolPosition { set; get; } = 0; // for grammar occur

      public Symbol() {}
      public Symbol(string s, int production, int symbolPosition)
      {
            this.symbol = s;
            this.production = production;
            this.symbolPosition = symbolPosition;
      }
      public Symbol(string s, List<Symbol> attr)
      {
          this.symbol = s;
          this.attr = new List<Symbol>(attr);
          this.production = 0;
          this.symbolPosition = 0;
      }

      public Symbol(string value) // ??????????????????  убрать   value -- значение аттрибута 
      {
          this.symbol = value;
          this.attr = null;
          this.production = 0;
          this.symbolPosition = 0;
      }

      /// Неявное преобразование строки в Symbol
      public static implicit operator Symbol(string str) => new Symbol(str);
      /// Равенсто. Требуется для Dictionary и HashSet
      public override bool Equals(object other)
      {
          return (other is Symbol) && (this.symbol == ((Symbol)other).symbol);
      }

      public bool SEquals(object other) { // Strong Equals with grammar occur  
      return (other is Symbol)&&(this.symbol==((Symbol)other).symbol)&&
               (((Symbol)other).production==this.production)&&
               (((Symbol)other).symbolPosition==this.symbolPosition);
      }

    public bool IsEpsilon() => string.IsNullOrEmpty(this.symbol)||this.symbol[0]=='\0'||this.symbol=="ε";
    /// Хеш-функция. Требуется для Dictionary и HashSet
    public override int GetHashCode()
      {
          return (this.symbol + this.production.ToString() + this.symbolPosition.ToString()).GetHashCode();
      }
      public static bool operator == (Symbol a, Symbol b)
      {
        // If both are null, or both are same instance, return true.
          if (System.Object.ReferenceEquals(a, b))
          {
              return true;
          }
        // If one is null, but not both, return false.
          if (((object)a == null) || ((object)b == null))
          {
              return false;
          }
          // Return true if the fields match:
          return a.symbol== b.symbol;
      }
      public static bool operator != (Symbol symbol1,Symbol symbol2) {
          return !(symbol1 == symbol2);
      }
        //###
      public virtual void print()
      {
          Console.Write(this.symbol);
          Console.Write("");
          if (attr == null)
              return;
          foreach (var a in attr)
              Console.Write("." + a.symbol + " ");
      }
        public virtual void printWithColor(List<Symbol> Syn, List<Symbol> Inh)
        {
            Console.Write($"{this.symbol}");

            if (attr == null)
                return;
            foreach (var a in attr)
                if (Syn.Contains(a))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("." + a.symbol);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("." + a.symbol);
                }
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        //####
        public override string ToString() => this != Epsilon ? this.symbol : "e";
      public static readonly Symbol Epsilon = new Symbol(""); ///< Пустой символ
      public static readonly Symbol Sentinel = new Symbol("$"); ///< Cимвол конца строки / Символ дна стека
  }
}
