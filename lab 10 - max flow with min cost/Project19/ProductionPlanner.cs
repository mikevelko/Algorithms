using System;
using System.Linq;
using ASD.Graphs;

namespace ASD
{
    public class ProductionPlanner : MarshalByRefObject
    {
        /// <summary>
        /// Flaga pozwalająca na włączenie wypisywania szczegółów skonstruowanego planu na konsolę.
        /// Wartość <code>true</code> spoeoduje wypisanie planu.
        /// </summary>
        public bool ShowDebug { get; } = false;

        /// <summary>
        /// Część 1. zadania - zaplanowanie produkcji telewizorów dla pojedynczego kontrahenta.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających maksymalną produkcję i zysk wymagane jest jedynie zwrócenie krotki.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica krotek zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartości pola Quantity oznaczają limit produkcji w danym tygodniu,
        /// a pola Value - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Tablica krotek zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Wartości pola Quantity oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola Value - cenę sprzedaży jednej sztuki.
        /// </param>
        /// <param name="storageInfo">
        /// Krotka zawierająca informacje o magazynie.
        /// Wartość pola Quantity oznacza pojemność magazynu,
        /// a pola Value - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Krotka opisująca wyznaczony plan.
        /// W polu Quantity powinna znaleźć się maksymalna liczba wyprodukowanych telewizorów,
        /// a w polu Value - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public (int Quantity, double Value) CreateSimplePlan((int Quantity, double Value)[] production, (int Quantity, double Value)[] sales,
            (int Quantity, double Value) storageInfo, out (int UnitsProduced, int UnitsSold, int UnitsStored)[] weeklyPlan)
        {
            if (production.Length < 1 || production.Length != sales.Length || storageInfo.Quantity < 0 || storageInfo.Value < 0) throw new ArgumentException();
            int count = production.Length;
            int entry = count;
            int exit = entry + 1;
            Graph values = new AdjacencyListsGraph<HashTableAdjacencyList>(true, count+2);
            Graph quantity = new AdjacencyListsGraph<HashTableAdjacencyList>(true, count + 2);
            for (int i = 0; i < count; i++) 
            {
                if (production[i].Quantity < 0 || production[i].Value < 0 || sales[i].Quantity < 0 || sales[i].Value < 0) throw new ArgumentException();
                values.AddEdge(entry, i, production[i].Value);
                values.AddEdge(i, exit, -sales[i].Value);
                quantity.AddEdge(entry, i, production[i].Quantity);
                quantity.AddEdge(i, exit, sales[i].Quantity);
            }
            for (int i = 0; i < count - 1; i++) 
            {
                values.AddEdge(i, i + 1, storageInfo.Value);
                quantity.AddEdge(i, i + 1, storageInfo.Quantity);
            }
            (double value, double cost, Graph flow) = quantity.MinCostFlow(values,entry,exit,false, MaxFlowGraphExtender.PushRelabelMaxFlow, MaxFlowGraphExtender.MKMBlockingFlow);
            weeklyPlan = new (int UnitsProduced, int UnitsSold, int UnitsStored)[count];
            for (int i = 0; i < count; i++) 
            {
                weeklyPlan[i].UnitsProduced = (int)flow.GetEdgeWeight(entry, i);
                weeklyPlan[i].UnitsSold = (int)flow.GetEdgeWeight(i, exit);
            }
            weeklyPlan[count-1].UnitsStored = 0; 
            for (int i = 0; i < count - 1; i++) 
            {
                weeklyPlan[i].UnitsStored = (int)flow.GetEdgeWeight(i, i + 1);
            }
            return ((int)value, -cost);
        }

        /// <summary>
        /// Część 2. zadania - zaplanowanie produkcji telewizorów dla wielu kontrahentów.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających produkcję dającą maksymalny zysk wymagane jest jedynie zwrócenie krotki.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica krotek zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartość pola Quantity oznacza limit produkcji w danym tygodniu,
        /// a pola Value - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Dwuwymiarowa tablica krotek zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Pierwszy wymiar tablicy jest równy liczbie kontrahentów, zaś drugi - liczbie tygodni w planie.
        /// Wartości pola Quantity oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola Value - cenę sprzedaży jednej sztuki.
        /// Każdy wiersz tablicy odpowiada jednemu kontrachentowi.
        /// </param>
        /// <param name="storageInfo">
        /// Krotka zawierająca informacje o magazynie.
        /// Wartość pola Quantity oznacza pojemność magazynu,
        /// a pola Value - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Krotka opisujący wyznaczony plan.
        /// W polu Quantity powinna znaleźć się optymalna liczba wyprodukowanych telewizorów,
        /// a w polu Value - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public (int Quantity, double Value) CreateComplexPlan((int Quantity, double Value)[] production, (int Quantity, double Value)[,] sales, (int Quantity, double Value) storageInfo,
            out (int UnitsProduced, int[] UnitsSold, int UnitsStored)[] weeklyPlan)
        {
            weeklyPlan = null;
            return (-1, -1.0);
        }

    }

}