using PoeBot.Core.Models;
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
    public class FollowBot
    {

        CurrenciesService _CurrenciesSerive;
        ReadLogsServce _LogReaderServices;
        LoggerService _loggerService;
        ItemsService _ItemService;
        private GameContext _GameContext;
        public FollowBot(LoggerService logger)
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

            // I'm in part?

            // Wait to be invited

            // Go to his place

            // begin to follow
        }

        public void Stop()
        {
            _LogReaderServices?.Dispose();
            _CurrenciesSerive?.Dispose();
        }
    }
}