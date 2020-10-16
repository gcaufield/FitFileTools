using System.Collections.Generic;

namespace FitFileTools.Tools.Converters
{
    public interface IDataStream
    {
        IEnumerable<Record> Records { get; }
    }
}