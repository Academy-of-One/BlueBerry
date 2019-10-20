using Blueberry.Dekstop.WindowsApp.Bluetooth;
using System;

namespace Blueberry.Desktop.ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var watcher = new DnaBlueberryBluetoothLEAdvertisementWatcher();

            watcher.StartedListening += () =>
            {
                Console.WriteLine("Started Listening");
            };

            watcher.StoppedListening += () =>
            {
                Console.WriteLine("Stopped Listening");
            };

            watcher.DeviceNameChanged += (device) =>
            {
                Console.WriteLine($"Device name changed: {device}");
            };

            watcher.DeviceTimedOut += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Device Timeout: {device}");
            };

            // Start listening
            watcher.StartListening();

            while(true)
            {
                Console.ReadLine();

                // get discoverd devices
                var devices = watcher.DiscoverdDevices;

                Console.WriteLine($"{devices.Count} devices......");

                foreach (var device in devices)
                    Console.WriteLine(device);
            }
        }
    }
}
