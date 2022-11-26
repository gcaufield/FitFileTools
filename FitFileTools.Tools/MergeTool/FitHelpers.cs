using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Dynastream.Fit;

namespace FitFileTools.Tools.MergeTool
{
    internal static class FitHelpers
    {
        public static void CopyField(Mesg from, Mesg to, byte fieldNum)
        {
            to.RemoveField(to.GetField(fieldNum));
            if (from.HasField(fieldNum))
            {
                to.SetField(from.GetField(fieldNum));
            }
        }

        public static float FieldDiff(Mesg first, Mesg second, byte fieldNum)
        {
            if (!first.HasField(fieldNum) || !second.HasField(fieldNum))
            {
                return 0;
            }

            return (float)first.GetFieldValue(fieldNum) - (float)second.GetFieldValue(fieldNum);
        }
    }
}
