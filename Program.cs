using System;
using System.Text;
using System.Threading.Tasks;
using NATS.Client;
using STAN.Client;

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
            var natsConn = new ConnectionFactory().CreateConnection(natsOptions);

            var stanOptions = StanOptions.GetDefaultOptions();
            stanOptions.NatsConn = natsConn;
            var stanConn = new StanConnectionFactory().CreateConnection("test-cluster", "net-client", stanOptions);

            TestNATS(natsConn);
            // TestSTAN(stanConn);
        }

        static void TestNATS(IConnection conn)
        {
            Task.Run(async () =>
            {
                var i = 0;
                while (true)
                {
                    try
                    {   
                        Console.WriteLine("Pub {0}", i);
                        var d = UTF8Encoding.UTF8.GetBytes(DateTime.Now.ToString());
                        conn.Publish("test", UTF8Encoding.UTF8.GetBytes(i.ToString()));
                        await Task.Delay(1000);
                        i++;
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err);
                    }
                }
            });

            EventHandler<MsgHandlerEventArgs> h = (sender, msgArs) =>
            {
                Console.WriteLine("Recv {0}", UTF8Encoding.UTF8.GetString(msgArs.Message.Data));
            };

            var s = conn.SubscribeAsync("test", h);
            s.Start();
        }

        static void TestSTAN(IStanConnection conn)
        {
            Task.Run(async () =>
            {
                var i = 0;
                while (true)
                {
                    try
                    {
                        string guid = conn.Publish("stest", UTF8Encoding.UTF8.GetBytes(i.ToString()), null);

                        await Task.Delay(1000);
                        i++;
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err);
                    }
                }
            });

            var sOpts = StanSubscriptionOptions.GetDefaultOptions();
            sOpts.AckWait = 60000;
            var s = conn.Subscribe("stest", sOpts, (obj, msgArs) =>
            {
                Console.WriteLine(UTF8Encoding.UTF8.GetString(msgArs.Message.Data));
            });
        }
    }
}