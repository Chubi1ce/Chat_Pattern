namespace UDP_Chat
{
    internal class Program
    {
       
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Chat.Server();
                Thread threadListener = new Thread(() =>
                {
                    Chat.ClientListener();
                });
                threadListener.Start();

            }
            else
            {
                Thread threadSendler = new Thread(() =>
                {
                    Chat.ClientSendler(args[0]);
                });
                threadSendler.Start();
            }
        }
    }
}
