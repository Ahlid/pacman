using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IStage
    {
        void BuildInitStage(int numPlayers);

        List<IMonster> GetMonsters();
        void AddMonster(IMonster monster);

        List<ICoin> GetCoins();
        void AddCoin(ICoin coin);
        void RemoveCoin(ICoin coin);

        List<IPlayer> GetPlayers();
        void AddPlayer(IPlayer player);
        void RemovePlayer(IPlayer player);
    }
}
