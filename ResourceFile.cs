using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JadeDlg
{
    class ResourceFile
    {
        private Game _game;
        private string _path;
        private List<Resource> _resources;

        public ResourceFile(Game game, string path)
        {
            _game = game;
            _path = path;
        }

        public string Path
        {
            get { return _path; }
        }

        public List<Resource> Resources
        {
            get
            {
                if (_resources == null)
                    LoadResources();
                return _resources;
            }
        }

        private void LoadResources()
        {
            _resources = new List<Resource>();
            FileStream stream = new FileStream(_path, FileMode.Open);
            try
            {
                BinaryReader reader = new BinaryReader(stream, Encoding.ASCII);
                stream.Position = 12;
                int resourceCount = reader.ReadInt32();
                int resourceOffset = reader.ReadInt32();
                stream.Position = resourceOffset;
                for(int i=0; i<resourceCount; i++)
                {
                    _resources.Add(LoadResource(reader));
                }
            }
            finally
            {
                stream.Close();
            }
        }

        private Resource LoadResource(BinaryReader reader)
        {
            string name = new string(reader.ReadChars(16)).TrimEnd('\0');
            int resType = reader.ReadInt32();
            reader.ReadInt32();  // skip ResID
            int offset = reader.ReadInt32();
            int size = reader.ReadInt32();

            string overridePath = _game.GetOverridePath(name, resType);
            if (overridePath != null)
                return new Resource(name, resType, overridePath, 0, -1);

            return new Resource(name, resType, _path, offset, size);
        }
    }
}
