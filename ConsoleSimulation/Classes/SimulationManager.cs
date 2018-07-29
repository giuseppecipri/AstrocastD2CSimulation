using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleSimulation
{
    class SimulationManager
    {
        // Target IoT Hub : connection string
        private string iotHubConnectionString;

        // Prefix of the devices id
        private string prefixDevice;  

        // Number of devices to simulate
        private int numberOfDevices;

        // Telemetry frequency (Seconds): Set how often to send telemetry from each device
        private int telemetryInterval; 

        // List of Devices for simulation (with same prefix)
        private List<DeviceEntity> listOfDevices;

        // Switch on/off simulation ongoing
        private bool isSimulationInProgress;


        public SimulationManager(string iotHubConnectionString, string prefixDevice, int numberOfDevices, int telemetryInterval)
        {
            this.iotHubConnectionString = iotHubConnectionString;
            this.prefixDevice = prefixDevice;
            this.numberOfDevices = numberOfDevices;
            this.telemetryInterval = telemetryInterval; //  Seconds
            listOfDevices = new List<DeviceEntity>();
        }

        public void StartCompleteSimulation()
        {
            // Add devices to your IoT hub's identity registry
            AddDevices_Async().Wait();

            // Reads devices to connect for simulation
            GetDevices_Async().Wait();

            // Launch the simulation
            StartSimulation();
        }

        private async Task AddDevices_Async()
        {
            // connect to IoT Hub device identity registry
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            for (int i = 0; i < numberOfDevices; i++)
            {
                try
                {
                    string deviceID = prefixDevice + i;
                    await registryManager.AddDeviceAsync(new Device(deviceID));
                    
                    // Output for console
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Device added {deviceID}");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private async Task<List<DeviceEntity>> GetDevices_Async()
        {
            try
            {
                listOfDevices = new List<DeviceEntity>();

                RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
                IQuery query = registryManager.CreateQuery("select * from devices", 100); ;

                while (query.HasMoreResults)
                {
                    IEnumerable<Twin> page = await query.GetNextAsTwinAsync();
                    foreach (Twin twin in page)
                    {
                        // Add the device for simulation only if the prefix is the same 
                        if (twin.DeviceId.StartsWith(prefixDevice))
                        {
                            // get a device objet from registry
                            Device device = await registryManager.GetDeviceAsync(twin.DeviceId);

                            // set member's deviceEntity objet with device
                            DeviceEntity deviceEntity = new DeviceEntity()
                            {
                                Id = twin.DeviceId,
                                ConnectionString = CreateDeviceConnectionString(device),
                            };
                            
                            deviceEntity.PrimaryKey = device.Authentication?.SymmetricKey?.PrimaryKey;
                            deviceEntity.SecondaryKey = device.Authentication?.SymmetricKey?.SecondaryKey;

                            deviceEntity.PrimaryThumbPrint = twin.X509Thumbprint?.PrimaryThumbprint;
                            deviceEntity.SecondaryThumbPrint = twin.X509Thumbprint?.SecondaryThumbprint;
                            
                            // add the device to the list
                            listOfDevices.Add(deviceEntity);

                            // diplay for test
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Device {deviceEntity.Id} got with registryManager.GetDeviceAsync");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listOfDevices;
        }

        private void StartSimulation()
        {
            // set true the switch for launch simulation
            isSimulationInProgress = true;

            foreach (DeviceEntity device in listOfDevices)
            {
                try
                {
                    // Connect to the IoT hub using the device's ConnectionString MQTT protocol
                    DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(device.ConnectionString, 
                                                                                    Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                    
                    // send a message to cloud 
                    SendD2CMessages_Async(deviceClient, device.Id);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                }
            }

        }

        private String CreateDeviceConnectionString(Device device)
        {
            StringBuilder deviceConnectionString = new StringBuilder();

            var hostName = String.Empty;
            var tokenArray = iotHubConnectionString.Split(';');
            for (int i = 0; i < tokenArray.Length; i++)
            {
                var keyValueArray = tokenArray[i].Split('=');
                if (keyValueArray[0] == "HostName")
                {
                    hostName =  tokenArray[i] + ';';
                    break;
                }
            }

            if (!String.IsNullOrWhiteSpace(hostName))
            {
                deviceConnectionString.Append(hostName);
                deviceConnectionString.AppendFormat("DeviceId={0}", device.Id);

                if (device.Authentication != null)
                {
                    if ((device.Authentication.SymmetricKey != null) && (device.Authentication.SymmetricKey.PrimaryKey != null))
                    {
                        deviceConnectionString.AppendFormat(";SharedAccessKey={0}", device.Authentication.SymmetricKey.PrimaryKey);
                    }
                    else
                    {
                        deviceConnectionString.AppendFormat(";x509=true");
                    }
                }

            }
            
            return deviceConnectionString.ToString();
        }

        private async void SendD2CMessages_Async(DeviceClient deviceClient, string deviceId)
        {

            while (isSimulationInProgress)
            {
                // generate a random telemetry data
                TelemetryData telemetryDataObject = new TelemetryData(1, deviceId);

                // serialize telemetry data object to JSON string
                var telemetryDataString = JsonConvert.SerializeObject(telemetryDataObject);

                // set the body of the message to the serialized value of the telemetry data
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(telemetryDataString));

                // add new Property "level", and his value, to message 
                message.Properties.Add("level", telemetryDataObject.level);

                // send message device-to-cloud
                await deviceClient.SendEventAsync(message);

                // display for test
                Console.ResetColor();
                Console.WriteLine("{0} > Sent message: {1}", DateTime.UtcNow, telemetryDataString);

                await Task.Delay(telemetryInterval * 1000);
            }

            // display for test
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0} : stop simulation", deviceId);

        }

        public void StopSimulation()
        {
            // set false for stop simulation
            isSimulationInProgress = false;
        }
  
        public async Task RemoveDevices_Async()
        {
            // connect to IoT Hub device identity registry
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            for (int i = 0; i < numberOfDevices; i++)
            {
                try
                {
                    string deviceID = prefixDevice + i;
                    await registryManager.RemoveDeviceAsync(deviceID);
                    
                    // Output for console
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Device removed {deviceID}");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public List<DeviceEntity> GetListOfDevices()
        {
            return listOfDevices;
        }


    }
}
