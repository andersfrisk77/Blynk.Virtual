using Blynk.Virtual;
using PowerArgs;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Blynk.Power
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class Options
    {
        [ArgShortcut("p")]
        [ArgShortcut("--pin")]
        [ArgDefaultValue(18)]
        [ArgDescription("Listening pin connected to power led.")]
        public int Pin { get; set; }

        [ArgShortcut("s")]
        [ArgShortcut("--server")]
        [ArgDefaultValue("tcp://127.0.0.1:8080")]
        [ArgDescription("The url to the Blynk server.")]
        public string Server { get; set; }

        [ArgShortcut("a")]
        [ArgShortcut("--authorization")]
        [ArgDefaultValue("****")]
        [ArgDescription("The device authorization token used to identitify the repeater.")]
        public string Authorization { get; set; }

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

                    var pin = options.Pin;
                    var debug = options.Debug;
                    Console.WriteLine("Read event on a pin");
                    using (var controller = new GpioController())
                    {
                        controller.OpenPin(pin, PinMode.Input);
                        Console.WriteLine($"GPIO pin enabled for use: {pin}");

                        var watch = new Stopwatch();
                        var risingEventHandler = new PinChangeEventHandler((@object, a) =>
                        {
                            var elapsed = watch.ElapsedMilliseconds / 1000.0; // elapsed time in seconds
                            if (elapsed != 0)
                            {
                                var power = 3600.0f / elapsed;
                                if (debug)
                                    Console.WriteLine($"Power consumption {power}");
                                client.WriteVirtualPin(pin, power);
                            }
                            watch.Restart();
                            if (debug)
                                Console.WriteLine("Rising");
                        });
                        var fallingEventHandler = new PinChangeEventHandler((@object, a) =>
                        {
                            if (debug)
                                Console.WriteLine("Falling");
                        });

                        var source = new TaskCompletionSource<bool>();
                        Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs eventArgs) =>
                        {
                            controller.UnregisterCallbackForPinValueChangedEvent(pin, risingEventHandler);
                            controller.UnregisterCallbackForPinValueChangedEvent(pin, fallingEventHandler);
                            source.SetResult(true);
                        };

                        controller.RegisterCallbackForPinValueChangedEvent(pin, PinEventTypes.Rising, risingEventHandler);
                        controller.RegisterCallbackForPinValueChangedEvent(pin, PinEventTypes.Falling, fallingEventHandler);
                        Console.WriteLine("Press CTRL+C to stop.");
                        source.Task.Wait();
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
