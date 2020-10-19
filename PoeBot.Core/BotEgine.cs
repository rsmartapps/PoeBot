using PoeBot.Core.Models.Test;
using PoeBot.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoeBot.Core
{
    public class BotEgine
    {

        CurrenciesService _CurrenciesSerive;
        ReadLogsServce _LogReaderServices;
        LoggerService _loggerService;
        TradeService _TradeService;
        ItemsService _ItemService;
        public BotEgine(LoggerService logger)
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

            _CurrenciesSerive = new CurrenciesService(_loggerService);
            _ItemService = new ItemsService(_CurrenciesSerive);
            _TradeService = new TradeService(_ItemService, _CurrenciesSerive, _loggerService, new Tab());

            // Go to hideout
            Win32.ChatCommand("/hideout");
            Thread.Sleep(700);

            // Detect Stash
            if (!_TradeService.OpenStash())
            {
                _loggerService.Log("Stash is not found in the area.");
                throw new Exception("Stash is not found in the area.");

            }

            // Clean inventory

            _TradeService.ClearInventory("recycle_tab");

            _TradeService.ScanTab();

            // Listen for trades
            _LogReaderServices =new ReadLogsServce(_loggerService, _CurrenciesSerive);
            _LogReaderServices.TradeIn += _TradeService.TradeIn;

            // Move for not been afk
            _LogReaderServices.AFK += _TradeService.StopAFK;

            // Trade accepted event
            _LogReaderServices.TradeAccepted += _TradeService.TradeAccepted;
            // Trade canceled event
            _LogReaderServices.TradeCanceled += _TradeService.TradeCanceled;
            //Trade customer left event
            _LogReaderServices.CustomerLeft += _TradeService.CustomerLeft;
            // Trade customer arrived event
            _LogReaderServices.CustomerArrived += _TradeService.CustomerArrived;
        }

        public void Stop()
        {
            _LogReaderServices?.Dispose();
            _CurrenciesSerive?.Dispose();
        }
    }
}