
namespace ASD
{
using System;
using System.Linq;

public class GameTestCase : TestCase
    {

    private int[] numbers;
    private int[] numbers2;
    private int expectedResult;
    private int result;
    private int[] moves;

    public GameTestCase(double timeLimit, Exception expectedException, string description, int[] numbers, int expectedResult)
            : base(timeLimit, expectedException, description)
        {
        this.numbers = numbers;
        this.numbers2 = (int[])numbers.Clone();
        this.expectedResult = expectedResult;
        }

    protected override void PerformTestCase(object prototypeObject)
        {
        result = (prototypeObject as Game).OptimalStrategy(numbers2, out moves);
        }

    protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
        if ( result!=expectedResult )
            return (Result.WrongResult, $"Wrong result: {result} (expected: {expectedResult})");
        if ( !(bool)settings )
            return (Result.Success,$"OK (time:{PerformanceTime,6:#0.000})");
        if ( moves==null )
            return (Result.WrongResult, "Null moves list");
        if ( moves.Length!=numbers.Length )
            return (Result.WrongResult, "Incorrect numbers of moves");
        int first = 0;
        int last = numbers.Length-1;
        for ( int i=0 ; i<moves.Length ; ++i )
            {
            if ( moves[i]!=first && moves[i]!=last )
                return (Result.WrongResult, "Incorrect move: {moves[i]}");
            if ( moves[i]==first ) ++first;
            if ( moves[i]==last ) --last;
            }
        int r=0;
        for ( int i=0 ; i<moves.Length ; i+=2 )
            r+=numbers[moves[i]];
        if ( r!=result )
            return (Result.WrongResult, $"Incorrect sum of moves values: {r} (expected: {expectedResult})");

        return (Result.Success,$"OK (time:{PerformanceTime,6:#0.000})");
        }

    }

class GemeTestModule : TestModule
    {

    public override void PrepareTestSets()
        {
        TestSets["LabGameResultTests"] = new TestSet(new Game(), "Lab. Tests - game result only",null,false);
        TestSets["LabMovesListTests"]  = new TestSet(new Game(), "Lab. Tests - game result and moves list",null,true);
        GameTestCase tc;
        Random rnd;
        int[] tab;

        tc = new GameTestCase(1, null, "Anti Greedy 1", new int[]{90,100,10,1}, 101);
        TestSets["LabGameResultTests"].TestCases.Add(tc);
        TestSets["LabMovesListTests"].TestCases.Add(tc);

        tc = new GameTestCase(1, null, "Anti Greedy 2", new int[]{5,100,20,1,10}, 35);
        TestSets["LabGameResultTests"].TestCases.Add(tc);
        TestSets["LabMovesListTests"].TestCases.Add(tc);

        tc = new GameTestCase(1, null, "Negative moves values", new int[]{-51,-40,50,30}, -1);
        TestSets["LabGameResultTests"].TestCases.Add(tc);
        TestSets["LabMovesListTests"].TestCases.Add(tc);

        tc = new GameTestCase(1, null, "One move", new int[]{-1}, -1);
        TestSets["LabGameResultTests"].TestCases.Add(tc);
        TestSets["LabMovesListTests"].TestCases.Add(tc);

        tc = new GameTestCase(1, null, "Many equal moves", Enumerable.Repeat(1,1111).ToArray(), 556);
        TestSets["LabGameResultTests"].TestCases.Add(tc);
        TestSets["LabMovesListTests"].TestCases.Add(tc);

        rnd = new Random(12345);
        tab = new int[800];
        for ( int i=0 ; i<tab.Length ; ++i )
            tab[i] = rnd.Next(-4000,4000);
        tc = new GameTestCase(1, null, "Random 1", tab, -17528);
        TestSets["LabGameResultTests"].TestCases.Add(tc);
        TestSets["LabMovesListTests"].TestCases.Add(tc);

        rnd = new Random(12347);
        tab = new int[1500];
        for ( int i=0 ; i<tab.Length ; ++i )
            tab[i] = rnd.Next(-500,500);
        tc = new GameTestCase(1, null, "Random 2", tab, 7014);
        TestSets["LabGameResultTests"].TestCases.Add(tc);
        TestSets["LabMovesListTests"].TestCases.Add(tc);

        rnd = new Random(12349);
        tab = new int[3000];
        for ( int i=0 ; i<tab.Length ; ++i )
            tab[i] = rnd.Next(-3000,3000);
        tc = new GameTestCase(3, null, "Random 3", tab, -28993);
        TestSets["LabGameResultTests"].TestCases.Add(tc);
        TestSets["LabMovesListTests"].TestCases.Add(tc);
        }

    }

class Lab01
    {

    public static void Main()
        {
        GemeTestModule lab02tests = new GemeTestModule();
        lab02tests.PrepareTestSets();
        foreach (var ts in lab02tests.TestSets)
            ts.Value.PerformTests(verbose:true, checkTimeLimit:false);
        }

    }

}