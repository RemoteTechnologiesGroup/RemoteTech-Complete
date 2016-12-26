using System;

namespace RemoteTech.Common.Interfaces
{
    /// <summary>
    ///     Command interface.
    /// </summary>
    public interface ICommand : IComparable<ICommand>
    {
        /*
         * Command fields 
         */

        /// <summary>
        ///     The command unique identifier.
        /// </summary>
        Guid CommandId { get; }

        /// <summary>
        /// Base delay for the command.
        /// </summary>
        double Delay { get; }

        /// <summary>
        ///     A complete command description.
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Extra delay added to the current command <see cref="TimeStamp" />.
        /// </summary>
        double ExtraDelay { get; set; }

        /// <summary>
        /// The command priority. From 0 (less privileged) to 255 (highest priority).
        /// </summary>
        byte Priority { get; }

        /// <summary>
        ///     A short command description.
        /// </summary>
        string ShortDescription { get; }

        /// <summary>
        ///     The time at which the command was enqueued.
        /// </summary>
        double TimeStamp { get; set; }

        /*
         * Command Events
         */

        event EventHandler CommandEnqueued;
        event EventHandler CommandRemoved;
        event EventHandler CommandExecuted;
        event EventHandler CommandAborted;

        /*
         * Command methods
         */

        /// <summary>
        ///     Abort the command.
        /// </summary>
        void Abort();

        /// <summary>
        ///     Execute the command.
        /// </summary>
        /// <returns>true if the command was successfully executed, false otherwise.</returns>
        bool Invoke();

        /// <summary>
        ///     Load the command.
        /// </summary>
        /// <returns>true if the command was successfully loaded, false otherwise.</returns>
        bool Load();

        /// <summary>
        ///     Save the command.
        /// </summary>
        /// <returns>true if the command was successfully saved, false otherwise.</returns>
        bool Save();
    }
}