using Calculator.Source.models;
using Calculator.Source.Models;
using CommonLibrary.Handlers;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TFTDataWorker.Models.DataModels;

namespace TFTDataWorker.Handlers
{
    public static class SetHandler
    {
        private static string baseAddress = "https://lolchess.gg/";

        public static void ExtractDataFromSet(Set set, ApplicationConfigHandler configHandler)
        {
            string folder = configHandler.configuration.DataFolder;
            string setName = set.set.Replace("update", ".5");

            string OddsFileLocation = $"{configHandler.configuration.DataFolder}/oddsConfiguration.json";

            string TraitFileLocation = $"{configHandler.configuration.DataFolder}/{set.name}/TraitConfiguration.json";
            string ChampionFileLocation = $"{configHandler.configuration.DataFolder}/{set.name}/ChampConfiguration.json";
            string ItemFileLocation = $"{configHandler.configuration.DataFolder}/{set.name}/ItemConfiguration.json";

            string PerfectsFileLocation = $"{configHandler.configuration.DataFolder}/{set.name}/PerfectConfiguration.json";

            List<Trait> setsTraits = GetTraits(setName);
            List<RollingOdds> setsRollingOdds = GetRollingOdds();
            List<Champion> setsChampions = GetChampions(setName);
            List<Item> setsItems = GetItems(setName);
            List<List<string>> perfects = PerfectsHandler.GetPerfects(setsChampions, setsTraits, setsRollingOdds);

            if (!File.Exists(OddsFileLocation)) writeProperties(OddsFileLocation, setsRollingOdds);
            if (!File.Exists(TraitFileLocation)) writeProperties(TraitFileLocation, setsTraits);
            if (!File.Exists(ChampionFileLocation)) writeProperties(ChampionFileLocation, setsChampions);
            if (!File.Exists(ItemFileLocation)) writeProperties(ItemFileLocation, setsItems);
            if (!File.Exists(PerfectsFileLocation)) writeProperties(PerfectsFileLocation, perfects);
        }


        public static void writeProperties(string path, object content)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            using (var textReader = new StreamWriter(fileStream))
            {
                string toWrite = JsonConvert.SerializeObject(content, Formatting.Indented);
                textReader.Write(toWrite);
            }
        }


        public static List<Trait> GetTraits(string set)
        {
            HtmlDocument html = ObtainHtml($"synergies/set{set}/");
            List<HtmlNode> synergies = html.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "")
            .Contains("guide-synergy-table__synergy__header")).ToList();
            List<HtmlNode> description = html.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "")
            .Contains("guide-synergy-table__synergy__stats")).ToList();

            List<Trait> traits = new List<Trait>();
            for (int i = 0; i < synergies.Count(); i++)
            {
                string name = synergies[i].Descendants("span").ToList()[0].InnerText;
                List<int> synergyNumber = new List<int>();
                List<string> synergyEffects = new List<string>();
                foreach (HtmlNode desc in description[i].Descendants("div").ToList())
                {
                    string text = desc.InnerText.Trim();
                    synergyNumber.Add(Convert.ToInt32(text.Split(')')[0].Split('(')[1]));
                    synergyEffects.Add(text.Split(')')[1].Trim());
                }
                traits.Add(new Trait()
                {
                    name = name,
                    setNumbers = synergyNumber.ToArray(),
                    setEffects = synergyEffects.ToArray()
                });
            }
            return traits;
        }


        public static List<RollingOdds> GetRollingOdds()
        {
            // Get the congfiguration File
            string configFile = $@"{AppDomain.CurrentDomain.BaseDirectory}/Config/{typeof(RollingOdds).Name}.json";

            // Deserialize Json to Object and Return It
            using (var fileStream = new FileStream(configFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var textReader = new StreamReader(fileStream))
            {
                string content = textReader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<RollingOdds>>(content);
            }
        }

        public static List<Champion> GetChampions(string set)
        {
            HtmlDocument html = ObtainHtml($"champions/set{set}/");
            List<HtmlNode> championsNode = html.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("class", "")
                 .Contains("guide-champion-list__item__name")).ToList();
            //guide-champion-list__item
            List<HtmlNode> aList = html.DocumentNode.Descendants("a").Where(node => node.GetAttributeValue("class", "")
                .Contains("guide-champion-list__item")).ToList();
            List<Champion> champions = new List<Champion>();
            for (int i = 0; i < championsNode.Count(); i++)
            {
                champions.Add(GetChampion(set,
                    HttpUtility.HtmlDecode(championsNode[i].InnerHtml.Trim()),
                    HttpUtility.HtmlDecode(aList[i].GetAttributeValue("href", "").Split('/').Last())
                    ));
            }
            return champions;
        }

        public static List<Item> GetItems(string set)
        {
            HtmlDocument html = ObtainHtml($"items/set{set}/");
            //
            List<HtmlNode> itemsNode = html.DocumentNode.Descendants("table").Where(node => node.GetAttributeValue("class", "")
                .Contains("guide-items-table")).ToList()[0].Descendants("tbody").ToList()[0].Descendants("tr").ToList();
            List<Item> items = new List<Item>();
            foreach (HtmlNode itemNode in itemsNode)
            {
                string name = itemNode.Descendants("span").ToList()[0].InnerText.Trim();
                string description = itemNode.Descendants("td").ToList()[1].InnerText.Trim();
                List<HtmlNode> combinationNodes = itemNode.Descendants("td").ToList()[2].Descendants("img").ToList();
                List<string> itemString = new List<string>();
                foreach (HtmlNode combination in combinationNodes)
                {
                    itemString.Add(HttpUtility.HtmlDecode(combination.GetAttributeValue("alt", "")).Trim());
                }
                items.Add(new Item
                {
                    name = HttpUtility.HtmlDecode(name),
                    description = HttpUtility.HtmlDecode(description),
                    combination = itemString.ToArray()
                });
            }
            return items;
        }

        private static Champion GetChampion(string set, string name, string urlName)
        {
            Champion champRet = new Champion();
            champRet.name = name;

            HtmlDocument html = ObtainHtml($"champions/set{set}/{urlName}");

            // tier traits Get tier and traits 
            List<HtmlNode> basicNodes = html.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "")
                 .Equals("guide-champion-detail__stats__row")).ToList();

            champRet.tier = GetTiers(basicNodes[0]);
            basicNodes.RemoveAt(0);
            champRet.traits = GetChampTraits(basicNodes);

            // Get statistics
            List<HtmlNode> statNode = html.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "")
                 .Equals("guide-champion-detail__base-stat")).ToList();
            champRet.stats = GetStatistics(statNode);

            // Get ability 
            HtmlNode abilityNode = html.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", "")
                 .Equals("guide-champion-detail__skill")).ToList()[0];
            champRet.ability = GetAbility(abilityNode);

            return champRet;
        }

        private static string[] GetChampTraits(List<HtmlNode> basicNodes)
        {
            List<string> traits = new List<string>();
            foreach (HtmlNode node in basicNodes)
            {
                List<HtmlNode> traitNode = node.Descendants("img").ToList();
                foreach (HtmlNode trait in traitNode)
                {
                    traits.Add(trait.GetAttributeValue("alt", ""));
                }

            }
            return traits.ToArray();
        }

        private static int GetTiers(HtmlNode htmlNode)
        {
            string tier = Regex.Replace(htmlNode.InnerText, @"\s+", ":").Split(':')[2];
            return Convert.ToInt32(tier);
        }

        private static Ability GetAbility(HtmlNode abilityNode)
        {
            Ability abilityRet = new Ability();
            abilityRet.name = abilityNode.Descendants("img").ToList()[0].GetAttributeValue("alt", "");
            abilityRet.type = abilityNode.Descendants("span").ToList()[0].InnerText.Trim();

            try
            {
                abilityRet.startingMana = Convert.ToInt32(abilityNode.Descendants("span").ToList()[2].InnerText.Split('/')[0].Replace("Mana:", ""));
            }
            catch (Exception) { }
            try
            {
                abilityRet.mana = Convert.ToInt32(abilityNode.Descendants("span").ToList()[2].InnerText.Split('/')[1]);

                abilityRet.description = abilityNode.Descendants("span").Where(node => node.GetAttributeValue("class", "").Equals("d-block mt-1")).ToList()[0].InnerText.Trim();
            }
            catch (Exception) { }

            return abilityRet;
        }

        private static Statistics GetStatistics(List<HtmlNode> statNodes)
        {
            Statistics statistics = new Statistics();
            foreach (HtmlNode statNode in statNodes)
            {
                string statName = statNode.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("guide-champion-detail__base-stat__name")).ToList()[0].InnerHtml.Trim();
                HtmlNode statList = statNode.Descendants("div").Where(node => node.GetAttributeValue("class", "").Equals("guide-champion-detail__base-stat__value")).ToList()[0];
                string value = statList.InnerHtml.Trim();
                string innerValues = "";
                if (statList.InnerHtml.Contains("span"))
                {
                    innerValues = statList.Descendants("span").ToList()[0].InnerText.Trim();
                }
                switch (statName)
                {
                    case "Health":
                        statistics.health = getStatSetValue(value, innerValues);
                        break;

                    case "Attack Damage":
                        statistics.attackDamage = getStatSetValue(value, innerValues);
                        break;

                    case "DPS":
                        statistics.DPS = getStatSetValue(value, innerValues);
                        break;

                    case "Attack Speed":
                        try
                        {
                            statistics.attackSpeed = getStatSetValue(value, innerValues);
                        }
                        catch (Exception ex)
                        {
                            statistics.attackSpeed = null;
                        }
                        break;

                    case "Armor":
                        statistics.armor = Convert.ToDouble(value);
                        break;

                    case "Magical Resistance":
                        statistics.armor = Convert.ToDouble(value);
                        break;

                    default:
                        continue;
                }
            }
            return statistics;
        }

        private static double[] getStatSetValue(string value, string innerValues)
        {
            if (value.Contains("<") && value.Contains("/"))
            {
                List<double> valueList = new List<double>();
                valueList.Add(Convert.ToDouble(value.Split('<')[0].Trim()));
                valueList.Add(Convert.ToDouble(innerValues.Split('/')[1]));
                valueList.Add(Convert.ToDouble(innerValues.Split('/')[2]));
                return valueList.ToArray();
            }
            else
            {
                return new double[1] { Convert.ToDouble(value) };
            }
        }

        private static HtmlDocument ObtainHtml(string url)
        {
            HttpClient client = new HttpClient();
            string htmlString = client.GetStringAsync(baseAddress + url).Result;
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(htmlString);
            return html;
        }
    }
}
