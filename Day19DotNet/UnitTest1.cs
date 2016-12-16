using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
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
            var m = new Molecule(new[]
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
            var m = new Molecule(new[]
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
                target
            }, true);
            int cost;
            var minSteps = m.GetStepsToMakeMoleculeFrom("e", out cost);

            Assert.AreEqual(steps, minSteps);
            Console.Out.WriteLine($"Cost = {cost}");
        }

        [Test]
        public void Part2()
        {

            var lines = File.ReadAllLines($"{TestContext.CurrentContext.TestDirectory}\\input.txt").Where(s => !string.IsNullOrEmpty(s)).ToArray();
            var m = new Molecule(lines, true);
            int cost;
            var minSteps = m.GetStepsToMakeMoleculeFrom("e", out cost);
            Console.Out.WriteLine(minSteps);
            Console.Out.WriteLine($"Cost = {cost}");
        }
    }

    public class Molecule
    {
        private static readonly Regex replacementRegex = new Regex(@"^(\w+)\s\=\>\s(\w+)$");
        private readonly Replacement[] replacements;
        private readonly string molecule;

        public Molecule(string[] lines, bool backwards = false)
        {
            var replacementMatches = lines.Take(lines.Length - 1)
                .Select(s => replacementRegex.Match(s)).ToArray();
            molecule = lines.Last();
            if (!replacementMatches.All(s => s.Success)) throw new InvalidOperationException("Invalid replacements");
            if (backwards)
                replacements = replacementMatches.Select(m => new Replacement(m.Groups[2].Value, m.Groups[1].Value)).ToArray();
            else
                replacements = replacementMatches.Select(m => new Replacement(m.Groups[1].Value, m.Groups[2].Value)).ToArray();
        }

        public int GetStepsToMakeMoleculeFrom(string initial, out int cost)
        {
            var paths = new List<Iteration> { new Iteration(molecule, 0) };
            cost = 0;
            var stepsToTarget = Transform(paths, ref cost);
            return stepsToTarget;
        }

        private int cycle;
        private int Transform(List<Iteration> paths, ref int cost)
        {
            Iteration target;
            while ((target = paths.FirstOrDefault(t => t.Value == "e")) == null)
            {
                cycle++;
                paths.Sort((i1, i2) =>
                {
                    var vis = i1.Visited.CompareTo(i2.Visited); //unvisited first
                    if (vis != 0) return vis;
                    var res = i1.Value.Length.CompareTo(i2.Value.Length); //short first
                    if (res != 0) return res;
                    return i1.Depth.CompareTo(i2.Depth);
                });
                var bestIteration = paths.First();
                if(bestIteration.Visited) throw new InvalidOperationException("No unvisited paths");
                LogManager.GetCurrentClassLogger().Info($"{cycle} : {bestIteration}");

                cost++;
                var newMolecules = GetDistinctMolecules(bestIteration)
                    .Select(s => new Iteration(s, bestIteration.Depth + 1))
                    .ToArray();
                paths.RemoveAll(i => newMolecules.Any(n =>
                {
                    var b = n.Depth < i.Depth && n.Value == i.Value;
                    if (b && i.Value == "CRnSiRnFYCaRnFArArFArThCaCaRnFAr") LogManager.GetCurrentClassLogger().Warn($"Removing {i} - because a better one is {n}");
                    return b;
                }));//prune any that are the same but can be got to quicker

                var path10210 = paths.Where(p => p.Id == 10210 || p.Id == 10216).ToArray();
                var newMoleculesToAdd = newMolecules.Where(n => !paths.Any(p => p.Value == n.Value)).ToArray();

                foreach (var cyclicElement in newMoleculesToAdd.Where(i => i.Value == "CRnSiRnFYCaRnFArArFArThCaCaRnFAr"))
                {
                    LogManager.GetCurrentClassLogger().Warn($"{cycle}: Adding cyclic element {cyclicElement}");
                }
                paths.AddRange(newMoleculesToAdd);

            }
            LogManager.GetCurrentClassLogger().Info(target);
            return target.Depth;
        }

        public string[] GetDistinctMolecules()
        {
            return GetDistinctMolecules(molecule);
        }

        private List<Iteration> searched = new List<Iteration>();
        private string[] GetDistinctMolecules(Iteration iteration)
        {
            if (iteration.Value == "CRnSiRnFYCaRnFArArFArThCaCaRnFAr")
            {
                LogManager.GetCurrentClassLogger().Warn($"Processing cyclic element {iteration}");
            }
            var alreadySearched = searched.Where(i => i.Value == iteration.Value);
            if (alreadySearched.Any())
            {
                throw new InvalidOperationException($"Already searched {iteration}");
            }
            searched.Add(iteration);
            iteration.Visited = true;
            return GetDistinctMolecules(iteration.Value);
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

    public class Iteration : IEquatable<Iteration>
    {
        public override string ToString()
        {
            return $"Depth: {Depth}: Id: {Id}, {Value}";
        }

        public Iteration(string value, int depth)
        {
            Value = value;
            Depth = depth;
        }

        private static int nextId;
        public int Id { get; } = nextId++;
        public string Value { get; }
        public int Depth { get; }
        public bool Visited { get; set; } = false;

        public bool Equals(Iteration other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Iteration) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        public static bool operator ==(Iteration left, Iteration right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Iteration left, Iteration right)
        {
            return !Equals(left, right);
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
