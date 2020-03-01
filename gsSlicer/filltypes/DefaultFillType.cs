namespace gs.FillTypes
{
    public class DefaultFillType : BaseFillType
    {
        public static string Label => "unknown";

        public override string GetLabel()
        {
            return Label;
        }
    }
}