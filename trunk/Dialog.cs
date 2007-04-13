using System;
using System.Collections.Generic;
using System.Text;

namespace JadeDlg
{
    class Dialog
    {
        private Game _game;
        private GffStruct _struct;
        private List<GffStruct> _tagList;
        private List<GffStruct> _entryList;
        private List<GffStruct> _replyList;
        private List<GffStruct> _startingList;

        public Dialog(Game game, GffStruct gffStruct)
        {
            _game = game;
            _struct = gffStruct;
            _tagList = (List<GffStruct>) _struct ["TagList"];
            _entryList = (List<GffStruct>) _struct ["EntryList"];
            _replyList = (List<GffStruct>) _struct ["ReplyList"];
            _startingList = (List<GffStruct>) _struct ["StartingList"];
        }

        public List<DialogNode> StartingNodes
        {
            get
            {
                List<DialogNode> result = new List<DialogNode>();
                foreach(GffStruct gffStruct in _startingList)
                    result.Add(new DialogNode(this, gffStruct, false));
                return result;
            }
        }

        public GffStruct GetEntryNode(int index)
        {
            return _entryList[index];
        }

        public GffStruct GetReplyNode(int index)
        {
            return _replyList[index];
        }

        public string GetTalkText(TalkRef talkRef)
        {
            return "\"" + _game.TalkFile[talkRef.Index] + "\"";
        }

        public string GetSpeaker(int index)
        {
            return (string) _tagList[index] ["Tag"];
        }
    }
}
