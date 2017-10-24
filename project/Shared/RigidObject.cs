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

        public bool IsColliding(Point centerA, int widthA, int heightA,
                         Point centerB, int widthB, int heightB)
        {
            int xA1 = centerA.X - widthA / 2;
            int xA2 = centerA.X + widthA / 2;

            int xB1 = centerB.X - widthB / 2;
            int xB2 = centerB.X + widthB / 2;

            int yA1 = centerA.Y - heightA / 2;
            int yA2 = centerA.Y + heightA / 2;

            int yB1 = centerB.Y - heightB / 2;
            int yB2 = centerB.Y + heightB / 2;

            //Check if there is a gap between the two AABB's in the X axis
            if (xA2 < xB1 || xB2 < xA1)
            {
                return false;
            }

            if (xA2 < xB1 || xB2 < xA1)
            {
                return false;
            }

            //Check if there is a gap between the two AABB's in the Y axis
            if (yA2 < yB1 || yB2 < yA1)
            {
                return false;
            }

            if (yA2 < yB1 || yB2 < yA1)
            {
                return false;
            }


            // We have an overlap
            return true;
        }
    }
}
