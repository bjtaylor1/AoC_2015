using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Day24
{
    [TestClass]
    public class UnitTest1
    {
        //private readonly int[] inputs = new[] {1, 2, 3, 3};
        private readonly int[] inputs = new[] {1, 2, 3, 4, 5,   7, 8, 9, 10, 11};
        //private readonly int[] inputs = File.ReadAllLines("input.txt").Select(int.Parse).ToArray();
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
        public void Part1()
        {
            var sleigh = new Sleigh(inputs);
            sleigh.Distribute();
        }
    }

    public class Sleigh
    {
        public List<int> Weights { get; }
        public List<List<int>> Bags = new List<List<int>>();
        private List<int[][]> balancingCombinations = new List<int[][]>();
        const int bagCount = 3;
        public Sleigh(int[] weights)
        {
            Weights = weights.ToList();
            for (int i = 0; i < bagCount; i++)
            {
                Bags.Add(new List<int>());
            }
            weightPerBag = weights.Sum()/bagCount;
        }

        private readonly Stack<int> bagsDistributedTo = new Stack<int>();
        private int weightPerBag;

        public void Distribute()
        {
            Distribute(0, 0);
        }

        public bool Distribute(int lowerLimit, int lowerBag)
        {
            var count = Weights.Count;
            if (Bags.Any(b => b.Sum() > weightPerBag)) return true;
            if (Weights.Count == 0)
            {
                balancingCombinations.Add(Bags.Select(b => b.ToArray()).ToArray());
            }
            for (int index = lowerLimit; index < count; index++)
            {
                bool overLimit = false;
                for (int bag = 0; bag < Bags.Count && !overLimit; bag++)
                {
                    var weight = Weights[index];
                    Bags[bag].Add(weight);
                    Weights.RemoveAt(index);
                    bagsDistributedTo.Push(bag);
                    overLimit = Distribute(index, bag);
                    Undo();
                }
            }
            return false;
        }

        public void Undo()
        {
            if(bagsDistributedTo.Count == 0) throw new InvalidOperationException("Nothing to undo");

            var bagDistributedTo = bagsDistributedTo.Pop();
            var bag = Bags[bagDistributedTo];
            var weight = bag.Last();
            bag.RemoveAt(bag.Count - 1);
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
