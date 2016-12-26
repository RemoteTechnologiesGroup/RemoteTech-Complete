using RemoteTech.Common.Utils.RemoteTech.Common.Utils;

namespace RemoteTech.Common.Utils
{
    public static class TimeUtils
    {
        /// <summary>
        ///     Format a <see cref="double" /> duration into a string.
        /// </summary>
        /// <param name="duration">
        ///     The time duration as a double.
        /// </param>
        /// <param name="withMicroSecs">
        ///     Whether or not to include microseconds in the output.
        /// </param>
        /// <returns>
        ///     A string corresponding to the <paramref name="duration" /> input parameter.
        /// </returns>
        public static string FormatDuration(double duration, bool withMicroSecs = true)
        {
            TimeStringConverter time;

            if (GameSettings.KERBIN_TIME)
                time = new KerbinTimeStringConverter();
            else
                time = new EarthTimeStringConverter();

            return time.ParseDouble(duration, withMicroSecs);
        }

        /// <summary>
        /// The simulation time, in seconds, since the current save was started.
        /// </summary>
        public static double GameTime => Planetarium.GetUniversalTime();
    }
}