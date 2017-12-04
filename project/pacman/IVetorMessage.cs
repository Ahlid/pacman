namespace pacman
{
    public interface IVectorMessage<T>
    {
         T Message { get; set; }
         int Index { get; set; }
         int[] Vector { get; set; }
    }
}