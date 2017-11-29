using System;
using Shared;

namespace pacman
{
    [Serializable]
    public class VectorMessage<T> : IVetorMessage<T>
    {

        public T Message { get; set; }
        public int Index { get; set; }
        public int[] Vector { get; set; }

    }
}