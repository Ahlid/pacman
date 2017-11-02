using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IPlayer : IStageObject, ICollide
    {
        string Username { get; set; }

        int Score { get; set; }

        bool Alive { get; set; }

        // necessário aqui?
        string Address { get; set; }

        void Move(Play play);
    }
}
