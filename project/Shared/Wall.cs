using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class Wall : IWall
    {
        public const int WALL_WIDTH = 20;
        public const int WALL_HEIGHT = 120;

        public Point Position { get; set; }
        
    }
}
