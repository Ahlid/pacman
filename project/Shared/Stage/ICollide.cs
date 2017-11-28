using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface ICollide : IStageObject
    {
        bool IsColliding(int widthA, int heightA,
                         IStageObject stageObject, int widthB, int heightB);
    }
}
