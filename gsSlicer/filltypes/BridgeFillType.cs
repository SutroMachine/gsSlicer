namespace gs.FillTypes
{
    public class BridgeFillType : BaseFillType
    {
        private readonly SingleMaterialFFFSettings settings;

        public BridgeFillType(SingleMaterialFFFSettings settings)
        {
            this.settings = settings;
        }

        public static string Label => "bridge";

        public override string GetLabel()
        {
            return Label;
        }

        public override double AdjustVolume(double volume)
        {
            return volume * settings.BridgeVolumeScale;
        }

        public override double ModifySpeed(double speed, SchedulerSpeedHint speedHint = SchedulerSpeedHint.Default)
        {
            return settings.CarefulExtrudeSpeed * settings.BridgeExtrudeSpeedX;
        }
    }
}