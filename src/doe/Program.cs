using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace doe

// Vertex in the graph
public class Node {
	public int id = 0;
	public int dimension = 3;
	public int[] size = [0,0,0];
	public Node() {
	}
	public Node(int nodeDimension, int[] nodeSize) {
		dimension = nodeDimension;
		size = nodeSize;
	}
	public void Resize(int[] newSize) {
		size = newSize;
	}
}

// Graph to be optimized
public class MetricSpace {
	public int dimension = 3;
	public int[] size = [0,0,0];
}

// Registered client
public class Service {
        [XmlAttribute("id")] public int id = 0;
        public Socket socket = null;
        [XmlAttribute("name")] public String name = "";
        [XmlAttribute("description")] public String description = "";

	public Service() {
        }
	
	public Service(Socket serviceSocket, string serviceName, string serviceDescription) {
		socket = serviceSocket;
		name = serviceName;
		description = serviceDescription;
	}

	public bool Query(String query) {
		if ((name.IndexOf(query) > -1) || (description.IndexOf(query) > -1)) return true;
		else return false;
	}
}

// List of registered clients
[XmlRoot("ServiceList")]
public class ServiceList {
	private ArrayList listService;
	
	public ServiceList() {
		listService = new ArrayList();
	}

	[XmlElement("Service")]
	public Service[] Services {
		get {
			Service[] services = new Service[ listService.Count() ];
			listService.CopyTo( services );
			return services;
		}
		set {
			if( value==null ) return;
			Service[] services = (Item[])value;
			listService.Clear();
			foreach( Service service in services )
				listService.Add( service );
		}
	}

	public int AddService( Service service ) {
		return listService.Add( service );
	}

}

public class ServiceManager {
	public int id;
	public ServiceList services;
	public string Register( Service service ) {
		id = services.AddService( service );
		return "Service registered";
	}
}

// State object for reading client data asynchronously
public class StateObject {
	public Socket workSocket = null;
	public const int BufferSize = 1024;
	public byte[] buffer = new byte[BufferSize];
	public StringBuilder sb = new StringBuilder();  
}

public class AsynchronousSocketListener {
    // Thread signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public AsynchronousSocketListener() {
    }

    public static void StartListening() {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // Establish the local endpoint for the socket.
        // The DNS name of the computer
        // running the listener is "host.contoso.com".
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        // Create a TCP/IP socket.
        Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp );

        // Bind the socket to the local endpoint and listen for incoming connections.
        try {
            listener.Bind(localEndPoint);
            listener.Listen(100);

            while (true) {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.
                Console.WriteLine("Waiting for a connection...");
                listener.BeginAccept( 
                    new AsyncCallback(AcceptCallback),
                    listener );

                // Wait until a connection is made before continuing.
                allDone.WaitOne();
            }

        } catch (Exception e) {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();
        
    }

    public static void AcceptCallback(IAsyncResult ar) {
        // Signal the main thread to continue.
        allDone.Set();

        // Get the socket that handles the client request.
        Socket listener = (Socket) ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.
        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar) {
        String content = String.Empty;
        
        // Retrieve the state object and the handler socket
        // from the asynchronous state object.
        StateObject state = (StateObject) ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket. 
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0) {
            // There  might be more data, so store the data received so far.
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer,0,bytesRead));

            // Check for end-of-file tag. If it is not there, read 
            // more data.
            content = state.sb.ToString();
            if (content.IndexOf("<EOF>") > -1) {
                // All the data has been read from the 
                // client. Display it on the console.
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content );
                // Echo the data back to the client.
                Send(handler, content);
            } else {
                // Not all data received. Get more.
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }
        }
    }
    
    private static void Send(Socket handler, String data) {
        / / Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }

    private static void SendCallback(IAsyncResult ar) {
        try {
            // Retrieve the socket from the state object.
            Socket handler = (Socket) ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

        } catch (Exception e) {
            Console.WriteLine(e.ToString());
        }
    }


    public static int Main(String[] args) {
        StartListening();
        return 0;
    }
}
