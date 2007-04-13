using System;
using System.Collections.Generic;
using System.Text;

namespace JadeDlg
{
    class GffStruct
    {
        private Dictionary<string, object> _fields = new Dictionary<string, object>();

        public void SetValue(string name, object value)
        {
            _fields[name] = value;
        }

        public IEnumerable<string> FieldNames
        {
            get { return _fields.Keys;  }
        }

        public object this[string name]
        {
            get { return _fields[name]; }
        }

        public bool HasField(string name)
        {
            return _fields.ContainsKey(name);
        }
    }
}
