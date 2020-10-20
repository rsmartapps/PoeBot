using PoeBot.Core.Models;
using PoeBot.Core.Models.Test;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoeBot.Core.Services
{
    public class TradeService
    {
        CurrenciesService _CurrenciesService;
        LoggerService _LoggerService;
        ItemsService _itemmService;
        private List<CustomerInfo> Customers;
        private List<CustomerInfo> CompletedTrades;
        private CustomerInfo CurrentCustomer;

        private readonly int Top_Stash64 = 135;
        private readonly int Left_Stash64 = 25;

        private Thread TradinngThread;

        Tab _Tabs;
        bool _InTrade = false;

        int tradeAttemts = 5;


        public TradeService(ItemsService itemmService, CurrenciesService currenciesService, LoggerService logger, Tab tab)
        {
            _itemmService = itemmService;
            _CurrenciesService = currenciesService;
            _LoggerService = logger;
            _Tabs = tab;
            Customers = new List<CustomerInfo>();
            CompletedTrades = new List<CustomerInfo>();
        }

        #region log requests
        internal void TradeRequest(object sender, TradeArgs e)
        {
            Customers.Add(e.customer);
            InviteCustomer(e.customer);
            _LoggerService.Log("Trade registered "+e.customer.Nickname);
        }
        internal void StopAFK(object sender, EventArgs e)
        {
            Win32.ChatCommand("Hi, are u there?");
            Win32.DoMouseRightClick();
        }
        internal void BeginTrade(object sender, TradeArgs e)
        {
            var customer = Customers.FirstOrDefault(c => c.Nickname == e.CustomerName);
            if(customer == null)
            {
                _LoggerService.Log($"Customer {e.CustomerName} not found in my trade list");
                return;
            }
            _LoggerService.Log("Customer arrived " + customer.Nickname);
            if(customer.OrderType == CustomerInfo.OrderTypes.SINGLE)
            {
                WithrawItemFromStash(customer);
            }
            else if (customer.OrderType == CustomerInfo.OrderTypes.MANY)
            {
                WithrawTradeItemsFromStash(customer);
            }
            CurrentCustomer = customer;
            RequestTrade();
        }
        internal void CustomerLeft(object sender, TradeArgs e)
        {
            _LoggerService.Log("Customer left " + e.CustomerName);
            var customer = Customers.FirstOrDefault(c => c.Nickname == e.CustomerName);
            if(customer != null)
                EndTrade(customer);
        }
        internal void TradeCanceled(object sender, TradeArgs e)
        {
            if (CurrentCustomer == null)
                return;
            _LoggerService.Log("Trade canceled " + CurrentCustomer.Nickname);
            --tradeAttemts;
            if (tradeAttemts <= 0)
                EndTrade(CurrentCustomer);
            else
            {
                TradinngThread.Abort();
                Thread.Sleep(500);
                RequestTrade();
            }
        }
        internal void TradeAccepted(object sender, TradeArgs e)
        {
            if(CurrentCustomer != null)
            {
                _LoggerService.Log("Trade accepted " + CurrentCustomer.Nickname);

                EndTrade(CurrentCustomer);
            }
        }
        #endregion

        #region Trade Functions
        private void InviteCustomer(CustomerInfo customer)
        {
            _LoggerService.Log("Invite in party...");

            string command = "/invite " + customer.Nickname;

            Win32.ChatCommand(command);
        }

        public void EndTrade(CustomerInfo customer,bool succeed = false)
        {
            _LoggerService.Log("Trade succeed with " + CurrentCustomer.Nickname);
            CurrentCustomer = null;
            KickFormParty(customer);
            if (succeed)
            {
                Win32.ChatCommand($"@{customer.Nickname} ty gl");
                CompletedTrades.Add(customer);
                _LoggerService.Log("Trade comlete sucessfully");
            }
            else
            {
                _LoggerService.Log($"Trade failed with {customer.Nickname}");
            }

            Customers.Remove(customer);

            if (!OpenStash())
            {
                _LoggerService.Log("Stash not found. I cant clean inventory after trade.");
            }
            else
            {
                ClearInventory();
            }

        }

        private bool CheckArea()
        {
            _LoggerService.Log("Check area...");
            for (int i = 0; i < 60; i++)
            {
                if (CurrentCustomer.IsInArea)
                {
                    return true;
                }
                Thread.Sleep(500);
            }
            _LoggerService.Log("Player not here");
            return false;
        }

        private bool TradeQuery()
        {
            Position found_pos = null;

            Bitmap screen_shot = null;

            bool amIdoRequest = false;

            for (int try_count = 0; try_count < 3; try_count++)
            {
                _LoggerService.Log("Try to accept or do trade...");

                for (int i = 0; i < 10; i++)
                {
                    if (!amIdoRequest)
                    {
                        screen_shot = ScreenCapture.CaptureRectangle(1030, 260, 330, 500);

                        found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/accespt.png");

                        if (found_pos.IsVisible)
                        {
                            _LoggerService.Log("I will Accept trade request!");

                            Win32.MoveTo(1030 + found_pos.Left + found_pos.Width / 2, 260 + found_pos.Top + found_pos.Height / 2);

                            Thread.Sleep(100);

                            Win32.DoMouseClick();

                            Thread.Sleep(100);

                            Win32.MoveTo((1030 + screen_shot.Width) / 2, (260 + screen_shot.Height) / 2);

                            amIdoRequest = true;
                        }
                        else
                        {
                            _LoggerService.Log("i write trade");
                            string trade_command = "/tradewith " + CurrentCustomer.Nickname;

                            Win32.ChatCommand(trade_command);

                            // TODO sacar esto fuera
                            screen_shot = ScreenCapture.CaptureRectangle(455, 285, 475, 210);

                            if (!CurrentCustomer.IsInArea)
                                return false;

                            found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/trade_waiting.png");
                            if (found_pos.IsVisible)
                            {
                                Win32.MoveTo(455 + found_pos.Left + found_pos.Width / 2, 285 + found_pos.Top + found_pos.Height / 2);

                                screen_shot.Dispose();

                                amIdoRequest = true;
                            }
                            else
                            {
                                _LoggerService.Log("Check trade window");
                                screen_shot = ScreenCapture.CaptureRectangle(330, 15, 235, 130);
                                found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/trade_window_title.png");
                                if (found_pos.IsVisible)
                                {
                                    _LoggerService.Log("I am in trade!");
                                    screen_shot.Dispose();
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        // TODO Sacar esto fuera
                        _LoggerService.Log("Check trade window");
                        screen_shot = ScreenCapture.CaptureRectangle(330, 15, 235, 130);
                        found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/trade_window_title.png");
                        if (found_pos.IsVisible)
                        {
                            screen_shot.Dispose();
                            _LoggerService.Log("I am in trade!");
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

                    if (!pos.IsVisible)
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

                        if (CurrentCustomer.Product.Contains(_itemmService.GetNameItem_PoE(ss)))
                        {
                            _LoggerService.Log($"{ss} is found in inventory");

                            Win32.CtrlMouseClick();

                            screen_shot.Dispose();

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

        private bool CheckCurrency()
        {
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
                Win32.ChatCommand($"@{CurrentCustomer.Nickname} exalted orb = {_CurrenciesService.GetCurrencyByName("exalted").ChaosEquivalent}");

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
                    Win32.MoveTo(0, 0);

                    Thread.Sleep(100);

                    screen_shot = ScreenCapture.CaptureRectangle(x_trade, y_trade, 465, 200);

                    found_positions = OpenCV_Service.FindCurrencies(screen_shot, cur.ImageName, 0.6);

                    foreach (Position pos in found_positions)
                    {
                        Win32.MoveTo(x_trade + pos.Left + pos.Width / 2, y_trade + pos.Top + pos.Height / 2);

                        Thread.Sleep(100);

                        string ctrlc = CommandsService.CtrlC_PoE();

                        var curbyname = _CurrenciesService.GetCurrencyByName(_itemmService.GetNameItem_PoE(ctrlc));

                        if (curbyname == null)

                            price += CommandsService.GetSizeInStack(CommandsService.CtrlC_PoE()) * cur.ChaosEquivalent;

                        else

                            price += CommandsService.GetSizeInStack(CommandsService.CtrlC_PoE()) * curbyname.ChaosEquivalent;


                        screen_shot.Dispose();
                    }

                    if (price >= CurrentCustomer.Chaos_Price && price != 0)
                        break;
                }

                _LoggerService.Log("Bid price (in chaos) = " + price + " Necessary (in chaos) = " + CurrentCustomer.Chaos_Price);

                if (price >= CurrentCustomer.Chaos_Price)
                {
                    _LoggerService.Log("I want accept trade");

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

        private void KickFormParty(CustomerInfo customer)
        {
            Win32.ChatCommand("/kick " + customer.Nickname);
        }

        private void RequestTrade()
        {
            TradinngThread = new Thread(() =>
            {
                if (Win32.GetActiveWindowTitle() != "Path of Exile")
                {
                    Win32.PoE_MainWindow();
                }

                // check if im invited
                if (!AcceptTradeRequest())
                {
                    // Send trade request
                    SendTradeRequest();
                }
                // Move items to trade window
                while(!IsTradeStarted())
                {
                    Thread.Sleep(100);
                }

                if(!PutItems())
                {
                    // TODO add actio to end trade if cant put items
                    return;
                }

                // validate until cancel or ok
                bool IsNotPaidYet = true;
                while (IsNotPaidYet)
                {
                    if (CheckCurrency())
                    {
                        IsNotPaidYet = false;
                    }
                }
                // accept trade
                AccepTrade();
            });
            TradinngThread.SetApartmentState(ApartmentState.STA);
            TradinngThread.Start();
        }
        bool SecondAcceptAttempt = false;
        private void AccepTrade()
        {
            _LoggerService.Log("Check trade window");
            Bitmap screen_shot = ScreenCapture.CaptureRectangle(330, 15, 235, 130);
            Position found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/trade_window_title.png");
            if (found_pos.IsVisible)
            {
                screen_shot.Dispose();
                _LoggerService.Log("I am in trade!");
            }
            else
            {
                if (SecondAcceptAttempt)
                {
                    return;
                }
                SecondAcceptAttempt = true;
                CheckCurrency();
                AccepTrade();
            }
        }

        private bool IsTradeStarted()
        {
            int x_trade = 220;
            int y_trade = 140;

            Bitmap screen_shot = ScreenCapture.CaptureRectangle(x_trade, y_trade, 465, 200);

            Position found_positions = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/empty_cel.png", 0.5);

            screen_shot.Dispose();

            return found_positions.IsVisible;
        }

        private void SendTradeRequest()
        {
            _LoggerService.Log($"Send trade request with {CurrentCustomer.Nickname}");
            string trade_command = "/tradewith " + CurrentCustomer.Nickname;
            Win32.ChatCommand(trade_command);
        }

        private bool AcceptTradeRequest()
        {
            Bitmap screen_shot = ScreenCapture.CaptureRectangle(1030, 260, 330, 500);

            Position found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/accespt.png");

            if (found_pos.IsVisible)
            {
                _LoggerService.Log("I will Accept trade request!");

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

        #region Inventory actions
        public bool OpenStash()
        {
            Bitmap screen_shot = null;
            Position found_pos = null;

            if (Win32.GetActiveWindowTitle() != "Path of Exile")
            {
                Win32.PoE_MainWindow();
            }

            //find stash poition

            _LoggerService.Log("Search stash in location...");

            for (int search_pos = 0; search_pos < 20; search_pos++)
            {
                screen_shot = ScreenCapture.CaptureScreen();
                found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/stashtitle.png");

                if (found_pos.IsVisible)
                {
                    Win32.MoveTo(found_pos.Left + found_pos.Width / 2, found_pos.Top + found_pos.Height);

                    Thread.Sleep(100);

                    Win32.DoMouseClick();

                    Thread.Sleep(100);

                    Win32.MoveTo(screen_shot.Width / 2, screen_shot.Height / 2);

                    var timer = DateTime.Now + new TimeSpan(0, 0, 5);

                    while (true)
                    {
                        screen_shot = ScreenCapture.CaptureRectangle(140, 32, 195, 45);

                        var pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/open_stash.png");

                        if (pos.IsVisible)
                        {
                            screen_shot.Dispose();

                            return true;
                        }

                        if (timer < DateTime.Now)
                            break;

                        Thread.Sleep(500);
                    }
                }

                screen_shot.Dispose();

                Thread.Sleep(500);
            }

            _LoggerService.Log("Stash is not found");

            return false;
        }
        public void ScanTab(string name_tab = "trade_tab")
        {
            Position found_pos = null;

            _LoggerService.Log($"Search {name_tab} trade tab...");

            for (int count_try = 0; count_try < 16; count_try++)
            {
                var screen_shot = ScreenCapture.CaptureRectangle(10, 90, 450, 30);

                found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/notactive_" + name_tab + ".jpg");

                if (found_pos.IsVisible)
                {
                    break;
                }
                else
                {
                    found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/active_" + name_tab + ".jpg");
                    if (found_pos.IsVisible)
                    {
                        screen_shot.Dispose();

                        break;
                    }
                }
                screen_shot.Dispose();

                Thread.Sleep(500);
            }

            if (found_pos.IsVisible)
            {
                Win32.MoveTo(10 + found_pos.Left + found_pos.Width / 2, 90 + found_pos.Top + found_pos.Height / 2);

                Thread.Sleep(200);

                Win32.DoMouseClick();

                Thread.Sleep(250);

                List<Cell> skip = new List<Cell>();

                for (int i = 0; i < 12; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        if (skip.Find(cel => cel.Left == i && cel.Top == j) != null)
                        {
                            continue;
                        }

                        Win32.MoveTo(0, 0);

                        Thread.Sleep(100);

                        Win32.MoveTo(Left_Stash64 + 38 * i, Top_Stash64 + 38 * j);

                        #region OpenCv

                        var screen_shot = ScreenCapture.CaptureRectangle(Left_Stash64 - 30 + 38 * i, Top_Stash64 - 30 + 38 * j, 60, 60);

                        Position pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/empty_cel.png", 0.5);

                        if (!pos.IsVisible)
                        {
                            #region Good code

                            string item_info = CommandsService.CtrlC_PoE();

                            if (item_info != "empty_string")
                            {
                                var item = new Item
                                {
                                    Price = _itemmService.GetPrice_PoE(item_info),
                                    Name = CommandsService.GetNameItem_PoE_Pro(item_info),
                                    StackSize = CommandsService.GetStackSize_PoE_Pro(item_info)
                                };

                                item.Places.Add(new Cell(i, j));

                                if (item.StackSize == 1)
                                {
                                    item.SizeInStack = 1;
                                }
                                else
                                {
                                    item.SizeInStack = (int)CommandsService.GetSizeInStack(item_info);
                                }

                                if (item.Name.Contains("Resonator"))
                                {
                                    if (item.Name.Contains("Potent"))
                                    {
                                        item.Places.Add(new Cell(i, j + 1));
                                        skip.Add(new Cell(i, j + 1));

                                    }

                                    if (item.Name.Contains("Prime") || item.Name.Contains("Powerful"))
                                    {
                                        item.Places.Add(new Cell(i, j + 1));
                                        skip.Add(new Cell(i, j + 1));
                                        item.Places.Add(new Cell(i + 1, j + 1));
                                        skip.Add(new Cell(i + 1, j + 1));
                                        item.Places.Add(new Cell(i + 1, j));
                                        skip.Add(new Cell(i + 1, j));
                                    }
                                }

                                _Tabs.AddItem(item);

                                #endregion
                            }

                            screen_shot.Dispose();

                            #endregion
                        }
                    }
                }

                Win32.SendKeyInPoE("{ESC}");

                _LoggerService.Log("Scan is end!");
            }
            else
            {
                throw new Exception($"{name_tab} not found.");
            }
        }
        public void ClearInventory(string recycle_tab = "recycle_tab")
        {
            Position found_pos = null;

            _LoggerService.Log($"Search {recycle_tab}...");

            Thread.Sleep(500);

            for (int count_try = 0; count_try < 16; count_try++)
            {
                var screen_shot = ScreenCapture.CaptureRectangle(10, 90, 450, 30);

                found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/notactive_" + recycle_tab + ".png");

                Thread.Sleep(1000);

                if (found_pos.IsVisible)
                {
                    break;
                }
                else
                {
                    found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/active_" + recycle_tab + ".png");
                    if (found_pos.IsVisible)
                    {
                        screen_shot.Dispose();

                        break;
                    }
                }
                screen_shot.Dispose();

                Thread.Sleep(500);
            }



            if (found_pos.IsVisible)
            {
                Win32.MoveTo(10 + found_pos.Left + found_pos.Width / 2, 90 + found_pos.Top + found_pos.Height / 2);

                Thread.Sleep(200);

                Win32.DoMouseClick();

                Thread.Sleep(250);


                int x_inventory = 925;
                int y_inventory = 440;
                int offset = 37;

                for (int j = 0; j < 12; j++)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Win32.MoveTo(x_inventory + offset * j, y_inventory + 175);

                        Thread.Sleep(100);

                        var screen_shot = ScreenCapture.CaptureRectangle(x_inventory - 30 + offset * j, y_inventory - 30 + offset * i, 60, 60);

                        Position pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/empty_cel.png", 0.5);

                        if (!pos.IsVisible)
                        {
                            Win32.MoveTo(x_inventory + offset * j, y_inventory + offset * i);

                            Thread.Sleep(100);

                            string item_info = CommandsService.CtrlC_PoE();

                            if (item_info != "empty_string")
                            {
                                Win32.CtrlMouseClick();
                            }
                        }
                    }
                }

            }

            else
            {
                throw new Exception($"{recycle_tab} not found!");
            }
        }
        private bool TakeProduct(CustomerInfo customer)
        {
            Bitmap screen_shot = null;
            Position found_pos = null;



            if (Win32.GetActiveWindowTitle() != "Path of Exile")
            {
                Win32.PoE_MainWindow();
            }


            _LoggerService.Log("Search trade tab...");

            for (int count_try = 0; count_try < 16; count_try++)
            {
                screen_shot = ScreenCapture.CaptureRectangle(10, 90, 450, 30);

                found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/notactive_trade_tab.jpg");

                if (found_pos.IsVisible)
                    break;
                else
                {
                    found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/active_trade_tab.jpg");
                    if (found_pos.IsVisible)
                    {
                        break;
                    }
                }

                screen_shot.Dispose();

                Thread.Sleep(500);
            }

            screen_shot.Dispose();

            if (found_pos.IsVisible)
            {
                Win32.MoveTo(10 + found_pos.Left + found_pos.Width / 2, 90 + found_pos.Top + found_pos.Height / 2);

                Thread.Sleep(100);

                Win32.DoMouseClick();

                Thread.Sleep(300);
                Win32.MoveTo(Constants.Left_Stash64 + 38 * (customer.Left - 1), Constants.Top_Stash64 + 38 * (customer.Top - 1));

                Thread.Sleep(1000);

                string ctrlc = CommandsService.CtrlC_PoE();

                string product_clip = _itemmService.GetNameItem_PoE(ctrlc);

                if (product_clip == null || !customer.Product.Contains(product_clip))
                {
                    _LoggerService.Log("not found item");

                    Win32.ChatCommand($"@{customer.Nickname} I sold it, sry");

                    Win32.SendKeyInPoE("{ESC}");

                    return false;
                }

                if (_itemmService.IsValidPrice(ctrlc, customer))
                {
                    _LoggerService.Log("Fake price");

                    Win32.ChatCommand($"@{customer.Nickname} It is not my price!");

                    Win32.SendKeyInPoE("{ESC}");

                    return false;
                }

                Win32.CtrlMouseClick();

                Thread.Sleep(100);

                Win32.MoveTo(750, 350);

                Win32.SendKeyInPoE("{ESC}");

                return true;

            }

            return false;
        }
        private bool TakeItems(string name_tab = "trade_tab")
        {
            Position found_pos = null;

            _LoggerService.Log($"Search {name_tab} trade tab...");

            for (int count_try = 0; count_try < 16; count_try++)
            {
                var screen_shot = ScreenCapture.CaptureRectangle(10, 90, 450, 30);

                found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/notactive_" + name_tab + ".jpg");

                if (found_pos.IsVisible)
                    break;
                else
                {
                    found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/active_" + name_tab + ".jpg");
                    if (found_pos.IsVisible)
                    {
                        screen_shot.Dispose();

                        break;
                    }
                }
                screen_shot.Dispose();

                Thread.Sleep(500);
            }

            if (found_pos.IsVisible)
            {
                Win32.MoveTo(10 + found_pos.Left + found_pos.Width / 2, 90 + found_pos.Top + found_pos.Height / 2);

                Thread.Sleep(200);

                Win32.DoMouseClick();

                Thread.Sleep(250);

                var customer = CurrentCustomer;

                var min_price = new Price
                {
                    Cost = customer.Cost,
                    CurrencyType = customer.Currency,
                    ForNumberItems = customer.NumberProducts
                };

                var items = _Tabs.GetItems(customer.NumberProducts, customer.Product, min_price);

                if (items.Any())
                {
                    int TotalAmount = 0;

                    foreach (Item i in items)
                    {
                        TotalAmount += i.SizeInStack;

                        Win32.MoveTo(Left_Stash64 + 38 * i.Places.First().Left, Top_Stash64 + 38 * i.Places.First().Top);

                        Thread.Sleep(100);

                        string item_info = CommandsService.CtrlC_PoE();

                        if (!item_info.Contains(i.Name))
                        {
                            _LoggerService.Log("Information incorrect.");

                            return false;
                        }

                        if (TotalAmount > customer.NumberProducts)
                        {
                            TotalAmount -= i.SizeInStack;

                            int necessary = customer.NumberProducts - TotalAmount;

                            i.SizeInStack -= necessary;

                            _Tabs.AddItem(i);

                            TotalAmount += necessary;

                            Win32.ShiftClick();

                            Thread.Sleep(100);

                            Win32.SendNumber_PoE(necessary);

                            Win32.SendKeyInPoE("{ENTER}");

                            PutInInventory();

                        }
                        else
                        {
                            Win32.CtrlMouseClick();
                        }


                        if (TotalAmount == customer.NumberProducts)
                        {
                            Win32.SendKeyInPoE("{ESC}");

                            return true;
                        }
                    }
                }
                else
                {
                    _LoggerService.Log("Items not found!");

                    Win32.ChatCommand($"@{customer.Nickname} maybe I sold it");
                }

            }

            _LoggerService.Log("Tab not found");

            return false;
        }
        private void PutInInventory()
        {
            var screen_shot = ScreenCapture.CaptureRectangle(900, 420, 460, 200);

            var empty_poss = OpenCV_Service.FindObjects(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/empty_cel.png", 0.5);

            if (empty_poss.Any())
            {
                foreach (Position pos in empty_poss)
                {
                    Win32.MoveTo(900 + pos.Left, 420 + pos.Top);

                    var info = CommandsService.CtrlC_PoE();

                    Thread.Sleep(100);

                    if (info == "empty_string")
                    {
                        Win32.DoMouseClick();

                        Thread.Sleep(150);

                        screen_shot.Dispose();

                        return;
                    }
                }
            }

            else
                _LoggerService.Log("Inventory is full");
        }
        private bool PutItems()
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

                    Win32.MoveTo(x_inventory + offset * j, y_inventory + 175);

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

                            _LoggerService.Log($"I put {TotalAmount} items in trade window");

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
        #endregion

        private void WithrawItemFromStash(CustomerInfo customer)
        {
            if(tradeAttemts == 5)
            {

            if (!OpenStash())
            {
                Win32.ChatCommand($"@{customer.Nickname} item gone sorry");
                KickFormParty(customer);
                Customers.Remove(customer);

                _LoggerService.Log($"\nTrade end with {customer.Nickname}! stash not found");
                return;
            }

            if (!TakeProduct(customer))
            {
                Win32.ChatCommand($"@{customer.Nickname} item gone sorry");
                KickFormParty(customer);
                Customers.Remove(customer);

                _LoggerService.Log($"\nTrade end with {customer.Nickname}! item not found");
                return;
                }
            }
        }
        private void WithrawTradeItemsFromStash(CustomerInfo customer)
        {
            #region Many items

            //if (Customer.First().OrderType == CustomerInfo.OrderTypes.MANY)
            //{
            //    InviteCustomer();

            //    if (!OpenStash())
            //    {
            //        KickFormParty();
            //        Customer.Remove(Customer.First());

            //        _LoggerService.Log("\nTrade end!");
            //        continue;
            //    }

            //    if (!TakeItems())
            //    {
            //        KickFormParty();
            //        Customer.Remove(Customer.First());

            //        _LoggerService.Log("\nTrade end!");
            //        continue;
            //    }

            //    //check is area contain customer
            //    if (!CheckArea())
            //    {
            //        KickFormParty();

            //        Customer.Remove(Customer.First());

            //        _LoggerService.Log("\nTrade end!");
            //        continue;
            //    }

            //    //start trade
            //    if (!TradeQuery())
            //    {
            //        KickFormParty();

            //        Customer.Remove(Customer.First());

            //        _LoggerService.Log("\nTrade end!");
            //        continue;
            //    }


            //    if (!PutItems())
            //    {
            //        KickFormParty();

            //        Customer.Remove(Customer.First());

            //        _LoggerService.Log("\nTrade end!");
            //        continue;
            //    }

            //    if (!CheckCurrency())
            //    {
            //        KickFormParty();

            //        Customer.Remove(Customer.First());

            //        _LoggerService.Log("\nTrade end!");
            //        continue;
            //    }
            //}

            #endregion
        }
    }
}
