using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace ConsoleSimulation
{
    class Program
    {
        // Target IoT Hub : connection string
        private static string connectionString = "<your_hub_connection_string>";

        // Prefix of the devices id
        private static string prefixDevice = "hesgc_astrocast_";

        // Number of devices to simulate
        private static int NumberOfDevices = 30;


        static void Main(string[] args)
        {
            SimulationManager simulation = new SimulationManager(connectionString, prefixDevice, NumberOfDevices);

            simulation.AddDevices().Wait();

            simulation.GetDevices().Wait();

  //          displayListOfDevices(simulation);
            simulation.LaunchTest();

            Console.WriteLine("Press the Enter key to exit.");
            Console.ReadLine();

            simulation.RemoveDevices().Wait();
        }

        private static void displayListOfDevices(SimulationManager simulation)
        {
            List<DeviceEntity> listOfDevices = simulation.getListOfDevices();
            foreach(DeviceEntity device in listOfDevices)
            {
                Console.WriteLine(device.Id);
            }
        }
    }
}

