using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Server
{
    [Serializable]
    public class Player : IPlayer
    {
        public Point Position { get; set; }
    }
}
