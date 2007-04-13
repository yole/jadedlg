using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JadeDlg
{
    class Game
    {
        private string _path;
        private List<ResourceFile> _resourceFiles;
        private TalkFile _talkFile;

        public Game(string path)
        {
            _path = path;
        }

        public string DataPath
        {
            get { return Path.Combine(_path, "data"); }
        }

        public List<ResourceFile> ResourceFiles
        {
            get
            {
                if (_resourceFiles == null)
                    LocateResourceFiles();
                return _resourceFiles;
            }
        }

        public TalkFile TalkFile
        {
            get
            {
                if (_talkFile == null)
                    _talkFile = new TalkFile(Path.Combine(_path, "dialog.tlk"));
                return _talkFile;
            }
        }

        private void LocateResourceFiles()
        {
            _resourceFiles = new List<ResourceFile>();
            FillResourceFiles(DataPath);
            foreach(string dir in Directory.GetDirectories(DataPath))
                FillResourceFiles(Path.Combine(DataPath, dir));
        }

        private void FillResourceFiles(string path)
        {
            foreach(string file in Directory.GetFiles(path, "*.rim"))
                _resourceFiles.Add(new ResourceFile(this, Path.Combine(path, file)));
        }

        public string GetOverridePath(string name, int type)
        {
            if (type == Resource.TYPE_DIALOG)
            {
                string expectName = name + ".dlg";
                string overridePath = Path.Combine(Path.Combine(_path, "override"), expectName);
                if (File.Exists(overridePath))
                    return overridePath;
            }
            return null;  // we don't know full mapping between file types and extensions
        }
    }
}
