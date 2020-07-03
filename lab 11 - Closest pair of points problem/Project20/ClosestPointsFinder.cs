using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ASD
{
    [Serializable]
    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point(double x, double y) { X = x; Y = y; }
        public override string ToString() => $"({X};{Y})";
    }

    public class ClosestPointsFinder : MarshalByRefObject
    {
        /// <summary>
        /// Szukanie najbliższej pary punktów (Złożoność O(n*logn)).
        /// </summary>
        /// <param name="points">
        /// Tablica unikatowych punktów wśród, który wyznaczana jest najbliższa para punktów.
        /// </param>
        /// <returns>
        /// Znaleziona najbliżej położona para punktów. Jeżeli jest kilka należy zwrócić dowolną.
        /// </returns>
        public (Point P1, Point P2) findClosestPoints(Point[] points)
        {
            Point[] SortedPointsX = points.OrderBy(p => p.X).ToArray();
            
            
            return closestUtil(SortedPointsX, points.Length);
        }
        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X)*(p1.X - p2.X) + (p1.Y - p2.Y)*(p1.Y - p2.Y));
        }
        private (Point p1,Point p2) BruteForce(Point[] P)
        {
            double min = double.MaxValue;
            (Point p1, Point p2) points = (new Point(), new Point());
            for (int i = 0; i < P.Length; ++i)
                for (int j = i + 1; j < P.Length; ++j)
                    if (Distance(P[i], P[j]) < min)
                    {
                        min = Distance(P[i], P[j]);
                        points = (P[i], P[j]);
                    }
            return points;
        }
        private double Min(double x, double y)
        {
            return (x < y) ? x : y;
        }
        private (Point p1, Point p2) closestUtil(Point[] Px, int n)
        {
            if (n <= 3)
                return BruteForce(Px);
 
            int mid = n / 2;
            Point midPoint = Px[mid];

            Point[] Pxl = new Point[mid];   
            Point[] Pxr = new Point[n - mid];  
            int li = 0, ri = 0;  
            for (int i = 0; i < n; i++)
            {
                if (Px[i].X <= midPoint.X && li < mid)
                    Pxl[li++] = Px[i];
                else
                    Pxr[ri++] = Px[i];
            }

        
            
            (Point left1, Point left2) = closestUtil(Pxl, mid);
            (Point right1, Point right2) = closestUtil(Pxr, n - mid);

            double dl, dr;
            
            dl = Distance(left1, left2);
            dr = Distance(right1, right2); 
            double dMin = Min(dl, dr);

            (Point best1, Point best2) best;
            if (dl < dr)
            {
                best = (left1, left2);
            }
            else 
            {
                best = (right1, right2);
            }

            // Build an array strip[] that contains points close (closer than d) 
            // to the line passing through the middle point 
            List<Point> PointList = new List<Point>();
            for (int i = 0; i < n; i++)
                if (Math.Abs(Px[i].X - midPoint.X) < dMin)
                    PointList.Add(Px[i]);

            Point[] Band = PointList.OrderBy(p => p.Y).ToArray();
            
            return stripClosest(Band, dMin,best.best1,best.best2);
        }
        (Point p1,Point p2) stripClosest(Point[] strip, double d,Point best1, Point best2)
        {
            double min = d;  // Initialize the minimum distance as d 
            (Point p1, Point p2) points = (best1, best2);
            // Pick all points one by one and try the next points till the difference 
            // between y coordinates is smaller than d. 
            // This is a proven fact that this loop runs at most 6 times 
            for (int i = 0; i < strip.Length; ++i)
                for (int j = i + 1; j < strip.Length && (strip[j].Y - strip[i].Y) < min; ++j)
                    if (Distance(strip[i], strip[j]) < min)
                    {
                        min = Distance(strip[i], strip[j]);
                        points = (strip[i], strip[j]);
                    }
            return points;
        }
    }
}
