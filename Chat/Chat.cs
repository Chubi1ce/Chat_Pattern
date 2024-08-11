using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace UDP_Chat
{
    internal class Chat
    {
        private UdpClient? client;
        static IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
        static UdpClient udpClient = new UdpClient(12345);
        static Dictionary<string, IPEndPoint> clients = new Dictionary<string, IPEndPoint>();

        public void UDP_Chat(string ip, int port)
        {
            client = new UdpClient();
            client.Connect(ip, port);
        }

        public static void Server()
        {
            Console.WriteLine("Waiting incomming messages...");
            Handler h1 = new ConcreteHandler1();
            Handler h2 = new ConcreteHandler2();
            Handler h3 = new ConcreteHandler3();
            h1.SetSuccessor(h2);
            h2.SetSuccessor(h3);

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    byte[] buffer = udpClient.Receive(ref endPoint);
                    string str = Encoding.UTF8.GetString(buffer);

                    try
                    {
                        Message? msg = Message.ConvertFromJSON(str);
                        if (msg != null)
                        {
                            Message newMsgToClient = new Message();

                            if (msg.ToNickName.Equals("Server"))
                            {
                                h1.HandleRequest(msg);
                            }

                            else if (msg.ToNickName.ToLower().Equals("all"))
                            {
                                foreach (var client in clients)
                                {
                                    msg.ToNickName = client.Key;
                                    string jsonStr = msg.ConvertToJSON();
                                    byte[] bytes1 = Encoding.UTF8.GetBytes(jsonStr);
                                    udpClient.Send(bytes1, client.Value);
                                }
                                newMsgToClient = new Message("Server", $"Message send to all users", DateTime.Now);
                            }

                            else if (clients.TryGetValue(msg.ToNickName, out IPEndPoint? value))
                            {
                                string jsonStr = msg.ConvertToJSON();
                                byte[] bytes1 = Encoding.UTF8.GetBytes(jsonStr);
                                udpClient.Send(bytes1, value);
                                newMsgToClient = new Message("Server", $"Message send to {msg.ToNickName}", DateTime.Now);
                            }

                            else
                            {
                                newMsgToClient = new Message("Server", $"Can not find user {msg.ToNickName}", DateTime.Now);
                            }

                            Console.WriteLine(msg.ToString());
                            //newMsgToClient = new Message("server", "message delivered", DateTime.Now);
                            
                        }
                        else
                        {
                            Console.WriteLine("Huston, we have a problem!!!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            });
            thread.Start();
        }

        abstract class Handler
        {
            protected Handler? successor;
            public void SetSuccessor(Handler successor)
            {
                this.successor = successor;
            }

            public abstract void HandleRequest(Message message);
        }
        class ConcreteHandler1 : Handler
        {
            public override void HandleRequest(Message inMesage)
            {
                Message newMsgToClient = new Message();
                if (inMesage.Text.ToLower().Equals("register"))
                {
                    if (clients.TryAdd(inMesage.FromNickName, endPoint))
                    {
                        newMsgToClient = new Message("Server", $"{inMesage.FromNickName} added to list", DateTime.Now);
                        SendMsg(newMsgToClient);
                    }
                }
                else if (successor != null)
                {
                    successor.HandleRequest(inMesage);
                }
            }

        }


        class ConcreteHandler2 : Handler
        {
            public override void HandleRequest(Message inMesage)
            {
                Message newMsgToClient = new Message();
                if (inMesage.Text.ToLower().Equals("delete"))
                {
                    clients.Remove(inMesage.FromNickName);
                    newMsgToClient = new Message("Server", $"{inMesage.FromNickName} deleted from list", DateTime.Now);
                    SendMsg(newMsgToClient);
                }
                else if (successor != null)
                {
                    successor.HandleRequest(inMesage);
                }
            }
        }

        class ConcreteHandler3 : Handler
        {
            public override void HandleRequest(Message inMesage)
            {
                Message newMsgToClient = new Message();
                if (inMesage.Text.ToLower().Equals("list"))
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var client in clients)
                    {
                        sb.Append(client.Key + "\n");
                    }
                    newMsgToClient = new Message("Server", $"Clients list\n {sb.ToString()}", DateTime.Now);
                    SendMsg(newMsgToClient);
                }
                else if (successor != null)
                {
                    successor.HandleRequest(inMesage);
                }
            }
        }

        public static void SendMsg(Message message)
        {
            string js = message.ConvertToJSON();
            byte[] bytes = Encoding.UTF8.GetBytes(js);
            udpClient.Send(bytes, endPoint);
        }




        public static void ClientSendler(string nickName)
        {
            IPEndPoint endPointSedler = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            UdpClient client = new UdpClient();
            //client.Connect(endPointSedler);

            while (true)
            {
                Console.WriteLine("Input message recipient:");
                string toNickName = Console.ReadLine();
                if (String.IsNullOrEmpty(toNickName))
                {
                    Console.WriteLine("You have not entered a message recipient");
                    continue;
                }

                Console.WriteLine("Input massage:");
                string? text = Console.ReadLine();
                if (String.IsNullOrEmpty(text) || text.ToLower().Equals("exit"))
                {
                    break;
                }

                Message newMsg = new Message(nickName, text, DateTime.Now);
                newMsg.ToNickName = toNickName;
                string js = newMsg.ConvertToJSON();
                byte[] bytes = Encoding.UTF8.GetBytes(js);
                client.Send(bytes, endPointSedler);

                byte[] buffer = client.Receive(ref endPointSedler);
                string str = Encoding.UTF8.GetString(buffer);
                Message? msgFromServer = Message.ConvertFromJSON(str);
                Console.WriteLine(msgFromServer);
            }
        }

        public static void ClientListener()
        {
                IPEndPoint endPointListener = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6789);
                UdpClient client = new UdpClient();
                client.Connect(endPointListener);
            
                while (true)
                {   
                    byte[] buffer = client.Receive(ref endPointListener);
                    string str = Encoding.UTF8.GetString(buffer);
                    Message? msgFromServer = Message.ConvertFromJSON(str);
                    Console.WriteLine(msgFromServer);
                }
        }

        public static void Client(string nickName)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            UdpClient client = new UdpClient();

            while (true)
            {
                Console.WriteLine("Input message recipient:");
                string toNickName = Console.ReadLine();
                if (String.IsNullOrEmpty(toNickName))
                {
                    Console.WriteLine("You have not entered a message recipient");
                    continue;
                }

                Console.WriteLine("Input massage:");
                string? text = Console.ReadLine();
                if (String.IsNullOrEmpty(text) || text.ToLower().Equals("exit"))
                {
                    break;
                }

                Message newMsg = new Message(nickName, text, DateTime.Now);
                newMsg.ToNickName = toNickName;
                string js = newMsg.ConvertToJSON();
                byte[] bytes = Encoding.UTF8.GetBytes(js);
                client.Send(bytes, endPoint);

                byte[] buffer = client.Receive(ref endPoint);
                string str = Encoding.UTF8.GetString(buffer);
                Message? msgFromServer = Message.ConvertFromJSON(str);
                Console.WriteLine(msgFromServer);
            }
        }

    }
}
