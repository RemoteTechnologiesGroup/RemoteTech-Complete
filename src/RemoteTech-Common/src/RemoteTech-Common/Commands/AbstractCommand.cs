using System;
using RemoteTech.Common.Interfaces;
using RemoteTech.Common.Utils;

namespace RemoteTech.Common.Commands
{
    public abstract class AbstractCommand : ICommand
    {
        /// <summary>
        /// Indicates whether or not the command is aborted.
        /// </summary>
        protected bool Aborted;

        public virtual int CompareTo(ICommand other)
        {
            // if priorities are equal then the discriminant is the time-stamp, otherwise the priority takes place.
            return Priority == other.Priority ? TimeStamp.CompareTo(other.TimeStamp) : Priority.CompareTo(other.Priority);
        }

        protected AbstractCommand()
        {
            CommandId = new Guid();
        }

        /// <summary>
        ///     The command unique identifier.
        /// </summary>
        public virtual Guid CommandId { get; }

        /// <summary>
        /// Base delay for the command.
        /// </summary>
        public virtual double Delay => Math.Max(TimeStamp - TimeUtils.GameTime, 0);

        /// <summary>
        ///     A complete command description.
        /// </summary>
        public virtual string Description
        {
            get
            {
                if (!(Delay > 0) && !(ExtraDelay > 0))
                    return string.Empty;

                var delayStr = TimeUtils.FormatDuration(Delay);
                if (ExtraDelay > 0)
                    delayStr = $"{delayStr} + {TimeUtils.FormatDuration(ExtraDelay)}";

                return $"Signal delay: {delayStr}";
            }
        }

        /// <summary>
        ///     Extra delay added to the current command <see cref="TimeStamp" />.
        /// </summary>
        public virtual double ExtraDelay { get; set; }

        /// <summary>
        /// The command priority. From 0 (less privileged) to 255 (highest priority).
        /// </summary>
        public virtual byte Priority => 0;

        /// <summary>
        ///     A short command description.
        /// </summary>
        public virtual string ShortDescription { get; }

        /// <summary>
        ///     The time at which the command was enqueued.
        /// </summary>
        public virtual double TimeStamp { get; set; }


        public virtual event EventHandler CommandEnqueued;
        public virtual event EventHandler CommandRemoved;
        public virtual event EventHandler CommandExecuted;
        public virtual event EventHandler CommandAborted;


        /// <summary>
        ///     Abort the command.
        /// </summary>
        public virtual void Abort()
        {
            Aborted = true;
        }

        /// <summary>
        ///     Execute the command.
        /// </summary>
        /// <returns>true if the command was successfully executed, false otherwise.</returns>
        public virtual bool Invoke()
        {
            throw new NotImplementedException();
        }

        public virtual bool Load()
        {
            throw new NotImplementedException();
        }

        public virtual bool Save()
        {
            throw new NotImplementedException();
        }
    }
}
