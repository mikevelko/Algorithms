using System;
using System.Collections.Generic;

namespace ASD
{

    abstract class L13TestCaseBase : TestCase
    {
        protected double result;
        protected double expectedResult;
        readonly double epsilon = 0.000001;

        protected L13TestCaseBase(double timeLimit, string description, double expectedResult) : base(timeLimit, null, description)
        {
            this.expectedResult = expectedResult;
        }
        protected override (Result, string) VerifyTestCase(object settings)
        {
            return Math.Abs(result-expectedResult)<epsilon ?
                (Result.Success, $"OK (time:{PerformanceTime,6:#0.000})") :
                (Result.WrongResult, $"Zły wynik - otrzymano: {result:R}, spodziewany wynik: {expectedResult:0.000}") ;
        }

    protected static bool AreNotEqual(Point p1, Point p2) => p1.x!=p2.x || p1.y!=p2.y ;
    }

    class L13TestCaseSegment : L13TestCaseBase
    {
        private (double y1, double y2)[] segments;
        private (double y1, double y2)[] segments_original;

        public L13TestCaseSegment(double timeLimit, string description, (double y1, double y2)[] segments, double expectedResult) : base(timeLimit, description, expectedResult)
        {
            segments_original = segments;
            this.segments = new (double y1, double y2)[segments.Length];
            segments_original.CopyTo(this.segments, 0);
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            if (prototypeObject is SweepLine sl)
            {
                result = sl.VerticalSegmentsUnionLength(segments);
            }
        }

        protected override (Result, string) VerifyTestCase(object settings)
        {
            if (segments_original.Length != segments.Length) return (Result.WrongResult, "Tablica odcinków została zmodyfikowana.");
            for (int i = 0; i < segments.Length; i++)
                if ( segments[i].y1!=segments_original[i].y1 || segments[i].y2!=segments_original[i].y2)
                    return (Result.WrongResult, "Tablica odcinków została zmodyfikowana.");
            return base.VerifyTestCase(settings);
        }
    }

    class L13TestCaseRectangle : L13TestCaseBase
    {
        private (Point p1, Point p2)[] rectangles;
        private (Point p1, Point p2)[] rectangles_original;

        public L13TestCaseRectangle(double timeLimit, string description, (Point p1, Point p2)[] rectangles, double expectedResult) : base(timeLimit, description, expectedResult)
        {
            rectangles_original = rectangles;
            this.rectangles = new (Point ps, Point pe)[rectangles.Length];
            rectangles_original.CopyTo(this.rectangles, 0);
        }
        protected override void PerformTestCase(object prototypeObject)
        {
            if (prototypeObject is SweepLine sl)
            {
                result = sl.RectanglesUnionArea(rectangles);
            }
        }

        protected override (Result, string) VerifyTestCase(object settings)
        {
            if (rectangles_original.Length != rectangles.Length) return (Result.WrongResult, "Tablica prostokątów została zmodyfikowana.");
            for (int i = 0; i < rectangles.Length; i++)
                if ( AreNotEqual(rectangles[i].p2, rectangles_original[i].p2) || AreNotEqual(rectangles[i].p1, rectangles_original[i].p1) ) 
                    return (Result.WrongResult, "Tablica prostokątów została zmodyfikowana.");
            return base.VerifyTestCase(settings);
        }
    }

    public class SweepingTestModule : TestModule
    {
        readonly string LABS = "LABS";
        readonly string LABR = "LABR";

        private (double y1, double y2)[] createRandomTestSegmentsLab(Random r, int size)
        {
            var testSegment = new List<(double y1, double y2)>();
            for (int i = 0; i < size; i++)
            {
                double ys = -5000 + 10000*r.NextDouble();
                double yd = -100 + 200*r.NextDouble();
                testSegment.Add((ys,ys+yd));
            }
            return testSegment.ToArray();
        }

        private (Point p1, Point p2)[] createRandomTestRectanglesLab(Random r, int size)
        {
            List<(Point p1, Point p2)> testRectangle = new List<(Point p1, Point p2)>();
            for (int i = 0; i < size; i++)
            {
                double xs = -50 + 100*r.NextDouble();
                double xd = -10 + 20*r.NextDouble();
                double ys = -50 + 100*r.NextDouble();
                double yd = -10 + 20*r.NextDouble();
                testRectangle.Add((new Point(xs, ys), new Point(xs+xd, ys+yd)));
            }
            return testRectangle.ToArray();
        }

        public override void PrepareTestSets()
        {
            InitLabSegmentsTest();
            InitLabRectangleTest();
        }

        private void InitLabSegmentsTest()
        {
            TestSets[LABS] = new TestSet(new SweepLine(), "Testy odcinków - Lab");

            var testSegment1 = new List<(double y1, double y2)>();
            testSegment1.Add((-2,-1));
            TestSets[LABS].TestCases.Add(new L13TestCaseSegment(1, "Jeden odcinek", testSegment1.ToArray(), 1));

            var testSegment2 = new List<(double y1, double y2)>();
            testSegment2.Add((-1, -2));
            TestSets[LABS].TestCases.Add(new L13TestCaseSegment(1, "Jeden odcinek", testSegment2.ToArray(), 1));

            var testSegment3 = new List<(double y1, double y2)>();
            testSegment3.Add((-2, -1));
            testSegment3.Add(( 0, 1));
            TestSets[LABS].TestCases.Add(new L13TestCaseSegment(1, "Odcinki rozłączne", testSegment3.ToArray(), 2));

            var testSegment4 = new List<(double y1, double y2)>();
            testSegment4.Add((-2, -1));
            testSegment4.Add((0, 1));
            testSegment4.Add((1, 2));
            TestSets[LABS].TestCases.Add(new L13TestCaseSegment(1, "Odcinki ze wspólnym wierzchołkiem", testSegment4.ToArray(), 3));

            var testSegment5 = new List<(double y1, double y2)>();
            testSegment5.Add((-2, -1));
            testSegment5.Add((0, 1));
            testSegment5.Add((0.5, 2));
            TestSets[LABS].TestCases.Add(new L13TestCaseSegment(1, "Odcinki czesciowa nakładające się", testSegment5.ToArray(), 3));

            var testSegment9 = new List<(double y1, double y2)>();
            testSegment9.Add((4, 1));
            testSegment9.Add((2, 3));
            TestSets[LABS].TestCases.Add(new L13TestCaseSegment(1, "Odcinek zawierający inny odcinek", testSegment9.ToArray(), 3));

            int[] size = { 100, 200, 500, 1000 };
            double[] res = { 3778.5778521460375, 6349.876989494016, 9439.0259018349589, 10014.257179160726 };

            for (int i = 0; i < size.Length; i++)
            {
                var test = createRandomTestSegmentsLab(new Random(345+2*i), size[i]);
                TestSets[LABS].TestCases.Add(new L13TestCaseSegment(1, $"Losowy test odcinków {i + 1} - rozmiar {size[i]}", test, res[i]));
            };
        }

        private void InitLabRectangleTest()
        {
            TestSets[LABR] = new TestSet(new SweepLine(), "Testy prostokątów - Lab");

            var testRectangle1 = new List<(Point p1, Point p2)>();
            testRectangle1.Add((new Point(-1, -1), new Point(1, 1)));
            TestSets[LABR].TestCases.Add(new L13TestCaseRectangle(1, "Jeden prostokąt", testRectangle1.ToArray(), 4));

            var testRectangle2 = new List<(Point p1, Point p2)>();
            testRectangle2.Add((new Point(-1, -1), new Point(1, 1)));
            testRectangle2.Add((new Point(-2, -2), new Point(2, 2)));
            TestSets[LABR].TestCases.Add(new L13TestCaseRectangle(1, "Dwa prostokąty zawierające się", testRectangle2.ToArray(), 16));

            var testRectangle3 = new List<(Point p1, Point p2)>();
            testRectangle3.Add((new Point(-1, -1), new Point(1, 1)));
            testRectangle3.Add((new Point(-2, -2), new Point(2, 2)));
            testRectangle3.Add((new Point(2, -2), new Point(3, 2)));
            TestSets[LABR].TestCases.Add(new L13TestCaseRectangle(1, "Trzy prostokąty", testRectangle3.ToArray(), 20));

            var testRectangle4 = new List<(Point p1, Point p2)>();
            testRectangle4.Add((new Point(-1, -1), new Point(1, 1)));
            testRectangle4.Add((new Point(-2, 2), new Point(2, -2)));
            testRectangle4.Add((new Point(3, 2), new Point(2, -2)));
            testRectangle4.Add((new Point(1.5, -3), new Point(2.5, -1.5)));
            TestSets[LABR].TestCases.Add(new L13TestCaseRectangle(1, "Cztery prostokąty", testRectangle4.ToArray(), 21));

            var testRectangle5 = new List<(Point p1, Point p2)>();
            testRectangle5.Add((new Point(0, 0), new Point(0, 0)));
            testRectangle5.Add((new Point(0, 0), new Point(0, 0)));
            testRectangle5.Add((new Point(1, 1), new Point(1, 1)));
            testRectangle5.Add((new Point(2, 3), new Point(2, 3)));
            TestSets[LABR].TestCases.Add(new L13TestCaseRectangle(1, "Prostokąty o zerowym polu", testRectangle5.ToArray(), 0));

            int[] size = { 100, 200, 500, 1000 };
            double[] res = { 2148.3711864910397, 3610.0019548657388, 7931.1004109868345, 10372.171663805817 }; 

            for (int i = 0; i < size.Length; i++)
            {
                var test = createRandomTestRectanglesLab(new Random(123+2*i), size[i]);
                TestSets[LABR].TestCases.Add(new L13TestCaseRectangle(1, $"Losowy test prostokątów {i + 1} - rozmiar {size[i]}", test, res[i]));
            };
        }

        public override double ScoreResult()
        {
            return 3.0;
        }

    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var lab13tests = new SweepingTestModule();
            lab13tests.PrepareTestSets();
            foreach (var ts in lab13tests.TestSets)
                ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
        }
    }
}

