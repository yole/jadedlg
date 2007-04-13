using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JadeDlg
{
    class GffReader
    {
        private static readonly int GFF_DWORD64 = 6;
        private static readonly int GFF_INT64 = 7;
        private static readonly int GFF_DOUBLE = 9;
        private static readonly int GFF_EXOSTRING = 10;
        private static readonly int GFF_RESREF = 11;
        private static readonly int GFF_EXOLOCSTRING = 12;
        private static readonly int GFF_VOID = 13;
        private static readonly int GFF_STRUCT = 14;
        private static readonly int GFF_LIST = 15;
        private static readonly int GFF_TALKREF = 18;
        
        private GffStruct _root;
        private int _fieldOffset;
        private string[] _labels;
        private int _structOffset;
        private int _fieldDataOffset;
        private int _fieldIndicesOffset;
        private int _listIndicesOffset;

        private GffReader(Stream data)
        {
            BinaryReader reader = new BinaryReader(data, Encoding.Default);
            data.Position = 8;
            _structOffset = reader.ReadInt32();
            reader.ReadInt32();     // skip struct count
            _fieldOffset = ReadOffsetSkipCount(reader);
            int labelOffset = reader.ReadInt32();
            int labelCount = reader.ReadInt32();
            _fieldDataOffset = ReadOffsetSkipCount(reader);
            _fieldIndicesOffset = ReadOffsetSkipCount(reader);
            _listIndicesOffset = ReadOffsetSkipCount(reader);

            LoadLabels(reader, labelOffset, labelCount);

            _root = ReadStruct(reader, 0);
        }

        private static int ReadOffsetSkipCount(BinaryReader reader)
        {
            int result = reader.ReadInt32();
            reader.ReadInt32();     // skip count
            return result;
        }

        private void LoadLabels(BinaryReader reader, int offset, int count)
        {
            reader.BaseStream.Position = offset;
            _labels = new string[count];
            for(int i=0; i<count; i++)
                _labels [i] = new string(reader.ReadChars(16)).TrimEnd('\0');
        }

        private GffStruct ReadStruct(BinaryReader reader, int index)
        {
            reader.BaseStream.Position = _structOffset + index*12;
            reader.ReadInt32();  // skip type - we don't care
            int dataOrDataOffset = reader.ReadInt32();
            int fieldCount = reader.ReadInt32();

            GffStruct result = new GffStruct();
            if (fieldCount == 1)
                ReadField(result, reader, dataOrDataOffset);
            else
            {
                reader.BaseStream.Position = _fieldIndicesOffset + dataOrDataOffset;
                int[] indices = new int[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                    indices[i] = reader.ReadInt32();
                foreach (int fieldIndex in indices)
                    ReadField(result, reader, fieldIndex);
            }
            return result;
        }

        private void ReadField(GffStruct gffStruct, BinaryReader reader, int index)
        {
            reader.BaseStream.Position = _fieldOffset + 12*index;
            int type = reader.ReadInt32();
            int labelIndex = reader.ReadInt32();
            int dataOrDataOffset = reader.ReadInt32();
            object value = ReadValue(reader, type, dataOrDataOffset);
            gffStruct.SetValue(_labels [labelIndex], value);
        }

        private object ReadValue(BinaryReader reader, int type, int dataOrDataOffset)
        {
            if (type == GFF_EXOSTRING || type == GFF_RESREF)
                return ReadString(reader, dataOrDataOffset, type);
            if (type == GFF_LIST)
                return ReadList(reader, dataOrDataOffset);
            if (type == GFF_TALKREF)
                return ReadTalkRef(reader, dataOrDataOffset);
            if (type == GFF_DWORD64 || type == GFF_INT64 || type == GFF_DOUBLE || type == GFF_EXOLOCSTRING ||
                type == GFF_VOID || type == GFF_STRUCT)
            {
                throw new Exception("Complex type " + type + " not supported yet");
            }
            return dataOrDataOffset;
        }

        private string ReadString(BinaryReader reader, int offset, int type)
        {
            reader.BaseStream.Position = _fieldDataOffset + offset;
            int length = (type == GFF_RESREF) ? reader.ReadByte() : reader.ReadInt32();
            return new string(reader.ReadChars(length));
        }

        private TalkRef ReadTalkRef(BinaryReader reader, int offset)
        {
            reader.BaseStream.Position = _fieldDataOffset + offset;
            int unknown = reader.ReadInt32();
            if (unknown != 4)
                throw new Exception("Unexpected value for unknown field: " + unknown);
            int talkIndex = reader.ReadInt32();
            return new TalkRef(unknown, talkIndex);
        }

        private List<GffStruct> ReadList(BinaryReader reader, int offset)
        {
            reader.BaseStream.Position = _listIndicesOffset + offset;
            int count = reader.ReadInt32();
            int[] indices = new int[count];
            for (int i = 0; i < count; i++)
                indices[i] = reader.ReadInt32();
            List<GffStruct> result = new List<GffStruct>();
            foreach(int index in indices)
                result.Add(ReadStruct(reader, index));
            return result;
        }

        public static GffStruct Read(Stream data)
        {
            GffReader reader = new GffReader(data);
            return reader._root;
        }
    }
}
