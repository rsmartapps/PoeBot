using PoeBot.Core.Models.Test;
using PoeBot.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoeBot.Core
{
    public class FarmBot
    {

        CurrenciesService _CurrenciesSerive;
        ReadLogsServce _LogReaderServices;
        LoggerService _loggerService;
        TradeService _TradeService;
        ItemsService _ItemService;
        StashHelper _StashHelper;

        public FarmBot(LoggerService logger)
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

            if (!ValidateFarmTabs())
            {
                throw new Exception("Tabs not settup propperly.");
            }

            _CurrenciesSerive = new CurrenciesService(_loggerService);
            _ItemService = new ItemsService(_CurrenciesSerive);
            _TradeService = new TradeService(_ItemService, _CurrenciesSerive, _loggerService, new Tab());

            // Go to hideout

            // Clean inventory

            _TradeService.ClearInventory("recycle_tab");

            _StashHelper.ScanTab();

            // Listen for trades
            _LogReaderServices =new ReadLogsServce(_loggerService, _CurrenciesSerive);
            _LogReaderServices.TradeRequest += _TradeService.TradeRequest;

            // Move for not been afk
            _LogReaderServices.AFK += _TradeService.StopAFK;

            // Trade accepted event
            _LogReaderServices.TradeAccepted += _TradeService.TradeAccepted;
            // Trade canceled event
            _LogReaderServices.TradeCanceled += _TradeService.TradeCanceled;
            //Trade customer left event
            _LogReaderServices.CustomerLeft += _TradeService.CustomerLeft;
            // Trade customer arrived event
            _LogReaderServices.CustomerArrived += _TradeService.BeginTrade;
        }

        private bool ValidateFarmTabs()
        {
            // Detect Stash
            if (!_StashHelper.OpenStash())
            {
                _loggerService.Log($"{MethodBase.GetCurrentMethod().Name} Stash is not found in the area.");
                return false;
            }
            var lTabs = OpenCV_Service.GetText(ScreenCapture.CaptureScreen());

            return true;
        }

        public void Stop()
        {
            _LogReaderServices?.Dispose();
            _CurrenciesSerive?.Dispose();
        }
    }
}