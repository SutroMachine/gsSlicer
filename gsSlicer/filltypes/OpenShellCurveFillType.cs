namespace gs.FillTypes
{
    public class OpenShellCurveFillType : BaseFillType
    {
        public static string Label => "solid layer";

        public override string GetLabel()
        {
            return Label;
        }
    }
}