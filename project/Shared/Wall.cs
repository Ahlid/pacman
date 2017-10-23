using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    /// <summary>
    /// Description of a Wall.
    /// </summary>
    [Serializable]
    public class Wall : RigidObject, IWall
    {
        /// <summary>
        /// Width of the wall.
        /// </summary>
        public const int WIDTH = 20;
        /// <summary>
        /// Height of the wall.
        /// </summary>
        public const int HEIGHT = 120;
        /// <summary>
        /// 2D Position of the wall.
        /// </summary>
        public Point Position { get; set; }
       

        /// <summary>
        /// Wall constructor that receives its 2D position.
        /// </summary>
        /// <param name="x">Horizontal position.</param>
        /// <param name="y">Vertical position.</param>
        public Wall(int x, int y)
        {
            this.Position = new Point(x, y);
        }

    }
}
