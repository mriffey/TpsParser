﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TpsParser.Binary;
using TpsParser.Tps.Type;

namespace TpsParser.Tests
{
    [TestFixture]
    public partial class TpsParserTest
    {
        [Test]
        public void ShouldBuildTableWithMemos()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var table = parser.BuildTable();
                var rows = table.Rows.OrderBy(r => r.Id).ToList();

                Assert.AreEqual(4, rows.Count());

                // Fixed length strings. Dead area is padded with spaces, except for memos.

                Assert.AreEqual(4, rows[0].Values.Count());
                Assert.AreEqual("Joe Smith".PadRight(64, ' '), rows[0].Values["Name"].Value);
                Assert.AreEqual(new DateTime(2016, 2, 9), rows[0].Values["Date"].Value);
                Assert.AreEqual("Joe is a great guy to work with.", rows[0].Values["Notes"].Value);
                Assert.AreEqual("He also likes sushi.", rows[0].Values["AdditionalNotes"].Value);

                Assert.AreEqual(4, rows[1].Values.Count());
                Assert.AreEqual("Jane Jones".PadRight(64, ' '), rows[1].Values["Name"].Value);
                Assert.AreEqual(new DateTime(2019, 8, 22), rows[1].Values["Date"].Value);
                Assert.AreEqual("Jane knows how to make a great pot of coffee.", rows[1].Values["Notes"].Value);
                Assert.AreEqual("She doesn't like sushi as much as Joe.", rows[1].Values["AdditionalNotes"].Value);

                Assert.AreEqual(2, rows[2].Values.Count());
                Assert.AreEqual("John NoNotes".PadRight(64, ' '), rows[2].Values["Name"].Value);
                Assert.AreEqual(new DateTime(2019, 10, 7), rows[2].Values["Date"].Value);
                Assert.IsFalse(rows[2].Values.TryGetValue("Notes", out var _));
                Assert.IsFalse(rows[2].Values.TryGetValue("AdditionalNotes", out var _));

                Assert.AreEqual(3, rows[3].Values.Count());
                Assert.AreEqual("Jimmy OneNote".PadRight(64, ' '), rows[3].Values["Name"].Value);
                Assert.AreEqual(new DateTime(2013, 3, 14), rows[3].Values["Date"].Value);
                Assert.IsFalse(rows[3].Values.TryGetValue("Notes", out var _));
                Assert.AreEqual("Has a strange last name.", rows[3].Values["AdditionalNotes"].Value);
            }
        }

        private IEnumerable<T> InvokeDeserialize<T>(TpsParser parser, Type targetObjectType, bool ignoreErrors)
        {
            // parser.Deserialize<T>()
            return (IEnumerable<T>)parser.GetType()
                .GetMethod(nameof(TpsParser.Deserialize))
                .MakeGenericMethod(targetObjectType)
                .Invoke(parser, new object[] { ignoreErrors });
        }

        [TestCase(typeof(DeserializeMemosInternalFields))]
        [TestCase(typeof(DeserializeMemosInternalSetters))]
        [TestCase(typeof(DeserializeMemosPrivateFields))]
        [TestCase(typeof(DeserializeMemosPrivateSetters))]
        [TestCase(typeof(DeserializeMemosProtectedFields))]
        [TestCase(typeof(DeserializeMemosProtectedSetters))]
        [TestCase(typeof(DeserializeMemosPublicFields))]
        [TestCase(typeof(DeserializeMemosPublicSetters))]
        public void ShouldDeserializeMemos(Type targetObjectType)
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var rows = InvokeDeserialize<IDeserializeMemos>(parser, targetObjectType, ignoreErrors: false)
                    .ToList();

                Assert.AreEqual(4, rows.Count());

                Assert.AreEqual("Joe Smith".PadRight(64, ' '), rows[0].Name);
                Assert.AreEqual(new DateTime(2016, 2, 9), rows[0].Date);
                Assert.AreEqual("Joe is a great guy to work with.", rows[0].Notes);
                Assert.AreEqual("He also likes sushi.", rows[0].AdditionalNotes);

                Assert.AreEqual("Jane Jones".PadRight(64, ' '), rows[1].Name);
                Assert.AreEqual(new DateTime(2019, 8, 22), rows[1].Date);
                Assert.AreEqual("Jane knows how to make a great pot of coffee.", rows[1].Notes);
                Assert.AreEqual("She doesn't like sushi as much as Joe.", rows[1].AdditionalNotes);
                
                Assert.AreEqual("John NoNotes".PadRight(64, ' '), rows[2].Name);
                Assert.AreEqual(new DateTime(2019, 10, 7), rows[2].Date);
                Assert.IsNull(rows[2].Notes);
                Assert.IsNull(rows[2].AdditionalNotes);

                Assert.AreEqual("Jimmy OneNote".PadRight(64, ' '), rows[3].Name);
                Assert.AreEqual(new DateTime(2013, 3, 14), rows[3].Date);
                Assert.IsNull(rows[3].Notes);
                Assert.AreEqual("Has a strange last name.", rows[3].AdditionalNotes);
            }
        }

        [Test]
        public void ShouldThrowMissingFieldWhenDeserializingMemos()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                Assert.Throws<TpsParserException>(() => parser.Deserialize<DeserializeMemosNotesRequired>().ToList());
            }
        }

        [Test]
        public void ShouldDeserializeRecordNumberField()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var rows = parser.Deserialize<DeserializememosRecordNumberField>().ToList();

                Assert.AreEqual(2, rows[0]._id);
                Assert.AreEqual(3, rows[1]._id);
                Assert.AreEqual(4, rows[2]._id);
                Assert.AreEqual(5, rows[3]._id);
            }
        }

        [Test]
        public void ShouldDeserializeRecordNumberProperty()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var rows = parser.Deserialize<DeserializeMemosRecordNumberProperty>().ToList();

                Assert.AreEqual(2, rows[0].Id);
                Assert.AreEqual(3, rows[1].Id);
                Assert.AreEqual(4, rows[2].Id);
                Assert.AreEqual(5, rows[3].Id);
            }
        }

        [Test]
        public void ShouldThrowRecordNumberAndFieldAttrOnSameMember()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                Assert.Throws<TpsParserException>(() => parser.Deserialize<DeserializeMemosRecordNumberAndFieldAttrOnSameMember>().ToList());
            }
        }

        private Row BuildRow(int rowNumber, params (string columnName, TpsObject value)[] fields) =>
            new Row(rowNumber, new Dictionary<string, TpsObject>(fields.Select(f => new KeyValuePair<string, TpsObject>(f.columnName, f.value))));

        [Test]
        public void ShouldDeserializeDate()
        {
            var date = new DateTime(2019, 7, 17);

            var row = BuildRow(1, ("Date", new TpsDate(date)));

            var deserialized = row.Deserialize<DeserializeDate>();

            Assert.AreEqual(date, deserialized.Date);
        }

        [Test]
        public void ShouldDeserializeDateFromLong()
        {
            int clarionStandardDate = 80085;

            var row = BuildRow(1, ("Date", new TpsLong(clarionStandardDate)));

            var deserialized = row.Deserialize<DeserializeDate>();

            Assert.AreEqual(new DateTime(2020, 4, 3), deserialized.Date);
        }

        [Test]
        public void ShouldDeserializeNullDate()
        {
            var row = BuildRow(1, ("Date", new TpsDate(new RandomAccess( new byte[] { 0, 0, 0, 0 } ))));

            var deserialized = row.Deserialize<DeserializeNullDate>();

            Assert.IsNull(deserialized.Date);
        }

        [Test]
        public void ShouldSetDefaultWhenDeserializingNullDateIntoNonNullableDate()
        {
            var row = BuildRow(1, ("Date", new TpsDate(new RandomAccess(new byte[] { 0, 0, 0, 0 }))));

            var deserialized = row.Deserialize<DeserializeDate>();

            Assert.AreEqual(default(DateTime), deserialized.Date);
        }

        [Test]
        public void ShouldDeserializeDateString()
        {
            var expected = new DateTime(2019, 7, 17);

            var row = BuildRow(1, ("Date", new TpsDate(expected)));

            var deserialized = row.Deserialize<DeserializeDateString>();

            Assert.AreEqual(expected.ToString(), deserialized.Date);
        }

        [Test]
        public void ShouldDeserializeDateStringFormatted()
        {
            var expected = new DateTime(2019, 7, 17);

            var row = BuildRow(1, ("Date", new TpsDate(expected)));

            var deserialized = row.Deserialize<DeserializeDateStringFormatted>();

            Assert.AreEqual(expected.ToString("MM - dd - yyyy"), deserialized.Date);
        }

        [Test]
        public void ShouldThrowDeserializingDateStringToNonStringMember()
        {
            var row = BuildRow(1, ("Date", new TpsDate(new DateTime(2019, 7, 17))));

            Assert.Throws<TpsParserException>(() => row.Deserialize<DeserializeDateStringNonStringMember>());
        }

        [Test]
        public void ShouldUseFallbackDeserializingNullDate()
        {
            var row = BuildRow(1, ("Date", new TpsDate((DateTime?)null)));

            var deserialized = row.Deserialize<DeserializeDateStringFallback>();

            Assert.AreEqual("nothing", deserialized.Date);
        }

        [Test]
        public void ShouldDeserializeTime()
        {
            var time = new TimeSpan(12, 13, 42);

            var row = BuildRow(1, ("Time", new TpsTime(time)));

            var deserialized = row.Deserialize<DeserializeTime>();

            Assert.AreEqual(time, deserialized.Time);
        }

        [Test]
        public void ShouldDeserializeTimeFromLong()
        {
            int centiseconds = 80085;

            var row = BuildRow(1, ("Time", new TpsLong(centiseconds)));

            var deserialized = row.Deserialize<DeserializeTime>();

            Assert.AreEqual(new TimeSpan(0, 0, 13, 20, 850), deserialized.Time);
        }

        [Test]
        public void ShouldDeserializeString()
        {
            string expected = " Hello world!     ";

            var row = BuildRow(1, ("Notes", new TpsString(expected)));

            var deserialized = row.Deserialize<DeserializeString>();

            Assert.AreEqual(expected, deserialized.Notes);
        }

        [Test]
        public void ShouldDeserializeAndTrimString()
        {
            var row = BuildRow(1, ("Notes", new TpsString(" Hello world!     ")));

            var deserialized = row.Deserialize<DeserializeStringTrimmingEnabled>();

            Assert.AreEqual(" Hello world!", deserialized.Notes);
        }

        [Test]
        public void ShouldDeserializeAndNotTrimString()
        {
            string expected = " Hello world!     ";

            var row = BuildRow(1, ("Notes", new TpsString(expected)));

            var deserialized = row.Deserialize<DeserializeStringTrimmingDisabled>();

            Assert.AreEqual(expected, deserialized.Notes);
        }

        [Test]
        public void ShouldDeserializeAndTrimNullString()
        {
            var row = BuildRow(1, ("Notes", new TpsString(null)));

            var deserialized = row.Deserialize<DeserializeStringTrimmingEnabled>();

            Assert.IsNull(deserialized.Notes);
        }
    }
}
