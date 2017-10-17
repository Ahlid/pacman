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

        public void sendRoundStage(IStage stage, int round)
        {
            //Construir o form através dos objetos que estão no stage
            //MessageBox.Show(string.Format("Stage number {0} received from the server.", round));
        }

        public void start(IStage stage)
        {
            //MessageBox.Show("The game has started(signal received from the server).");
        }
    }
}
