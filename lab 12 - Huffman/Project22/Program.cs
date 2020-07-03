using ASD;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASD_2020_12
{

public abstract class HuffmanTestCase : TestCase
    {

    public HuffmanTestCase(double timeLimit, Exception expectedException, string description)
        : base(timeLimit, expectedException, description)
    {
    }

    public static HuffmanNode HuffmanNode_Create(HuffmanNode other)
    {
        return new HuffmanNode
        {
            Character = other.Character,
            Frequency = other.Frequency,
            Left = other.Left != null
                ? HuffmanNode_Create(other.Left)
                : null,
            Right = other.Right != null
                ? HuffmanNode_Create(other.Right)
                : null,
        };
    }

    public static HuffmanNode HuffmanNode_DeepClone(HuffmanNode node)
    {
        return node == null
                ? null
                : HuffmanNode_Create(node);
    }

    public static bool HuffmanNode_Equals(HuffmanNode lhs, HuffmanNode rhs)
    {
        if (lhs == null && rhs == null) { return true; }
        if (lhs != null && rhs != null)
        {
            if (lhs.Character != rhs.Character) { return false; }
            if (lhs.Frequency != rhs.Frequency) { return false; }
            return HuffmanNode_Equals(lhs.Left, rhs.Left) && HuffmanNode_Equals(lhs.Right, rhs.Right);
        }
        return false;
    }

    }

public class HuffmanTreeTestCase : HuffmanTestCase
{
    private readonly string baseText;
    private HuffmanNode Root;

    public HuffmanTreeTestCase(double timeLimit, Exception expectedException, string description, string baseText)
        : base(timeLimit, expectedException, description)
    {
        this.baseText = baseText;
    }

    protected override void PerformTestCase(object prototypeObject)
    {
        if (prototypeObject is Huffman huffman)
        {
            Root = huffman.CreateHuffmanTree(baseText);
        }
    }

    protected override (Result resultCode, string message) VerifyTestCase(object settings)
    {
        string errorMessage = null;

        if (Root == null)
        {
            errorMessage = $"{nameof(Root)} should not be null";
        }
        if (errorMessage == null && Root.Frequency != baseText.Length)
        {
            errorMessage = "Huffman tree root frequency should be equal to base text length";
        }
        if (errorMessage == null)
        {
            errorMessage = AssertNoCircularReferences(Root);
        }
        if (errorMessage == null)
        {
            errorMessage = AssertNoDuplicatedCharacters(Root);
        }
        if (errorMessage == null)
        {
            errorMessage = AssertOnlyBaseTextCharacters(Root);
        }
        if (errorMessage == null)
        {
            errorMessage = AssertAllBaseTextCharacters(Root);
        }
        if (errorMessage == null)
        {
            errorMessage = AssertTreeStructure(Root);
        }

        return errorMessage != null
            ? (Result.WrongResult, errorMessage)
            : (Result.Success, $"Seems OK (time:{PerformanceTime,6:#0.000})");
    }

    public string NodeVisitor(HuffmanNode root, Func<HuffmanNode, string> action)
    {
        string errorMessage = null;
        if (root.Left != null)
        {
            errorMessage = NodeVisitor(root.Left, action);
            if (errorMessage != null) { return errorMessage; }
        }
        errorMessage = action.Invoke(root);
        if (errorMessage != null) { return errorMessage; }
        if (root.Right != null)
        {
            errorMessage = NodeVisitor(root.Right, action);
            if (errorMessage != null) { return errorMessage; }
        }
        return errorMessage;
    }

    private string AssertNoDuplicatedCharacters(HuffmanNode root)
    {
        var visitedCharacters = new SortedSet<char>();
        return NodeVisitor(root, node =>
            {
            if (node.Character > 0)
                {
                if (visitedCharacters.Contains(node.Character))
                    {
                    return $"{root.Character} was encountered more than once in huffman tree";
                    }
                visitedCharacters.Add(node.Character);
                }
            return null;
           });
    }

    private string AssertNoCircularReferences(HuffmanNode root)
    {
        var visitedNodes = new List<HuffmanNode>();
        return NodeVisitor(root, node =>
            {
            string errorMessage = null;
            visitedNodes.ForEach(visitedNode =>
                {
                if (ReferenceEquals(visitedNode, node))
                    {
                    errorMessage = "Huffman tree contains circular references";
                    }
                });
            visitedNodes.Add(node);
            return errorMessage;
            });
    }

    private string AssertOnlyBaseTextCharacters(HuffmanNode root)
    {
        bool[] characters = new bool[char.MaxValue];

        foreach (char character in baseText)
        {
            characters[character] = true;
        }

        return NodeVisitor(root, node =>
            {
            if (node.Character > 0)
                {
                if (!characters[node.Character])
                    {
                    return $"Huffman tree should contains only characters that were in base text";
                    }
                }
            return null;
            });
    }

    private string AssertAllBaseTextCharacters(HuffmanNode root)
    {
        bool[] characters = new bool[char.MaxValue];

        foreach (char character in baseText)
        {
            characters[character] = true;
        }

        var huffmanTreeCharacters = new SortedSet<char>();
        var baseTextCharacters = new SortedSet<char>(characters
            .Select((wasEncountered, index) => new { WasEncountered = wasEncountered, Character = (char)index })
            .Where(x => x.WasEncountered)
            .Select(x => x.Character));

        NodeVisitor(root, node =>
        {
            if (node.Character > 0)
            {
                huffmanTreeCharacters.Add(node.Character);
            }
            return null;
        });

        baseTextCharacters.ExceptWith(huffmanTreeCharacters);

        return baseTextCharacters.Any()
            ? $"Huffman tree does not contain '{baseTextCharacters.First()}' that was in base text"
            : null;
    }

    private string AssertTreeStructure(HuffmanNode root)
    {
        return NodeVisitor(root, node =>
            {
            if (node.Frequency <= 0)
                {
                return "Node should have positive frequency";
                }
            if (node.Character == 0)
                {
                if (node.Left == null)
                    {
                    return "Non-terminating node (character==0) should have left descendant";
                    }
                if (node.Right == null)
                    {
                    return "Non-terminating node (character==0) should have right descendant";
                    }
                if (node.Frequency != node.Right.Frequency + node.Left.Frequency)
                    {
                    return "Non-terminating node (character==0) should have frequency equal to sum of descendant frequencies";
                    }
                }
            else
                {
                if (node.Left != null)
                    {
                    return $"Leaf node (character='{node.Character}')  should not have left descendant";
                    }
                if (node.Right != null)
                    {
                    return $"Leaf node (character='{node.Character}') should not have right descendant";
                    }
                }
            return null;
            });
    }
}

public class HuffmanCompressTestCase : HuffmanTestCase
{
    private readonly string text;
    private readonly BitArray expectedResult;
    private readonly HuffmanNode tree;
    private readonly HuffmanNode treeCopy;
    private BitArray encoding;

    public HuffmanCompressTestCase(double timeLimit, Exception expectedException, string description, HuffmanNode tree, string text, BitArray expectedResult)
        : base(timeLimit, expectedException, description)
    {
        this.tree = tree;
        this.text = text;
        this.expectedResult = expectedResult;
        treeCopy = HuffmanNode_DeepClone(tree);
    }

    protected override void PerformTestCase(object prototypeObject)
    {
        if (prototypeObject is Huffman huffman)
        {
            encoding = huffman.Compress(tree, text);
        }
    }

    protected override (Result resultCode, string message) VerifyTestCase(object settings)
    {
        string errorMessage = null;

        if (!HuffmanNode_Equals(tree, treeCopy))
        {
            errorMessage = $"{nameof(Huffman.Compress)} should not modify huffman tree";
        }

        if (encoding == null)
        {
            errorMessage = $"{nameof(encoding)} should not be null";
        }
        if (errorMessage == null && encoding.Length == 0)
        {
            errorMessage = "Compressed value should not be empty";
        }

        if (errorMessage == null && encoding.Length != expectedResult.Length)
        {
            errorMessage = $"Compressed value should have {expectedResult.Length} bits. Has {encoding.Length} bits.";
        }

        if (errorMessage == null)
        {
            for (int i = 0; i < encoding.Length; i++)
            {
                if (encoding[i] != expectedResult[i])
                {
                    errorMessage = $"Bit at position {i} (0-based) should be {(expectedResult[i] ? "set" : "unset")}";
                    break;
                }
            }
        }

        return errorMessage != null
            ? (Result.WrongResult, errorMessage)
            : (Result.Success, $"OK (time:{PerformanceTime,6:#0.000})");
    }
}

public class HuffmanDecompressTestCase : HuffmanTestCase
{
    private readonly string expectedResult;
    private readonly HuffmanNode tree;
    private readonly HuffmanNode treeCopy;

    private readonly BitArray encoding;
    private string decodedValue;

    public HuffmanDecompressTestCase(double timeLimit, Exception expectedException, string description, HuffmanNode tree, BitArray encoding, string expectedResult)
        : base(timeLimit, expectedException, description)
    {
        this.tree = tree;
        this.encoding = encoding;
        this.expectedResult = expectedResult;
        treeCopy = HuffmanNode_DeepClone(tree);
    }

    protected override void PerformTestCase(object prototypeObject)
    {
        if (prototypeObject is Huffman huffman)
        {
            decodedValue = huffman.Decompress(tree, encoding);
        }
    }

    protected override (Result resultCode, string message) VerifyTestCase(object settings)
    {
        string errorMessage = null;


        if (!HuffmanNode_Equals(tree, treeCopy))
        {
            errorMessage = $"{nameof(Huffman.Decompress)} should not modify huffman tree";
        }

        if (decodedValue == null)
        {
            errorMessage = $"{nameof(decodedValue)} should not be null";
        }
        if (errorMessage == null && decodedValue.Length == 0)
        {
            errorMessage = "Decompressed value should not be empty";
        }

        if (errorMessage == null && decodedValue.Length != expectedResult.Length)
        {
            errorMessage = $"Decompressed value should have {expectedResult.Length} characters. Has {decodedValue.Length} characters.";
        }

        if (errorMessage == null)
        {
            for (int i = 0; i < decodedValue.Length; i++)
            {
                if (decodedValue[i] != expectedResult[i])
                {
                    errorMessage = $"Character at position {i} (0-based) should be {(expectedResult[i])}";
                    break;
                }
            }
        }

        return errorMessage != null
            ? (Result.WrongResult, errorMessage)
            : (Result.Success, $"OK (time:{PerformanceTime,6:#0.000})");
    }
}

public class HuffmanTestModule : TestModule
{

    private class RandomTestData
    {
        public string Text { get; set; }
        public string Description { get; set; }
    };

    private HuffmanNode[] trees;

    private HuffmanNode[] DefineSampleTrees()  // 5 drzew
    {
    HuffmanNode[] _trees = new HuffmanNode[] {
            new HuffmanNode
            {
                Frequency = 42,
                Character = 'a'
            }, // a -> 0;
            new HuffmanNode
            {
                Frequency = 5,
                Left = new HuffmanNode
                {
                    Character = 'b',
                    Frequency = 3,
                },
                Right = new HuffmanNode
                {
                    Character = 'c',
                    Frequency = 2,
                }
            }, // b -> 0; c -> 1
            new HuffmanNode
            {
                Frequency = 11,
                Left = new HuffmanNode
                {
                    Character = 'a',
                    Frequency = 5,
                },
                Right = new HuffmanNode
                {
                    Frequency = 6,
                    Left = new HuffmanNode
                    {
                        Character = 'c',
                        Frequency = 2,
                    },
                    Right = new HuffmanNode
                    {
                        Character = 'b',
                        Frequency = 4,
                    }
                }
            }, // a -> 0; b -> 11; c -> 10;
            new HuffmanNode
            {
                Frequency = 12,
                Left = new HuffmanNode
                {
                    Character = 'a',
                    Frequency = 5,
                },
                Right = new HuffmanNode
                {
                    Character = 'b',
                    Frequency = 7,
                },
            }, // a -> 0; b -> 1
            new HuffmanNode
            {
                Frequency = 12,
                Left = new HuffmanNode
                {
                    Character = 'b',
                    Frequency = 7,
                },
                Right = new HuffmanNode
                {
                    Character = 'a',
                    Frequency = 5,
                },
            } // a -> 1; b -> 0
        };
    return _trees;
    }

    public override void PrepareTestSets()
    {
        trees = DefineSampleTrees();
        PrepareLabHuffmanTreeTestSets();
        PrepareLabCompressTestSets();
        PrepareLabDecompressTestSets();
    }

    private void PrepareLabHuffmanTreeTestSets()
    {
        var testSet = new TestSet(new Huffman(), "Lab - Huffman tree");
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, new ArgumentNullException(), "Input validation 1", (string)null));
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, new ArgumentNullException(), "Input validation 2", string.Empty));
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, null, "2 unique characters, 2 in total", "ab"));
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, null, "2 unique characters, 3 in total", "aba"));
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, null, "2 unique characters, 5 in total", "aabaa"));
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, null, "2 unique characters, 10 in total", "aaabbababb"));
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, null, "3 unique characters, 16 in total", "aacabccbcacbcabb"));
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, null, "words - ala ma kota", "ala ma kota"));
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, null, "words - polamaniec jezykowy 1", "stol z powylamywanymi nogami"));
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, null, "words - polamaniec jezykowy 2", "Na wyrewolwerowanym wzgórzu przy wyrewolwerowanym rewolwerowcu leży wyrewolwerowany rewolwer wyrewolwerowanego rewolwerowca"));
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, null, "words - inwokacja", "litwo ojczyzno moja, ty jestes jak zdrowie"));
        testSet.TestCases.Add(new HuffmanTreeTestCase(1.0, null, "1 unique character", "aaa"));
        var sb = new StringBuilder();
        const int uniqueCharacters = 127;
        for (int i = 0; i < uniqueCharacters; i++)
        {
            sb.Append('A' + i);
        }
        sb.Append(new string('A', 1 << 23));
        for (int i = 0; i < uniqueCharacters; i++)
        {
            sb.Append('A' + i);
        }
        testSet.TestCases.Add(new HuffmanTreeTestCase(3, null, "8.3 mln characters; 127 unique characters", sb.ToString()));
        TestSets[testSet.Description] = testSet;
    }

    private void PrepareLabCompressTestSets()
    {
        var testSet = new TestSet(new Huffman(), "Lab - Huffman compression");
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, new ArgumentNullException(), "Input validation 1", null, null, null));
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, new ArgumentNullException(), "Input validation 2", trees[0], null, null));
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, new ArgumentNullException(), "Input validation 3", trees[0], string.Empty, null));
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, new ArgumentOutOfRangeException(), "Input validation 4", trees[0], "some-characters-not-known-by-huffman-trees", null));
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, null, "1 unique character", trees[0], "aaaaaaaa", new BitArray(new byte[] { 0x0 })));
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, null, "2 unique characters", trees[1], "b", new BitArray(new bool[] { false })));
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, null, "2 unique characters", trees[1], "c", new BitArray(new bool[] { true })));
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, null, "2 unique characters", trees[1], "bc", new BitArray(new bool[] { false, true })));
        // bits in byte are ordered from low to highest
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, null, "2 unique characters", trees[1], "bccbbbcc", new BitArray(new byte[] { 0b1_1_0_0_0_1_1_0 })));
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, null, "3 unique characters", trees[2], "abcba", new BitArray(new byte[] { 0b0_11_01_11_0 })));
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, null, "2 unique characters", trees[3], "ab", new BitArray(new bool[] { false, true })));
        testSet.TestCases.Add(new HuffmanCompressTestCase(1.0, null, "2 unique characters", trees[4], "ab", new BitArray(new bool[] { true, false })));
        TestSets[testSet.Description] = testSet;
    }

    private void PrepareLabDecompressTestSets()
    {
        var testSet = new TestSet(new Huffman(), "Lab - Huffman decompression");
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, new ArgumentNullException(), "Input validation 1", null, null, null));
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, new ArgumentNullException(), "Input validation 2", trees[0], null, null));
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, new ArgumentNullException(), "Input validation 3", trees[0], new BitArray(0, false), null));
        // encoding is not correct; we will end decoding in non-terminating node
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, new ArgumentException(), "Input validation 4", trees[2], new BitArray(new bool[] { false /*a*/, false /*a*/, true, true /*c*/, true /*?*/}), null));
        // encoding is not correct because in case of single element, code should be all 0
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, new ArgumentException(), "Input validation 5", trees[0], new BitArray(new byte[] { 0b010100101 }), null));
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, null, "1 unique character", trees[0], new BitArray(new byte[] { 0x0 }), "aaaaaaaa"));
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, null, "2 unique characters", trees[1], new BitArray(new bool[] { false }), "b"));
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, null, "2 unique characters", trees[1], new BitArray(new bool[] { true }), "c"));
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, null, "2 unique characters", trees[1], new BitArray(new bool[] { false, true }), "bc"));
        // bits in byte are ordered from low to highest
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, null, "2 unique characters", trees[1], new BitArray(new byte[] { 0b1_1_0_0_0_1_1_0 }), "bccbbbcc"));
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, null, "3 unique characters", trees[2], new BitArray(new byte[] { 0b0_11_01_11_0 }), "abcba"));
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, null, "2 unique characters", trees[3], new BitArray(new bool[] { false, true }), "ab"));
        testSet.TestCases.Add(new HuffmanDecompressTestCase(1.0, null, "2 unique characters", trees[4], new BitArray(new bool[] { true, false }), "ab"));
        TestSets[testSet.Description] = testSet;
    }

    public override double ScoreResult()
        {
        return 3;
        }

}

public class Program
{
    public static void Main(string[] args)
    {
        var testModule = new HuffmanTestModule();
        testModule.PrepareTestSets();
        foreach (var ts in testModule.TestSets)
            ts.Value.PerformTests(verbose: true, checkTimeLimit: false);
    }
}

}
