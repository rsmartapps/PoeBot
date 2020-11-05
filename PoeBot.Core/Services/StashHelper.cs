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
    }
}
