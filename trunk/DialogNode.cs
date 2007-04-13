using System;
using System.Collections.Generic;
using System.Text;

namespace JadeDlg
{
    class DialogNode
    {
        private const string SPEAKER_INDEX = "SpeakerIndex";
        private const string LISTENER_INDEX = "ListenerIndex";
        private const string TEXT = "Text";
        private const string REPLIES_LIST = "RepliesList";
        private const string ENTRIES_LIST = "EntriesList";
        private const string INDEX = "Index";
        private const string DESIGNER_NUMBER = "DesignerNumber";

        private readonly Dialog _dialog;
        private readonly GffStruct _syncStruct;
        private readonly GffStruct _mainStruct;
        private bool _isReply;

        public DialogNode(Dialog dialog, GffStruct gffStruct, bool isReply)
        {
            _dialog = dialog;
            _syncStruct = gffStruct;
            _isReply = isReply;
            if (gffStruct.HasField(INDEX))
            {
                if (_isReply)
                    _mainStruct = _dialog.GetReplyNode((int)gffStruct[INDEX]);
                else
                    _mainStruct = _dialog.GetEntryNode((int)gffStruct[INDEX]);
            }
        }

        public string DisplayText
        {
            get
            {
                string speaker = "";
                if (_isReply)
                    speaker = "PC: ";
                else if (_mainStruct.HasField(SPEAKER_INDEX))
                {
                    speaker = _dialog.GetSpeaker((int) _mainStruct[SPEAKER_INDEX]);
                    if (_mainStruct.HasField(LISTENER_INDEX))
                        speaker += " to " + _dialog.GetSpeaker((int)_mainStruct[LISTENER_INDEX]);
                    speaker += ": ";
                }

                TalkRef talkRef = GetTalkRef();
                return speaker + (talkRef != null ? _dialog.GetTalkText(GetTalkRef()) : "<no text>");
            }
        }

        public string Text
        {
            get
            {
                TalkRef talkRef = GetTalkRef();
                return talkRef != null ? _dialog.GetTalkText(talkRef) : "";
            }
        }

        private TalkRef GetTalkRef()
        {
            TalkRef talkRef = null;
            if (_syncStruct.HasField(TEXT))
                talkRef = (TalkRef) _syncStruct [TEXT];
            else if (_mainStruct.HasField(TEXT))
                talkRef = (TalkRef) _mainStruct [TEXT];
            return talkRef;
        }

        public GffStruct Details
        {
            get
            {
                GffStruct result = new GffStruct();
                foreach (string name in _syncStruct.FieldNames)
                {
                    if (IsDetail(name))
                        result.SetValue(name, _syncStruct[name]);
                }
                if (_mainStruct != null)
                {
                    foreach (string name in _mainStruct.FieldNames)
                    {
                        if (IsDetail(name))
                            result.SetValue(name, _mainStruct[name]);
                    }
                }
                return result;
            }
        }

        public List<DialogNode> Replies
        {
            get
            {
                List<DialogNode> result = new List<DialogNode>();
                if (!_isReply && _mainStruct.HasField(REPLIES_LIST))
                {
                    List<GffStruct> replies = (List<GffStruct>) _mainStruct[REPLIES_LIST];
                    foreach(GffStruct reply in replies)
                        result.Add(new DialogNode(_dialog, reply, true));
                }
                if (_isReply && _mainStruct.HasField(ENTRIES_LIST))
                {
                    List<GffStruct> entries = (List<GffStruct>)_mainStruct[ENTRIES_LIST];
                    foreach (GffStruct entry in entries)
                        result.Add(new DialogNode(_dialog, entry, false));
                }
                if (result.Count == 1 && result[0].IsEmptyNode())
                {
                    DialogNode node = result[0];
                    result.Clear();
                    result.AddRange(node.Replies);
                }
                return result;
            }
        }

        public bool HasReplies
        {
            get
            {
                if (!_isReply)
                    return ((List<GffStruct>) _mainStruct [REPLIES_LIST]).Count > 0;
                else
                {
                    if (_mainStruct.HasField(ENTRIES_LIST))
                    {
                        List<GffStruct> entriesList = (List<GffStruct>)_mainStruct[ENTRIES_LIST];
                        return entriesList.Count > 0;
                    }
                }

                return false;
            }
        }

        private bool IsDetail(string name)
        {
            return name != INDEX && name != TEXT && name != REPLIES_LIST && name != ENTRIES_LIST && name != SPEAKER_INDEX &&
                name != LISTENER_INDEX;
        }

        private bool IsEmptyNode()
        {
            foreach(string name in _syncStruct.FieldNames)
            {
                if (name != INDEX && name != DESIGNER_NUMBER && name != REPLIES_LIST && name != ENTRIES_LIST)
                    return false;
            }
            foreach (string name in _mainStruct.FieldNames)
            {
                if (name != INDEX && name != DESIGNER_NUMBER && name != REPLIES_LIST && name != ENTRIES_LIST)
                    return false;
            }
            return true;
        }
    }
}
