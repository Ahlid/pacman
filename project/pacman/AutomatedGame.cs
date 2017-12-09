using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            if(instructions.Length > nextMove)
            {
                string[] pairInstructions = instructions[nextMove++].Split(','); // [0] -> round, [1] -> move
                if(pairInstructions[0] != "")
                {
                    Play mv = getPlay(pairInstructions[1]);
                    this.Move = mv;
                }
            }else
            {
                this.Move = Shared.Play.NONE;
            }
        }

        private Play getPlay(string move)
        {
            switch (move)
            {
                case "UP":
                    return Shared.Play.UP;
                case "DOWN":
                    return Shared.Play.DOWN;
                case "LEFT":
                    return Shared.Play.LEFT;
                case "RIGHT":
                    return Shared.Play.RIGHT;
                default:
                    return Shared.Play.NONE;
            }
        }

    }
}
