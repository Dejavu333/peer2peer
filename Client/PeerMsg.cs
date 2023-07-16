using System.Text;

namespace peer2peer
{
    public class PeerMsg 
    {
        public const string END_OF_FILE_TOKEN = "|EOF|";
        public int timeToLive;
        public string sourceHost;
        public string destinationHost;
        public byte[] byteArrayRepresOfparsedContent;
        public dynamic rawContent;
        public dynamic parsedContent;
    }

    public abstract class AbstractPeerMsg 
    {
        //----------------------------------
        // fields
        //----------------------------------
        public const string END_OF_FILE_TOKEN = "|EOF|";
        public int timeToLive;
        public string sourceHost;
        public string destinationHost;
        public byte[] byteArrayRepresOfparsedContent;
        public dynamic rawContent;
        public dynamic parsedContent;

        //----------------------------------
        // constructors
        //----------------------------------
        public AbstractPeerMsg(string p_rawContent, string p_sourceHost, string p_destinationHost)
        {
            this.rawContent = p_rawContent;
            this.sourceHost = p_sourceHost;
            this.destinationHost = p_destinationHost;
            this.parsedContent = parsedContentFromrawContent(p_rawContent);
        }
        public AbstractPeerMsg(byte[] p_rawContent, string p_sourceHost, string p_destinationHost) : this(Encoding.UTF8.GetString(p_rawContent), p_sourceHost, p_destinationHost)
        {
        }

        //----------------------------------
        // methods
        //----------------------------------
        public abstract dynamic parsedContentFromrawContent(dynamic p_rawContent);
        public abstract dynamic rawContentFromParsedContent(dynamic p_parsedContent);
    }


    public sealed class IntentionAbstractPeerMsg : AbstractPeerMsg
    {
        //----------------------------------
        // fields
        //----------------------------------
        const string START_TOKEN = "<MSG>";
        const string END_TOKEN = "</MSG>";

        //----------------------------------
        // constructors
        //----------------------------------
        public IntentionAbstractPeerMsg (string p_rawContent, string p_sourceHost, string p_destinationHost) : base(p_rawContent, p_sourceHost, p_destinationHost)
        {
        }
        public IntentionAbstractPeerMsg(byte[] p_rawContent, string p_sourceHost, string p_destinationHost) : base(p_rawContent, p_sourceHost, p_destinationHost)
        {
        }

        //----------------------------------
        // methods
        //----------------------------------
        override public dynamic parsedContentFromrawContent(dynamic p_rawContent) 
        {
            throw new NotImplementedException();
        }

        public override dynamic rawContentFromParsedContent(dynamic p_parsedContent)
        {
            throw new NotImplementedException();
        }
    }


    public sealed class KnownHostsAbstractPeerMsg : AbstractPeerMsg 
    {
        //----------------------------------
        // fields
        //----------------------------------
        const string START_TOKEN = "<HOSTS>";
        const string END_TOKEN = "</HOSTS>";

        //----------------------------------
        // constructors
        //----------------------------------
        public KnownHostsAbstractPeerMsg(string p_rawContent, string p_sourceHost, string p_destinationHost) : base(p_rawContent, p_sourceHost, p_destinationHost)
        {
        }
        public KnownHostsAbstractPeerMsg(byte[] p_rawContent, string p_sourceHost, string p_destinationHost) : base(p_rawContent, p_sourceHost, p_destinationHost)
        {
        }

        //----------------------------------
        // methods
        //----------------------------------
        override public dynamic parsedContentFromrawContent(dynamic p_rawContent)
        {
            throw new NotImplementedException();
        }

        public override dynamic rawContentFromParsedContent(dynamic p_parsedContent)
        {
            throw new NotImplementedException();
        }
    }


    public sealed class AcknowledgementAbstractPeerMsg : AbstractPeerMsg
    {
        //----------------------------------
        // fields
        //----------------------------------
        const string START_TOKEN = "<ACK>";
        const string END_TOKEN = "</ACK>";

        //----------------------------------
        // constructors
        //----------------------------------
        public AcknowledgementAbstractPeerMsg(string p_rawContent, string p_sourceHost, string p_destinationHost) : base(p_rawContent, p_sourceHost, p_destinationHost)
        {
        }
        public AcknowledgementAbstractPeerMsg(byte[] p_rawContent, string p_sourceHost, string p_destinationHost) : base(p_rawContent, p_sourceHost, p_destinationHost)
        {
        }

        //----------------------------------
        // methods
        //----------------------------------
        public override dynamic rawContentFromParsedContent(dynamic p_parsedContent)
        {
            throw new NotImplementedException();
        }
        public override dynamic parsedContentFromrawContent(dynamic p_rawContent)
        {
            throw new NotImplementedException();
        }
    }
}
