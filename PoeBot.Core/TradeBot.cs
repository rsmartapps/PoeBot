using Emgu.CV.Structure;
using PoeBot.Core.Models;
using PoeBot.Core.Models.Test;
using PoeBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoeBot.Core
{
    public class TradeBot
    {

        CurrenciesService _CurrenciesService;
        ReadLogsServce _LogReaderServices;
        LoggerService _loggerService;
        ItemsService _ItemService;
        StashHelper _StashHelper;


        private List<CustomerInfo> Customers;
        private List<CustomerInfo> CompletedTrades;
        private CustomerInfo CurrentCustomer;
        private Thread TradinngThread;
        private bool _InTrade;
        public TradeBot(LoggerService logger)
        {
            _loggerService = logger;
        }

        public void Start()
        {
            if (!Win32.IsPoERun())
            {
                throw new Exception("Path of Exile is not running!");
            }


            if (Win32.GetActiveWindowTitle() != "Path of Exile")
            {
                Win32.PoE_MainWindow();
            }

            _CurrenciesService = new CurrenciesService(_loggerService);
            _ItemService = new ItemsService(_CurrenciesService);

            // Go to hideout
            Win32.ChatCommand("/hideout");
            Thread.Sleep(700);

            // Detect Stash
            if (!_StashHelper.OpenStash())
            {
                _loggerService.Log("Stash is not found in the area.");
                throw new Exception("Stash is not found in the area.");

            }

            // Clean inventory

            _StashHelper.ClearInventory("5");

            _StashHelper.ScanTab();

            // Listen for trades
            _LogReaderServices =new ReadLogsServce(_loggerService, _CurrenciesService);
            _LogReaderServices.TradeRequest += TradeRequest;

            // Move for not been afk
            _LogReaderServices.AFK += StopAFK;

            // Trade accepted event
            _LogReaderServices.TradeAccepted += TradeAccepted;
            // Trade canceled event
            _LogReaderServices.TradeCanceled += TradeCanceled;
            //Trade customer left event
            _LogReaderServices.CustomerLeft += CustomerLeft;
            // Trade customer arrived event
            _LogReaderServices.CustomerArrived += BeginTrade;
        }

        public void Stop()
        {
            _LogReaderServices?.Dispose();
            _CurrenciesService?.Dispose();
        }

        #region log requests
        private void TradeRequest(object sender, TradeArgs e)
        {
            Customers.Add(e.customer);
            CommandsService.InviteCustomer(e.customer);
            _loggerService.Log("Trade registered " + e.customer.Nickname);
        }
        private void StopAFK(object sender, EventArgs e)
        {
            Win32.ChatCommand("/afkoff");
        }
        private void BeginTrade(object sender, TradeArgs e)
        {
            bool requestTrade = false;
            var customer = Customers.FirstOrDefault(c => c.Nickname == e.CustomerName);
            if (customer == null)
            {
                _loggerService.Log($"Customer {e.CustomerName} not found in my trade list");
                return;
            }
            if (_InTrade)
            {
                _loggerService.Log($"Currently in trade, queue him");
                return;
            }
            _loggerService.Log("Customer arrived " + customer.Nickname);
            if (customer.OrderType == CustomerInfo.OrderTypes.SINGLE)
            {
                requestTrade = _StashHelper.WithrawItemFromStash(customer);
            }
            else if (customer.OrderType == CustomerInfo.OrderTypes.MANY)
            {
                requestTrade = _StashHelper.WithrawTradeItemsFromStash(customer);
            }
            if (requestTrade)
            {
                CurrentCustomer = customer;
                RequestTrade();
            }
            else
            {
                Customers.Remove(customer);
            }
        }
        private void CustomerLeft(object sender, TradeArgs e)
        {
            if (_InTrade)
            {
                TradinngThread.Abort();
                _InTrade = false;
            }
            _loggerService.Log("Customer left " + e.CustomerName);
            var customer = Customers.FirstOrDefault(c => c.Nickname == e.CustomerName);
            if (customer != null)
                EndTrade(customer);
        }
        private void TradeCanceled(object sender, TradeArgs e)
        {
            _loggerService.Log("Trade canceled " + CurrentCustomer.Nickname);
            if (CurrentCustomer == null)
                return;
            TradinngThread.Abort();
            Thread.Sleep(500);
            RequestTrade();
        }
        private void TradeAccepted(object sender, TradeArgs e)
        {
            if (CurrentCustomer != null)
            {
                _loggerService.Log("Trade accepted " + CurrentCustomer.Nickname);

                EndTrade(CurrentCustomer, true);
            }
        }
        #endregion


        #region Trade Operations
        private void RequestTrade()
        {
            _InTrade = true;
            TradinngThread = new Thread(() =>
            {
                if (Win32.GetActiveWindowTitle() != "Path of Exile")
                {
                    Win32.PoE_MainWindow();
                }

                if (CurrentCustomer == null)
                {
                    Thread.CurrentThread.Abort();
                    return;
                }

                // check if im invited
                if (!AcceptTradeRequest())
                {
                    // Send trade request
                    SendTradeRequest();
                }
                // Move items to trade window
                while (!IsTradeStarted())
                {
                    Thread.Sleep(100);
                }

                if (!PutItems())
                {
                    _InTrade = false;
                    // TODO add actio to end trade if cant put items
                    Win32.ChatCommand($"@{CurrentCustomer.Nickname} Item gone, sorry");
                    CommandsService.KickFormParty(CurrentCustomer);
                    Thread.CurrentThread.Abort();
                    return;
                }

                // validate until cancel or ok
                bool IsNotPaidYet = true;
                while (IsNotPaidYet)
                {
                    IsNotPaidYet = !CheckTradeCurrency();
                }
                // accept trade
                while (!AccepTrade())
                {
                    Thread.Sleep(100);
                }
                Thread.CurrentThread.Abort();
            });
            TradinngThread.SetApartmentState(ApartmentState.STA);
            TradinngThread.Start();
        }
        private bool AccepTrade()
        {
            MoveMouseOverBoxes();
            _loggerService.Log("Check trade window");
            Win32.MoveTo(280, 590);
            Thread.Sleep(100);
            Win32.DoMouseClick();
            return Utils.IsTradeAccepted();
        }
        #endregion


        #region Trade Functions
        private void InviteCustomer(CustomerInfo customer)
        {
            _loggerService.Log("Invite in party...");

            string command = "/invite " + customer.Nickname;

            Win32.ChatCommand(command);
        }

        public void EndTrade(CustomerInfo customer, bool succeed = false)
        {
            _loggerService.Log("Trade succeed with " + CurrentCustomer.Nickname);
            CurrentCustomer = null;
            CommandsService.KickFormParty(customer);
            if (succeed)
            {
                Win32.ChatCommand($"@{customer.Nickname} ty gl");
                CompletedTrades.Add(customer);
                _loggerService.Log("Trade comlete sucessfully");
            }
            else
            {
                _loggerService.Log($"Trade failed with {customer.Nickname}");
            }

            Customers.Remove(customer);

            if (!_StashHelper.OpenStash())
            {
                _loggerService.Log("Stash not found. I cant clean inventory after trade.");
            }
            else
            {
                if (succeed)
                {

                }
                else
                    PutItemBack(customer);
            }
            _InTrade = false;
        }

        private bool CheckArea()
        {
            _loggerService.Log("Check area...");
            for (int i = 0; i < 60; i++)
            {
                if (CurrentCustomer.IsInArea)
                {
                    return true;
                }
                Thread.Sleep(500);
            }
            _loggerService.Log("Player not here");
            return false;
        }

        private bool TradeQuery()
        {
            Position found_pos = null;

            Bitmap screen_shot = null;

            bool amIdoRequest = false;

            for (int try_count = 0; try_count < 3; try_count++)
            {
                _loggerService.Log("Try to accept or do trade...");

                for (int i = 0; i < 10; i++)
                {
                    if (!amIdoRequest)
                    {
                        screen_shot = ScreenCapture.CaptureRectangle(1030, 260, 330, 500);

                        found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/accespt.png");

                        if (found_pos != null && found_pos.IsVisible)
                        {
                            _loggerService.Log("I will Accept trade request!");

                            Win32.MoveTo(1030 + found_pos.Left + found_pos.Width / 2, 260 + found_pos.Top + found_pos.Height / 2);

                            Thread.Sleep(100);

                            Win32.DoMouseClick();

                            Thread.Sleep(100);

                            Win32.MoveTo((1030 + screen_shot.Width) / 2, (260 + screen_shot.Height) / 2);

                            amIdoRequest = true;
                        }
                        else
                        {
                            _loggerService.Log("i write trade");
                            string trade_command = "/tradewith " + CurrentCustomer.Nickname;

                            Win32.ChatCommand(trade_command);

                            screen_shot = ScreenCapture.CaptureRectangle(455, 285, 475, 210);

                            if (!CurrentCustomer.IsInArea)
                                return false;

                            found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/trade_waiting.png");
                            if (found_pos != null && found_pos.IsVisible)
                            {
                                Win32.MoveTo(455 + found_pos.Left + found_pos.Width / 2, 285 + found_pos.Top + found_pos.Height / 2);

                                screen_shot.Dispose();

                                amIdoRequest = true;
                            }
                            else
                            {
                                _loggerService.Log("Check trade window");
                                screen_shot = ScreenCapture.CaptureRectangle(330, 15, 235, 130);
                                found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/trade_window_title.png");
                                if (found_pos != null && found_pos.IsVisible)
                                {
                                    _loggerService.Log("I am in trade!");
                                    screen_shot.Dispose();
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        _loggerService.Log("Check trade window");
                        screen_shot = ScreenCapture.CaptureRectangle(330, 15, 235, 130);
                        found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/trade_window_title.png");
                        if (found_pos != null && found_pos != null && found_pos.IsVisible)
                        {
                            screen_shot.Dispose();
                            _loggerService.Log("I am in trade!");
                            return true;
                        }
                    }
                    Thread.Sleep(500);
                }

            }
            return false;
        }

        private bool GetProduct()
        {
            int x_inventory = 925;
            int y_inventory = 440;
            int offset = 37;

            Bitmap screen_shot;

            for (int j = 0; j < 12; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    Win32.MoveTo(x_inventory + offset * j, y_inventory + 175);

                    Thread.Sleep(100);

                    screen_shot = ScreenCapture.CaptureRectangle(x_inventory - 30 + offset * j, y_inventory - 30 + offset * i, 60, 60);

                    Position pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/empty_cel.png", 0.4);

                    if (pos != null && !pos.IsVisible)
                    {
                        Clipboard.Clear();

                        string ss = null;

                        Thread.Sleep(100);

                        Win32.MoveTo(x_inventory + offset * j, y_inventory + offset * i);

                        var time = DateTime.Now + new TimeSpan(0, 0, 5);

                        while (ss == null)
                        {
                            Win32.SendKeyInPoE("^c");
                            ss = Win32.GetText();

                            if (time < DateTime.Now)
                                ss = "empty_string";
                        }

                        if (ss == "empty_string")
                            continue;

                        if (CurrentCustomer.Product.Contains(CommandsService.GetNameItem_PoE(ss)))
                        {
                            _loggerService.Log($"{ss} is found in inventory");

                            Win32.CtrlMouseClick();

                            screen_shot.Dispose();

                            return true;
                        }

                    }
                    screen_shot.Dispose();
                }
            }
            Win32.SendKeyInPoE("{ESC}");

            Win32.ChatCommand("@" + CurrentCustomer.Nickname + " I sold it, sry");

            return false;
        }

        int x_trade = 221;
        int y_trade = 145;
        int tab_height = 38;
        int tab_width = 38;
        private bool CheckTradeCurrency()
        {
            if (Win32.GetActiveWindowTitle() != "Path of Exile")
            {
                Win32.PoE_MainWindow();
            }

            Item currency;
            List<Item> lCurrency = new List<Item>();
            double totalChaos = 0;

            Bitmap cell;
            Bitmap template = new Bitmap($"Assets/{Properties.Settings.Default.UI_Fragments}/empty_cel2.png");

            Win32.MoveTo(0, 0);
            Thread.Sleep(100);

            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    int xInit = x_trade + (tab_width * i) - i / 2;
                    int yInit = y_trade + (tab_height * j) - j / 2;
                    cell = ScreenCapture.CaptureRectangle(xInit, yInit, tab_width, tab_height);
                    if (!OpenCV_Service.Match(cell, template, 0.80f))
                    {
                        Win32.MoveTo(xInit + (tab_width / 2), yInit + (tab_height / 2));
                        Thread.Sleep(100);

                        string clip = CommandsService.CtrlC_PoE();
                        currency = _ItemService.GetCurrency(clip);

                        if (currency != null)
                        {
                            lCurrency.Add(currency);
                            totalChaos += currency.Price.Cost;
                        }

                    }
                }
            }

            // validate price
            if (CurrentCustomer != null)
            {
                // Check value

                if (CurrentCustomer.Chaos_Price < totalChaos)
                {
                    return true;
                }
            }
            return false;
        }
        private bool CheckCurrency()
        {
            // TODO create new check currency which check currencys with control + c and checks name and stack size
            List<Position> found_positions = null;

            List<Currency_ExRate> main_currs = new List<Currency_ExRate>();

            //set main currencies

            main_currs.Add(CurrentCustomer.Currency);

            if (CurrentCustomer.Currency.Name != "chaos orb")
            {
                main_currs.Add(_CurrenciesService.GetCurrencyByName("chaos"));
            }

            if (CurrentCustomer.Currency.Name != "divine orb")
            {
                main_currs.Add(_CurrenciesService.GetCurrencyByName("divine"));
            }

            if (CurrentCustomer.Currency.Name != "exalted orb")
            {
                main_currs.Add(_CurrenciesService.GetCurrencyByName("exalted"));
            }

            if (CurrentCustomer.Currency.Name != "orb of alchemy")
            {
                main_currs.Add(_CurrenciesService.GetCurrencyByName("alchemy"));
            }

            if (CurrentCustomer.Currency.Name == "exalted orb")
            {
                //Win32.ChatCommand($"@{CurrentCustomer.Nickname} exalted orb = {_CurrenciesService.GetCurrencyByName("exalted").ChaosEquivalent}");

                main_currs.Add(_CurrenciesService.GetCurrencyByName("exalted"));
            }

            Bitmap screen_shot = null;

            int x_trade = 220;
            int y_trade = 140;

            for (int i = 0; i < 30; i++)
            {
                double price = 0;

                foreach (Currency_ExRate cur in main_currs)
                {
                    if (!System.IO.File.Exists(cur.ImageName))
                    {
                        continue;
                    }
                    Win32.MoveTo(0, 0);

                    Thread.Sleep(100);

                    screen_shot = ScreenCapture.CaptureRectangle(x_trade, y_trade, 465, 200);

                    found_positions = OpenCV_Service.FindCurrencies(screen_shot, cur.ImageName, 0.6);

                    foreach (Position pos in found_positions)
                    {
                        Win32.MoveTo(x_trade + pos.Left + pos.Width / 2, y_trade + pos.Top + pos.Height / 2);

                        Thread.Sleep(100);

                        string ctrlc = CommandsService.CtrlC_PoE();
                        // TODO fix this currency stack count
                        var curbyname = _CurrenciesService.GetCurrencyByName(CommandsService.GetNameItem_PoE(ctrlc));

                        if (curbyname == null)

                            price += CommandsService.GetSizeInStack(ctrlc) * cur.ChaosEquivalent;

                        else

                            price += CommandsService.GetSizeInStack(ctrlc) * curbyname.ChaosEquivalent;


                        screen_shot.Dispose();
                    }

                    if (price >= CurrentCustomer.Chaos_Price && price != 0)
                        break;
                }

                _loggerService.Log("Bid price (in chaos) = " + price + " Necessary (in chaos) = " + CurrentCustomer.Chaos_Price);

                if (price >= CurrentCustomer.Chaos_Price)
                {
                    _loggerService.Log("I want accept trade");

                    screen_shot = ScreenCapture.CaptureRectangle(200, 575, 130, 40);

                    Position pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/accept_tradewindow.png");

                    if (pos.IsVisible)
                    {
                        Win32.MoveTo(210 + pos.Left + pos.Width / 2, 580 + pos.Top + pos.Height / 2);

                        Thread.Sleep(100);

                        Win32.DoMouseClick();

                        screen_shot.Dispose();

                        var timer = DateTime.Now + new TimeSpan(0, 0, 5);

                        while (CurrentCustomer.TradeStatus != CustomerInfo.TradeStatuses.ACCEPTED)
                        {
                            if (CurrentCustomer.TradeStatus == CustomerInfo.TradeStatuses.CANCELED)
                                return false;

                            if (DateTime.Now > timer)
                                break;
                        }

                        if (CurrentCustomer.TradeStatus == CustomerInfo.TradeStatuses.ACCEPTED)
                            return true;

                        else continue;

                    }
                }
                else
                {
                    screen_shot.Dispose();
                }

                Thread.Sleep(500);
            }

            Win32.SendKeyInPoE("{ESC}");

            return false;
        }

        private void MoveMouseOverBoxes()
        {
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    int xInit = x_trade + (tab_width * i) - i / 2;
                    int yInit = y_trade + (tab_height * j) - j / 2;
                    Win32.MoveTo(xInit + (tab_width / 2), yInit + (tab_height / 2));
                    Thread.Sleep(100);
                }
            }

        }


        private bool IsTradeStarted()
        {
            var zeroTrade = Utils.ZeroTrade();
            int x_trade = 220; // zeroTrade.X;
            int y_trade = 140; // zeroTrade.Y;

            Bitmap screen_shot = ScreenCapture.CaptureRectangle(x_trade, y_trade, 465, 200);

            Position found_positions = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/empty_cel.png", 0.5);

            screen_shot.Dispose();

            return found_positions.IsVisible;
        }

        private void SendTradeRequest()
        {
            if (CurrentCustomer == null) return;
            _loggerService.Log($"Send trade request with {CurrentCustomer.Nickname}");
            string trade_command = "/tradewith " + CurrentCustomer.Nickname;
            Win32.ChatCommand(trade_command);
        }

        private bool AcceptTradeRequest()
        {
            Bitmap screen_shot = ScreenCapture.CaptureRectangle(1030, 260, 330, 500);

            Position found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/accespt.png");

            if (found_pos != null && found_pos.IsVisible)
            {
                _loggerService.Log("I will Accept trade request!");

                Win32.MoveTo(1030 + found_pos.Left + found_pos.Width / 2, 260 + found_pos.Top + found_pos.Height / 2);

                Thread.Sleep(100);

                Win32.DoMouseClick();

                Thread.Sleep(100);

                Win32.MoveTo((1030 + screen_shot.Width) / 2, (260 + screen_shot.Height) / 2);

                return true;
            }
            return false;
        }

        #endregion


        public bool PutItems()
        {
            int x_inventory = 925;
            int y_inventory = 440;
            int offset = 37;

            var customer = CurrentCustomer;

            int TotalAmount = 0;

            List<Cell> skip = new List<Cell>();

            for (int j = 0; j < 12; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (skip.Find(cel => cel.Left == i && cel.Top == j) != null)
                    {
                        continue;
                    }

                    Win32.MoveTo(x_inventory + offset * j, +175);

                    Thread.Sleep(100);

                    var screen_shot = ScreenCapture.CaptureRectangle(x_inventory - 30 + offset * j, y_inventory - 30 + offset * i, 60, 60);

                    var pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/empty_cel.png", 0.5);

                    if (!pos.IsVisible)
                    {
                        Win32.MoveTo(x_inventory + offset * j, y_inventory + offset * i);

                        var item_info = CommandsService.CtrlC_PoE();

                        string name = CommandsService.GetNameItem_PoE_Pro(item_info);

                        if (name != customer.Product)
                        {
                            continue;
                        }

                        int SizeInStack = CommandsService.GetStackSize_PoE_Pro(item_info);

                        TotalAmount += SizeInStack;

                        if (name.Contains("Resonator"))
                        {
                            if (name.Contains("Potent"))
                            {
                                skip.Add(new Cell(i, j + 1));

                            }

                            if (name.Contains("Prime") || name.Contains("Powerful"))
                            {
                                skip.Add(new Cell(i, j + 1));
                                skip.Add(new Cell(i + 1, j + 1));
                                skip.Add(new Cell(i + 1, j));
                            }
                        }

                        Win32.CtrlMouseClick();

                        Thread.Sleep(250);

                        if (TotalAmount >= customer.NumberProducts)
                        {
                            screen_shot.Dispose();

                            _loggerService.Log($"I put {TotalAmount} items in trade window");

                            return true;
                        }
                    }

                    screen_shot.Dispose();
                }
            }
            Win32.SendKeyInPoE("{ESC}");

            Win32.ChatCommand("@" + CurrentCustomer.Nickname + " i sold it, sry");

            return false;
        }
        public void PutItemBack(CustomerInfo customer)
        {
            bool storedBack = false;
            _loggerService.Log($"Put item back because of trade failed with customer {customer.Nickname} trading {customer.Product}");
            if (customer != null)
            {
                if (!_StashHelper.OpenStash())
                {
                    throw new Exception("Stash not found");
                }
                if (!_StashHelper.OpenTab(customer.Stash_Tab))
                {
                    throw new Exception("Tab not found");
                }
                if (!_StashHelper.MoveItemBack(customer.Product))
                {
                    _StashHelper.ClearInventory();
                }
                Win32.SendKeyInPoE("ESC");
            }
        }


    }
}