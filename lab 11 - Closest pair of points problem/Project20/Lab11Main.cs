using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    public class ClosestPointsFinderTestCase : TestCase
    {
        private readonly Point[] points;
        private readonly Point[] pointsCopy;

        private readonly (Point P1, Point P2)? expectedClosestPairPoints;
        private readonly double expectedDistance;
        private readonly bool checkOnlyDistance;

        private (Point P1, Point P2) closestPairPoints;

        public ClosestPointsFinderTestCase(double timeLimit, Exception expectedException, string description, (Point[] Points, bool CheckOnlyDistance,
            (Point P1, Point P2)? ExpectedClosestPairPoints, double ExpectedDistance) data) : base(timeLimit, expectedException, description)
        {
            points = data.Points;
            pointsCopy = (Point[])points.Clone();
            expectedClosestPairPoints = data.ExpectedClosestPairPoints;
            expectedDistance = data.ExpectedDistance;
            checkOnlyDistance = data.CheckOnlyDistance;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            var finder = (ClosestPointsFinder)prototypeObject;
            closestPairPoints = finder.findClosestPoints(points);
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            if (!points.SequenceEqual(pointsCopy))
            {
                return (Result.WrongResult, "Input data was modified");
            }

            if (!checkOnlyDistance)
            {
                if (!((expectedClosestPairPoints.Value.P1.Equals(closestPairPoints.P1) && expectedClosestPairPoints.Value.P2.Equals(closestPairPoints.P2)) ||
                    (expectedClosestPairPoints.Value.P2.Equals(closestPairPoints.P1) && expectedClosestPairPoints.Value.P1.Equals(closestPairPoints.P2))))
                    return (Result.WrongResult, $"Wrong closest points! Expected result: {expectedClosestPairPoints}, your result: {closestPairPoints}");
            }

            double distance = LabTests.Length(closestPairPoints);

            if (Math.Abs(expectedDistance - LabTests.Length(closestPairPoints)) > 1e-4)
            {
                return (Result.WrongResult, $"Wrong distance! Expected result: {expectedDistance:F6}, your result: {distance:F6}");
            }

            return (Result.Success, $"OK, time: {PerformanceTime:F3}");
        }


    }

    public static class LabTests
    {
        public static double Length((Point P1, Point P2) segment)
        {
            double dx = segment.P1.X - segment.P2.X;
            double dy = segment.P1.Y - segment.P2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static (Point[] Points, bool CheckOnlyDistance, (Point P1, Point P2)? ExpectedClosestPairPoints, double ExpectedDistance) TwoPointsTest()
        {
            //2 punkty
            var points = new[] { new Point(10, 10), new Point(11, 11) };
            var properResult = (new Point(10, 10), new Point(11, 11));

            return (points, false, properResult, 1.4142135623731);
        }

        public static (Point[] Points, bool CheckOnlyDistance, (Point P1, Point P2)? ExpectedClosestPairPoints, double ExpectedDistance) SameXTest()
        {
            //wszystkie z tą samą x-owa wspołrzędną
            var points = new[] { new Point(10, 9.5), new Point(10, 10), new Point(10, 11), new Point(10, 12), new Point(10, 13), new Point(10, 14), new Point(10, 15), new Point(10, 16) };
            var properResult = (new Point(10, 9.5), new Point(10, 10));

            return (points, false, properResult, 0.5);
        }

        public static (Point[] Points, bool CheckOnlyDistance, (Point P1, Point P2)? ExpectedClosestPairPoints, double ExpectedDistance) SameYTest()
        {
            //wszystkie z tą samą y-owa wspołrzędną
            var points = new[] { new Point(1, 10), new Point(2, 10), new Point(3.5, 10), new Point(4, 10), new Point(5, 10), new Point(6, 10) };
            var properResult = (new Point(3.5, 10), new Point(4, 10));

            return (points, false, properResult, 0.5);
        }

        public static (Point[] Points, bool CheckOnlyDistance, (Point P1, Point P2)? ExpectedClosestPairPoints, double ExpectedDistance) ThreePointsTest()
        {
            //3 punkty
            var points = new[] { new Point(10, 10), new Point(11, 11), new Point(15, -17) };
            var properResult = (new Point(10, 10), new Point(11, 11));

            return (points, false, properResult, 1.4142135623731);
        }

        public static (Point[] Points, bool CheckOnlyDistance, (Point P1, Point P2)? ExpectedClosestPairPoints, double ExpectedDistance) SquareTest()
        {
            //kwadrat
            var points = new[] { new Point(10, 10), new Point(-10, 10), new Point(10, -10), new Point(-10, -10) };
            var properResult = (new Point(10, 10), new Point(-10, 10));

            return (points, true, properResult, 20);
        }

        public static (Point[] Points, bool CheckOnlyDistance, (Point P1, Point P2)? ExpectedClosestPairPoints, double ExpectedDistance) RandomTest(int count, int minX = -100, int maxX = 100, int minY = -100, int maxY = 100, double xFactor = 1, double yFactor = 1, int seed = 1024, double eps = 1e-4F)
        {
            //chmura punktow, najblizsze generowane automatycznie
            Random random = new Random(seed);
            var points = Enumerable.Repeat(0, count).Select(i => new Point(random.Next(minX, maxX), random.Next(minY, maxY))).Distinct().ToList();
            var p1 = points[random.Next(0, points.Count())];
            var p2 = new Point(p1.X + xFactor * eps, p1.Y + yFactor * eps);
            points.Add(p2);
            var properResult = (p1, p2);

            return (points.ToArray(), false, properResult, LabTests.Length(properResult));
        }
    }

    public class Lab11TestModule : TestModule
    {
        public override void PrepareTestSets()
        {
            var subjectUnderTest = new ClosestPointsFinder();

            var labClosestPairTests = new TestSet(
                subjectUnderTest,
                "Laboratorium - Szukanie najbliższej pary punktów"
            );
            labClosestPairTests.TestCases.AddRange(new List<TestCase>
            {
                new ClosestPointsFinderTestCase(1, null, "Dwa punkty", LabTests.TwoPointsTest()),
                new ClosestPointsFinderTestCase(1, null, "Taka sama współrzędna x", LabTests.SameXTest()),
                new ClosestPointsFinderTestCase(1, null, "Taka sama współrzędna y", LabTests.SameYTest()),
                new ClosestPointsFinderTestCase(1, null, "Trzy punkty", LabTests.ThreePointsTest()),
                new ClosestPointsFinderTestCase(1, null, "Kwadrat", LabTests.SquareTest()),
                new ClosestPointsFinderTestCase(1, null, "500 losowych punktów", LabTests.RandomTest(500, yFactor: -2)),
                new ClosestPointsFinderTestCase(1, null, "1000 losowych punktów", LabTests.RandomTest(1000, yFactor: -2)),
                new ClosestPointsFinderTestCase(1, null, "1500 losowych punktów", LabTests.RandomTest(1500, yFactor: -2)),
                new ClosestPointsFinderTestCase(1, null, "2000 losowych punktów", LabTests.RandomTest(2000, yFactor: -2)),
                new ClosestPointsFinderTestCase(3, null, "50000 losowych punktów", LabTests.RandomTest(50000, -10000, 10000, yFactor: -2)),
            });

            TestSets = new Dictionary<string, TestSet>
            {
                {"LabClosestPointsTests", labClosestPairTests},
            };
        }

        public override double ScoreResult()
        {
            return 3.0;
        }

    }

    public class Lab11Main
    {
        public static void Main()
        {
            var testModule = new Lab11TestModule();
            testModule.PrepareTestSets();
            foreach (var testSet in testModule.TestSets)
            {
                testSet.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }
    }
}
