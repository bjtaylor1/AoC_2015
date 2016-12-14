using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;

namespace Day24
{
    [TestClass]
    public class UnitTest1
    {
        //private readonly int[] inputs = new[] {1, 2, 3, 3};
        private readonly int[] testInputs = new[] { 1, 2, 3, 4, 5, 7, 8, 9, 10, 11 };
        private readonly int[] inputs = File.ReadAllLines("input.txt").Select(int.Parse).ToArray();
        [TestMethod]
        public void RoundTripRepresentation()
        {
            {
                var representation = new PartDistribution(4, inputs.Length);
                Assert.AreEqual((int)4, representation.GetRepresentation());
            }
            {
                var representation = new PartDistribution(48879, inputs.Length);
                Assert.AreEqual((int)48879, representation.GetRepresentation());
            }
        }

        [TestMethod]
        public void Part1Example()
        {
            var sleigh = new Sleigh(testInputs, 3);
            sleigh.Distribute();
            Assert.AreEqual(2, sleigh.BestCount);
            Assert.AreEqual(99, Convert.ToInt32(sleigh.BestQe));
        }

        [TestMethod]
        public void Part2Example()
        {
            var sleigh = new Sleigh(testInputs, 4);
            sleigh.Distribute();
            Assert.AreEqual(2, sleigh.BestCount);
            Assert.AreEqual(44, Convert.ToInt32(sleigh.BestQe));
        }

        [TestMethod]
        public void Part1()
        {
            var sleigh = new Sleigh(inputs, 3);
            sleigh.Distribute();
            Console.Out.WriteLine(sleigh.BestCount);
            Console.Out.WriteLine(sleigh.BestQe);
        }
    }

    public class Sleigh
    {
        public List<int> Weights { get; }
        public List<int> Bag = new List<int>();
        private List<int[]> balancingCombinations = new List<int[]>();
        public Sleigh(int[] weights, int numBags)
        {
            Weights = weights.Reverse().ToList();

            weightPerBag = weights.Sum() / numBags;
        }

        private readonly Stack<int> bagsDistributedTo = new Stack<int>();
        private readonly int weightPerBag;

        public int BestCount { get; private set; } = int.MaxValue;
        public ulong BestQe { get; private set; } = ulong.MaxValue;

        public bool Distribute()
        {
            if (Bag.Sum() > weightPerBag) return true;
            //does the smallest bag already have a QE as high as our best?
            if (Bag.Count > BestCount) return true;
            if (Bag.Count == BestCount)
            {
                var qeOfSmallest = QE(Bag);
                if (qeOfSmallest >= BestQe)
                    return true; //mark branch as degenerate
            }
            if (Bag.Sum() == weightPerBag)
            {
                if (Bag.Count < BestCount) //been beaten, reset QE
                    BestQe = ulong.MaxValue;

                if (Bag.Count <= BestCount)
                {
                    BestCount = Bag.Count;
                    var qe = QE(Bag);
                    if (qe < BestQe) BestQe = qe;
                }
                LogManager.GetCurrentClassLogger().Info($"{string.Join("     ", string.Join(",", Bag.Select(n => n.ToString())))}, bestCount = {BestCount}, bestQe = {BestQe}");
                var item = Bag.ToArray();
                balancingCombinations.Add(item);
                return true;
            }


            int[] weightsToDistribute;

            if (Bag.Count == BestCount - 1)
                weightsToDistribute = Weights.Where(w => w == weightPerBag - Bag.Sum()).ToArray();
            else
                weightsToDistribute = Weights.ToArray();
            foreach (int w in weightsToDistribute)
            {
                bool degenerate = false;
                var weight = w;
                Bag.Add(weight);
                var actualIndex = Weights.IndexOf(weight);
                Weights.RemoveAt(actualIndex);
                degenerate = Distribute();
                Undo();
                if (degenerate) break;
            }
            return false;
        }

        private static ulong QE(IEnumerable<int> ints)
        {
            return ints.Aggregate((ulong)1, (c, i) => c * (ulong)i);
        }

        public void Undo()
        {
            var weight = Bag.Last();
            Bag.RemoveAt(Bag.Count - 1);
            Weights.Add(weight);
        }

    }

    public class NumericalDistribution : IEquatable<NumericalDistribution>
    {
        private readonly int[] distributions;

        public NumericalDistribution(int[] distributions)
        {
            Array.Sort(distributions);
            this.distributions = distributions;
        }

        public bool Equals(NumericalDistribution other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return distributions.SequenceEqual(other.distributions);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NumericalDistribution)obj);
        }

        public override int GetHashCode()
        {
            return distributions != null ? distributions.Aggregate(17, (c, s) => c + s * 397) : 0;
        }

        public static bool operator ==(NumericalDistribution left, NumericalDistribution right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NumericalDistribution left, NumericalDistribution right)
        {
            return !Equals(left, right);
        }
    }

    public class PartDistribution
    {
        public int[] PartIndexes { get; }

        public static PartDistribution[] GetAll(int length)
        {
            var partDistributions = new List<PartDistribution>();
            for (int i = 0; i < 3; i++)
            {
                var distribution = new PartDistribution(0, length);
                AddPartDistribution(partDistributions, distribution, 0, i, length);
            }
            return partDistributions.ToArray();
        }

        private static void AddPartDistribution(List<PartDistribution> partDistributions, PartDistribution partDistribution, int depth, int i, int length)
        {
            var newPartDistribution = partDistribution.Clone();
            newPartDistribution.PartIndexes[depth] = i;
            partDistributions.Add(newPartDistribution);
            if (depth < length - 1)
            {
                for (int j = i; j < 3; j++)
                {
                    AddPartDistribution(partDistributions, newPartDistribution, depth + 1, j, length);
                }
            }
        }

        private PartDistribution Clone()
        {
            return new PartDistribution(PartIndexes.ToArray());
        }

        public override string ToString()
        {
            return string.Join(" ", PartIndexes.Select(i => i.ToString()));
        }

        public long GetRepresentation()
        {
            long l = PartIndexes.Select((p, bit) => new { p, bit }).Aggregate((long)0, (res, a) =>
              {
                  var u = a.p * (long)Math.Pow(3, a.bit);
                  return res + u;
              });
            return l;
        }
        public PartDistribution(int[] partIndexes)
        {
            PartIndexes = partIndexes;
        }

        public PartDistribution(int representation, int count)
        {
            PartIndexes = new int[count];
            for (int i = 0; i < count && representation > 0; i++)
            {
                PartIndexes[i] = representation % 3;
                representation = (representation - PartIndexes[i]) / 3;
            }
        }

        public bool IsValid(int[] inputs, out int[][] distribution, int totalPerBag)
        {
            var bags = Enumerable.Repeat(new Func<List<int>>(() => new List<int>()), 3).Select(f => f()).ToArray();
            for (int i = 0; i < inputs.Length; i++)
            {
                bags[PartIndexes[i]].Add(inputs[i]);
                if (bags[PartIndexes[i]].Sum() > totalPerBag)
                {
                    distribution = null;
                    return false;
                }
            }
            distribution = bags.Select(i => i.ToArray()).ToArray();
            return true;

        }
    }

    public class Part
    {
        public Part(int[] weights)
        {
        }
    }
}
