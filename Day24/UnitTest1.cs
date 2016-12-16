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

        [TestMethod]
        public void Part2()
        {
            var sleigh = new Sleigh(inputs, 4);
            sleigh.Distribute();
            Console.Out.WriteLine(sleigh.BestCount);
            Console.Out.WriteLine(sleigh.BestQe);
        }
    }

    public class Sleigh
    {
        public List<int> Weights { get; }
        public List<int> Bag = new List<int>();
        public Sleigh(int[] weights, int numBags)
        {
            Weights = weights.Reverse().ToList();

            weightPerBag = weights.Sum() / numBags;
        }

        private readonly int weightPerBag;

        public int BestCount { get; private set; } = int.MaxValue;
        public ulong BestQe { get; private set; } = ulong.MaxValue;

        public bool Distribute()
        {
            if (Bag.Sum() > weightPerBag) return true; //prune branch if any are overloaded

            if (Bag.Count > BestCount) return true; //prune branches with more in the bag than the smallest possible number

            if (Bag.Count == BestCount)
            {
                var qeOfSmallest = QE(Bag);
                if (qeOfSmallest >= BestQe)
                    return true; //prune branches with worse QE than the best
            }
            if (Bag.Sum() == weightPerBag)
            {
                if (Bag.Count < BestCount) //been beaten, reset best QE
                    BestQe = ulong.MaxValue;

                if (Bag.Count <= BestCount)
                {
                    BestCount = Bag.Count;
                    var qe = QE(Bag);
                    if (qe < BestQe) BestQe = qe;
                }
                LogManager.GetCurrentClassLogger().Info($"{string.Join("     ", string.Join(",", Bag.Select(n => n.ToString())))}, bestCount = {BestCount}, bestQe = {BestQe}");
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
}
