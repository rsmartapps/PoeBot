﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoeBot.Core.Models
{
    public class Position
    {
        public int Left { get; set; } = -1;

        public int Top { get; set; } = -1;

        public int Width { get; set; }

        public int Height { get; set; }

        public Point Center
        {
            get
            {
                return new Point() { X=Left+(Width/2),Y=Top+(Height/2) };
            }
        }

        public bool IsVisible
        {
            get
            {
                if (Left >= 0 && Top >= 0)
                    return true;
                else
                    return false;
            }
        }
    }
}
