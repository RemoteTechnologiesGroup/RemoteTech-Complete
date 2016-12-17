
namespace RemoteTech.Common.Interfaces
{
    /// <summary>
    /// Interface for signal processor.
    /// </summary>
    public interface ISignalProcessorUnit
    {
        /// <summary>
        /// Name of the Signal Processor.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The vessel using this Signal Processor Unit.
        /// </summary>
        Vessel Vessel { get; }

    }
}
