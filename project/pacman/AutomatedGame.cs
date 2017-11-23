using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace pacman
{
    public class AutomatedGame : IGame
    {
        public event OnPlayDelegate OnPlayHandler;

        private Play move;
        public Play Move { get { return move; } set { this.move = value; if (OnPlayHandler != null) { OnPlayHandler(); } } } // on set activate the event
        private string[] instructions;
        private int nextMove;

        public AutomatedGame(string instructions)
        {
            this.instructions = instructions.Split('\n');
            this.nextMove = 0;
            this.Move = Shared.Play.NONE;
        }

        public void Play(int round)
        {
            // preciso do numero da ronda 
            if(instructions.Length < nextMove)
            {
                string[] pairInstructions = instructions[nextMove++].Split(','); // [0] -> round, [1] -> move
                this.Move = getPlay(pairInstructions[1]); 
            }
            this.Move = Shared.Play.NONE;
        }

        private Play getPlay(string move)
        {
            switch (move)
            {
                case "Up":
                    return Shared.Play.UP;
                case "Down":
                    return Shared.Play.DOWN;
                case "Left":
                    return Shared.Play.LEFT;
                case "Right":
                    return Shared.Play.RIGHT;
                default:
                    return Shared.Play.NONE;
            }
        }

    }
}
