using System;
using System.Linq;
using ASD;
using ASD.Graphs;

namespace Lab09
{

public class MuseumTestCase : TestCase
    {

    private readonly Graph gOrg, gUsed;
    private readonly int[] levels;
    private readonly int[] entrances;
    private readonly int[] exits;
    private readonly int[] levelsCopy;
    private readonly int[] entrancesCopy;
    private readonly int[] exitsCopy;
    private readonly int expectedRoutesCount;
    private readonly bool checkRoutes;

    private int routesCount;
    private int[][] routes;

    public MuseumTestCase(double timeLimit, Graph g, int[] levels, int[] entrances, int[] exits, int expectedRoutesCount, bool checkRoutes, string description) : base(timeLimit, null, description)
        {
        this.gOrg = g;
        this.gUsed = gOrg.Clone();
        this.levels = levels;
        this.entrances = entrances;
        this.exits = exits;
        this.levelsCopy = (int[])levels.Clone();
        this.entrancesCopy = (int[])entrances.Clone();
        this.exitsCopy = (int[])exits.Clone();
        this.expectedRoutesCount = expectedRoutesCount;
        this.checkRoutes = checkRoutes;
        }

    protected override void PerformTestCase(object prototypeObject)
        {
        routesCount = ((Museum)prototypeObject).FindRoutes(gUsed, levels, entrances, exits, out routes);
        }

    protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
        if( !gOrg.IsEqual(gUsed) || !levels.SequenceEqual(levelsCopy) || !entrances.SequenceEqual(entrancesCopy) || !exits.SequenceEqual(exitsCopy) )
            return (Result.WrongResult,"Dane wejściowe zostały zmienione");

        if ( routesCount!=expectedRoutesCount )
            return (Result.WrongResult,$"Oczekiwano tras: {expectedRoutesCount}, program policzył: {routesCount}");

        if ( !checkRoutes )
            return (Result.Success,$"OK {PerformanceTime,6:0.00}");

        if ( routes==null || routes.Length!=routesCount )
            return (Result.WrongResult,"Zły rozmiar tablicy tras lub tablica tras równa null");

        if ( routes.Any( x=>(x==null || x.Length==0) ) )
            return (Result.WrongResult,"Tablica tras zawiera nulle lub puste tablice");

        return CheckPaths(routes);
        }

    public static bool HasEdge(Graph g, int from, int to)
        {
        return !double.IsNaN(g.GetEdgeWeight(from, to));
        }

    (Result resultCode, string message) CheckPaths(int[][] paths)
        {
        int[] realLevel = new int[gUsed.VerticesCount];

        for ( int i=0 ; i<paths.Length ; ++i )
            {

            if ( !entrances.Contains(paths[i].First()) || !exits.Contains(paths[i].Last()) )
                return (Result.WrongResult,"Początek lub koniec trasy to nie drzwi!");

            for ( int j=0 ; j<paths[i].Length ; ++j )
                {

                if ( paths[i][j]<0 || paths[i][j]>=gUsed.VerticesCount )
                    return (Result.WrongResult,"Trasy zawierają wierzchołki spoza grafu");

                if ( j>0 && !HasEdge(gUsed, paths[i][j-1], paths[i][j]) )
                    return (Result.WrongResult,"Trasy zawierają krawędzi spoza grafu");

                if ( levels[paths[i][j]]==realLevel[paths[i][j]] )
                    return (Result.WrongResult,"Wierzchołek odwiedzony częściej niż poziom ciekawości");

                ++realLevel[paths[i][j]];
                }
            }

        return (Result.Success,$"OK {PerformanceTime,6:0.00}");
        }

    }

public class RoutesTests : TestModule
    {

    public override void PrepareTestSets()
        {
        TestSets["etap1-stud"] = PrepareLabTests(false, "Lab Etap 1 - Tylko liczności multizbiorów");
        TestSets["etap2-stud"] = PrepareLabTests(true, "Lab Etap 2 - Poprawne multizbiory");
        }

    private static TestSet PrepareLabTests(bool checkRoutes, string testSetName)
        {

        Graph triplePath = new AdjacencyListsGraph<SimpleAdjacencyList>(false, 5);
        triplePath.AddEdge(0, 1);
        triplePath.AddEdge(0, 2);
        triplePath.AddEdge(0, 3);
        triplePath.AddEdge(4, 1);
        triplePath.AddEdge(4, 2);
        triplePath.AddEdge(4, 3);

        Graph grid5 = new AdjacencyListsGraph<SimpleAdjacencyList>(false, 25);
        for ( int i=0 ; i<5 ; ++i )
            for ( int j=0 ; j<4 ; ++j )
                {
                grid5.AddEdge(5*i+j, 5*i+j+1);
                grid5.AddEdge(5*j+i, 5*(j+1)+i);
                }

        Graph crossing = new AdjacencyListsGraph<SimpleAdjacencyList>(false, 5);
        crossing.AddEdge(0, 4);
        crossing.AddEdge(1, 4);
        crossing.AddEdge(2, 4);
        crossing.AddEdge(3, 4);

        Graph path = new AdjacencyListsGraph<SimpleAdjacencyList>(false, 5);
        for (int i = 1; i < path.VerticesCount; i++)
            {
            path.AddEdge(i - 1, i);
            }

        Graph disjunction = new AdjacencyListsGraph<SimpleAdjacencyList>(false, 5);
        disjunction.AddEdge(0, 1);
        disjunction.AddEdge(1, 2);
        disjunction.AddEdge(2, 3);
        disjunction.AddEdge(1, 4);

        MuseumTestCase triplePathCase = new MuseumTestCase(1, triplePath, new int[] { 3, 1, 1, 1, 3 }, new int[] { 0 }, new int[] { 4 }, 3, checkRoutes, "Trzy ścieżki - wszystkie ścieżki");
        MuseumTestCase doublePathCase = new MuseumTestCase(1, triplePath, new int[] { 1, 1, 1, 1, 1 }, new int[] { 0, 4 }, new int[] { 0, 4 }, 2, checkRoutes, "Trzy ścieżki - tylko wejściowo-wyjściowe");
        MuseumTestCase singleVertexCase1 = new MuseumTestCase(1, triplePath, new int[] { 1, 1, 1, 1, 1 }, new int[] { 0 }, new int[] { 0 }, 1, checkRoutes, "Trzy ścieżki - początek wejściowo-wyjściowy");
        MuseumTestCase singleVertexCase2 = new MuseumTestCase(1, triplePath, new int[] { 2, 1, 1, 1, 2 }, new int[] { 0 }, new int[] { 4 }, 2, checkRoutes, "Trzy ścieżki - maksymalnie dwie ścieżki");
        MuseumTestCase doubledPathCase = new MuseumTestCase(1, triplePath, new int[] { 6, 2, 2, 2, 6 }, new int[] { 0 }, new int[] { 4 }, 6, checkRoutes, "Trzy ścieżki - wszystkie ścieżki po dwa razy");
        MuseumTestCase almostDoubledPathCase = new MuseumTestCase(1, triplePath, new int[] { 6, 2, 2, 2, 5 }, new int[] { 0 }, new int[] { 4 }, 5, checkRoutes, "Trzy ścieżki - dwie ścieżki po dwa razy i jedna raz (ograniczenie na ujściu)");
        MuseumTestCase almostDoubledPathCase2 = new MuseumTestCase(1, triplePath, new int[] { 5, 2, 2, 2, 6 }, new int[] { 0 }, new int[] { 4 }, 5, checkRoutes, "Trzy ścieżki - dwie ścieżki po dwa razy i jedna raz (ograniczenie na wejściu)");
        MuseumTestCase middleVertexLimit = new MuseumTestCase(1, crossing, new int[] { 1, 1, 1, 1, 1 }, new int[] { 0, 1 }, new int[] { 2, 3 }, 1, checkRoutes, "Gwiazda - ograniczenie do jeden w centrum");

        MuseumTestCase centerToCornersGrid = new MuseumTestCase(1, grid5, new int[]
            {
            1,3,3,3,2,
            0,3,0,0,0,
            0,4,6,1,1,
            0,1,0,0,1,
            1,1,0,0,1
            }, new int[] { 12 }, new int[] { 0, 4, 20, 24 }, 5, checkRoutes, "Siatka 5x5 - od środka na zewnątrz");

        MuseumTestCase cornersToCenterGrid = new MuseumTestCase(1, grid5, new int[]
            {
            1,3,3,3,2,
            0,3,0,0,0,
            0,4,6,1,1,
            0,1,0,0,1,
            1,1,0,0,1
            }, new int[] { 0, 4, 20, 24 }, new int[] { 12 }, 5, checkRoutes, "Siatka 5x5 - od zewnątrz do środka");

        TestSet set = new TestSet(new Museum(), testSetName);
        set.TestCases.Add(triplePathCase);
        set.TestCases.Add(doublePathCase);
        set.TestCases.Add(singleVertexCase1);
        set.TestCases.Add(singleVertexCase2);
        set.TestCases.Add(doubledPathCase);
        set.TestCases.Add(almostDoubledPathCase);
        set.TestCases.Add(almostDoubledPathCase2);
        set.TestCases.Add(middleVertexLimit);
        set.TestCases.Add(centerToCornersGrid);
        set.TestCases.Add(cornersToCenterGrid);
        set.TestCases.Add(RandomTest(500, 1337, 15, 383, checkRoutes));
        set.TestCases.Add(RandomTest(750, 2021, 45, 46, checkRoutes));
        set.TestCases.Add(RandomTest(1000, 1410, 110, 680, checkRoutes));
        return set;
        }

    public static MuseumTestCase RandomTest(int size, int seed, double time, int solution, bool checkRoutes)
        {
        Random random = new Random(seed);
        RandomGraphGenerator rgg = new RandomGraphGenerator(seed);

        Graph g = rgg.UndirectedGraph(typeof(AdjacencyListsGraph<HashTableAdjacencyList>), size, .3, 1, 1);

        int[] cLevel = Enumerable.Range(0, size).Select(x => random.Next(20)).ToArray();
        int[] entrances = Enumerable.Range(0,random.Next(size/10)).Select(x => random.Next(size)).ToArray();
        int[] exits = Enumerable.Range(0,random.Next(size/10)).Select(x => random.Next(size)).ToArray();
        return new MuseumTestCase(time, g, cLevel, entrances, exits, solution, checkRoutes, $"Losowy o {size} wierzchołkach");
        }

    public override double ScoreResult()
        {
        return 3;
        }

    }

 class MainClass
    {

    public static void Main (string[] args)
        {
        var tests = new RoutesTests();
        tests.PrepareTestSets();
        foreach (var ts in tests.TestSets)
            {
            ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }

    }

}
