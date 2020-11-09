using PoeBot.Core.Models;
using PoeBot.Core.Models.Test;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoeBot.Core.Services
{
    public class StashHelper
    {
        LoggerService _LoggerService;
        ItemsService _itemmService;
        Tab _Tabs = new Tab();

        public bool OpenTab(string stash_tab)
        {
            _LoggerService.Log($"Search {stash_tab} trade tab...");
            var screen = ScreenCapture.CaptureScreen();
            var position = OpenCV_Service.ContainsText(screen, stash_tab);
            if (position != null)
            {
                Win32.MoveTo(position.Center.X, position.Center.Y);
                Thread.Sleep(50);
                Win32.DoMouseClick();
            }
            return false;
        }

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

                if (found_pos != null && found_pos.IsVisible)
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

                        if (pos != null && pos.IsVisible)
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

                if (found_pos != null && found_pos.IsVisible)
                {
                    break;
                }
                else
                {
                    found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/active_" + name_tab + ".jpg");
                    if (found_pos != null && found_pos.IsVisible)
                    {
                        screen_shot.Dispose();

                        break;
                    }
                }
                screen_shot.Dispose();

                Thread.Sleep(500);
            }

            if (found_pos != null && found_pos.IsVisible)
            {
                Win32.MoveTo(10 + found_pos.Left + found_pos.Width / 2, 90 + found_pos.Top + found_pos.Height / 2);

                Thread.Sleep(200);

                Win32.DoMouseClick();

                Thread.Sleep(250);

                List<Cell> skip = new List<Cell>();
                Point _StashTabSize = Utils.ZeroStash();
                int tabSize = Utils.WidthHeightTab();
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

                        Win32.MoveTo(_StashTabSize.Y + tabSize * i, _StashTabSize.Y + tabSize * j);

                        #region OpenCv

                        var screen_shot = ScreenCapture.CaptureRectangle(_StashTabSize.Y - 30 + tabSize * i, _StashTabSize.X - 30 + tabSize * j, 60, 60);

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

        #region Inventory actions
        public void ClearInventory(string recycle_tab = "recycle_tab")
        {
            Position found_pos = null;

            _LoggerService.Log($"Clear inventory in stash {recycle_tab}...");

            Thread.Sleep(500);

            for (int count_try = 0; count_try < 16; count_try++)
            {
                var screen_shot = ScreenCapture.CaptureRectangle(10, 90, 450, 30);

                found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/notactive_" + recycle_tab + ".png");

                Thread.Sleep(1000);

                if (found_pos != null && found_pos.IsVisible)
                {
                    break;
                }
                else
                {
                    found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/active_" + recycle_tab + ".png");
                    if (found_pos != null && found_pos.IsVisible)
                    {
                        screen_shot.Dispose();

                        break;
                    }
                }
                screen_shot.Dispose();

                Thread.Sleep(500);
            }



            if (found_pos != null && found_pos.IsVisible)
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

            _LoggerService.Log($"Take products for {customer?.Nickname}");
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

                if (found_pos != null && found_pos.IsVisible)
                    break;
                else
                {
                    found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/active_trade_tab.jpg");
                    if (found_pos != null && found_pos.IsVisible)
                    {
                        break;
                    }
                }

                screen_shot.Dispose();

                Thread.Sleep(500);
            }

            screen_shot.Dispose();

            if (found_pos != null && found_pos.IsVisible)
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
        private bool TakeItems(CustomerInfo CurrentCustomer)
        {
            Position found_pos = null;

            _LoggerService.Log($"Search {CurrentCustomer.Stash_Tab} trade tab...");

            for (int count_try = 0; count_try < 16; count_try++)
            {
                var screen_shot = ScreenCapture.CaptureRectangle(10, 90, 450, 30);

                found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/notactive_" + CurrentCustomer.Stash_Tab + ".jpg");

                if (found_pos != null && found_pos.IsVisible)
                    break;
                else
                {
                    found_pos = OpenCV_Service.FindObject(screen_shot, $"Assets/{Properties.Settings.Default.UI_Fragments}/active_" + CurrentCustomer.Stash_Tab + ".jpg");
                    if (found_pos != null && found_pos.IsVisible)
                    {
                        screen_shot.Dispose();

                        break;
                    }
                }
                screen_shot.Dispose();

                Thread.Sleep(500);
            }

            if (found_pos != null && found_pos.IsVisible)
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
                    var stashZero = Utils.ZeroStash();
                    int Left_Stash64 = stashZero.Y;
                    int Top_Stash64 = stashZero.X;
                    int Size = Utils.WidthHeightTab();
                    foreach (Item i in items)
                    {
                        TotalAmount += i.SizeInStack;

                        Win32.MoveTo(Left_Stash64 + (Size * i.Places.First().Left), Top_Stash64 + (Size * i.Places.First().Top));

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
        public bool PutItems(CustomerInfo CurrentCustomer)
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
        public void PutItemBack(CustomerInfo customer)
        {
            bool storedBack = false;
            _LoggerService.Log($"Put item back because of trade failed with customer {customer.Nickname} trading {customer.Product}");
            if (customer != null)
            {
                if (!OpenStash())
                {
                    throw new Exception("Stash not found");
                }
                if (!OpenTab(customer.Stash_Tab))
                {
                    throw new Exception("Tab not found");
                }
                if (!MoveItemBack(customer.Product))
                {
                    ClearInventory();
                }
                Win32.SendKeyInPoE("ESC");
            }
        }
        public bool MoveItemBack(string product)
        {
            // TODO Find intem in inventory
            Point zeroInventory = Utils.ZeroInventory();
            int y_point;
            int x_point;
            for (int i = 0; i < 12; i++)
            {
                x_point = zeroInventory.X + (Utils.WidthHeightTab() * i);
                for (int j = 0; j < 5; j++)
                {
                    y_point = zeroInventory.Y + (Utils.WidthHeightTab() * j);

                }
            }
            // TODO Calculate height and put it back

            //string posItem = CommandsService.GetItemNamePosition(new Position { Left = Left_Stash64 + customer.Left, Top = Top_Stash64 + customer.Top });
            //if (String.IsNullOrWhiteSpace(posItem))
            //{
            //    Win32.DoMouseClick();
            //    Thread.Sleep(300);
            //    Win32.MoveTo(Left_Stash64 + customer.Left, Top_Stash64 + customer.Top);
            //    Thread.Sleep(300);
            //    Win32.DoMouseClick();
            //    posItem = CommandsService.GetItemNamePosition(new Position { Left = Left_Stash64 + customer.Left, Top = Top_Stash64 + customer.Top });
            //    if (posItem != customer.Product)
            //    {
            //        storedBack = true;
            //    }
            //}
            return false;
        }

        public bool WithrawItemFromStash(CustomerInfo customer)
        {
            _LoggerService.Log($"Run WithrawItemFromStash for {customer?.Nickname}");
            if (!OpenStash())
            {
                Win32.ChatCommand($"@{customer.Nickname} item gone sorry");
                CommandsService.KickFormParty(customer);
                _LoggerService.Log($"\nTrade end with {customer.Nickname}! stash not found");
                return false;
            }

            if (!TakeProduct(customer))
            {
                Win32.ChatCommand($"@{customer.Nickname} item gone sorry");
                CommandsService.KickFormParty(customer);
                _LoggerService.Log($"\nTrade end with {customer.Nickname}! item not found");
                return false;
            }
            return true;
        }
        public bool WithrawTradeItemsFromStash(CustomerInfo customer)
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
            return true;
        }

        #endregion
    }
}
