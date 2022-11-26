using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Dynastream.Fit;
using FitFileTools.Tools.MergeTool;
using File = System.IO.File;

namespace FitFileTools.Tools.FileFilter
{
    public class ActivityTypeFilter : IFilter
    {
        private enum FilterState
        {
            Unknown,
            Match,
            NoMatch
        }

        private readonly Dictionary<ushort, Action<Mesg>> _mesgNums;
        private FilterState _state;
        private HashSet<Sport> _sports;

        public ActivityTypeFilter(IEnumerable<Sport> sports)
        {
            _state = FilterState.Unknown;
            _sports = new HashSet<Sport>(sports);
            _mesgNums = new Dictionary<ushort, Action<Mesg>>
                {{MesgNum.FileId, ProcessFileId}, {MesgNum.Session, ProcessSession}};
        }

        public bool FileMatchesFilter(FitReader reader)
        {
            _state = FilterState.Unknown;
            foreach (var message in reader.ReadMesgs(mesg => _mesgNums.ContainsKey(mesg.Num)))
            {
                _mesgNums[message.Num](message);
                if (_state != FilterState.Unknown)
                {
                    return _state == FilterState.Match;
                }
            }

            return false;
        }

        private void ProcessSession(Mesg obj)
        {
            if (obj.HasField(SessionMesg.FieldDefNum.Sport))
            {
                var sport = (Sport)obj.GetFieldValue(SessionMesg.FieldDefNum.Sport);
                _state = _sports.Contains(sport) ? FilterState.Match : FilterState.NoMatch;
            }
        }

        private void ProcessFileId(Mesg obj)
        {
            if (obj.HasField(FileIdMesg.FieldDefNum.Type))
            {
                var file = (Dynastream.Fit.File)obj.GetFieldValue(FileIdMesg.FieldDefNum.Type);
                if (file != Dynastream.Fit.File.Activity)
                {
                    _state = FilterState.NoMatch;
                }
            }
        }

        private void ProcessMessage(Mesg message)
        {
            switch (message.Num)
            {
                case MesgNum.FileId:
                    break;
            }
        }
    }
}
