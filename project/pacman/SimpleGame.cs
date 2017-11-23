using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace pacman
{
    public class SimpleGame : IGame
    {
        public event OnPlayDelegate OnPlayHandler;

        private Play move;
        public Play Move { get { return move; } set { this.move = value; if (OnPlayHandler != null) { OnPlayHandler(); } } } // on set activate the event

        public void Play(int round) { }
    }
}
