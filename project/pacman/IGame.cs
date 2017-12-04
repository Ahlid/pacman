using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace pacman
{
    public delegate void OnPlayDelegate();

    public interface IGame
    {
        event OnPlayDelegate OnPlayHandler;

        Play Move { get; set; }

        void Play(int round);
    }
}
