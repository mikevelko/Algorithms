
using System;

namespace ASD
{

public class Game : MarshalByRefObject
    {

    /// <summary>
    /// Zadanie na lab 2 z ASD2
    /// </summary>
    /// <param name="numbers">Zadany ciąg liczb</param>
    /// <param name="moves">Przykładowa sekwencja ruchów</param>
    /// <returns>Wynik gry dla gracza rozpoczynającego grę (suma zebranych liczb)</returns>
    public int OptimalStrategy(int[] numbers, out int[] moves)
        {
            moves = new int[numbers.Length];
            int[][] results1 = new int[numbers.Length][];
            int[][] results2 = new int[numbers.Length][];
            for (int i = 0; i < numbers.Length; i++)
            {
                results1[i] = new int[numbers.Length];
                results2[i] = new int[numbers.Length];
                results1[0][i] = numbers[i];
                
            }

            for (int i = 1; i < numbers.Length; i++)
            {
                for (int j = 0; j + i < numbers.Length; j++)
                {
                    if (results2[i-1][j] + numbers[i + j] >= results2[i-1][j + 1] + numbers[j])
                    {
                        results1[i][j] = results2[i-1][j] + numbers[i + j];
                        results2[i][j] = results1[i-1][j];
                    }
                    else
                    {
                        results1[i][j] = results2[i-1][j+1] + numbers[j];
                        results2[i][j] = results1[i-1][j+1];
                    }
                }
            }
            int x = 0;
            int y = numbers.Length - 1;
            for (int i = numbers.Length; i > 1; i--) 
            {
                if (results2[i - 2][x] + numbers[i + x - 1] >= results2[i - 2][x + 1] + numbers[x])
                {
                    moves[numbers.Length - i] = y;
                    y--;
                }
                else 
                {
                    moves[numbers.Length - i] = x;
                    x++;
                }
            }
            moves[numbers.Length - 1] = x;
            return results1[numbers.Length-1][0];
        }


    }

}
