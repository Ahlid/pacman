using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IMonster : IStageObject, ICollide
    {
        Point Speed { get; set; }
        void Step(IStage stage);

    }
}
