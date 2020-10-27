using Newtonsoft.Json;
using PoeBot.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PoeBot.Core.Services
{
    public class CurrenciesService : IDisposable
    {
        private HttpClient Client;

        private static List<Currency_ExRate> CurrenciesList;
        Timer _Timer;
        LoggerService _LoggerService;

        public CurrenciesService(LoggerService logger)
        {
            Client = new HttpClient();
            CurrenciesList = new List<Currency_ExRate>();
            _LoggerService = logger;
            Update();
            _Timer = new Timer();
            _Timer.Interval = new TimeSpan(0, 30, 0).TotalMilliseconds;
            _Timer.Elapsed += Update;
            _Timer.AutoReset = true;
            _Timer.Enabled = true;

        }

        public Currency_ExRate GetCurrencyByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            name = name.ToLower();

            switch (name)
            {
                case "alt":
                    name = "alteration";
                    break;

                case "fuse":
                    name = "fusing";
                    break;
                case "exa":
                    name = "exalted";
                    break;
                case "alch":
                    name = "alchemy";
                    break;
                case "jewellers":
                    name = "jeweller's";
                    break;

            }

            return CurrenciesList.Find((Currency_ExRate c) => c.Name.Contains(name.ToLower()));
        }

        private void Update()
        {
            var response = Client.GetAsync($"https://poe.ninja/api/data/currencyoverview?league={Properties.Settings.Default.League}&type=Currency&language=en").Result;
            var responseBody = response.Content.ReadAsStringAsync().Result;

            var ExchangeRatesJson = JsonConvert.DeserializeObject<CurrenciesJson>(responseBody);

            CurrenciesList.Clear();

            foreach (Line l in ExchangeRatesJson.Lines)
            {
                Currency_ExRate c = new Currency_ExRate(l.CurrencyTypeName, l.ChaosEquivalent);

                CurrenciesList.Add(c);
            }
            CurrenciesList.Add(new Currency_ExRate("Chaos Orb", 1));

            foreach (CurrencyDetail cd in ExchangeRatesJson.CurrencyDetails)
            {
                var img = "Assets/Currencies/" + cd.Name.ToLower().Replace(" ", "") + ".png";

                if (!File.Exists(img))
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(cd.Icon, img);
                    }
                }
            }


            _LoggerService.Log("Curencies updated!");
        }


        private void Update(Object source, System.Timers.ElapsedEventArgs e)
        {
            Update();
        }

        public void Dispose()
        {
           if(_Timer != null)
                _Timer.Stop();
        }
    }
}
