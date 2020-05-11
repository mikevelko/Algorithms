using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using ASD.Graphs;

namespace Lab09
{

public class Museum : MarshalByRefObject
    {

        /// <summary>
        /// Znajduje najliczniejszy multizbiór tras
        /// </summary>
        /// <returns>Liczność wyznaczonego multizbioru tras (liczba ścieżek w tym multizbiorze)</returns>
        /// <param name="g">Graf opisujący muzeum</param>
        /// <param name="levels">Tablica o długości równej liczbie wierzchołków w grafie -- poziomy ciekawości wystaw</param>
        /// <param name="entrances">Wejścia</param>
        /// <param name="exits">Wyjścia</param>
        /// <param name="routes">Ścieżki należące do wyznaczonego multizbioru</param>
        /// <remarks>
        /// Parametr routes to rablica tablic.
        /// Rozmiar zewnętrznej tablicy musi być równy liczbie ścieżek w multizbiorze.
        /// Każda wewnętrzna tablica opisuje jedną ścieżkę, a ściślej wymienia sale przez, które ona przechodzi.
        /// Kolejność sal jest istotna.
        /// </remarks>
        /// 
        private int[][] Check(int przeplyw, Graph Graf, int start, int end) 
        {
            int[][] tab = new int[przeplyw][];
            for (int i = 0; i < przeplyw; i++) 
            {
                int current = start;
                int licznik = 1;
                List<int> nodes = new List<int>();
                while (current != end) 
                {
                    if (licznik % 2 == 0) 
                    {
                        nodes.Add(current/2);
                    }
                    foreach (Edge edge in Graf.OutEdges(current)) 
                    {
                        if (edge.Weight != 0) 
                        {
                            Graf.ModifyEdgeWeight(edge.From,edge.To,-1);
                            current = edge.To;
                            licznik++;
                            break;
                        }
                    }
                }
                
                tab[i] = nodes.ToArray();
            }
            return tab;
        }
    public int FindRoutes(Graph g, int[] levels, int[] entrances, int[] exits, out int[][] routes)
        {
            Graph edgeGraph = g.IsolatedVerticesGraph(true, g.VerticesCount*2 + 2);
            int n = g.VerticesCount;
            for (int i = 0; i < g.VerticesCount; i++) 
            {
                if (levels[i] > 0)
                {
                    edgeGraph.AddEdge(2 * i, 2 * i + 1, levels[i]);
                    edgeGraph.AddEdge(2 * i + 1, 2 * i, levels[i]);
                    foreach (Edge edge in g.OutEdges(i))
                    {

                        edgeGraph.AddEdge( 2 * i + 1, edge.To * 2, int.MaxValue);
                    }
                }
                
            }
            int entry = 2 * n ;
            int exit = entry + 1;
            for (int i = 0; i < entrances.Length; i++) 
            {
                edgeGraph.AddEdge(entry, entrances[i] * 2, int.MaxValue);
            }
            for (int i = 0; i < exits.Length; i++)
            {
                edgeGraph.AddEdge(exits[i]*2+1,exit, int.MaxValue);
            }
            AugmentFlow augment = MaxFlowGraphExtender.MKMBlockingFlow;
            (double wartosc, Graph przeplyw) = edgeGraph.FordFulkersonDinicMaxFlow(entry, exit,augment , true);

            routes = Check((int)wartosc, przeplyw,entry,exit);
            
            return (int)wartosc;
        }

    }

}

