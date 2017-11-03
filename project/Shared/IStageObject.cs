using System.Drawing;

namespace Shared
{
    public interface IStageObject
    {
        int ID { get; set; }
        Point Position { get; set; }
    }
}