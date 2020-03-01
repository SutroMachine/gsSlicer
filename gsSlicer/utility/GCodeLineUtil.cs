using Sutro.PathWorks.Plugins.API;

namespace gs.utility
{
    public static class GCodeLineUtil
    {
        public static bool ExtractFillType(GCodeLine line, ref string featureType)
        {
            if (line.comment != null)
            {
                int indexOfFillType = line.comment.IndexOf("feature");
                if (indexOfFillType >= 0)
                {
                    featureType = line.comment.Substring(indexOfFillType + 8).Trim();
                    return true;
                }
            }
            return false;
        }
    }
}