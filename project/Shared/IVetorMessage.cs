namespace Shared
{
    /// <summary>
    /// A vector mensage
    /// </summary>
    /// <typeparam name="T">The ATD of the mensage</typeparam>
    public interface IVetorMessage<T>
    {
        /// <summary>
        /// The mensage
        /// </summary>
        T Message { get; set; }

        /// <summary>
        /// THe index of the message
        /// </summary>
        /// 
        int Index { get; set; }
        /// <summary>
        /// The current vetor when this message was added
        /// </summary>
        int[] Vector { get; set; }
    }
}