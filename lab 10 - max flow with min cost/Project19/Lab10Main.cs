using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASD
{

    public class SimpleProductionPlanTestCase : TestCase
    {
        private readonly (int Quantity, double Value)[] production;
        private readonly (int Quantity, double Value)[] sales;
        private readonly (int Quantity, double Value) storageInfo;

        private readonly (int Quantity, double Value)[] productionCopy;
        private readonly (int Quantity, double Value)[] salesCopy;

        private readonly int expectedMaxProduction;
        private readonly double expectedMaxProfit;

        private (int Quantity, double Value) productionPlan;
        private (int UnitsProduced, int UnitsSold, int UnitsStored)[] weeklyPlan;

        public SimpleProductionPlanTestCase(double timeLimit, Exception expectedException, string description, 
            ((int Quantity, double Value)[], (int Quantity, double Value)[], (int Quantity, double Value)) inputData,
            int expectedMaxProduction, double expectedMaxProfit) : base(timeLimit, expectedException, description)
        {
            (production, sales, storageInfo) = inputData;
            productionCopy = ((int Quantity, double Value)[])production.Clone();
            salesCopy = ((int Quantity, double Value)[])sales.Clone();
            // storageInfo nie kopiujemy, bo nie ma potrzeby - jest strukturą

            this.expectedMaxProduction = expectedMaxProduction;
            this.expectedMaxProfit = expectedMaxProfit;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            var planner = (ProductionPlanner)prototypeObject;
            productionPlan = planner.CreateSimplePlan(production, sales, storageInfo, out weeklyPlan);

            if (planner.ShowDebug)
            {
                Console.WriteLine();
                Console.WriteLine("\n--- START OF DEBUG INFO ---");
                for (int i = 0; i < weeklyPlan.Length; ++i)
                {
                    Console.WriteLine($"Week {i,4}: Produced: {weeklyPlan[i].UnitsProduced}, sold: {weeklyPlan[i].UnitsSold}, stored: {weeklyPlan[i].UnitsStored}");
                }
                Console.WriteLine("--- END OF DEBUG INFO ---\n");
            }
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            int weeks = production.Length;
            int unitsProduced = 0;
            int unitsInStorage = 0;
            double productionExpenses = 0;
            double storageExpenses = 0;
            double salesProfits = 0;

            if (!production.SequenceEqual(productionCopy) || !sales.SequenceEqual(salesCopy))
            {
                return (Result.WrongResult, "Input data was modified");
            }

            if (productionPlan.Quantity != expectedMaxProduction)
            {
                return (Result.WrongResult, $"Wrong number of units produced: expected {expectedMaxProduction}, method returned {productionPlan.Quantity}");
            }

            if (Math.Abs(productionPlan.Value - expectedMaxProfit) > 1e-4)
            {
                return (Result.WrongResult, $"Wrong profit calculated: expected {expectedMaxProfit}, method returned {productionPlan.Value}");
            }

            if(weeklyPlan == null)
            {
                return (Result.WrongResult, "No plan returned");
            }
            if (weeks != weeklyPlan.Length)
            {
                return (Result.WrongResult, $"Bad plan - wrong number of weeks; expected {weeks} weeks, plan has length {weeklyPlan.Length}");
            }

            for (int i = 0; i < weeks; ++i)
            {
                (int UnitsProduced, int UnitsSold, int UnitsStored) planStep = (weeklyPlan[i].UnitsProduced, weeklyPlan[i].UnitsSold, weeklyPlan[i].UnitsStored);
                if (planStep.UnitsProduced < 0 || planStep.UnitsSold < 0 || planStep.UnitsStored < 0)
                {
                    return (Result.WrongResult, $"Bad plan - week {i} specifies negative amounts");
                }
                if (planStep.UnitsProduced > production[i].Quantity)
                {
                    return (Result.WrongResult, $"Bad plan - production in week {i} exceeds maximum supply limit");
                }
                if (planStep.UnitsStored > storageInfo.Quantity)
                {
                    return (Result.WrongResult, $"Bad plan - storage capacity exceeded in week {i}");
                }
                if (planStep.UnitsSold > sales[i].Quantity)
                {
                    return (Result.WrongResult, $"Bad plan - sales in week {i} exceed maximum demand limit");
                }
                // Sprawdzamy, czy są sztuki, z którymi nic nie zrobiono
                if (planStep.UnitsProduced + unitsInStorage > planStep.UnitsStored + planStep.UnitsSold)
                {
                    return (Result.WrongResult, $"Bad plan - some units in week {i} have not been either sold or stored");
                }
                if (planStep.UnitsProduced + unitsInStorage < planStep.UnitsStored + planStep.UnitsSold)
                {
                    return (Result.WrongResult, $"Bad plan - nonexistent units sold or stored in week {i}");
                }
                // Produkcja
                unitsProduced += planStep.UnitsProduced;
                productionExpenses += planStep.UnitsProduced * production[i].Value;
                // Sprzedaż
                salesProfits += planStep.UnitsSold * sales[i].Value;
                // Wstawienie do magazynu
                unitsInStorage = planStep.UnitsStored;
                // Naliczenie kosztu za przetrzymanie
                storageExpenses += planStep.UnitsStored * storageInfo.Value;
            }

            if (unitsProduced != productionPlan.Quantity)
            {
                return (Result.WrongResult, $"Bad plan: total number of units produced is {productionPlan.Quantity}, " +
                          $"following plan results in {unitsProduced} units");
            }

            var profit = salesProfits - productionExpenses - storageExpenses;
            if (Math.Abs(profit - productionPlan.Value) > 1e-4)
            {
                return (Result.WrongResult, $"Bad plan: net profit is {productionPlan.Value}, " +
                          $"following plan results in profit of {profit}");
            }
            return (Result.Success, $"OK, time: {PerformanceTime:F3}");
        }
    }

    public class ComplexProductionPlanTestCase : TestCase
    {
        private readonly (int Quantity, double Value)[] production;
        private readonly (int Quantity, double Value)[,] sales;
        private readonly (int Quantity, double Value) storageInfo;

        private readonly (int Quantity, double Value)[] productionCopy;
        private readonly (int Quantity, double Value)[,] salesCopy;

        private readonly double expectedMaxProfit;

        private (int Quantity, double Value) productionPlan;
        private (int UnitsProduced, int[] UnitsSold, int UnitsStored)[] weeklyPlan;

        public ComplexProductionPlanTestCase(double timeLimit, Exception expectedException, string description,
            ((int Quantity, double Value)[], (int Quantity, double Value)[,], (int Quantity, double Value)) inputData, double expectedMaxProfit) : base(timeLimit, expectedException, description)
        {
            (production, sales, storageInfo) = inputData;
            productionCopy = ((int Quantity, double Value)[])production.Clone();
            salesCopy = ((int Quantity, double Value)[,])sales.Clone();
            // storageInfo nie kopiujemy, bo nie ma potrzeby - jest strukturą

            this.expectedMaxProfit = expectedMaxProfit;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            var planner = (ProductionPlanner)prototypeObject;
            productionPlan = planner.CreateComplexPlan(production, sales, storageInfo, out weeklyPlan);
            if (planner.ShowDebug)
            {
                Console.WriteLine();
                Console.WriteLine("\n--- START OF DEBUG INFO ---");
                for (int i = 0; i < weeklyPlan.Length; ++i)
                {
                    Console.Write($"Week {i,4}: ");
                    var planStep = weeklyPlan[i];
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"Produced: {planStep.UnitsProduced,4}, stored: {planStep.UnitsStored,4}");
                    stringBuilder.AppendLine("\tSales plan:");
                    for (int j = 0; j < planStep.UnitsSold.Length; ++j)
                    {
                        stringBuilder.AppendLine($"\t\tCustomer {j}: sold {planStep.UnitsSold[j]}");
                    }
                    Console.WriteLine(stringBuilder.ToString());
                }
                Console.WriteLine("--- END OF DEBUG INFO ---\n");
            }
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            int weeks = production.Length;
            int customers = sales.GetLength(0);
            int unitsProduced = 0;
            int unitsInStorage = 0;
            double productionExpenses = 0;
            double storageExpenses = 0;
            double salesProfits = 0;

            if (!production.SequenceEqual(productionCopy))
            {
                return (Result.WrongResult, "Input data was modified");
            }
            for (var j = 0; j < salesCopy.GetLength(0); ++j)
            {
                for (var i = 0; i < salesCopy.GetLength(1); ++i)
                {
                    if (!salesCopy[j, i].Equals(sales[j, i]))
                    {
                        return (Result.WrongResult, "Input data was modified");
                    }
                }
            }

            if (Math.Abs(productionPlan.Value - expectedMaxProfit) > 1e-4)
            {
                return (Result.WrongResult, $"Wrong profit calculated: expected {expectedMaxProfit}, method returned {productionPlan.Value}");
            }

            if (weeklyPlan == null)
            {
                return (Result.WrongResult, "No plan returned");
            }
            if (weeks != weeklyPlan.Length)
            {
                return (Result.WrongResult, $"Bad plan - wrong number of weeks; expected {weeks} weeks, WeeklyPlan has length {weeklyPlan.Length}");
            }
            for (int i = 0; i < weeks; ++i)
            {
                var planStep = weeklyPlan[i];
                if (planStep.UnitsSold.Length != customers)
                {
                    return (Result.WrongResult, $"Bad plan - UnitsSold array in week {i} has invalid length; expected {customers}, got {planStep.UnitsSold.Length}");
                }
                if (planStep.UnitsProduced < 0 || planStep.UnitsStored < 0 || planStep.UnitsSold.Any(sold => sold < 0))
                {
                    return (Result.WrongResult, $"Bad plan - week {i} specifies negative amounts");
                }
                if (planStep.UnitsProduced > production[i].Quantity)
                {
                    return (Result.WrongResult, $"Bad plan - production in week {i} exceeds maximum supply limit");
                }
                if (planStep.UnitsStored > storageInfo.Quantity)
                {
                    return (Result.WrongResult, $"Bad plan - storage capacity exceeded in week {i}");
                }

                for (int customer = 0; customer < customers; ++customer)
                {
                    if (planStep.UnitsSold[customer] > sales[customer, i].Quantity)
                    {
                        return (Result.WrongResult, $"Bad plan - sales in week {i} to customer {customer} exceed maximum demand limit");
                    }
                }
                var totalSold = planStep.UnitsSold.Sum();
                if (planStep.UnitsProduced + unitsInStorage > planStep.UnitsStored + totalSold)
                {
                    return (Result.WrongResult, $"Bad plan - some units in week {i} have not been either sold or stored");
                }
                if (planStep.UnitsProduced + unitsInStorage < planStep.UnitsStored + totalSold)
                {
                    return (Result.WrongResult, $"Bad plan - nonexistent units sold or stored in week {i}");
                }
                // Produkcja
                unitsProduced += planStep.UnitsProduced;
                productionExpenses += planStep.UnitsProduced * production[i].Value;
                // Sprzedaż
                for (int customer = 0; customer < customers; ++customer)
                {
                    salesProfits += planStep.UnitsSold[customer] * sales[customer, i].Value;
                }
                // Wstawienie do magazynu
                unitsInStorage = planStep.UnitsStored;
                // Naliczenie kosztu za przetrzymanie
                storageExpenses += planStep.UnitsStored * storageInfo.Value;
            }

            if (unitsProduced != productionPlan.Quantity)
            {
                return (Result.WrongResult, $"Bad plan: total number of units produced is {productionPlan.Quantity}, " +
                          $"following plan results in {unitsProduced} units");
            }

            var profit = salesProfits - productionExpenses - storageExpenses;
            if (Math.Abs(profit - productionPlan.Value) > 1e-4)
            {
                return (Result.WrongResult, $"Bad plan: net profit is {productionPlan.Value}, " +
                          $"following plan results in profit of {profit}");
            }
            return (Result.Success, $"OK, time: {PerformanceTime:F3}");
        }
    }

    public static class LabSimpleTests
    {
        public static ((int Quantity, double Value)[], (int Quantity, double Value)[], (int Quantity, double Value)) SmallTest()
        {
            var productionInfo = new[]
            {
                (15, 5.0),
                (10, 11.0),
                (20, 9.0)
            };
            var storageInfo = (10, 10);
            var salesInfo = new[]
            {
                (10, 7.0),
                (15, 9.0),
                (30, 11.0)
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[], (int Quantity, double Value)) SmallTestWithStorageProfit()
        {
            var productionInfo = new[]
            {
                (15, 5.0),
                (10, 11.0),
                (20, 9.0)
            };
            var storageInfo = (10, 2.0);
            var salesInfo = new[]
            {
                (10, 7.0),
                (15, 9.0),
                (30, 11.0)
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[], (int Quantity, double Value)) DemandBottleneck()
        {
            var productionInfo = new[]
            {
                (30, 5.0),
                (30, 7.0),
                (30, 10.0),
                (30, 8.0),
            };
            var storageInfo = (10, 2.0);
            var salesInfo = new[]
            {
                (10, 8.0),
                (20, 6.0),
                (30, 17.0),
                (15, 6.0),
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[], (int Quantity, double Value)) SupplyBottleneck()
        {
            var productionInfo = new[]
            {
                (5, 10.0),
                (15, 15.0),
                (5, 12.0),
                (0, 15.0),
                (10, 20.0),
            };
            var storageInfo = (20, 5.0);
            var salesInfo = new[]
            {
                (20, 12.0),
                (25, 8.0),
                (50, 22.0),
                (50, 25.0),
                (40, 16.0),
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[], (int Quantity, double Value)) FreeStorage()
        {
            var productionInfo = new[]
            {
                (20, 15.0),
                (15, 20.0),
                (25, 25.0),
                (15, 15.0),
            };
            var storageInfo = (100, 0.0);
            var salesInfo = new[]
            {
                (10, 5.0),
                (5, 2.0),
                (75, 40.0),
                (20, 10.0),
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[], (int Quantity, double Value)) BigLoss()
        {
            var productionInfo = new[]
            {
                (30, 25.0),
                (25, 30.0),
                (35, 40.0),
                (30, 35.0),
                (40, 37.5),
                (45, 40.0),
            };
            var storageInfo = (60, 3.0);
            var salesInfo = new[]
            {
                (30, 15.0),
                (30, 25.0),
                (45, 42.0),
                (45, 28.0),
                (50, 22.0),
                (50, 18.0),
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[], (int Quantity, double Value)) NoStorage()
        {
            var storageInfo = (0, 0.0);
            var productionInfo = new (int, double)[8];
            var salesInfo = new (int, double)[8];
            for (int i = 0; i < 8; ++i)
            {
                productionInfo[i] = (10 * (i + 1), 5.0 + i % 2 );
                salesInfo[i] = (80 - 10 * i, 6.0 - i % 2 );
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[], (int Quantity, double Value)) NoProduction()
        {
            var storageInfo = (10, 10.0);
            var productionInfo = new (int, double)[6];
            var salesInfo = new (int, double)[6];
            for (int i = 0; i < 6; ++i)
            {
                productionInfo[i] = (0, 50.0);
                salesInfo[i] = (30, 60.0);
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[], (int Quantity, double Value)) NoBuyers()
        {
            var storageInfo = (10, 10.0);
            var productionInfo = new (int, double)[10];
            var salesInfo = new (int, double)[10];
            for (int i = 0; i < 10; ++i)
            {
                productionInfo[i] = (50, 30.0);
                salesInfo[i] = (0, 40.0);
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[], (int Quantity, double Value)) RandomTest(int size, int seed)
        {
            var rng = new Random(seed);
            var storageInfo = (rng.Next(1000), (double) 5 + rng.Next(15));
            var productionInfo = new (int, double)[size];
            var salesInfo = new (int, double)[size];
            for (int i = 0; i < size; ++i)
            {
                productionInfo[i] = (rng.Next(1000), 25 + rng.Next(50));
                salesInfo[i] = (rng.Next(1000), 35 + rng.Next(40));
            }
            return (productionInfo, salesInfo, storageInfo);
        }
    }

    public static class LabComplexTests
    {
        public static ((int Quantity, double Value)[], (int Quantity, double Value)[,], (int Quantity, double Value)) SmallTest()
        {
            var productionInfo = new[]
            {
                (10, 8.0),
                (15, 9.0),
                (5, 12.0)
            };
            var storageInfo = (5, 10.0);
            var salesInfo = new[,]
            {
                {
                    (6, 9.0),
                    (10, 10.0),
                    (5, 6.0)
                },
                {
                    (4, 10.0),
                    (15, 7.0),
                    (5, 12.0)
                }
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[,], (int Quantity, double Value)) SmallTestWithStorageProfit()
        {
            var productionInfo = new[]
            {
                (10, 8.0),
                (15, 9.0),
                (5, 12.0)
            };
            var storageInfo = (5, 2.0);
            var salesInfo = new[,]
            {
                {
                    (6, 9.0),
                    (10, 10.0),
                    (5, 6.0)
                },
                {
                    (4, 10.0),
                    (15, 7.0),
                    (5, 12.0)
                }
            };
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[,], (int Quantity, double Value)) DemandBottleneck()
        {
            var productionInfo = new (int Quantity, double Value)[]
            {
                (50, 10.0),
                (40, 15.0),
                (45, 12.0),
                (60, 8.0),
                (50, 10.0)
            };
            var storageInfo = (10, 3.0);
            var salesInfo = new (int Quantity, double Value)[3, productionInfo.Length];
            for (int week = 0; week < productionInfo.Length; ++week)
            {
                var demand = productionInfo[week].Quantity / 5;
                var value = productionInfo[week].Value;
                for (int customer = 0; customer < salesInfo.GetLength(0); ++customer)
                {
                    salesInfo[customer, week] = (demand, value + customer - 1);
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[,], (int Quantity, double Value)) SupplyBottleneck()
        {
            var productionInfo = new (int Quantity, double Value)[]
            {
                (15, 30.0),
                (10, 35.0),
                (20, 25.0),
                (30, 20.0),
                (10, 20.0)
            };
            var storageInfo = (10, 3.0);
            var salesInfo = new (int Quantity, double Value)[5, productionInfo.Length];
            for (int week = 0; week < productionInfo.Length; ++week)
            {
                var demand = productionInfo[week].Quantity / 2;
                var value = productionInfo[week].Value;
                for (int customer = 0; customer < salesInfo.GetLength(0); ++customer)
                {
                    salesInfo[customer, week] = (demand, value + 5 + Math.Abs(customer - week));
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[,], (int Quantity, double Value)) FreeStorage()
        {
            var productionInfo = new (int Quantity, double Value)[]
            {
                (30, 30.0),
                (40, 5.0),
                (35, 30.0),
                (50, 35.0),
                (15, 40.0),
                (40, 40.0)
            };
            var storageInfo = (100, 0.0);
            var salesInfo = new (int Quantity, double Value)[4, productionInfo.Length];
            for (int week = 0; week < productionInfo.Length; ++week)
            {
                var demand = productionInfo[week].Quantity / 3;
                var cost = productionInfo[week].Value;
                for (int customer = 0; customer < salesInfo.GetLength(0); ++customer)
                {
                    salesInfo[customer, week] = (demand, cost - 5 + (week == 4 ? 25 : 0));
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[,], (int Quantity, double Value)) BigLoss()
        {
            var productionInfo = new (int Quantity, double Value)[]
            {
                (50, 40.0),
                (30, 50.0),
                (35, 50.0),
                (27, 33.0),
                (34, 28.0),
                (19, 35.0),
                (38, 33.0)
            };
            var storageInfo = (50, 10.0);
            var salesInfo = new (int Quantity, double Value)[10, productionInfo.Length];
            for (int week = 0; week < productionInfo.Length; ++week)
            {
                var demand = productionInfo[week].Quantity / 10;
                var cost = productionInfo[week].Value;
                for (int customer = 0; customer < salesInfo.GetLength(0); ++customer)
                {
                    salesInfo[customer, week] = (demand + customer, cost - 3 - customer);
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[,], (int Quantity, double Value)) NoStorage()
        {
            var productionInfo = new (int Quantity, double Value)[]
            {
                (25, 25.0),
                (50, 33.0),
                (30, 18.0),
                (14, 23.5),
                (17, 13.7),
                (20, 20.0)
            };
            var storageInfo = (0, 0.0);
            var salesInfo = new (int Quantity, double Value)[8, productionInfo.Length];
            for (int week = 0; week < productionInfo.Length; ++week)
            {
                var demand = productionInfo[week].Quantity * 2 / 3;
                var cost = productionInfo[week].Value;
                for (int customer = 0; customer < salesInfo.GetLength(0); ++customer)
                {
                    salesInfo[customer, week] = (demand, cost + 2 * (customer % 3) - 3.5 * (week % 2));
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[,], (int Quantity, double Value)) NoProduction()
        {
            int weeks = 7, customers = 4;
            var productionInfo = new (int Quantity, double Value)[weeks];
            var salesInfo = new (int Quantity, double Value)[customers, weeks];
            var storageInfo = (5, 15.0);
            for (int week = 0; week < weeks; ++week)
            {
                productionInfo[week] = (0, 0);
                for (int customer = 0; customer < customers; ++customer)
                {
                    salesInfo[customer, week] = (15, 50);
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[,], (int Quantity, double Value)) NoBuyers()
        {
            int weeks = 6, customers = 8;
            var productionInfo = new (int Quantity, double Value)[weeks];
            var salesInfo = new (int Quantity, double Value)[customers, weeks];
            var storageInfo = (5, 15.0);
            for (int week = 0; week < weeks; ++week)
            {
                productionInfo[week] = (20, 50);
                for (int customer = 0; customer < customers; ++customer)
                {
                    salesInfo[customer, week] = (0, 50);
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }

        public static ((int Quantity, double Value)[], (int Quantity, double Value)[,], (int Quantity, double Value)) RandomTest(int weeks, int customers, int seed)
        {
            var rng = new Random(seed);
            var storageInfo = (rng.Next(1000), 5 + rng.Next(15));
            var productionInfo = new (int Quantity, double Value)[weeks];
            var salesInfo = new (int Quantity, double Value)[customers, weeks];
            for (int week = 0; week < weeks; ++week)
            {
                productionInfo[week] = (rng.Next(1000), 30 + rng.Next(60));
                for (int customer = 0; customer < customers; ++customer)
                {
                    salesInfo[customer, week] = (rng.Next(2000 / customers), 25 + rng.Next(80));
                }
            }
            return (productionInfo, salesInfo, storageInfo);
        }
    }

    public class Lab10TestModule : TestModule
    {
        public override void PrepareTestSets()
        {
            var subjectUnderTest = new ProductionPlanner();

            var labSimplePlanTests = new TestSet(
                subjectUnderTest,
                "Laboratorium - część 1. - wyznaczenie planu produkcji"
            );
            labSimplePlanTests.TestCases.AddRange(new List<TestCase>
            {
                new SimpleProductionPlanTestCase(1, null, "Mały test", LabSimpleTests.SmallTest(), 45, 10.0),
                new SimpleProductionPlanTestCase(1, null, "Mały test - większy zysk z magazynem", LabSimpleTests.SmallTestWithStorageProfit(), 45, 50.0),
                new SimpleProductionPlanTestCase(1, null, "Wąskie gardło - popyt", LabSimpleTests.DemandBottleneck(), 75, 200.0),
                new SimpleProductionPlanTestCase(1, null, "Wąskie gardło - podaż", LabSimpleTests.SupplyBottleneck(), 35, 50.0),
                new SimpleProductionPlanTestCase(1, null, "Darmowy magazyn", LabSimpleTests.FreeStorage(), 75, 1100.0),
                new SimpleProductionPlanTestCase(1, null, "Duża strata", LabSimpleTests.BigLoss(), 205, -1870.0),
                new SimpleProductionPlanTestCase(1, null, "Brak magazynu", LabSimpleTests.NoStorage(), 200, 0.0),
                new SimpleProductionPlanTestCase(1, null, "Fabryka zamknięta", LabSimpleTests.NoProduction(), 0, 0.0),
                new SimpleProductionPlanTestCase(1, null, "Brak kupujących", LabSimpleTests.NoBuyers(), 0, 0.0),
                new SimpleProductionPlanTestCase(6, null, "Średni test losowy", LabSimpleTests.RandomTest(100, 42), 40233, 220922.0),
                new SimpleProductionPlanTestCase(20, null, "Duży test losowy", LabSimpleTests.RandomTest(150, 1337), 57375, 504271.0),
            });

            var labComplexPlanTests = new TestSet(
                subjectUnderTest,
                "Laboratorium - część 2. - wyznaczenie planu produkcji"
            );
            labComplexPlanTests.TestCases.AddRange(new List<TestCase>
            {
                new ComplexProductionPlanTestCase(1, null, "Mały test", LabComplexTests.SmallTest(), 24.0),
                new ComplexProductionPlanTestCase(1, null, "Mały test - większy zysk z magazynem", LabComplexTests.SmallTestWithStorageProfit(), 29.0),
                new ComplexProductionPlanTestCase(1, null, "Wąskie gardło - popyt", LabComplexTests.DemandBottleneck(), 69.0),
                new ComplexProductionPlanTestCase(1, null, "Wąskie gardło - podaż", LabComplexTests.SupplyBottleneck(), 652.0),
                new ComplexProductionPlanTestCase(1, null, "Darmowy magazyn", LabComplexTests.FreeStorage(), 1860.0),
                new ComplexProductionPlanTestCase(1, null, "Duża strata", LabComplexTests.BigLoss(), 0.0),
                new ComplexProductionPlanTestCase(1, null, "Brak magazynu", LabComplexTests.NoStorage(), 330.0),
                new ComplexProductionPlanTestCase(1, null, "Fabryka zamknięta", LabComplexTests.NoProduction(), 0.0),
                new ComplexProductionPlanTestCase(1, null, "Brak kupujących", LabComplexTests.NoBuyers(), 0.0),
                new ComplexProductionPlanTestCase(5, null, "Średni test losowy", LabComplexTests.RandomTest(30, 10, 11111), 227625.0),
                new ComplexProductionPlanTestCase(30, null, "Duży test losowy", LabComplexTests.RandomTest(45, 15, 22222), 532936.0),
            });

            TestSets = new Dictionary<string, TestSet>
            {
                {"LabSimplePlanTests", labSimplePlanTests},
                {"LabComplexPlanTests", labComplexPlanTests},
            };
        }

        public override double ScoreResult()
        {
            return 3.0;
        }

    }

    public class Lab10Main
    {
        public static void Main()
        {
            if (ASD.Graphs.GraphLibraryVersion.FullNumber != "7.3.2")
            {
                Console.WriteLine("Pobierz nowa wersje biblioteki Graph");
                return;
            }
            var testModule = new Lab10TestModule();
            testModule.PrepareTestSets();
            foreach (var testSet in testModule.TestSets)
            {
                testSet.Value.PerformTests(verbose: true, checkTimeLimit: false);
            }
        }
    }
}