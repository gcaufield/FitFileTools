using System.IO;
using System.Threading.Tasks;
using Dynastream.Fit;
using DateTime = System.DateTime;
using File = Dynastream.Fit.File;

namespace FitFileTools.Tools
{
    public class FitWriter
    {
        private readonly IDataStream _dataStream;
        private readonly string _outputFile;

        public FitWriter(string outputFile, IDataStream dataStream)
        {
            _outputFile = outputFile;
            _dataStream = dataStream;
        }

        public async Task Run()
        {
            await Task.Run(() =>
            {
                using var outStream = new FileStream(_outputFile, FileMode.Create, FileAccess.Write);
                var encode = new Encode(outStream, ProtocolVersion.V20);

                var fileId = new FileIdMesg();
                fileId.SetType(File.Activity);
                fileId.SetTimeCreated(new Dynastream.Fit.DateTime(DateTime.Now));
                encode.Write(fileId);
                foreach (var record in _dataStream.Records)
                {
                    var rcrd = new RecordMesg();

                    rcrd.SetTimestamp(new Dynastream.Fit.DateTime(record.Timestamp));
                    rcrd.SetAltitude((float?) record.Elevation);
                    if (record.Latitude != null) rcrd.SetPositionLat(record.Latitude / 180.0);
                }
            });
        }
    }
}