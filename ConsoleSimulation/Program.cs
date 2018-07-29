using System;
using System.Collections.Generic;
using System.Configuration;

namespace ConsoleSimulation
{
    class Program
    {
        // Target Azure IoT Hub : connection string (example : HostName=<your iot hub name>.azure-devices.net;SharedAccessKeyName=<your user>;SharedAccessKey=<your key>)
        private static string connectionString;

        // Prefix of the devices id (example : "hesgc_astrocast_")
        private static string prefixDevice;

        // Number of devices to simulate (example : 20)
        private static int numberOfDevices;

        // Telemetry frequency (Seconds): Set how often to send telemetry from each device (example : 1)
        private static int telemetryInterval;

        static void Main(string[] args)
        {
            try
            {
                // Read file configuration
                GetConfiguration_AppSettings();

                //----------- Declare a new simulation
                SimulationManager simulation = new SimulationManager(connectionString, prefixDevice, numberOfDevices, telemetryInterval);
                if(simulation == null)
                {
                    throw new Exception("Error during the connection. Please check your connection string");
                }

                //----------- Start the complet simulation scenario
                simulation.StartCompleteSimulation();

                // Wait for stop simulation
                Console_output("Press the Enter key to stop simulation.", ConsoleColor.Blue);
                //----------- Stop simulation devices
                simulation.StopSimulation();

                // Wait for remove devices
                Console_output("Press the Enter key to remove devices.", ConsoleColor.Blue);
                //----------- Delete devices after simulation
                simulation.RemoveDevices_Async().Wait();

                // Wait for exit
                Console_output("Press the Enter key to exit.", ConsoleColor.Blue);

            }
            catch (FormatException ex)
            {
                Console_output(ex.Message + " : Please check your connection string", ConsoleColor.Red);
            }
            catch (Exception ex)
            {
                Console_output(ex.Message, ConsoleColor.Red);
            }
        }

        private static void GetConfiguration_AppSettings()
        {
 
            // Target IoT Hub : connection string
            //string connectionString = "HostName=hesgcAstrocast-S1.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=qmuYOARsJUB8Oa6NGK0tDPGwygehA5seLSwDP9OpEtA=";
            connectionString = ConfigurationManager.AppSettings["IoTHub.ConnectionString"];

            // Prefix of the devices id (for exemple "hesgc_astrocast_")
            prefixDevice = ConfigurationManager.AppSettings["prefixDevice"];

            // Number of devices to simulate
            numberOfDevices = int.Parse(ConfigurationManager.AppSettings["NumberOfDevices"]);

            // Telemetry frequency (Seconds): Set how often to send telemetry from each device
            telemetryInterval = int.Parse(ConfigurationManager.AppSettings["telemetryInterval"]);
   
        }

        private static void Console_output(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ReadLine();
        }

        // For test
        private static void displayListOfDevices(SimulationManager simulation)
        {
            List<DeviceEntity> listOfDevices = simulation.GetListOfDevices();
            foreach(DeviceEntity device in listOfDevices)
            {
                Console.WriteLine(device.Id);
            }
        }
    }
}

