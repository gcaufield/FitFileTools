using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynastream.Fit;

namespace FitFileTools.Tools.MergeTool
{
    internal class FitIndexer
    {
        private Dictionary<ushort, List<Mesg>> _index = new Dictionary<ushort, List<Mesg>>();

        public FitIndexer(FitReader file, HashSet<ushort> mesgs)
        {
            foreach (var num in mesgs)
            {
                _index[num] = new List<Mesg>();
            }

            foreach(var mesg in file.ReadMesgs(m => mesgs.Contains(m.Num)))
            {
                _index[mesg.Num].Add(mesg);
            }
        }

        public Mesg Get(ushort mesg)
        {
            if (!_index.ContainsKey(mesg)) return null;

            return _index[mesg].Count > 0 ? _index[mesg].First() : null;
        }
    }

    public class FitReader
    {
        private Stream _readStream;
        private Decode _decode;
        private Mesg _lastMesg;
        private bool _isValid;

        public FitReader(Stream readStream)
        {
            _readStream = readStream;
            Reset();
        }

        public void Reset()
        {
            _readStream.Seek( 0, SeekOrigin.Begin);
            InitDecode();
            _isValid = _decode.CheckIntegrity(_readStream);
            if (_isValid)
            {
                Header h = new Header(_readStream);
            }
        }

        public Mesg ReadNextMesg()
        {
            while(_readStream.Position < (_readStream.Length - 2))
            {
                _decode.DecodeNextMessage(_readStream);
                if (_lastMesg != null)
                {
                    var temp = _lastMesg;
                    _lastMesg = null;
                    return temp;
                }
            }

            return null;
        }

        public IEnumerable<Mesg> ReadMesgs(Func<Mesg, bool> matcher)
        {
            var mesg = ReadNextMesg(matcher);
            while (mesg != null)
            {
                yield return mesg;
                mesg = ReadNextMesg(matcher);
            }
        }

        public Mesg ReadNextMesg(Func<Mesg, bool> matcher)
        {
            var temp = ReadNextMesg();
            while (temp != null)
            {
                if (matcher(temp))
                {
                    return temp;
                }

                temp = ReadNextMesg();
            }

            return null;
        }

        private void InitDecode()
        {
            _decode = new Decode();
            _decode.MesgEvent += (sender, args) => { _lastMesg = args.mesg; };
        }

        public bool IsValid()
        {
            return _isValid;
        }
    }
}
