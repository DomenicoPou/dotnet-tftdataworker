using Calculator.Source.models;
using Calculator.Source.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFTDataWorker.Handlers
{
    public static class PerfectsHandler
    {
        const int averageTier = 8;

        public static List<List<string>> GetPerfects(List<Champion> champions, List<Trait> traits, List<RollingOdds> rollingOdds)
        {
            List<Champion[]> variations = new List<Champion[]>();
            Dictionary<string, Trait> traitMap = traits.ToDictionary(x => x.name);
            for (int champCount = 1; champCount <= averageTier; champCount++)
            {
                List<Champion> champList = (new List<Champion>(champions)).FindAll(x => x.tier <= rollingOdds[champCount].canPurchase);
                IEnumerable<Champion[]> combinations = CombinationsRosettaWoRecursion(champList.ToArray(), champCount, traitMap);
                variations.AddRange(combinations);
            }

            List<List<string>> ret = new List<List<string>>();
            Parallel.ForEach(variations, cariationChamps =>
            {
                List<string> addition = new List<string>();
                foreach (Champion champion in cariationChamps)
                {
                    addition.Add(champion.name);
                }
                ret.Add(addition);
            });

            return ret;
        }

        private static List<Champion> GetChampions(IEnumerable<int> enumerable, List<Champion> champions)
        {
            List<Champion> ret = new List<Champion>();
            foreach (int num in enumerable)
            {
                ret.Add(champions[num]);
            }
            return ret;
        }

        private static bool IsPerfect(Champion[] champions, Dictionary<string, Trait> traitMap)
        {
            Dictionary<string, int> traitCount = new Dictionary<string, int>();
            int impossibleTraits = 0;
            foreach (Champion champion in champions)
            {
                foreach (string trait in champion.traits)
                {
                    if (traitCount.ContainsKey(trait))
                    {
                        traitCount[trait]++;
                    }
                    else
                    {
                        impossibleTraits += 1;
                        if (impossibleTraits > champions.Length) return false;
                        traitCount.Add(trait, 1);
                    }
                    if (traitCount.Count() > champions.Length) return false;
                }
            }
            foreach (KeyValuePair<string, int> traitPair in traitCount)
            {
                if (!traitMap[traitPair.Key].setNumbers.Contains(traitPair.Value))
                {
                    return false;
                }
            }
            return true;
        }

        public static IEnumerable<Champion[]> CombinationsRosettaWoRecursion(Champion[] array, int count, Dictionary<string, Trait> traitMap)
        {
            /*Champion[] result = new Champion[count];
            foreach (int[] j in CombinationsRosettaWoRecursion(count, array.Length))
            {
                bool dontYield = false;
                for (int i = 0; i < count; i++)
                {
                    result[i] = array[j[i]];
                }
                //if (IsPerfect(result, traitMap))
                //{
                    yield return result.ToArray();
                //}
            }*/
            foreach (Champion[] champions in ChampionRosetta(count, array))
            {
                if (IsPerfect(champions, traitMap))
                {
                    yield return champions;
                }
            }
        }


        public static IEnumerable<Champion[]> ChampionRosetta(int count, Champion[] array)
        {
            Champion[] result = new Champion[count];
            Stack<int> stack = new Stack<int>(count);
            stack.Push(0);
            while (stack.Count > 0)
            {
                int index = stack.Count - 1;
                int value = stack.Pop();
                while (value < array.Length)
                {
                    result[index++] = array[value++];
                    stack.Push(value);
                    if (index != count) continue;
                    yield return (Champion[])result.Clone(); // thanks to @xanatos
                                                        //yield return result;
                    break;
                }
            }
        }

        public static IEnumerable<int[]> CombinationsRosettaWoRecursion(int m, int n)
        {
            int[] result = new int[m];
            Stack<int> stack = new Stack<int>(m);
            stack.Push(0);
            while (stack.Count > 0)
            {
                int index = stack.Count - 1;
                int value = stack.Pop();
                while (value < n)
                {
                    result[index++] = value++;
                    stack.Push(value);
                    if (index != m) continue;
                    yield return (int[])result.Clone(); // thanks to @xanatos
                                                        //yield return result;
                    break;
                }
            }
        }
    }
}
