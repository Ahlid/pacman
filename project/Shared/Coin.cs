using Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    public class Coin :  ICoin
    {
        public const int WIDTH = 20;
        public const int HEIGHT = 18;

        public Point Position { get; set; }
    }
}
