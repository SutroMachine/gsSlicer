namespace gs.FillTypes
{
    public class SolidFillType : BaseFillType
    {
        public static string Label => "solid layer";

        public override string GetLabel()
        {
            return Label;
        }
    }
}