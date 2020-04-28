using System;
using System.Linq;
using System.Collections.Generic;
using ASD;
using ASD.Graphs;

namespace lab08
{

class MatchingTestCase : TestCase
    {
        private readonly Graph g;
        private readonly Graph g1;
        private readonly int allowedCollisions;
        private readonly int expectedResult;
        private readonly double expectedWeight;
        private (int edgesCount, List<Edge> edgesList) solution;

        public MatchingTestCase(double timeLimit, Graph g, int allowedCollisions, int expectedResult, double expectedWeight, string description) : base(timeLimit, null, description)
            {
            this.g = g;
            this.allowedCollisions = allowedCollisions;
            this.expectedResult = expectedResult;
            this.expectedWeight = expectedWeight;
            this.g1 = g.Clone();
            }

        protected override void PerformTestCase(object obj)
            {
            solution = ((AlmostMatching)obj).LargestS(g1, allowedCollisions);
            }

        public static bool HasEdge(Graph g, int from, int to, double weight)
            {
            return g.GetEdgeWeight(from, to) == weight;
            }

        protected override (Result, String) VerifyTestCase(object obj)
            {
            if (!g.IsEqual(g1))
                return (Result.WrongResult, String.Format("Zmieniono graf"));

            if (solution.edgesCount != expectedResult)
                return (Result.WrongResult, String.Format("Zła liczność rozwiązania: {0}, oczekiwano {1}", solution.edgesCount, expectedResult));

            var edges = solution.edgesList;
            if (edges == null)
                return (Result.WrongResult, "Zwrócono edges==null");

            if (expectedResult != edges.Count)
                return (Result.WrongResult, String.Format("Zła liczba krawędzi w liście: {0}, zadeklarowano {1}", edges.Count, expectedResult));

            if (edges.Distinct().Count() != edges.Count)
                return (Result.WrongResult, "Wynik zawiera powtórzenia krawędzi!!");

            if (edges.Any(e => !HasEdge(g, e.From, e.To, e.Weight)))
                return (Result.WrongResult, "Wynik zawiera krawędzie spoza grafu");

            int[] collisions = new int[g.VerticesCount];
            foreach (Edge e in edges)
                {
                collisions[e.From]++;
                collisions[e.To]++;
                }
            int collSum = collisions.Select(x => Math.Max(0, x - 1)).Aggregate((x, y) => x + y);
            if (collSum > allowedCollisions)
                return (Result.WrongResult, "Zbyt dużo kolizji krawędzi w liście");

            if (edges.Count > 0 && edges.Select(e => e.Weight).Aggregate((a, b) => a + b) != expectedWeight)
                return (Result.WrongResult, String.Format("Zbyt duża suma wag krawędzi, otrzymano {0}, oczekiwane {1}", edges.Select(e => e.Weight).Aggregate((a, b) => a + b), expectedWeight));

            return (Result.Success, $"OK {PerformanceTime,6:#0.000}");
            }

    }

public class AlmostmatchingTests : TestModule
    {
        public TestSet UnweightedTest()
            {
            Graph grid3 = new AdjacencyListsGraph<SimpleAdjacencyList>(false, 9);
            for (int i = 0; i < 9; i += 3)
                {
                grid3.AddEdge(i, i + 1);
                grid3.AddEdge(i + 1, i + 2);
                }

            for (int i = 0; i < 3; i++)
                {
                grid3.AddEdge(i + 0, i + 3);
                grid3.AddEdge(i + 3, i + 6);
                }

            Graph path20 = new AdjacencyListsGraph<SimpleAdjacencyList>(false, 20);
            for (int i = 1; i < path20.VerticesCount; i++)
                {
                path20.AddEdge(i - 1, i);
                }

            Graph shiftedPath20 = new AdjacencyListsGraph<SimpleAdjacencyList>(false, 20);
            shiftedPath20.AddEdge(18, 19);
            shiftedPath20.AddEdge(0, 1);
            shiftedPath20.AddEdge(19, 1);
            for (int i = 2; i < shiftedPath20.VerticesCount; i++)
                {
                shiftedPath20.AddEdge(i - 1, i);
                }

            RandomGraphGenerator rgg = new RandomGraphGenerator(240044);
            Graph eCycle24 = rgg.UndirectedCycle(typeof(AdjacencyListsGraph<SimpleAdjacencyList>), 24, 1, 1, true);
            Graph oCycle23 = rgg.UndirectedCycle(typeof(AdjacencyListsGraph<SimpleAdjacencyList>), 23, 1, 1, true);
            Graph g3 = rgg.UndirectedGraph(typeof(AdjacencyListsGraph<SimpleAdjacencyList>), 20, 0.2, 1, 11, true);
            Graph iso = new AdjacencyListsGraph<SimpleAdjacencyList>(false, 1024);

            TestSet set1 = new TestSet(new AlmostMatching(), "Grafy nieważone -- znane");

            set1.TestCases.Add(new MatchingTestCase(1, path20, 0, 10, 10, "Ścieżka 20 wierzchołków, poziom ryzyka 0"));
            set1.TestCases.Add(new MatchingTestCase(1, shiftedPath20, 0, 10, 10, "Ścieżka 20 wierzchołków w przypadkowej kolejności, poziom ryzyka 0"));
            set1.TestCases.Add(new MatchingTestCase(1, eCycle24, 0, 12, 12, "Cykl parzysty, poziom ryzyka 0"));
            set1.TestCases.Add(new MatchingTestCase(1, oCycle23, 0, 11, 11, "Cykl nieparzysty, poziom ryzyka 0"));
            set1.TestCases.Add(new MatchingTestCase(1, grid3, 0, 4, 4, "Siatka 2D, poziom ryzyka 0"));
            set1.TestCases.Add(new MatchingTestCase(1, grid3, 1, 5, 5, "Siatka 2D, poziom ryzyka 1"));
            set1.TestCases.Add(new MatchingTestCase(1, grid3, 2 * grid3.EdgesCount - grid3.VerticesCount, grid3.EdgesCount, grid3.EdgesCount, "Siatka 2D, wysoki poziom ryzyka"));
            set1.TestCases.Add(new MatchingTestCase(1, path20, 1, 10, 10, "Ścieżka 20, poziom ryzyka 1"));
            set1.TestCases.Add(new MatchingTestCase(1, shiftedPath20, 1, 10, 10, "Inna ścieżka 20, poziom ryzyka 1"));
            set1.TestCases.Add(new MatchingTestCase(1, eCycle24, 1, 12, 12, "Cykl parzysty, poziom ryzyka 1"));
            set1.TestCases.Add(new MatchingTestCase(1, oCycle23, 1, 12, 12, "Cykl nieparzysty, poziom ryzyka 1"));
            set1.TestCases.Add(new MatchingTestCase(1, iso, 0, 0, 0, "Izolowane wierzchołki 1"));
            set1.TestCases.Add(new MatchingTestCase(1, iso, 10000, 0, 0, "Izolowane wierzchołki 2"));
            set1.TestCases.Add(new MatchingTestCase(8, g3, 2, 11, 43, "graf losowy poziom ryzyka 2"));
            set1.TestCases.Add(new MatchingTestCase(40, g3, 3, 11, 35, "graf losowy poziom ryzyka 3"));
            return set1;
            }

        public TestSet WeightedTest()
            {
            Graph grid3 = new AdjacencyListsGraph<SimpleAdjacencyList>(false, 9);
            for (int i = 0; i < 9; i += 3)
                {
                grid3.AddEdge(i, i + 1, 3);
                grid3.AddEdge(i + 1, i + 2, 2);
                }

            for (int i = 0; i < 3; i++)
                {
                grid3.AddEdge(i + 0, i + 3, 2);
                grid3.AddEdge(i + 3, i + 6, 1);
                }

            RandomGraphGenerator rgg = new RandomGraphGenerator(240044);
            Graph eCycle24 = rgg.UndirectedCycle(typeof(AdjacencyListsGraph<SimpleAdjacencyList>), 24, 1, 5, true);
            Graph oCycle23 = rgg.UndirectedCycle(typeof(AdjacencyListsGraph<SimpleAdjacencyList>), 23, 1, 5, true);
            Graph g3 = rgg.UndirectedGraph(typeof(AdjacencyListsGraph<SimpleAdjacencyList>), 20, 0.2, 1, 11, true);

            TestSet set2 = new TestSet(new AlmostMatching(), "Grawy ważone -- znane");

            set2.TestCases.Add(new MatchingTestCase(1, grid3, 0, 4, 5, "siatka prostokątna, poziom ryzyka 0"));
            set2.TestCases.Add(new MatchingTestCase(1, grid3, 1, 5, 7, "siatka prostokątna, poziom ryzyka 1"));
            set2.TestCases.Add(new MatchingTestCase(1, grid3, 2 * grid3.EdgesCount - grid3.VerticesCount, grid3.EdgesCount, 3 + 6 + 6 + 9, "siatka prostokątna, bardzo duży poziom ryzyka"));
            set2.TestCases.Add(new MatchingTestCase(1, grid3, 2 * grid3.EdgesCount - grid3.VerticesCount - 1, grid3.EdgesCount - 1, 3 + 6 + 6 + 6, "siatka prostokątna, duży poziom ryzyka"));
            set2.TestCases.Add(new MatchingTestCase(1, eCycle24, 0, 12, 33, "cykl parzysty, poziom ryzyka 0"));
            set2.TestCases.Add(new MatchingTestCase(1, oCycle23, 0, 11, 30, "cykl nieparzysty, poziom ryzyka 0"));
            set2.TestCases.Add(new MatchingTestCase(1, eCycle24, 1, 12, 24, "cykl parzysty, poziom ryzyka 1"));
            set2.TestCases.Add(new MatchingTestCase(1, oCycle23, 1, 12, 32, "cykl nieparzysty, poziom ryzyka 1"));
            set2.TestCases.Add(new MatchingTestCase(1, g3, 0, 10, 45, "graf losowy poziom ryzyka 0"));
            set2.TestCases.Add(new MatchingTestCase(8, g3, 2, 11, 43, "graf losowy poziom ryzyka 2"));
            set2.TestCases.Add(new MatchingTestCase(40, g3, 3, 11, 35, "graf losowy poziom ryzyka 3"));
            return set2;
            }

        private TestSet unw;
        private TestSet w;

        public override void PrepareTestSets()
            {
            TestSets["unweighted"] = unw = UnweightedTest();
            TestSets["weighted"] = w = WeightedTest();
            }

        public override double ScoreResult() => 3.0;

    }

static class MainClass
    {

        public static void Main(string[] args)
            {
            var tests = new AlmostmatchingTests();
            tests.PrepareTestSets();
            foreach (var ts in tests.TestSets)
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }

    }

}


