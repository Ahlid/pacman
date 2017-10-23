using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pacman
{
    class ConcreteClient : MarshalByRefObject, IClient
    {
        public static FormStage stageForm;
        public string Address { get; set; }
        private int round;
        private bool gameStart;

        public void sendRoundStage(IStage stage, int round)
        {
            if (!gameStart)
            {
                MessageBox.Show("Something went wrong.");
                return;
            }
            //Construir o form através dos objetos que estão no stage
            //MessageBox.Show(string.Format("Stage number {0} received from the server.", round));
            buildMonsters(stage);
            buildCoins(stage);
            buildPlayers(stage);
            this.round = round;
        }

        public void start(IStage stage)
        {
            if(gameStart)
            {
                MessageBox.Show("Game has already started.");
                return;
            }
            //MessageBox.Show("The game has started(signal received from the server).");
            buildMonsters(stage);
            buildCoins(stage);
            buildPlayers(stage);
            gameStart = true;
            round = 0;
        }

        private void buildPlayers(IStage stage)
        {
            throw new NotImplementedException();
        }

        private void buildCoins(IStage stage)
        {
            throw new NotImplementedException();
        }

        private void buildMonsters(IStage stage)
        {
            throw new NotImplementedException();
        }
    }
}
