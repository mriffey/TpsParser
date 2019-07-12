﻿using System;

namespace TpsParser.Tests
{
    public partial class TpsParserTest
    {
        public interface IDeserializeMemos
        {
            string Name { get; }
            DateTime Date { get; }
            string Notes { get; }

            string AdditionalNotes { get; }
        }

        [TpsTable]
        public class DeserializeMemosPrivateSetters : IDeserializeMemos
        {
            [TpsField("Name")]
            public string Name { get; private set; }

            [TpsField("Date")]
            public DateTime Date { get; private set; }

            [TpsField("Notes")]
            public string Notes { get; private set; }

            [TpsField("AdditionalNotes")]
            public string AdditionalNotes { get; private set; }
        }

        [TpsTable]
        public class DeserializeMemosInternalSetters : IDeserializeMemos
        {
            [TpsField("Name")]
            public string Name { get; internal set; }

            [TpsField("Date")]
            public DateTime Date { get; internal set; }

            [TpsField("Notes")]
            public string Notes { get; internal set; }

            [TpsField("AdditionalNotes")]
            public string AdditionalNotes { get; internal set; }
        }

        [TpsTable]
        public class DeserializeMemosProtectedSetters : IDeserializeMemos
        {
            [TpsField("Name")]
            public string Name { get; protected set; }

            [TpsField("Date")]
            public DateTime Date { get; protected set; }

            [TpsField("Notes")]
            public string Notes { get; protected set; }

            [TpsField("AdditionalNotes")]
            public string AdditionalNotes { get; protected set; }
        }

        [TpsTable]
        public class DeserializeMemosPublicSetters : IDeserializeMemos
        {
            [TpsField("Name")]
            public string Name { get; set; }

            [TpsField("Date")]
            public DateTime Date { get; set; }

            [TpsField("Notes")]
            public string Notes { get; set; }

            [TpsField("AdditionalNotes")]
            public string AdditionalNotes { get; set; }
        }

        [TpsTable]
        public class DeserializeMemosPrivateFields : IDeserializeMemos
        {
#pragma warning disable IDE0044 // Add readonly modifier
            [TpsField("Name")]
            private string _name;

            [TpsField("Date")]
            private DateTime _date;

            [TpsField("Notes")]
            private string _notes;

            [TpsField("AdditionalNotes")]
            private string _additionalNotes;
#pragma warning restore IDE0044 // Add readonly modifier

            public string Name => _name;
            public DateTime Date => _date;
            public string Notes => _notes;
            public string AdditionalNotes => _additionalNotes;
        }

        [TpsTable]
        public class DeserializeMemosInternalFields : IDeserializeMemos
        {
            [TpsField("Name")]
            internal string _name;

            [TpsField("Date")]
            internal DateTime _date;

            [TpsField("Notes")]
            internal string _notes;

            [TpsField("AdditionalNotes")]
            internal string _additionalNotes;

            public string Name => _name;
            public DateTime Date => _date;
            public string Notes => _notes;
            public string AdditionalNotes => _additionalNotes;
        }

        [TpsTable]
        public class DeserializeMemosProtectedFields : IDeserializeMemos
        {
            [TpsField("Name")]
            protected string _name;

            [TpsField("Date")]
            protected DateTime _date;

            [TpsField("Notes")]
            protected string _notes;

            [TpsField("AdditionalNotes")]
            protected string _additionalNotes;

            public string Name => _name;
            public DateTime Date => _date;
            public string Notes => _notes;
            public string AdditionalNotes => _additionalNotes;
        }

        [TpsTable]
        public class DeserializeMemosPublicFields : IDeserializeMemos
        {
            [TpsField("Name")]
            public string _name;

            [TpsField("Date")]
            public DateTime _date;

            [TpsField("Notes")]
            public string _notes;

            [TpsField("AdditionalNotes")]
            public string _additionalNotes;

            public string Name => _name;
            public DateTime Date => _date;
            public string Notes => _notes;
            public string AdditionalNotes => _additionalNotes;
        }

        [TpsTable]
        public class DeserializeMemosNotesRequired : IDeserializeMemos
        {
            [TpsField("Name")]
            public string Name { get; set; }

            [TpsField("Date")]
            public DateTime Date { get; set; }

            [TpsField("Notes", isRequired: true)]
            public string Notes { get; set; }

            [TpsField("AdditionalNotes")]
            public string AdditionalNotes { get; set; }
        }
    }
}
