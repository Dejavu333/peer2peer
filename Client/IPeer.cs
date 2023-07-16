//namespace peer2peer
//{
//    interface IPeer
//    {
//        void connect(string p_host, string p_port);
//        //void disconnect();
//        void send(AbstractPeerMsg p_msg);
//        void broadcast();
//        void listen(string p_port); //on a thread

//        MsgHandlerChain msgHandlerChain();
//        LinkedList<string> knownHosts();

//    }


//    sealed class DefaultPeer : IPeer
//    {
//        public void broadcast()
//        {
//            throw new NotImplementedException();
//        }

//        public void connect(string p_host, string p_port)
//        {
//            throw new NotImplementedException();
//        }

//        public void listen(string p_port)
//        {
//            throw new NotImplementedException();
//        }

//        public MsgHandlerChain msgHandlerChain()
//        {
//            throw new NotImplementedException();
//        }

//        public void send(AbstractPeerMsg p_msg)
//        {
//            throw new NotImplementedException();
//        }

//        public LinkedList<string> knownHosts()
//        {
//            throw new NotImplementedException();
//        }
//    }


//    sealed class MsgHandlerChain
//    {
//        int currentIndex = 0;
//        List<IMsgHandler> handlers;
//        IMsgHandler? firstHandler()
//        {
//            if (handlers == null || handlers.Count == 0) { return null; }
//            return this.handlers[0];

//        }
//        public IMsgHandler? nextHandler()
//        {
//            if (handlers == null || currentIndex >= handlers.Count) { return null; }

//            return this.handlers[currentIndex++];
//        }
//        public void addHandler(IMsgHandler p_msgHandler) 
//        {
//            this.handlers.Add(p_msgHandler);
//        }
//        public void removeHandler(IMsgHandler p_msgHandler) 
//        {
//            this.handlers.Remove(p_msgHandler);
//        }
//    }

//    interface IMsgHandler
//    {
//        void handleMsg(AbstractPeerMsg p_msg);
//    }


//    sealed class AckPeerMsgHandler : IMsgHandler
//    {
//        public void handleMsg(AbstractPeerMsg p_msg)
//        {
//            throw new NotImplementedException();
//        }
//    }


//    sealed class IntentionPeerMsgHandler : IMsgHandler
//    {
//        public void handleMsg(AbstractPeerMsg p_msg)
//        {
//            throw new NotImplementedException();
//        }
//    }


//    sealed class KnownHostsPeerMsgHandler : IMsgHandler
//    {
//        public void handleMsg(AbstractPeerMsg p_msg)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}