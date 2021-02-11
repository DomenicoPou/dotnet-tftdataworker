using CommonLibrary.Handlers;
using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TFTDataWorker.Handlers;
using TFTDataWorker.Models;
using TFTDataWorker.Models.DataModels;

namespace TFTDataWorker.Workers
{
    public class TFTSetWorker : BackgroundService
    {
        private readonly ILogger<TFTSetWorker> _logger;

        private readonly string tftDocUrl = "https://developer.riotgames.com/docs/tft";

        private ApplicationConfigHandler configHandler;

        public TFTSetWorker(ILogger<TFTSetWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("TFT Data Worker running at: {time}", DateTimeOffset.Now);

                // Obtain current configuration, set tft set to a placeholder
                configHandler = new ApplicationConfigHandler();

                List<Set> allSets = GetSets(out string currentSet);

                
                Set[] missingSets = allSets.Where(x => !configHandler.configuration.AllTftSet.Contains(x)).ToArray();
                foreach (Set set in missingSets)
                {
                    string dataDirectory = ExtractLatestSetZip(set.url, set.name);
                    set.isExtracted = false;
                }

                // Save the new configuration
                configHandler.configuration.CurrentTftSet = currentSet;
                configHandler.configuration.AllTftSet = allSets;
                configHandler.save();

                // Extract Latest Set Data
                foreach (Set set in allSets) { 
                    if (set.isExtracted == false || set.isCurrent)
                    {
                        SetHandler.ExtractDataFromSet(set, configHandler);
                    }
                }

                await Task.Delay(2000, stoppingToken);
            }
        }

        private List<Set> GetSets(out string currentSet)
        {
            List<Set> allSets = new List<Set>();
            
            // Obtain the current set
            HtmlDocument htmlDoc = WebCrawlerHandler.ObtainHtml(tftDocUrl);

            HtmlNode currentNode = htmlDoc.DocumentNode.Descendants("h2").Where(node => node.Id.Equals("static-data_current-set")).ToList()[0].NextSibling.NextSibling.FirstChild;
            string setName;
            setName = currentNode.GetAttributeValue("href", "").Split("set")[1];
            setName = setName.Substring(0, setName.Length - 4);
            allSets.Add(new Set()
            {
                set = setName,
                name = WindowsFriendly(currentNode.InnerText),
                url = currentNode.GetAttributeValue("href", ""),
                isCurrent = true,
                isExtracted = true // for now
            });

            // Set the current sets name
            currentSet = WindowsFriendly(currentNode.InnerText);

            // Get all previous sets
            List<HtmlNode> previousSetNode = htmlDoc.DocumentNode.Descendants("h2").Where(node => node.Id.Equals("static-data_previous-sets")).ToList()[0].NextSibling.NextSibling.Descendants("a").ToList();
            foreach(HtmlNode setNode in previousSetNode)
            {
                string prevSet;
                prevSet = setNode.GetAttributeValue("href", "").Split("set")[1];
                prevSet = prevSet.Substring(0, prevSet.Length - 4);
                allSets.Add(new Set()
                {
                    set = prevSet,
                    name = WindowsFriendly(setNode.InnerText.Replace(":", string.Empty)),
                    url = setNode.GetAttributeValue("href", ""),
                    isCurrent = false,
                    isExtracted = true // for now
                });
            }

            // Return all sets
            return allSets;
        }

        private string ExtractLatestSetZip(string url, string setName)
        {
            // Create the temp folder and current set temp folder
            Directory.CreateDirectory(configHandler.configuration.DataFolder);
            string SetFolder = $"{configHandler.configuration.DataFolder}/{setName}/";
            Directory.CreateDirectory(SetFolder);

            // Download current zip set to the temp folder as tmp name
            string tempZipName = $"{setName}.zip";
            using (WebClient Client = new WebClient())
            {
                Client.DownloadFile(url, $"{configHandler.configuration.DataFolder}/{tempZipName}");
            }
                
            // Extract the zip to the temporary folder and delete the zip
            ZipFile.ExtractToDirectory($"{configHandler.configuration.DataFolder}/{tempZipName}", SetFolder);
            File.Delete($"{configHandler.configuration.DataFolder}/{tempZipName}");

            // Return the setfolder location
            return SetFolder;
        }

        public string WindowsFriendly(string name)
        {
            return Regex.Replace(name, @"[^0-9a-zA-Z ]+", "");
        }
    }
}
