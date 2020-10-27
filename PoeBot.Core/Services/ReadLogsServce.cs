using PoeBot.Core.Models;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace PoeBot.Core.Services
{
    public class ReadLogsServce : IDisposable
    {
        LoggerService _LoggerService;
        CurrenciesService _CurrenciesService;
        bool isReading;
        private static string PoE_Path;
        private static string PoE_Logs_Dir;
        private static string PoE_Logs_File;

        public event EventHandler<TradeArgs> TradeRequest;
        public event EventHandler<TradeArgs> CustomerLeft;
        public event EventHandler<TradeArgs> CustomerArrived;
        public event EventHandler<TradeArgs> TradeCanceled;
        public event EventHandler<TradeArgs> TradeAccepted;
        public event EventHandler AFK;
        Thread thread;

        public ReadLogsServce(LoggerService logger,CurrenciesService currenies)
        {
            _LoggerService = logger;
            _CurrenciesService = currenies;
            SetupPaths();
            thread = new Thread(() => 
            {
                while (true)
                {
                    ReadLogsInBack();
                    Thread.Sleep(100);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }


        private void SetupPaths()
        {
            var lRootPaths = Properties.Settings.Default.ClientPaths;
            foreach (var path in lRootPaths)
            {
                if (Directory.Exists(path))
                {
                    PoE_Path = path;
                    PoE_Logs_Dir = Path.Combine(PoE_Path, "logs");
                    PoE_Logs_File = Path.Combine(PoE_Logs_Dir, "Client.txt");
                    break;
                }
            }
            ClearLog();

        }

        public void Dispose()
        {
            thread?.Abort();
        }

        int last_index = -1;
        bool not_first = false;
        private void ReadLogsInBack()
        {
            if (isReading)
            {
                return;
            }
            isReading = true;
            using (FileStream fs = new FileStream(PoE_Logs_File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    int li = 0;
                    string ll = string.Empty;
                    while (!sr.EndOfStream)
                    {
                        li++;
                        ll = sr.ReadLine();

                        if (not_first && li > last_index)
                        {
                            if (ll.Contains($"{Properties.Settings.Default.PreppendInfoClient} [INFO Client"))
                            {
                                _LoggerService.Log(ll);
                                if (ll.Contains("AFK mode is now ON"))
                                {
                                    AFK.Invoke(this,new EventArgs());
                                }
                                else if (ll.Contains("has left the area"))
                                {
                                    CustomerLeft.Invoke(this, new TradeArgs {  CustomerName = GetCustomerNick(ll) });
                                }
                                else if (ll.Contains("has joined the area"))
                                {
                                    CustomerArrived.Invoke(this, new TradeArgs { CustomerName = GetCustomerNick(ll) });
                                }
                                else if(ll.Contains("Trade accepted"))
                                {
                                    TradeAccepted.Invoke(this, new TradeArgs { });
                                }
                                else if (ll.Contains("Trade cancel"))
                                {
                                    TradeCanceled.Invoke(this, new TradeArgs { });
                                }
                                else if (ll.Contains("@"))
                                {
                                    var customer = GetInfo(ll);
                                    if(customer != null)
                                    {
                                        TradeRequest.Invoke(this, new TradeArgs { customer = customer });
                                    }
                                }

                            }
                        }
                    }

                    if (li > last_index)
                    {
                        last_index = li;
                        if (!not_first)
                            not_first = true;
                    }
                    isReading = false;
                }
            }
        }

        private string GetCustomerNick(string ll)
        {
            var stripped = ll.Split(":".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            if(stripped.Count() > 1)
            {
                stripped = stripped.Last().Split(" ".ToArray(),StringSplitOptions.RemoveEmptyEntries);
                if(stripped.Count() > 0)
                {
                    return stripped[0];
                }
            }
            return "";
        }

        private CustomerInfo GetInfo(string log24)
        {
            //GetFullInfoCustomer
            try
            {
                if(log24.Contains("@From"))
                {
                    if (log24.Contains("Hi, I would like to buy your"))
                    {
                        var cus_inf = new CustomerInfo();

                        cus_inf.OrderType = CustomerInfo.OrderTypes.SINGLE;

                        int length;
                        int begin;
                        //Nickname

                        if (!log24.Contains("> "))
                        {
                            begin = log24.IndexOf("@From ") + 6;
                            length = log24.IndexOf(": ") - begin;
                            cus_inf.Nickname = log24.Substring(begin, length);
                        }
                        else
                        {
                            begin = log24.IndexOf("> ") + 2;
                            length = log24.IndexOf(": ") - begin;
                            cus_inf.Nickname = log24.Substring(begin, length);
                        }


                        //Product
                        begin = log24.IndexOf("your ") + 5;
                        length = log24.IndexOf(" listed") - begin;
                        cus_inf.Product = log24.Substring(begin, length);

                        //Currency
                        begin = log24.IndexOf(" in") - 1;
                        for (int i = 0; i < 50; i++)
                        
                        {
                            if (log24[begin - i] == ' ')
                            {
                                begin = begin - i + 1;
                                break;
                            }
                        }
                        length = log24.IndexOf(" in") - begin;
                        cus_inf.Currency = _CurrenciesService.GetCurrencyByName(log24.Substring(begin, length));

                        //Price
                        begin = log24.IndexOf("for ") + 4;
                        cus_inf.Cost = Utils.GetNumber(begin, log24);

                        //Stash Tab
                        begin = log24.IndexOf("tab \"") + 5;
                        length = log24.IndexOf("\"; position") - begin;
                        cus_inf.Stash_Tab = log24.Substring(begin, length);

                        //left
                        begin = log24.IndexOf("left ") + 5;
                        cus_inf.Left = (int)Utils.GetNumber(begin, log24);

                        //top
                        begin = log24.IndexOf("top ") + 4;
                        cus_inf.Top = (int)Utils.GetNumber(begin, log24);

                        //to chaos chaosequivalent
                        cus_inf.Chaos_Price = cus_inf.Currency.ChaosEquivalent * cus_inf.Cost;

                        //trade accepted
                        cus_inf.TradeStatus = CustomerInfo.TradeStatuses.STARTED;

                        return cus_inf;
                    }

                    if (log24.Contains("I'd like to buy your"))
                    {
                        var cus = new CustomerInfo();

                        cus.OrderType = CustomerInfo.OrderTypes.MANY;

                        cus.Nickname = Regex.Replace(log24, @"([\w\s\W]+@From )|(: [\w\W\s]*)|(<[\w\W\s]+> )", "");

                        cus.Product = Regex.Replace(log24, @"([\w\W]+your +[\d,]* )|( for+[\w\s\W]*)|( Map [()\d\w]+)", "");

                        string test = Regex.Match(log24, @"your ([\d]+)").Groups[1].Value;

                        cus.NumberProducts = Convert.ToInt32(test);

                        cus.Cost = Convert.ToDouble(Regex.Replace(log24, @"([\s\w\W]+for my )|([\D])", "").Replace(".", ","));

                        cus.Currency = _CurrenciesService.GetCurrencyByName(Regex.Replace(log24, @"([\w\s\W]+my +[\d,.]* )|( in +[\w\W\s]*)", ""));

                        return cus;
                    }
                }
            }
            catch (Exception e)
            {

                _LoggerService.Log(e.Message);
            }
            return null;
        }
        private void ClearLog()
        {
            try
            {
                using (var fileStream = new FileStream(PoE_Logs_File, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    string line;
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        line = streamReader.ReadLine();
                    }
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        fileStream.Position = 0;
                        streamWriter.WriteLine(line);
                    }
                }
            }
            catch(Exception ex)
            {
                _LoggerService.Log(ex.Message);
            }
        }
    }
}
