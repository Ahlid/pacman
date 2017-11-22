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
    public class MonsterHorizontal : RigidObject, IMonster
    {
        public const int WIDTH = 30;
        public const int HEIGHT = 30;
        public const int SPEED = 3;
        public Shared.Action.Direction DirectionAction = Shared.Action.Direction.RIGHT;

        public Point Speed { get; set; }

        public MonsterHorizontal(int x, int y)
        {
            this.Position = new Point(x, y);
        }

        public Shared.Action Step(IStage stage)
        {
            Point displacement = new Point(Position.X, Position.Y);
            Shared.Action.Direction direction = Shared.Action.Direction.RIGHT;

            if (this.DirectionAction == direction)
            {
                Position = new Point(Position.X + SPEED, Position.Y);
            }
            else
            {
                Position = new Point(Position.X - SPEED, Position.Y);
            }

            int x = Position.X - displacement.X;
            int y = Position.Y - displacement.Y;
            displacement = new Point(x, y);

            foreach (Wall wall in stage.GetWalls())
            {
                bool colliding = this.IsColliding(WIDTH, HEIGHT,
                    wall, Wall.WIDTH, Wall.HEIGHT);

                if (colliding)
                {
                    if (this.DirectionAction == direction)
                    {
                        this.DirectionAction = Action.Direction.LEFT;
                    }
                    else
                    {
                        this.DirectionAction = Action.Direction.RIGHT;
                    }
                    break;
                }
            }

            return new Shared.Action
            {
                ID = this.ID,
                action = Shared.Action.ActionTaken.MOVE,
                direction = direction,
                displacement = displacement
            };
        }
    }
}
