
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ASD_2020_12
{
    /// <summary>
    ///     Kolejka priorytetowa węzłów Huffmana. Metoda Get pobiera (i usuwa z kolejki) element o minimalnej częstotliwości
    /// </summary>
    public class HuffmanPriorityQueue : MarshalByRefObject
    {
        private readonly ASD.Graphs.PriorityQueue<HuffmanNode, long> queue = 
            new ASD.Graphs.PriorityQueue<HuffmanNode, long>((lhs, rhs) => lhs.Key.Frequency < rhs.Key.Frequency);

        public bool Put(HuffmanNode node) => queue.Put(node, node.Frequency); // Dodaje węzeł Huffmana do kolejki

        public HuffmanNode Get() => queue.Get(); // Pobiera węzeł Huffmana o najmniejszej częstotliwości

        public int Count => queue.Count; // Zwraca liczbę węzłów w kolejce
    }

    [Serializable]
    public class HuffmanNode
    {
        public char Character { get; set; }
        public long Frequency { get; set; }
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }
    }

    // Od tego miejsca można modyfikować
    // zaklęcia MarshalByRefObject nie ruszamy

    // można dodawać prywatne metody pomocnicze

    public class Huffman : MarshalByRefObject
    {

    // ETap I

        /// <summary>
        /// Metoda tworzy drzewo Huffmana dla zadanego tekstu
        /// </summary>
        /// <param name="baseText">Zadany tekst</param>
        /// <returns>Drzewo Huffmana</returns>
        public HuffmanNode CreateHuffmanTree(string baseText)
        {
            if (baseText == null || baseText=="") throw new ArgumentNullException();
            HuffmanPriorityQueue queue = new HuffmanPriorityQueue();
            Dictionary<char, int> symbols = new Dictionary<char, int>();
            foreach (char c in baseText) 
            {
                if (symbols.ContainsKey(c))
                {
                    symbols[c]++;
                }
                else symbols.Add(c, 1);
            }
            foreach (char c in symbols.Keys) 
            {
                HuffmanNode node = new HuffmanNode();
                node.Character = c;
                node.Frequency = symbols[c];
                queue.Put(node);
            }
            while (queue.Count > 1) 
            {
                HuffmanNode first = queue.Get();
                HuffmanNode second = queue.Get();
                HuffmanNode nowy = new HuffmanNode();
                nowy.Left = first;
                nowy.Right = second;
                nowy.Frequency = first.Frequency + second.Frequency;
                queue.Put(nowy);
            }
            if (queue.Count == 0) return null;
            return queue.Get();
        }

        // Etap II

        /// <summary>
        /// Metoda dokonuje kompresji Huffmana zadanego tekstu
        /// </summary>
        /// <param name="root">Drzewo Huffmana wykorzystywane do kompresji</param>
        /// <param name="content">Zadany tekst</param>
        /// <returns>Skompresowany tekst</returns>
        /// 
        private void TreeAddingElements(HuffmanNode root, Dictionary<char, byte[]> slownik, List<byte> lista) 
        {
            HuffmanNode iter = root;
            if (iter.Left != null) 
            {
                lista.Add(0);
                TreeAddingElements(iter.Left, slownik,lista);
                lista.RemoveAt(lista.Count - 1);
                lista.Add(1);
                TreeAddingElements(iter.Right, slownik,lista);
                lista.RemoveAt(lista.Count - 1);
            }
            else slownik.Add(iter.Character, lista.ToArray());
        }
        public BitArray Compress(HuffmanNode root, string content)
        {
            if (root == null || (content == null || content == "")) throw new ArgumentNullException();
            Dictionary<char, byte[]> slownik = new Dictionary<char, byte[]>();
            List<byte> empty = new List<byte>();
            TreeAddingElements(root, slownik, empty);
            int length = 0;
            foreach (char c in content) 
            {
                if (slownik.ContainsKey(c))
                {
                    if (slownik[c].Length > 0) length += slownik[c].Length;
                    else length++;
                }
                else {
                    throw new ArgumentOutOfRangeException();
                }
            }
            BitArray bitArray = new BitArray(length);
            if (slownik.Count == 1) 
            {
                for (int j = 0; j < length; j++) 
                {
                    bitArray[j] = false;
                }
                return bitArray;
            }
            
            int i = 0;
            foreach (char c in content)
            {
                foreach (byte b in slownik[c]) 
                {
                    if (b == 0)
                    {
                        bitArray[i] = false;
                    }
                    else bitArray[i] = true;
                    i++;
                }
            }
            return bitArray;
        }

    // Etap III

        /// <summary>
        /// Metoda dokonuje dekompresji tekstu skompresowanego metodą Huffmana
        /// </summary>
        /// <param name="root">Drzewo Huffmana wykorzystywane do dekompresji</param>
        /// <param name="encoding">Skompresowany tekst</param>
        /// <returns>Odtworzony tekst</returns>
        /// 
        
        public string Decompress(HuffmanNode root, BitArray encoding)
        {
            if (root == null || encoding==null || encoding.Count==0) throw new ArgumentNullException();
            StringBuilder builder = new StringBuilder();
            HuffmanNode iterator = root;
            if (root.Character != '\0') //kiedy mamy tylko sam root
            {
                foreach (bool b in encoding)
                {
                    if (b != false) throw new ArgumentException();
                }
                foreach (bool b in encoding) 
                {
                    builder.Append(root.Character, 1);
                }
                return builder.ToString();
            }
            else 
            {
                int iter = 0;
                while (iter < encoding.Count) { 
                    if (iterator.Character != '\0')
                    {
                        builder.Append(iterator.Character, 1);
                        iterator = root;
                        
                    }
                    iterator = encoding[iter] ? iterator.Right : iterator.Left;
                    iter++;
                    //if (iterator == null) throw new ArgumentException();
                }
                if (iterator.Character == '\0') throw new ArgumentException();
                if (iterator.Character != '\0') builder.Append(iterator.Character, 1); //dodajemy ostatni element
                return builder.ToString();
            }
        }

    }

}
