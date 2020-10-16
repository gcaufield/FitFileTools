using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using FitFileTools.Tools.Converters;

namespace FitFileTools.Tools
{
    public class TcxDataStream : IDataStream
    {
        private readonly FileInfo _inputFile;

        public TcxDataStream(FileInfo inputFile)
        {
            _inputFile = inputFile;
        }

        public IEnumerable<Record> Records
        {
            get
            {
                using var inputStream = new FileStream(_inputFile.FullName, FileMode.Open, FileAccess.Read);
                using var xmlReader = XmlReader.Create(inputStream);

                if (xmlReader.ReadToDescendant("Track"))
                {
                    using var track = xmlReader.ReadSubtree();

                    if (!track.ReadToDescendant("Trackpoint")) yield break;

                    do
                    {
                        using var trackPoint = xmlReader.ReadSubtree();
                        var record = ParseTrackPoint(trackPoint);

                        if (record.IsValid) yield return record;
                    } while (track.ReadToFollowing("Trackpoint"));
                }
            }
        }

        private Record ParseTrackPoint(XmlReader trackPoint)
        {
            var record = new Record();
            while (!trackPoint.EOF)
            {
                if (!trackPoint.IsStartElement())
                {
                    trackPoint.Read();
                    continue;
                }

                switch (trackPoint.Name)
                {
                    case "Time":
                        record.Timestamp = DateTime.Parse(trackPoint.ReadElementContentAsString(),
                            CultureInfo.InvariantCulture);
                        break;

                    case "AltitudeMeters":
                        record.Elevation = trackPoint.ReadElementContentAsDouble();
                        break;

                    case "DistanceMeters":
                        record.TotalDistance = trackPoint.ReadElementContentAsDouble();
                        break;

                    case "Position":
                        using (var positionNode = trackPoint.ReadSubtree())
                        {
                            ParsePosition(record, positionNode);
                        }

                        break;

                    default:
                        // Uninteresting Node, continue reading
                        trackPoint.Read();
                        break;
                }
            }

            return record;
        }

        private void ParsePosition(Record record, XmlReader positionNode)
        {
            while (!positionNode.EOF)
            {
                if (!positionNode.IsStartElement())
                {
                    positionNode.Read();
                    continue;
                }

                switch (positionNode.Name)
                {
                    case "LatitudeDegrees":
                        record.Latitude = positionNode.ReadElementContentAsDouble();
                        break;

                    case "LongitudeDegrees":
                        record.Longitude = positionNode.ReadElementContentAsDouble();
                        break;

                    default:
                        positionNode.Read();
                        break;
                }
            }
        }
    }
}