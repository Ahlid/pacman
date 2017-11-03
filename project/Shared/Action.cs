using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [Serializable]
    public class Action
    {
        public enum ActionTaken { MOVE, REMOVE }
        public enum Direction { LEFT, RIGHT, UP, DOWN }

        public int ID { get; set; }
        public ActionTaken action { get; set; }
        public Direction direction { get; set; }
        public Point displacement { get; set; }
    }
}
