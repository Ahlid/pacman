using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pacman.GameLogic
{
    public class Game
    {
        enum GameState { WAITING, PLAYING, GAME_OVER }

        private string username;
        private int round;
        private bool hasGameStarted;

        private ChatRoom chatRoom;
        static volatile Mutex mutex;
        private Dictionary<int, string> roundState;

        private Shared.IStage stage;

        private Player player;

        //PlayerNumber to Player
        private Dictionary<int, Player> OposingPlayers;


        Game(string username)
        {
            this.username = username;
            hasGameStarted = false;
            chatRoom = new ChatRoom(username);
            mutex = new Mutex(false);
            roundState = new Dictionary<int, string>();
            OposingPlayers = new Dictionary<int, Player>();
        }

        public void Stage()
        {
            
        }

        public void MoveLeft()
        {
            
        }

        public void MoveRight()
        {

        }

        public void MoveUp()
        {

        }

        public void MoveDown()
        {

        }






    }
}
