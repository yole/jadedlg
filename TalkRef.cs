using System;
using System.Collections.Generic;
using System.Text;

namespace JadeDlg
{
    class TalkRef
    {
        private int _unknown;
        private int _index;

        public TalkRef(int unknown, int index)
        {
            _unknown = unknown;
            _index = index;
        }

        public int Index
        {
            get { return _index; }
        }
    }
}
