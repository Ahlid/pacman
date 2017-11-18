using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Shared
{
    [Serializable]
    public class Player : RigidObject, IPlayer
    {
        public const int WIDTH = 34;
        public const int HEIGHT = 32;
        public const int SPEED = 10;

        public string Username { get; set; }
        public Uri Address { get; set; }
        public int Score { get; set; }
        public bool Alive { get; set; }

        public Player() { }

        public Player(int x, int y)
        {
            this.Position = new Point(x, y);
        }

        public Shared.Action Move(Play play)
        {

            Point displacement = new Point(Position.X, Position.Y);

            if (play == Play.NONE)
                return null;

            Shared.Action.Direction direction = Shared.Action.Direction.UP;

            switch (play)
            {
                case Play.LEFT:
                    direction = Shared.Action.Direction.LEFT;
                    if (this.Position.X - Player.SPEED - Player.WIDTH / 2 < 0)
                    {
                        this.Position = new Point(Player.WIDTH / 2, this.Position.Y);
                    }
                    else
                    {
                        this.Position = new Point(this.Position.X - Player.SPEED, this.Position.Y);
                    }
                    break;
                case Play.RIGHT:
                    direction = Shared.Action.Direction.RIGHT;
                    if (this.Position.X + Player.SPEED + Player.WIDTH / 2 > Stage.WIDTH)
                    {
                        this.Position = new Point(Stage.WIDTH + Player.WIDTH / 2, this.Position.Y);
                    }
                    else
                    {
                        this.Position = new Point(this.Position.X + Player.SPEED, this.Position.Y);
                    }
                    break;
                case Play.UP:
                    direction = Shared.Action.Direction.UP;
                    //The Y axis grows downwards
                    if (this.Position.Y - Player.SPEED - Player.HEIGHT / 2 < 0)
                    {
                        this.Position = new Point(this.Position.X, Player.HEIGHT / 2);
                    }
                    else
                    {
                        this.Position = new Point(this.Position.X, this.Position.Y - Player.SPEED);
                    }
                    break;
                case Play.DOWN:
                    direction = Shared.Action.Direction.DOWN;
                    if (this.Position.Y + Player.SPEED + Player.HEIGHT / 2 > Stage.HEIGHT)
                    {
                        this.Position = new Point(this.Position.X, Stage.HEIGHT- Player.HEIGHT / 2);
                    }
                    else
                    {
                        this.Position = new Point(this.Position.X, this.Position.Y + Player.SPEED);
                    }

                    break;
            }

            int x = Position.X - displacement.X;
            int y = Position.Y - displacement.Y;
            displacement = new Point(x, y);
            
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
