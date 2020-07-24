using System;
using System.Collections.Generic;
using System.Threading;

namespace ASD
{
    public class CodesCounting : MarshalByRefObject
    {
        public int CountCodes(string text, string[] codes, out int[][] solutions )
        {
            List<List<int>>[] sol = new List<List<int>>[text.Length + 1];
            sol[0] = new List<List<int>>();
            sol[0].Add(new List<int>());
            int[] tab = new int[text.Length+1];
            tab[0] = 1;

            for (int n=1;n<=text.Length;n++)
            {
                sol[n] = new List<List<int>>();
                for (int k=0;k<codes.Length;k++)
                {
                    string sub = text.Substring(0,n);
                    if (sub.EndsWith(codes[k]))
                    {
                        tab[n] += tab[n - codes[k].Length];
                        foreach (var l in sol[n - codes[k].Length])
                        {
                            var b = new List<int>(l);
                            b.Add(k);
                            sol[n].Add(b);
                        }
                    }
                }
            }
            solutions = new int[tab[text.Length]][];
            for(int i=0;i<tab[text.Length];i++)
            {
                solutions[i] = sol[text.Length][i].ToArray();
            }
            return tab[text.Length];
        }
    }
}
