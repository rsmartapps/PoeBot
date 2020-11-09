using Emgu.CV.Structure;
using PoeBot.Core.Models;
using PoeBot.Core.Models.Test;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private StashHelper _StashHelper;

        private readonly int Top_Stash64 = 135;
        private readonly int Left_Stash64 = 25;

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

                        if (CurrentCustomer.Product.Contains(CommandsService.GetNameItem_PoE(ss)))
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

            Win32.ChatCommand("@" + CurrentCustomer.Nickname + " I sold it, sry");

            return false;
        }
    }
}
