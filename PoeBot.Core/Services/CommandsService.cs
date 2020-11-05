using PoeBot.Core.Models;
using PoeBot.Core.Models.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoeBot.Core.Services
{
    public static class CommandsService
    {
        public static string CtrlC_PoE()
        {
            Clipboard.Clear();

            string ss = null;

            Thread.Sleep(100);

            var time = DateTime.Now + new TimeSpan(0, 0, 1);

            while (ss == null)
            {
                Win32.SendKeyInPoE("^c");
                ss = Win32.GetText();

                if (time < DateTime.Now)
                    ss = "empty_string";
            }

            return ss.Replace("\r", "");
        }

        public static double GetSizeInStack(string ctrlC_PoE)
        {
            if (!String.IsNullOrEmpty(ctrlC_PoE) && ctrlC_PoE != "empty_string")
            {
                int begin = ctrlC_PoE.IndexOf("Stack Size: ") + 12;
                int length = ctrlC_PoE.IndexOf("/") - begin;

                return Convert.ToDouble(ctrlC_PoE.Substring(begin, length));
            }
            return 0;
        }

        public static int GetStackSize_PoE_Pro(string item_info)
        {
            if (!item_info.Contains("Stack Size:"))
                return 1;

            int res = Convert.ToInt32(Regex.Match(item_info, @"Stack Size: [0-9.]+/([0-9.]+)").Groups[1].Value);

            return res;
        }

        public static string GetNameItem_PoE_Pro(string item_info)
        {
            if (item_info.Contains("Rarity: Currency"))
            {
                string str = Regex.Match(item_info, @"Rarity: Currency\s([\w ']+)").Groups[1].Value;
                return str;
            }

            else if (item_info.Contains("Map Tier:"))
            {
                if (item_info.Contains("Rarity: Rare"))
                {
                    var match = Regex.Match(item_info, @"Rarity: Rare\s([\w ']*)\s([\w ']*)");

                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        return match.Groups[2].Value.Replace(" Map", "");
                    }
                    else
                        return match.Groups[1].Value.Replace(" Map", "");
                }

                if (item_info.Contains("Rarity: Normal"))
                {
                    return Regex.Match(item_info, @"Rarity: Normal\s([\w ']*)").Groups[1].Value.Replace(" Map", "");
                }

                if (item_info.Contains("Rarity: Unique"))
                {
                    var match = Regex.Match(item_info, @"Rarity: Unique\s([\w ']*)\s([\w ']*)");

                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        return $"{match.Groups[1].Value} {match.Groups[2].Value}".Replace(" Map", "");
                    }
                    else
                        return "Undefined item";
                }

            }

            else if (item_info.Contains("Rarity: Divination Card"))
            {
                return Regex.Match(item_info, @"Rarity: Divination Card\s([\w ']*)").Groups[1].Value;
            }

            //I think that it for predicate fragments
            else if (!item_info.Contains("Requirements:"))
            {
                if (item_info.Contains("Rarity: Normal"))
                {
                    return Regex.Match(item_info, @"Rarity: Normal\s([\w ']*)").Groups[1].Value;
                }
            }
            else
            {
                var stbName = new StringBuilder();
                var splitteddName = item_info.Split(Environment.NewLine.ToArray(),StringSplitOptions.RemoveEmptyEntries);
                foreach(var item in splitteddName)
                {
                    if (item.Contains("Rarity"))
                        continue;
                    if (item.Contains("---"))
                    {
                        return stbName.ToString();
                    } 
                    else
                    {
                        stbName.Append(item);
                    }

                }
            }

            return "Not For Sell";

        }

        public static string GetNameItem_PoE(string clip)
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

        internal static string GetItemNamePosition(Position position)
        {
            Win32.MoveTo(position.Left,position.Top);
            Thread.Sleep(300);
            string clip = CtrlC_PoE();
            Thread.Sleep(200);
            return GetNameItem_PoE(clip);
        }
    }
}
