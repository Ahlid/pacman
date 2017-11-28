namespace Shared
{
    public interface IVetorMessage<T>
    {

         T Message { get; set; }
         int Index { get; set; }
         int[] Vector { get; set; }
    }
}