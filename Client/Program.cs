using System.Net;
using System.Net.Sockets;
using System.Text;

//üzenet pl <MSG>hello</MSG>|EOF|
//a tracker nem ismer senkit de mindenki ismeri a trackert, először  hozzá csatlakozik mindenki
//todo hogy a trackerserver küldje vissza az általa ismert hostok listáját is mert így most csak a tracker
//todo elso csatlakozasnal a peernek nem kell elraknia az ismert ipk közé azt a peert aki connectel hozzá mert ha az ismert hostok listáját elküldi abban is benne van 
//todo time to live , egy msg néhány hop után ne küldődjön tovább
public class Program
{
    //=============================================================================================================================
    // GLOBALS
    //=============================================================================================================================
    //------------------------------------------------------
    // CONSTANTS
    //------------------------------------------------------
    const string TRACKER_SERVER_IP_ENV_NAME = "TRACKER_SERVER_IP";
    const string DEFAULT_TRACKER_SERVER_IP = "172.18.0.2";

    const int SENDINGPORT = 3000;
    const int LISTENINGPORT = 3000;

    const string END_OF_FILE_TOKEN = "|EOF|";
    const string END_OF_HOSTS_TOKEN = "</HOSTS>";
    const string END_OF_MSG_TOKEN = "</MSG>";
    const string START_OF_MSG_TOKEN = "<MSG>";
    const string START_OF_HOSTS_TOKEN = "<HOSTS>";

    const int TIME_TO_LIVE = 20; //defines hogy many hops a peer msg can endure
    //------------------------------------------------------
    // VARIABLES
    //------------------------------------------------------
    static string? trackerServerIp = null; // fresh peers will come here first to obtain the list of known hosts
    static string? localIp = null;
    static LinkedList<string>? knownHosts = null;


    //=============================================================================================================================
    // MAIN
    //=============================================================================================================================
    public static void Main(String[] args)
    {
        //------------------------------------------------------
        // SETUP
        //------------------------------------------------------
        trackerServerIp = GetEnvOrDefault(TRACKER_SERVER_IP_ENV_NAME, DEFAULT_TRACKER_SERVER_IP);
        localIp = GetLocalIPv4Address(); //string localIP = "127.0.0.1"; but wee need the ip that was given by the router if its behind nat

        knownHosts = new LinkedList<string>();
        knownHosts.AddFirst(localIp);
        knownHosts.AddFirst(trackerServerIp); //todo ez nem kellene ide hogyha tacker server futtatja éppen a kódot

        // (1) listens for incoming connections and sends incoming data to each known host using Broadcast() on a secondary thread 
        Thread serverthread = new Thread(() => StartServerBehaviour(knownHosts, LISTENINGPORT));
        serverthread.Start();

        // (2) gets input from user and sends it to each known host using Broadcast() on the main thread 
        string input = "";
        while (input != "exit")
        {
            Thread.Sleep(400);
            Console.WriteLine("______________________________________________________________");
            Console.WriteLine("Enter a message to send to all known hosts:");
            input = Console.ReadLine();
            if (input == "exit" || input == "")
            {
                Environment.Exit(0);
            }
            else
            {
                Broadcast(knownHosts, input, null);
            }
        }
    }


    //=============================================================================================================================
    // FUNCTIONS
    //=============================================================================================================================
    public static void LoadEnv()
    {
        string[] envLines = File.ReadAllLines("./.env"); // bin/Debug/net6.0

        /* Parses the environment variables and sets them */
        foreach (string envLine in envLines)
        {
            string[] envParts = envLine.Split('=');
            string envName = envParts[0];
            string envValue = envParts[1];

            Environment.SetEnvironmentVariable(envName, envValue);
        }
    }

    public static string GetEnvOrDefault(string p_envName, string p_defaultValue)
    {
        string? value = Environment.GetEnvironmentVariable(p_envName);
        if (string.IsNullOrEmpty(value))
        {
            value = p_defaultValue;
        }
        return value;
    }

    public static string GetLocalIPv4Address()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }


    //! listens for incoming connections then uses the knownHosts list to transit the incoming message to each host in the list using the Broadcast function
    public static void StartServerBehaviour(LinkedList<string> p_knownHosts, int p_port = 8080, int p_backlog = 10, int p_frameSize = 1024)
    {
        IPAddress ipAddress = IPAddress.Parse(localIp);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, p_port);

        try
        {
            //? INIT SOCKET
            // Create a Socket that will use Tcp protocol //listening socket
            Socket bindingSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //? BIND
            // A Socket must be associated with an endpoint using the Bind method
            bindingSocket.Bind(localEndPoint);
            //? LISTEN
            // Specify how many requests a Socket can listen before it gives Server busy response.
            // We will listen 10 requests at a time
            bindingSocket.Listen(p_backlog);

            Console.WriteLine("SERVER_STATE_PEER:\tWaiting for a connection at {0} on {1}...", localIp, p_port);

            //? ACCEPT
            while (true)
            {
                Socket handlerSocket = bindingSocket.Accept();

                // Checks the endpoint of the conn and saves it if it's new
                IPEndPoint sourceHost = (IPEndPoint)handlerSocket.RemoteEndPoint;
                Console.WriteLine("SERVER_STATE_PEER:\tSourceHost: {0}\tAddress: {1}\tPort: {2}", sourceHost.ToString(), sourceHost.AddressFamily, sourceHost.Port);
                string sourceIpAdr = sourceHost.Address.ToString();
                if (p_knownHosts.Contains(sourceIpAdr) == false)
                {
                    p_knownHosts.AddLast(sourceIpAdr);
                    Console.WriteLine("SERVER_STATE_PEER:\t" + sourceIpAdr + " was added to knownHosts list");
                };

                // Reads incoming message from client
                byte[] buffer = null;
                string incomingData = "";
                string incomingMsg = "";
                string[] hosts = null;
                int timeToLive = -1;

                //? RECEIVE
                while (true)
                {
                    buffer = new byte[p_frameSize];
                    int receivedBytesCount = handlerSocket.Receive(buffer);
                    incomingData += Encoding.ASCII.GetString(buffer, 0, receivedBytesCount); Console.WriteLine("SERVER_STATE_PEER:\tData received form clientPeer: {0}", incomingData);

                    if (incomingData.IndexOf("|EOF|") > -1)
                    {
                        incomingData = incomingData.TrimEnd("|EOF|".ToCharArray());
                        try { incomingMsg = incomingData.Split(new string[] { "<MSG>", "</MSG>" }, StringSplitOptions.None)[1]; } catch { Console.WriteLine("[there was no msg part in the received data]"); }
                        try { hosts = incomingData.Split(new string[] { "<HOSTS>", "</HOSTS>" }, StringSplitOptions.None)[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); } catch { Console.WriteLine("[there was no hosts part in the received data]"); }
                        try { timeToLive = Convert.ToInt16(incomingData.Split(new string[] { "<TTL>", "</TTL>" }, StringSplitOptions.None)[1]); } catch { Console.WriteLine("[there was no timeToLive part in the received data]"); timeToLive = TIME_TO_LIVE; }
                        break;
                    }
                }

              

                if (hosts != null && hosts.Length > 0) 
                {
                    Console.WriteLine("SERVER_STATE_PEER:\tSaving received hosts...");
                    foreach (var host in hosts)
                    {
                        if (knownHosts.Contains(host)) continue;

                        knownHosts.AddLast(host);
                    }
                }

                if (incomingMsg!= string.Empty) 
                {
                   //TODO handleMsg(incomingmsg)  
                }  
                
                if (timeToLive > 0) { timeToLive--; }
                if (timeToLive == 0) { continue; }

                //? SEND
                // Sends response to client this won't be broadcasted because the socket will be closed after this on both sides
                byte[] resMsg = Encoding.ASCII.GetBytes( " <ACK>"+localIp+" "+DateTime.Now+"</ACK>" + END_OF_FILE_TOKEN); //acknowledged

                handlerSocket.Send(resMsg);

    
                //? CLOSE
                handlerSocket.Shutdown(SocketShutdown.Both);
                handlerSocket.Close();
 

                string knownHostsMsg = string.Join(',', knownHosts);
                // Pass on incomingData to each host in knownHosts list
                Broadcast(p_knownHosts, $"<MSG>{incomingMsg}</MSG><HOSTS>{knownHostsMsg}</HOSTS><TTL>{timeToLive}</TTL>{END_OF_FILE_TOKEN}" , sourceIpAdr);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        //Console.WriteLine("\n Press any key to continue...");
        //Console.ReadKey();
    }

    public static void Broadcast(LinkedList<string> p_knownHosts, string p_incomingData, string p_sourceHost)
    {
        try
        {
            Console.WriteLine("BROADCASTING..."); //client state so init as many conn sockets as known ports and pass on data
            foreach (string currentHost in p_knownHosts)
            {
                if (currentHost != p_sourceHost && currentHost != localIp)
                {
                    //parse currentHost string to ipaddress
                    IPAddress targetIP = IPAddress.Parse(currentHost);

                    Console.WriteLine("...sending to " + targetIP);
                    StartClientBehaviour(targetIP, SENDINGPORT, p_incomingData);

                }
            }
            Console.WriteLine("LISTENING AGAIN...");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

    }

    //! connects to a specified ip and port and sends messages
    public static void StartClientBehaviour(IPAddress p_ipAddress, int p_port, string p_message, int p_frameSize = 1024)
    {
        byte[] buffer = new byte[p_frameSize];
        IPEndPoint remoteEP = new IPEndPoint(p_ipAddress, p_port);

        //? INIT SOCKET
        // Create a TCP/IP  socket.
        Socket connectingSocket = new Socket(p_ipAddress.AddressFamily,
                                             SocketType.Stream,
                                             ProtocolType.Tcp);

        // Connect the socket to the remote endpoint. Catch any errors.
        try
        {
            //? CONNECT
            // Connect to Remote EndPoint
            connectingSocket.Connect(remoteEP);

            Console.WriteLine("CLIENT_STATE_PEER:\tSocket connected to serverPeer {0}", connectingSocket.RemoteEndPoint.ToString());

            // Encode the data string into a byte array.
            byte[] msg = Encoding.ASCII.GetBytes(p_message);

            //? SEND
            // Send the data through the socket.
            int sentBytesCount = connectingSocket.Send(msg);

            //? RECEIVE
            // Receive the response from the remote device.
            connectingSocket.ReceiveTimeout = 5000; //if not receiving anything in 5 seconds close connection
            int receivedBytesCount = connectingSocket.Receive(buffer);
            Console.WriteLine("CLIENT_STATE_PEER:\tData received from serverPeer: {0}", Encoding.ASCII.GetString(buffer, 0, receivedBytesCount));

            //? CLOSE
            // Release the socket.
            connectingSocket.Shutdown(SocketShutdown.Both);
            connectingSocket.Close();

        }
        catch (ArgumentNullException ane)
        {
            Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
        }
        catch (SocketException se)
        {
            Console.WriteLine("SocketException : {0}", se.ToString());
        }
        catch (Exception e)
        {
            Console.WriteLine("Unexpected exception : {0}", e.ToString());
        }
    }
}
