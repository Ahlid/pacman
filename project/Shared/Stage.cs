using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Server
{
    [Serializable]
    public class Stage :  IStage
    {
        public const int WIDTH = 600;
        public const int HEIGHT = 400;
        public const int COINS = 30;

        private List<IMonster> monsters;
        private List<ICoin> coins;
        private List<IPlayer> players;
        private List<IWall> walls;

        /// <summary>
        /// Empty constructor, for remoting
        /// </summary>
        public Stage()
        {
            monsters = new List<IMonster>();
            coins = new List<ICoin>();
            players = new List<IPlayer>();
            walls = new List<IWall>();
        }

        public void BuildInitStage(int numPlayers)
        {
            // build static elements
            buildWalls();
            buildCoins();
            
            // build dynamic elements
            buildInitMonsters();
            buildPlayers(numPlayers);
            
            
        }

        /// <summary>
        /// Set positions of the walls in the inital stage.
        /// </summary>
        private void buildWalls()
        {
            this.walls.Add(new Wall(117, 49));
            this.walls.Add(new Wall(331, 49));
            this.walls.Add(new Wall(171, 295));
            this.walls.Add(new Wall(384, 295));
        }

        // todo: check if overlapping walls
        private void buildCoins()
        {
            int coinsBuilt = 0;
            int initX = 12, initY = 12, space = 35;
            int x = initX, y = initY;
            while(coinsBuilt != COINS)
            {
                ICoin ic = new Coin(x, y);
                coins.Add(ic);
                coinsBuilt++;
                x += space;
                if(x > Stage.HEIGHT)
                {
                    x = initX;
                    y += space;
                }
            }
        }

        /// <summary>
        /// Set positions of the monsters in the initial stage.
        /// </summary>
        private void buildInitMonsters()
        {
            // red monster 
            this.monsters.Add(new MonsterHorizontal(240, 90));
            // yellow monster
            this.monsters.Add(new MonsterHorizontal(295, 336));
            // pink monster
            this.monsters.Add(new MonsterAware(401, 89));
        }

        /// <summary>
        /// Set position of the players in the initial stage.
        /// </summary>
        /// <param name="num">Number of players in the stage.</param>
        private void buildPlayers(int num)
        {
            // not checking if player position is overlapping a wall, coin or monster

            int initX = 8, initY = 8, space = 40;
            int x = initX, y = initY;

            for (int i = 0; i < num; i++)
            {
                IPlayer player = this.players[i];
                player.Alive = true;
                player.Position = new Point(x, y);

                y *= space;
                if(y >= Stage.HEIGHT)
                {
                    x = initX + space;
                    y = initY;
                }
            }
        }


        

        
        public void AddCoin(ICoin coin)
        {
            coins.Add(coin);
        }

        public void AddMonster(IMonster monster)
        {
            monsters.Add(monster);
        }

        public void AddPlayer(IPlayer player)
        {
            players.Add(player);
        }

        public List<ICoin> GetCoins()
        {
            return coins.ToList();
        }

        public List<IMonster> GetMonsters()
        {
            return monsters.ToList();
        }

        public List<IPlayer> GetPlayers()
        {
            return players.ToList();
        }

        public void RemoveCoin(ICoin coin)
        {
            coins.Remove(coin);
        }

        public void RemovePlayer(IPlayer player)
        {
            players.Remove(player);
        }

        public List<IWall> GetWalls()
        {
            return this.walls;
        }

        public void AddWall(IWall wall)
        {
            this.walls.Add(wall);
        }
    }
}
