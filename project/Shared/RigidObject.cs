using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [Serializable]
    public class RigidObject : ICollide
    {
        public int ID { get; set; }

        public Point Position { get; set; }
        
        public bool IsColliding(int widthA, int heightA,
                         IStageObject stageObject, int widthB, int heightB)
        {
            int xA1 = Position.X - widthA / 2;
            int xA2 = Position.X + widthA / 2;

            int xB1 = stageObject.Position.X - widthB / 2;
            int xB2 = stageObject.Position.X + widthB / 2;

            int yA1 = Position.Y - heightA / 2;
            int yA2 = Position.Y + heightA / 2;

            int yB1 = stageObject.Position.Y - heightB / 2;
            int yB2 = stageObject.Position.Y + heightB / 2;

            //Check if there is a gap between the two AABB's in the X axis
            if (xB1 > xA2 || xA1 > xB2)
            {
                return false;
            }

            //Check if there is a gap between the two AABB's in the Y axis
            if (yB1 > yA2 || yA1 > yB2)
            {
                return false;
            }


            // We have an overlap
            return true;
        }
    }
}
