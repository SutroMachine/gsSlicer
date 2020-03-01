namespace gs.FillTypes
{

    public class InteriorShellFillType : BaseFillType
    {
        public static string Label => DefaultFillType.Label;

        public override string GetLabel()
        {
            return Label;
        }
    }
}