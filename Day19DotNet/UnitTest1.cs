using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using CollectionAssert = NUnit.Framework.CollectionAssert;


namespace Day19DotNet
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void Part1()
        {
            var lines = File.ReadAllLines("input.txt").Where(s => !string.IsNullOrEmpty(s)).ToArray();
            var m = new Molecule(lines);
            var distinctMolecules = m.GetDistinctMolecules();
            Console.Out.WriteLine(distinctMolecules.Length);
        }

        [Test]
        public void Example1()
        {
            var m =new Molecule(new []
            {
                "H => HO", 
                "H => OH", 
                "O => HH",
                "HOH"
            });
            var molecules = m.GetDistinctMolecules();
            CollectionAssert.AllItemsAreUnique(molecules);
            Assert.AreEqual(4, molecules.Length);
            CollectionAssert.Contains(molecules, "HOOH");
            CollectionAssert.Contains(molecules, "HOHO");
            CollectionAssert.Contains(molecules, "OHOH");
            CollectionAssert.Contains(molecules, "HHHH");
        }

        [Test]
        public void Example2()
        {
            var m =new Molecule(new []
            {
                "H => HO", 
                "H => OH", 
                "O => HH",
                "HOHOHO"
            });
            var molecules = m.GetDistinctMolecules();
            CollectionAssert.AllItemsAreUnique(molecules);
            Assert.AreEqual(7, molecules.Length);
        }

        [TestCase("HOH", 3)]
        [TestCase("HOHOHO", 6)]
        public void Part2Example(string target, int steps)
        {
            var m = new Molecule(new[]
            {
                "e => H",
                "e => O",
                "H => HO",
                "H => OH",
                "O => HH",
                "HOH"
            });
            int cost;
            var minSteps = m.GetStepsToMakeMoleculeFrom("e", out cost);

            Assert.AreEqual(3, minSteps);
            Console.Out.WriteLine($"Cost = {cost}");
        }
    }

    public class Molecule
    {
        private static readonly Regex replacementRegex = new Regex(@"^(\w+)\s\=\>\s(\w+)$");
        private readonly Replacement[] replacements;
        private readonly string molecule;

        public Molecule(string[] lines)
        {
            var replacementMatches = lines.Take(lines.Length - 1)
                .Select(s => replacementRegex.Match(s)).ToArray();
            molecule = lines.Last();
            if (!replacementMatches.All(s => s.Success)) throw new InvalidOperationException("Invalid replacements");
            replacements = replacementMatches.Select(m => new Replacement(m.Groups[1].Value, m.Groups[2].Value)).ToArray();
        }

        public int GetStepsToMakeMoleculeFrom(string initial, out int cost)
        {
            var paths = new List<int>();
            cost = 0;
            Transform(initial, 1, paths, ref cost);
            return paths.Min();
        }

        private const int STOP = -1;
        private const int CONTINUE = -2;
        private int Transform(string current, int depth, List<int> paths, ref int cost )
        {
            var distinctMolecules = GetDistinctMolecules(current);
            cost++;
            foreach(var newMolecule in distinctMolecules)
            {
                if (newMolecule == molecule)
                {
                    paths.Add(depth);
                }
                else if (newMolecule.Length <= molecule.Length)
                {
                    var res = Transform(newMolecule, depth + 1, paths, ref cost);
                    if (res == STOP) return STOP;
                }
            }
            return CONTINUE;
        }

        public string[] GetDistinctMolecules()
        {
            return GetDistinctMolecules(molecule);
        }

        private string[] GetDistinctMolecules(string thisMolecule)
        {
            var molecules = new List<string>();

            foreach (var replacement in replacements)
            {
                int lastIndex = 0;

                while (lastIndex < thisMolecule.Length)
                {
                    bool matched = false;
                    var newMolecule = replacement.Regex.Replace(thisMolecule, m =>
                    {
                        matched = m.Success;
                        lastIndex = m.Index + m.Length;
                        return replacement.ReplaceWith;
                    }, 1, lastIndex);
                    if (!matched) break;
                    if (!molecules.Contains(newMolecule)) molecules.Add(newMolecule);
                }
            }
            return molecules.ToArray();
        }
    }

    public class Replacement
    {
        public Replacement(string pattern, string replaceWith)
        {
            Regex = new Regex(pattern, RegexOptions.Compiled);
            ReplaceWith = replaceWith;
        }

        public Regex Regex { get; }
        public string ReplaceWith { get; }
    }
}
