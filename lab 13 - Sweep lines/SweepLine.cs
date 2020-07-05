
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{

    // Niw wolno modyfikować struktury Point !!!
    [Serializable]
    public struct Point
    {
        public double x;
        public double y;
        public Point(double px, double py) { x = px; y = py; }
    }

    public class SweepLine : MarshalByRefObject
    {

    // można zdafiniowac prywatną strukturę pomocniczą opisującą zdarzenie

        /// <summary>
        /// Funkcja obliczająca długość teoriomnogościowej sumy pionowych odcinków
        /// </summary>
        /// <param name="segments">Tablica z odcinkami, których długość sumy teoriomnogościowej należy policzyć</param>
        /// <returns>Długość teoriomnogościowej sumy pionowych odcinków</returns>
        public double VerticalSegmentsUnionLength((double y1, double y2)[] segments)
        {
        return -1.0;
        }

        /// <summary>
        /// Funkcja obliczająca pole teoriomnogościowej sumy prostokątów
        /// </summary>
        /// <param name="rectangles">Tablica z prostokątami, których pole sumy teoriomnogościowej należy policzyć</param>
        /// <returns>Pole teoriomnogościowej sumy prostokątów</returns>
        public double RectanglesUnionArea((Point p1, Point p2)[] rectangles)
        {
        return -1.0;
        }

    }

}
