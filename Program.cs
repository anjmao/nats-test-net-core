using System;
using System.Threading.Tasks;
using NATS.Client;

namespace NATS
{
    class Program
    {
        static void Main(string[] args)
        {
            var natsOptions = ConnectionFactory.GetDefaultOptions();

            natsOptions.Url = "nats://localhost:4222";
            natsOptions.PingInterval = 1 * 1000;
            natsOptions.MaxPingsOut = 3;
            natsOptions.AllowReconnect = true;
            natsOptions.MaxReconnect = Options.ReconnectForever;
            natsOptions.ReconnectWait = 1 * 1000;

            var natsConnection = new ConnectionFactory().CreateConnection(natsOptions);

            // var stanOptions = StanOptions.GetDefaultOptions();
            // stanOptions.NatsConn = natsConnection;
            // stanOptions.PubAckWait = 100 * 1000;
            // stanOptions.ConnectTimeout = 100 * 1000;
            // var connection = new StanConnectionFactory().CreateConnection("test-cluster", "net-client", stanOptions);

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        natsConnection.Publish("test", System.Text.UTF8Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
                        Console.WriteLine("publishing");
                        await Task.Delay(3000);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err);
                    }
                }
            });

            EventHandler<MsgHandlerEventArgs> h = (sender, msgArs) =>
            {
                Console.WriteLine(System.Text.UTF8Encoding.UTF8.GetString(msgArs.Message.Data));
            };

            var s = natsConnection.SubscribeAsync("test", h);
            s.Start();
        }
    }
}
