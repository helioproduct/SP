using System;
using System.Collections.Generic;
class Program
{ 
    static void Main(string[] args)
    {
        string expression = "| ( cos ( x ) + sin ( y ) - z ) |"; //| cos x 1CALL sin y 1CALL + z -3CALL |        
        string[] substring = expression.Split(' ');
        string[] binaryOperatorsArray = { "+", "-" };
        string[] multidimensionalOperationArray = { "cos", "sin","|"};
        Stack<string> stack = new Stack<string>();
        string result = "";
        int counter = 0;
        for (int i = 0; i < substring.Length; i++)
        {
            string elem = substring[i];
            if (elem == "(")
            {
                stack.Push(elem);
            }
            else if (elem == ")")
            {
                while (stack.Peek() != "(")
                {
                    counter += 1;
                    result += stack.Pop();
                }
                counter += 1;
                result += counter.ToString();
                result += "CALL";
                result += " ";
                stack.Pop();
            }
            else if (Array.IndexOf(multidimensionalOperationArray,elem) != -1)
            {
                result += elem;
                result += " ";
                counter = 0;
            }
            else if (Array.IndexOf(binaryOperatorsArray, elem) != -1)
            {
                while (stack.Count > 0 && Array.IndexOf(binaryOperatorsArray, stack.Peek()) != -1)
                {
                    result += stack.Pop();
                    result += " ";
                }
                stack.Push(elem);
            }
            else
            {
                result += elem;
                result += " ";
            }
        }
        while (stack.Count > 0)
        {
            result += stack.Pop();
            result += " ";
        }
        Console.WriteLine(result); }}