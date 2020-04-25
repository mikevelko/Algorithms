using System;
using System.Collections.Generic;

namespace ASD
{
    public class Lab07 : MarshalByRefObject
    {
        private const int maxBuildings = 1000000;
        private const int minBuildings = 2;

        /// <summary>
        /// funkcja do sprawdzania czy da się wybudować k obiektów w odległości co najmniej dist od siebie
        /// </summary>
        /// <param name = "a" > posortowana tablica potencjalnych lokalizacji obiektów</param>
        /// <param name = "dist" > zadany dystans</param>
        //    / <param name = "k" > liczba obiektów do wybudowania</param>
        //    / <param name = "exampleSolution" > wybrane lokalizacje</param>
        //    / <returns>true - jeśli zadanie da się zrealizować</returns>
        public bool CanPlaceBuildingsInDistance(int[] a, int dist, int k, out List<int> exampleSolution)
        {
            
            int iter = 1;
            int prev = a[0];
            exampleSolution = new List<int>();
            exampleSolution.Add(prev);
            for (int i = 1; i < a.Length ; i++) 
            {
                if (a[i] - prev >= dist) 
                {
                    prev = a[i];
                    iter++;
                    exampleSolution.Add(a[i]);
                    if (iter == k) return true;
                }
            }
            if (iter > 0) 
            {
                exampleSolution = null;
            }

            return false;
        }
       
        /// <summary>
        /// Funkcja wybiera k lokalizacji z tablicy a, tak aby minimalny dystans
        /// pomiędzy dowolnymi dwoma lokalizacjami (spośród wybranych) był maksymalny
        /// </summary>
        /// <param name="a">posortowana tablica potencjalnych lokalizacji</param>
        /// <param name="k">liczba lokalizacji do wybrania</param>
        /// <param name="exampleSolution">wybrane lokalizacje</param>
        /// <returns>Maksymalny dystans jaki można uzyskać pomiędzy dwoma najbliższymi budynkami</returns>
        public int LargestMinDistance(int[] a, int k, out List<int> exampleSolution)
        {
            if (a.Length < 1 || a.Length > 1000000) throw new System.ArgumentException();
            if(k<2 || k>a.Length) throw new System.ArgumentException();
            
            uint wynik = 0;
            uint left = 0, right = int.MaxValue;

            while (left <= right)
            {
                uint mid = (right+left)/2 ;
                if (CanPlaceBuildingsInDistance(a, (int)mid,k, out exampleSolution))
                {
                    wynik = mid;
                    left = mid + 1;
                }
                else
                {
                    right = mid-1;
                }
            }
            CanPlaceBuildingsInDistance(a, (int)wynik, k, out exampleSolution);
            return (int)wynik;
            
        }

    }

}
