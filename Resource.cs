using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JadeDlg
{
    class Resource
    {
        public static readonly int TYPE_DIALOG = 2029;

        private readonly string _name;
        private readonly int _type;
        private readonly string _path;
        private readonly int _offset;
        private int _size;

        public Resource(string name, int type, string path, int offset, int size)
        {
            _name = name;
            _type = type;
            _path = path;
            _offset = offset;
            _size = size;
        }

        public string Name
        {
            get { return _name; }
        }

        public int Type
        {
            get { return _type; }
        }

        public Stream Data
        {
            get
            {
                FileStream stream = new FileStream(_path, FileMode.Open);
                try
                {
                    if (_size < 0)
                        _size = (int) stream.Length;
                    byte[] data = new byte[_size];
                    stream.Position = _offset;
                    stream.Read(data, 0, _size);
                    return new MemoryStream(data);
                }
                finally
                {
                    stream.Close();
                }
            }
        }
    }
}
