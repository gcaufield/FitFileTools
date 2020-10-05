using System.Collections.Generic;

namespace FitFileTools.Tools
{
    public interface IDataStream
    {
        IEnumerable<Record> Records { get; }
    }
}