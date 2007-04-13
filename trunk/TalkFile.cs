using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JadeDlg
{
    class TalkFile
    {
        private Stream _stream;
        private BinaryReader _reader;
        private int _entriesOffset;

        public TalkFile(string path)
        {
            _stream = new FileStream(path, FileMode.Open);
            _stream.Position = 16;
            _reader = new BinaryReader(_stream, Encoding.Default);
            _entriesOffset = _reader.ReadInt32();            
        }

        public string this[int index]
        {
            get
            {
                _stream.Position = _entriesOffset + index*10 + 4;
                int offset = _reader.ReadInt32();
                int size = _reader.ReadInt16();
                _stream.Position = offset;
                return new string(_reader.ReadChars(size));
            }
        }
    }
}
