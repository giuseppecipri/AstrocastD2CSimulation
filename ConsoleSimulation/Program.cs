using System;
using System.Collections.Generic;

namespace ConsoleSimulation
{
    class Program
    {
        // Target IoT Hub : connection string
        private static string connectionString = "<your_hub_connection_string>";

        // Prefix of the devices id
        private static string prefixDevice = "hesgc_astrocast_";

        // Number of devices to simulate
        private static int NumberOfDevices = 2;

        // Telemetry frequency (Seconds): Set how often to send telemetry from each device
        private static int telemetryInterval = 1;

        static void Main(string[] args)
        {
            // Declare new simulation
            SimulationManager simulation = new SimulationManager(connectionString, prefixDevice, NumberOfDevices, telemetryInterval);

            // Add devices to your IoT hub's identity registry
            simulation.AddDevices().Wait();

            // Reads devices to connect for simulation
            simulation.GetDevices().Wait();

//          displayListOfDevices(simulation); // for test

            // Launch the simulation
            simulation.StartSimulation();

            // Wait for stop simulation
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Press the Enter key to stop simulation.");
            Console.ReadLine();

            // Stop simulation devices
            simulation.StopSimulation();

            // Wait for exit
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Press the Enter key to exit.");
            Console.ReadLine();

            // Delete devices after simulation
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

