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
    public class MonsterAware : IMonster
    {
        public const int WIDTH = 40;
        public const int HEIGHT = 36;
        public const int SPEED = 3;
        public Point direction;

        public Point Position { get; set; } 
        public Point Speed { get; set; }

        public MonsterAware()
        {
            direction = new Point(1, 1);
        }

        public void step(IStage stage)
        {
            Position = new Point(Position.X + SPEED * direction.X, Position.Y + SPEED * direction.Y);
            //do Collision detection

        }
    }
}
