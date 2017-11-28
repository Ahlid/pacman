using Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [Serializable]
    public class Coin : ICoin
    {
        public const int WIDTH = 15;
        public const int HEIGHT = 15;

        public int ID { get; set; }
        public Point Position { get; set; }

        public Coin(int x, int y)
        {
            this.Position = new Point(x, y);
        }
    }
}
