using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    public class Stage :  IStage
    {
        private List<IMonster> monsters;
        private List<ICoin> coins;
        private List<IPlayer> players;

        /// <summary>
        /// Empty constructor, for remoting
        /// </summary>
        public Stage()
        {
            monsters = new List<IMonster>();
            coins = new List<ICoin>();
            players = new List<IPlayer>();
        }

        public void addCoin(ICoin coin)
        {
            coins.Add(coin);
        }

        public void addMonster(IMonster monster)
        {
            monsters.Add(monster);
        }

        public void addPlayer(IPlayer player)
        {
            players.Add(player);
        }

        public List<ICoin> getCoins()
        {
            return coins.ToList();
        }

        public List<IMonster> getMonsters()
        {
            return monsters.ToList();
        }

        public List<IPlayer> getPlayers()
        {
            return players.ToList();
        }

        public void removeCoin(ICoin coin)
        {
            coins.Remove(coin);
        }

        public void removePlayer(IPlayer player)
        {
            players.Remove(player);
        }
    }
}
