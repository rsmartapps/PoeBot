using PoeBot.Core.Models;
using PoeBot.Core.Models.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PoeBot.Core.Services
{
    public class ItemsService
    {
        CurrenciesService _CurrenciesService;
        public ItemsService(CurrenciesService currencyServices)
        {
            _CurrenciesService = currencyServices;
        }
        public string GetNameItem_PoE(string clip)
        {
            if (!String.IsNullOrEmpty(clip) && clip != "empty_string")
            {
                var lines = clip.Split('\n');

                if (lines.Count() == 1)
                    return null;

                if (!lines[2].Contains("---"))
                {
                    return lines[1].Replace("\r", "") + " " + lines[2].Replace("\r", "");
                }
                else
                    return lines[1].Replace("\r", "");

            }
            return null;
        }
        public bool IsValidPrice(string ctrlC_PoE,CustomerInfo customer)
        {
            bool isvalidprice = false;
            bool isvalidcurrency = false;

            if (!String.IsNullOrEmpty(ctrlC_PoE) && ctrlC_PoE != "empty_string")
            {
                var lines = ctrlC_PoE.Split('\n');

                foreach (string str in lines)
                {
                    if (str.Contains("Note: ~price"))
                    {
                        var result = Regex.Replace(str, "[^0-9.]", "");

                        double price = Convert.ToDouble(result);

                        if (price <= customer.Cost)
                            isvalidprice = true;

                        int length = str.Length - 1;
                        int begin = 0;

                        for (int i = length; i > 0; i--)
                        {
                            if (str[i] == ' ')
                            {
                                begin = i + 1;
                                break;
                            }
                        }

                        result = str.Substring(begin, str.Length - begin).Replace("\r", "");

                        if (_CurrenciesService.GetCurrencyByName(result).Name == customer.Currency.Name)
                        {
                            isvalidcurrency = true;
                        }


                        if (isvalidcurrency && isvalidprice)
                            return true;
                    }
                }
            }
            return false;
        }
        public Price GetPrice_PoE(string item_info)
        {
            Price price = new Price();

            if (!item_info.Contains("Note: ~price"))
                return new Price();

            if (Regex.IsMatch(item_info, "~price [0-9.]+/[0-9.]+"))
            {
                price.Cost = Convert.ToDouble(Regex.Replace(item_info, @"([\w\s\W\n]+Note: ~price )|(/+[\w\s\W]*)|([^0-9.])", ""));

                price.ForNumberItems = Convert.ToInt32(Regex.Replace(item_info, @"([\w\s\W]+/)|([^0-9.])", ""));

                price.CurrencyType = _CurrenciesService.GetCurrencyByName(Regex.Replace(item_info, @"[\w\s\W]+\d+\s|\n", ""));
            }
            if (Regex.IsMatch(item_info, @"~price +[0-9.]+\s\D*"))
            {
                price.Cost = Convert.ToDouble(Regex.Replace(item_info, @"[\w\W]*~price |[^0-9.]*", "").Replace('.', ','));

                price.ForNumberItems = CommandsService.GetStackSize_PoE_Pro(item_info);

                price.CurrencyType = _CurrenciesService.GetCurrencyByName(Regex.Replace(item_info, @"[\w\s\W]+\d+\s|\n", ""));
            }

            if (!price.IsSet)
                return new Price();
            else
                return price;
        }

        internal Item GetCurrency(string clip)
        {
            if(String.IsNullOrEmpty(clip) || clip == "empty_string")
            {
                return null;
            }
            Item retItem = new Item();
            retItem.Name = GetName(clip);
            retItem.StackSize = GetStackSize(clip);
            retItem.Price = new Price() { CurrencyType = _CurrenciesService.GetCurrencyByName(retItem.Name) };
            if (retItem.Price.CurrencyType == null)
                return null;
            retItem.Price.Cost = retItem.Price.CurrencyType.ChaosEquivalent * retItem.StackSize;
            return retItem;
        }

        public int GetStackSize(string clip)
        {
            if (!String.IsNullOrEmpty(clip) && clip != "empty_string")
            {
                var lines = clip.Split('\n');

                if (lines.Count() == 1)
                    return 0;

                foreach(var line in lines)
                {
                    if (line.Contains("Stack Size"))
                    {
                        string quantity = line.Split(":".ToArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                        var total = quantity.Split("/".ToArray())[0];
                        return Convert.ToInt32(total);
                    }
                }

            }
            return 0;
        }

        public string GetName(string clip)
        {
            return CommandsService.GetNameItem_PoE(clip);
        }
    }
}
