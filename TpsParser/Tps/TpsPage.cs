﻿using System;
using System.Collections.Generic;
using TpsParser.Binary;

namespace TpsParser.Tps
{
    public sealed class TpsPage
    {
        public int Address { get; }
        public int PageSize { get; }
        public int PageSizeUncompressed { get; }
        public int PageSizeUncompressedWithoutHeader { get; }
        public int RecordCount { get; }
        public int Flags { get; }

        private RandomAccess CompressedData { get; }
        private List<TpsRecord> Records { get; }

        private RandomAccess _data;

        private bool IsFlushed => _data is null;

        public TpsPage(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Address = rx.LongLE();
            PageSize = rx.ShortLE();

            var header = rx.Read(PageSize - 6);

            PageSizeUncompressed = header.ShortLE();
            PageSizeUncompressedWithoutHeader = header.ShortLE();
            RecordCount = header.ShortLE();

            CompressedData = header.Read(PageSize - 13);
        }

        private void Decompress()
        {
            if ((PageSize != PageSizeUncompressed)
                && (Flags == 0))
            {
                try
                {
                    CompressedData.PushPosition();
                    _data = CompressedData.UnpackRunLengthEncoding();
                }
                catch (Exception ex)
                {
                    throw new RunLengthEncodingException($"Bad RLE data block at index {CompressedData} in {ToString()}", ex);
                }
                finally
                {
                    CompressedData.PopPosition();
                }
            }
            else
            {
                _data = CompressedData;
            }
        }

        private void Flush()
        {
            _data = null;
            Records.Clear();
        }

        public RandomAccess GetUncompressedData()
        {
            if (IsFlushed)
            {
                Decompress();
            }

            return _data;
        }

        public void ParseRecords()
        {
            var rx = GetUncompressedData();

            Records.Clear();

            // Skip pages with non 0x00 flags as they don't seem to contain TpsRecords.
            if (Flags == 0x00)
            {
                rx.PushPosition();

                try
                {
                    TpsRecord previousRecord = null;

                    do
                    {
                        TpsRecord currentRecord = null;

                        if (previousRecord is null)
                        {
                            currentRecord = new TpsRecord(rx);
                        }
                        else
                        {
                            currentRecord = new TpsRecord(previousRecord, rx);
                        }

                        Records.Add(currentRecord);

                        previousRecord = currentRecord;
                    }
                    while (!rx.IsAtEnd && Records.Count < RecordCount);
                }
                finally
                {
                    rx.PopPosition();
                }
            }
        } 

        public IEnumerable<TpsRecord> GetRecords()
        {
            if (IsFlushed)
            {
                ParseRecords();
            }

            return Records;
        }

        public override string ToString() =>
            $"TpsPage({_data.ToHex8(Address)},{_data.ToHex4(PageSize)},{_data.ToHex4(PageSizeUncompressed)},{_data.ToHex4(PageSizeUncompressedWithoutHeader)}," +
                $"{_data.ToHex4(RecordCount)},{_data.ToHex2(Flags)})";
    }
}
