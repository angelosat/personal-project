namespace Start_a_Town_
{
    static class Ticks
    {
        public const int IngameMillisecondsPerTick = 1440; // one tick is 1.44 ingame seconds
        public const int TicksPerSecond = 60;
        public const int TicksPerGameMinute = (int)(TicksPerSecond * 1.44f);
        public const int TicksPerGameHour = 60 * TicksPerGameMinute;
        public const int TicksPerGameDay = 24 * TicksPerGameHour;

        public static int FromMinutes(int minutes)
        {
            return TicksPerGameMinute * minutes;
        }
        public static int FromHours(int hours)
        {
            return TicksPerGameHour * hours;
        }
        public static int FromDays(int days)
        {
            return TicksPerGameDay * days;
        }
        public static int From( int days, int hours, int minutes)
        {
            return FromDays(days) * FromHours(hours) * FromMinutes(minutes);
        }
    }
}
