using Blynk.Virtual;
using PowerArgs;
using System;

using System.Threading.Tasks;

namespace Telldus
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class Options
    {
        [ArgShortcut("s")]
        [ArgShortcut("--server")]
        [ArgDefaultValue("tcp://127.0.0.1:8080")]
        [ArgDescription("The url to the Blynk server.")]
        public string Server { get; set; }

        [ArgShortcut("a")]
        [ArgShortcut("--authorization")]
        [ArgDefaultValue("****")]
        [ArgDescription("The device authorization token used to identitify this application.")]
        public string Authorization { get; set; }

        [ArgShortcut("t")]
        [ArgShortcut("--telldus")]
        [ArgDefaultValue("ws://192.168.1.17/ws")]
        [ArgDescription("The Telldus websocket device uri.")]
        public string Telldus { get; set; }

        [ArgShortcut("d")]
        [ArgShortcut("--debug")]
        [ArgDescription("Show debug information.")]
        public bool Debug { get; set; }

    }
    class Program
    {
        static void Main(string[] args)
        {
            Options options = null;
            try
            {
                options = Args.Parse<Options>(args);
            }
            catch (ArgException e)
            {
                Console.WriteLine(string.Format("Problems with the command line options: {0}", e.Message));
                Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<Options>());
                return;
            }
            var url = options.Server; // Blynk server address
            var authorization = options.Authorization; // Authorization token
            using (var client = new Client(url, authorization))
            {
                client.Connect();
                var tcs = new TaskCompletionSource<bool>();
                client.OnAuthorized += v => tcs.SetResult(v);
                client.Login();
                var authorized = tcs.Task.Result;
                if (authorized)
                {
                    Console.WriteLine("Hardware client is authorized with given token");


                    using (var ws = new TelldusClient(options.Telldus))
                    {
                        ws.ConnectAsync().Wait();
                        ws.OnMessage += m =>
                        {
                            var id = (int)m["id"];
                            var type = (string)m["type"];
                            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            var name = (string)m["name"];
                            if (type == "device")
                            {
                                var state = (int)m["state"];
                                switch (state)
                                {
                                    case 1:
                                        client.WriteVirtualPin(id, $"{time} {name} on\r\n");
                                        break;
                                    case 2:
                                        client.WriteVirtualPin(id, $"{time} {name} off\r\n");
                                        break;
                                    default:
                                        client.WriteVirtualPin(id, $"{time} {name} other state {state}\r\n");
                                        break;
                                }
                            }
                            else if (type == "sensor")
                            {
                                var valueType = (int)m["valueType"];
                                if (valueType == 1)
                                {
                                    var value = (double)m["value"];
                                    client.WriteVirtualPin(id, value);
                                }
                            }
                            if (options.Debug)
                                Console.WriteLine(m.ToString());
                        };

                        var closeTcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
                        Console.CancelKeyPress += (o, e) =>
                        {
                            closeTcs.SetResult(true);
                        };
                        ws.StartMessageLoop();
                        Console.WriteLine("Server is active. Press CTRL+C to stop.");
                        closeTcs.Task.Wait();
                        Console.WriteLine("Stopping server.");
                        ws.CloseAsync().Wait();
                    }



                }
                else
                {
                    Console.WriteLine("Cannot authorize client with given token.");
                }
                client.Disconnect();
            }
        }
    }
}
