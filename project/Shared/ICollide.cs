using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface ICollide
    {
        bool IsColliding(Point centerA, int widthA, int heightA,
                         Point centerB, int widthB, int heightB);
    }
}
