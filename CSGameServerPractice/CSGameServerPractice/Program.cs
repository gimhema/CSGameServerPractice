using CSGameServerPractice;


internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Start Server");
        Server server= new Server();
        server.Run();
    }
}