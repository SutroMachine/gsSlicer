namespace gs
{
    public interface IFillType
    {
        double ModifySpeed(double speed, SchedulerSpeedHint speedHint);

        double AdjustVolume(double volume);

        static string Label { get; }

        string GetLabel();

        bool IsPart()
        {
            return true;
        }

        static int Flag { get; } = 0;
    }
}