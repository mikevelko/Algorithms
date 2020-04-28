using System;
using System.Collections.Generic;
using ASD.Graphs;

namespace lab08
{
    public class AlmostMatching : MarshalByRefObject
    {

        /// <summary>
        /// Metoda zwraca najliczniejszy możliwy zbiór krawędzi, którego poziom ryzyka nie przekracza limitu.
        /// W przypadku istnenia kilku takich zbiorów zwraca zbiór o najmniejszej sumie wag ze wszystkich najliczniejszych.
        /// </summary>
        /// <returns>Liczba i lista linek (krawędzi)</returns>
        /// <param name="g">Graf linek</param>
        /// <param name="allowedCollisions">Limit ryzyka</param>
        public (int edgesCount, List<Edge> solution) LargestS(Graph g, int allowedCollisions)
        {
            
            int tabSize = 0;
            for (int i = 0; i < g.VerticesCount; i++)
            {
                foreach (Edge edge in g.OutEdges(i))
                {
                    if (edge.From < edge.To) tabSize++;
                }
            }
            bool[] EdgesExists = new bool[tabSize];
            Edge[] AllEdges = new Edge[tabSize];
            int iter = 0;
            for (int i = 0; i < g.VerticesCount; i++)
            {
                foreach (Edge edge in g.OutEdges(i))
                {
                    if (edge.From < edge.To) 
                    {
                        AllEdges[iter] = edge;
                        iter++;
                    }
                }
            }
            BestSolution best = new BestSolution(g.VerticesCount,tabSize);
            //int[] AllVertices = new int[g.VerticesCount];
            //Largest(g, allowedCollisions, EdgesExists,AllVertices, AllEdges, 0, 0, 0,0, best);
            Largest(g, allowedCollisions,  AllEdges,tabSize, 0, 0, 0, 0, best);

            List<Edge> result = new List<Edge>();
            for (int i = 0; i < tabSize; i++)
            {
                if (best.BestEdgesExists[i])
                {
                    result.Add(AllEdges[i]);
                }
            }
            return (result.Count, result);
        }

        // można dodawać pomocnicze klasy i metody (prywatne!)

        private class BestSolution
        {
            public bool[] BestEdgesExists;
            public double zuzycie;
            public int count;
            public int[] Vertices;
            public bool[] EdgesExists;
            public BestSolution(int VerticesCount, int EdgesCount)
            {
                BestEdgesExists = null;
                zuzycie = 0;
                count = 0;
                Vertices = new int[VerticesCount];
                EdgesExists = new bool[EdgesCount];
            }
        }

        private void Largest(Graph g, int allowedCollisions, Edge[] AllEdges,int tabSize, double prevZuzycie, int prevCollision, int step, int count, BestSolution best)
        {

            if (prevCollision > allowedCollisions)
            {
                return;
            }
            if (best.count > (count + tabSize - step))
            {
                return;
            }
            ///////////////////////////////
            if (count > best.count)
            {
                best.BestEdgesExists = new bool[tabSize];
                for (int i = 0; i < tabSize; i++)
                {
                    best.BestEdgesExists[i] = best.EdgesExists[i];
                }
                best.count = count;
                best.zuzycie = prevZuzycie;
            }
            else if (count == best.count && prevZuzycie < best.zuzycie)
            {
                best.BestEdgesExists = new bool[tabSize];
                for (int i = 0; i < tabSize; i++)
                {
                    best.BestEdgesExists[i] = best.EdgesExists[i];
                }
                best.zuzycie = prevZuzycie;
            }
            ////////////////////////////////

            if (step == tabSize)
            {
                return;
            }
            //int[] NewVertices = new int[g.VerticesCount];
            //for (int i = 0; i < g.VerticesCount; i++) 
            //{
            //    NewVertices[i] = prevVertices[i];
            //}

            int NewCollision = prevCollision;
            double newZuzycie;

            newZuzycie = prevZuzycie + AllEdges[step].Weight;
            best.Vertices[AllEdges[step].From]++;
            best.Vertices[AllEdges[step].To]++;
            if (best.Vertices[AllEdges[step].From] > 1) NewCollision++;
            if (best.Vertices[AllEdges[step].To] > 1) NewCollision++;
            count++;
            best.EdgesExists[step] = true;

            Largest(g, allowedCollisions,  AllEdges,tabSize, newZuzycie, NewCollision, step + 1, count, best);

            newZuzycie = prevZuzycie;
            if (best.Vertices[AllEdges[step].From] > 1) NewCollision--;
            if (best.Vertices[AllEdges[step].To] > 1) NewCollision--;
            best.Vertices[AllEdges[step].From]--;
            best.Vertices[AllEdges[step].To]--;

            count--;
            best.EdgesExists[step] = false;

            Largest(g, allowedCollisions,  AllEdges,tabSize, newZuzycie, NewCollision, step + 1, count, best);

            //NewCollision = prevCollision;
            //newZuzycie = prevZuzycie;
            //NewVertices[AllEdges[step].From]--;
            //NewVertices[AllEdges[step].To]--;
            //count--;
            //EdgesExistsNew[step] = false;

            //Largest(g, allowedCollisions, EdgesExistsNew, NewVertices, AllEdges, newZuzycie, NewCollision, step + 1, count, best);
        }
    }

}


