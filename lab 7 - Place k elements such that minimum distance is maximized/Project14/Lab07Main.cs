using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    public abstract class DistanceTestCase : TestCase
    {
        protected int[] table;
        protected int[] tableCopy;
        protected int k;
        protected List<int> exampleSolution;
        protected Dictionary<int, int> elemsOccurences;

        public DistanceTestCase(double timeLimit, Exception expectedException, string description, int[] a, int k) : base(timeLimit, expectedException, description)
        {
            table = a;
            tableCopy = (int[])a.Clone();
            elemsOccurences = new Dictionary<int, int>();
            for (int i = 0; i < table.Length; i++)
            {
                if (elemsOccurences.ContainsKey(table[i]))
                    elemsOccurences[table[i]]++;
                else elemsOccurences.Add(table[i], 1);
            }
            this.k = k;
        }

        protected bool CheckExampleSolution(int distance)
        {
            exampleSolution.Sort();
            if (exampleSolution.Count != k) return false;
            if (!elemsOccurences.ContainsKey(exampleSolution[0]) || elemsOccurences[exampleSolution[0]] == 0)
                return false;
            elemsOccurences[exampleSolution[0]]--;
            int pos = exampleSolution[0];
            int elements = 1;
            for (int i = 1; i < exampleSolution.Count; i++)
            {
                if (!elemsOccurences.ContainsKey(exampleSolution[i]) || elemsOccurences[exampleSolution[i]] == 0)
                    return false;
                elemsOccurences[exampleSolution[i]]--;
                if (exampleSolution[i] - pos >= distance)
                {
                    pos = exampleSolution[i];
                    if (++elements == k) return true;
                }
            }
            return false;
        }
    }

    public class CanPlaceElementsInDistanceTestCase : DistanceTestCase
    {
        private int distance;
        private bool result;
        private bool expectedResult;

        public CanPlaceElementsInDistanceTestCase(double timeLimit, Exception expectedException, string description, int[] a, int k, int distance, bool result) : base(timeLimit, expectedException, description, a, k)
        {
            this.distance = distance;
            expectedResult = result;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab07)prototypeObject).CanPlaceBuildingsInDistance(tableCopy, distance, k, out exampleSolution);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings = null)
        {
            if (!table.SequenceEqual(tableCopy))
                return (Result.WrongResult, "Illegal data change");
            if (result != expectedResult)
                return (Result.WrongResult, $"Incorrect result: {result} (expected: {expectedResult})");
            if (exampleSolution == null && expectedResult == true)
                return (Result.WrongResult, "No elements in solution");
            if (exampleSolution != null && expectedResult == false)
                return (Result.WrongResult, $"Solution should be null");
            if (exampleSolution != null && !CheckExampleSolution(distance))
                return (Result.WrongResult, $"Elements in example solution do not match expected result: {expectedResult}");
            return (Result.Success, $"OK (time:{PerformanceTime,6:#0.000})");
        }

    }

    public class LargestMinDistanceTestCase : DistanceTestCase
    {
        private int result;
        private int expectedResult;

        public LargestMinDistanceTestCase(double timeLimit, Exception expectedException, string description, int[] a, int k, int result) : base(timeLimit, expectedException, description, a, k)
        {
            expectedResult = result;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            result = ((Lab07)prototypeObject).LargestMinDistance(tableCopy, k, out exampleSolution);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings = null)
        {
            if (!table.SequenceEqual(tableCopy))
                return (Result.WrongResult, "Illegal data change");
            if (exampleSolution == null || exampleSolution.Count == 0)
                return (Result.WrongResult, "No solution");
            if (result != expectedResult)
                return (Result.WrongResult, $"Incorrect result: {result} (expected: {expectedResult})");
            if (!CheckExampleSolution(result))
                return (Result.WrongResult, $"Elements in example solution do not match expected result: {expectedResult}");
            return (Result.Success, $"OK (time:{PerformanceTime,6:#0.000})");
        }
    }

    class Lab07TestModule : TestModule
    {
        private const int maxElements = 1000000;
        public override void PrepareTestSets()
        {
            Lab07 solver = new Lab07();
            PrepareLabTests(solver);
        }

        private void PrepareLabTests(Lab07 solver)
        {
            TestSets["ElementsInDistance"] = new TestSet(solver, "Part 1 - CanPlaceElementsInDistance");
            TestSets["ElementsInDistance"].TestCases.Add(new CanPlaceElementsInDistanceTestCase(1, null, "Równoodległe 1", new int[] { 1, 2, 3 }, 3, 1, true));
            TestSets["ElementsInDistance"].TestCases.Add(new CanPlaceElementsInDistanceTestCase(1, null, "Równoodległe 2", new int[] { 0, 2, 4, 6, 8, 10 }, 2, 4, true));
            TestSets["ElementsInDistance"].TestCases.Add(new CanPlaceElementsInDistanceTestCase(1, null, "Równoodległe z przerwą", new int[] { 4, 5, 6, 7, 8, 9, 15, 16, 17, 18, 19, 20 }, 5, 5, false));
            TestSets["ElementsInDistance"].TestCases.Add(new CanPlaceElementsInDistanceTestCase(1, null, "Powtórzenie", new int[] { 2, 2, 5, 6, 7, 8, 9, 10 }, 6, 4, false));
            TestSets["ElementsInDistance"].TestCases.Add(new CanPlaceElementsInDistanceTestCase(1, null, "Wiele powtórzeń jednej pozycji", new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 3, 0, true));
            TestSets["ElementsInDistance"].TestCases.Add(new CanPlaceElementsInDistanceTestCase(1, null, "Mały test, niepowodzenie", new int[] { 1, 2, 4, 8, 9 }, 4, 10, false));
            TestSets["ElementsInDistance"].TestCases.Add(new CanPlaceElementsInDistanceTestCase(1, null, "Mały test, powodzenie", new int[] { 0, 5, 6, 7, 8, 10 }, 3, 5, true));

            TestSets["LargestMinDistance"] = new TestSet(solver, "Part 2 - LargestMinDistance");
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Równoodległe 1", new int[] { 1, 2, 3 }, 3, 1));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Równoodległe 2", new int[] { 0, 2, 4, 6, 8, 10 }, 3, 4));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Równoodległe z przerwą", new int[] { 4, 5, 6, 7, 8, 9, 15, 16, 17, 18, 19, 20 }, 3, 5));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Powtórzenie", new int[] { 2, 2, 5, 6, 7, 8, 9, 10 }, 3, 4));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Wiele powtórzeń jednej pozycji", new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 3, 0));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Mały test 1", new int[] { 1, 2, 4, 8, 9 }, 3, 3));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Mały test 2", new int[] { 0, 5, 6, 7, 8, 10 }, 3, 5));

            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, new ArgumentException(), "Zbyt mało pozycji 1", new int[] { 2 }, 2, 0));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, new ArgumentException(), "Zbyt mało pozycji 2", new int[] { 5, 21, 44, 45 }, 5, 0));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, new ArgumentException(), "Zbyt mało obiektów", new int[] { 5, 21, 44, 45 }, 1, 0));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Powtórzenie, wszystkie zajęte", new int[] { 2, 2 }, 2, 0));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Odległa pozycja", new int[] { 2, 4, 5, 7, 9, 11, 20, 200 }, 6, 2));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Równoodległe 3", new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5, 6));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Wiele powtórzeń różnych pozycji", new int[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6 }, 6, 1));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Średni test 1", new int[] { 4, 5, 6, 7, 8, 11, 20, 21, 22, 33, 44, 45, 46, 99 }, 2, 95));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Średni test 2", new int[] { 4, 5, 6, 7, 8, 11, 20, 21, 22, 33, 44, 45, 46, 99 }, 4, 18));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Średni test 3", new int[] { 4, 5, 6, 7, 8, 11, 20, 21, 22, 33, 44, 45, 46, 99 }, 5, 13));
            TestSets["LargestMinDistance"].TestCases.Add(new LargestMinDistanceTestCase(1, null, "Średni test 4", new int[] { 1, 2, 4, 5, 7, 8, 10, 43, 76, 99 }, 4, 23));
        }

    }

    class Lab07Main
    {
        static void Main(string[] args)
        {
            Lab07TestModule lab07test = new Lab07TestModule();
            lab07test.PrepareTestSets();

            foreach (var ts in lab07test.TestSets)
            {
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }
    }
}