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
    public class Player : RigidObject, IPlayer
    {
        public const int WIDTH = 34;
        public const int HEIGHT = 32;
        public const int SPEED = 5;

        public Point Position { get; set; }
        public string Address { get; set; }
        public int Score { get; set; }
        public bool Alive { get; set; }

        public Player() { }

        public Player(int x, int y)
        {
            this.Position = new Point(x, y);
        }

        public void Move(Play play)
        {
            switch (play)
            {
                case Play.LEFT:
                    if (this.Position.X + Player.SPEED + Player.HEIGHT / 2 > 0)
                    {
                        this.Position = new Point(this.Position.X - Player.SPEED - Player.WIDTH / 2, this.Position.Y);
                    }
                    else
                    {
                        this.Position = new Point(this.Position.X - Player.SPEED, this.Position.Y);
                    }
                    break;
                case Play.RIGHT:

                    if (this.Position.X + Player.SPEED + Player.HEIGHT / 2 < Stage.WIDTH)
                    {
                        this.Position = new Point(this.Position.X + Player.SPEED + Player.WIDTH / 2, this.Position.Y);
                    }
                    else
                    {
                        this.Position = new Point(this.Position.X + Player.SPEED, this.Position.Y);
                    }
                    break;
                case Play.UP:
                    //The Y axis grows downwards
                    if (this.Position.Y - Player.SPEED - Player.HEIGHT / 2 > 0)
                    {
                        this.Position = new Point(this.Position.X, this.Position.Y - Player.SPEED - Player.HEIGHT / 2);
                    }
                    else
                    {
                        this.Position = new Point(this.Position.X, this.Position.Y - Player.SPEED);
                    }
                    break;
                case Play.DOWN:
                    if (this.Position.Y + Player.SPEED + Player.HEIGHT / 2 < Stage.HEIGHT)
                    {
                        this.Position = new Point(this.Position.X, this.Position.Y + Player.SPEED + Player.HEIGHT / 2);
                    }
                    else
                    {
                        this.Position = new Point(this.Position.X, this.Position.Y + Player.SPEED);
                    }

                    break;
            }
        }
    }
}
