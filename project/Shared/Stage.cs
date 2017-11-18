using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Shared
{
    [Serializable]
    public class Stage :  IStage
    {
        public const int WIDTH = 600;
        public const int HEIGHT = 400;
        public const int COINS = 30;

        private int lastID = 1;
        private int initX = 8, initY = 8, space = 40;

        private List<IMonster> monsters;
        private List<ICoin> coins;
        private List<IPlayer> players;
        private List<IWall> walls;

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
            //players must be added because we need their addresses
            //buildPlayers(numPlayers);
                        
        }


        private void buildWalls()
        {
            AddWall(new Wall(117, 49));
            AddWall(new Wall(331, 49));
            AddWall(new Wall(171, 295));
            AddWall(new Wall(384, 295));
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
                AddCoin(ic);
                coinsBuilt++;
                x += space;
                if(x > Stage.HEIGHT)
                {
                    x = initX;
                    y += space;
                }
            }
        }

        private void buildInitMonsters()
        {
            // red monster 
            AddMonster(new MonsterHorizontal(240, 90));
            // yellow monster
            AddMonster(new MonsterHorizontal(295, 336));
            // pink monster
            AddMonster(new MonsterAware(401, 89));
        }

        public void AddCoin(ICoin coin)
        {
            coin.ID = lastID++;
            coins.Add(coin);
        }

        public void AddMonster(IMonster monster)
        {
            monster.ID = lastID++;
            monsters.Add(monster);
        }

        public void AddPlayer(IPlayer player)
        {
            player.ID = lastID++;
            int x = initX, y = initY;

            player.Alive = true;
            player.Position = new Point(x, y);

            y *= space;
            if (y >= Stage.HEIGHT)
            {
                x = initX + space;
                y = initY;
            }
      
            players.Add(player);
        }

        public void AddWall(IWall wall)
        {
            wall.ID = lastID++;
            this.walls.Add(wall);
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

    }
}
