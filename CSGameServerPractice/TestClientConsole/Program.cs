using CSTestClient;


internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Start Client");

        Client client = new Client();
        client.StartClient();
    }
}