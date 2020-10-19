using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PoeBot.Core.Models
{
    public class Currency_ExRate
    {
        public string Name { get; set; }

        public string ImageName { get; set; }

        public double ChaosEquivalent { get; set; }

        public Currency_ExRate(string name, double chaosequivalent)
        {
            Name = name.ToLower();

            ChaosEquivalent = chaosequivalent;

            ImageName = "Assets/Currencies/" + Name.Replace(" ", "") + ".png";

        }

        public override string ToString()
        {
            return Name;
        }
    }
}
