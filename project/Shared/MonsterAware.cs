using Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = Shared.Action;

namespace Shared
{
    [Serializable]
    public class MonsterAware : RigidObject, IMonster
    {
        public const int WIDTH = 40;
        public const int HEIGHT = 36;
        public const int SPEED = 3;
        public Point direction { get; set; }
        public Point Speed { get; set; }

        public MonsterAware(int x, int y)
        {
            // diagonal movement
            direction = new Point(1, 1);
            this.Position = new Point(x, y);
        }

        public Action Step(IStage stage)
        {
            Position = new Point(Position.X + SPEED * direction.X, Position.Y + SPEED * direction.Y);
            //do Collision detection
            return null;
            /*
             * é necessário fazer collision detection aqui? se o player já faz não será necess+ario fazer aqui.
             */
        }
    }
}
