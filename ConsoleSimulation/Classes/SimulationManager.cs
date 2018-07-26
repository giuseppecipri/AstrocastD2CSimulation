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

        // RegistryManager of current IoT Hub
        private RegistryManager registryManager;

        // Telemetry frequency (Seconds): Set how often to send telemetry from each device
        private int telemetryInterval; 

        // List of Devices for simulation (with same prefix)
        private List<DeviceEntity> listOfDevices;

        private bool isSimulationStart;


        public SimulationManager(string iotHubConnectionString, string prefixDevice, int numberOfDevices, int telemetryInterval)
        {
            this.iotHubConnectionString = iotHubConnectionString;
            this.numberOfDevices = numberOfDevices;
            this.prefixDevice = prefixDevice;
            this.listOfDevices = new List<DeviceEntity>();
            this.registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            this.telemetryInterval = telemetryInterval; //  Seconds
        }
        public List<DeviceEntity> getListOfDevices()
        {
            return listOfDevices;
        }

        public async Task<List<DeviceEntity>> GetDevices()
        {
            try
            {
                listOfDevices = new List<DeviceEntity>();
                DeviceEntity deviceEntity;
                IQuery query = registryManager.CreateQuery("select * from devices", null); ;

                while (query.HasMoreResults)
                {
                    IEnumerable<Twin> page = await query.GetNextAsTwinAsync();
                    foreach (Twin twin in page)
                    {
                        // Add the device for simulation only if the prefix is the same 
                        if (twin.DeviceId.StartsWith(prefixDevice))
                        {
                            Device device = await registryManager.GetDeviceAsync(twin.DeviceId);
                            deviceEntity = new DeviceEntity()
                            {
                                Id = twin.DeviceId,
                                ConnectionState = twin.ConnectionState.ToString(),
                                LastActivityTime = twin.LastActivityTime,
                                LastStateUpdatedTime = twin.StatusUpdatedTime,
                                MessageCount = twin.CloudToDeviceMessageCount,
                                State = twin.Status.ToString(),
                                SuspensionReason = twin.StatusReason,

                                ConnectionString = CreateDeviceConnectionString(device),
                                LastConnectionStateUpdatedTime = device.ConnectionStateUpdatedTime
                            };

                            deviceEntity.PrimaryThumbPrint = twin.X509Thumbprint?.PrimaryThumbprint;
                            deviceEntity.SecondaryThumbPrint = twin.X509Thumbprint?.SecondaryThumbprint;

                            deviceEntity.PrimaryKey = device.Authentication?.SymmetricKey?.PrimaryKey;
                            deviceEntity.SecondaryKey = device.Authentication?.SymmetricKey?.SecondaryKey;

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Device {deviceEntity.Id} got with registryManager.GetDeviceAsync");
                            listOfDevices.Add(deviceEntity);
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

       public void StartSimulation()
        {
            // set true for launch simulation
            isSimulationStart = true;

            foreach (DeviceEntity device in listOfDevices)
            {
                try
                {
  //                  Console.ResetColor();
 //                   Console.WriteLine(device.Id);
                    // Connect to the IoT hub using the MQTT protocol
                    DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(device.ConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                    SendDeviceToCloudMessagesAsync(deviceClient, device.Id);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                }
            }

        }

        public void StopSimulation()
        {
            // set false for stop simulation
            isSimulationStart = false;
        }

            private async void SendDeviceToCloudMessagesAsync(DeviceClient deviceClient, string deviceId)
        {

            while (isSimulationStart)
            {
                /*
                var telemetryDataPoint = new
                {
                    protocolVersion = 1,
                    sendingDateTime = DateTime.Now.ToUniversalTime(),
                    deviceId = deviceId,
                    messageData = Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes(infoString)),
                    pointInfo = infoString,
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
                */
                TelemetryData telemetryDataObject = new TelemetryData(1, deviceId);


                var telemetryDataString = JsonConvert.SerializeObject(telemetryDataObject);

                //set the body of the message to the serialized value of the telemetry data
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(telemetryDataString));
                message.Properties.Add("level", telemetryDataObject.level);

                await deviceClient.SendEventAsync(message);
                Console.ResetColor();
                Console.WriteLine("{0} > Sent message: {1}", DateTime.UtcNow, telemetryDataString);

                await Task.Delay(telemetryInterval * 1000);
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0} : stop simulation", deviceId);

        }
        public async Task AddDevices()
        {
            string deviceID = null;
            registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            for (int i = 0; i < numberOfDevices; i++)
            {
                try
                {
                    deviceID = prefixDevice + i;
                    await registryManager.AddDeviceAsync(new Device(deviceID));
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
        public async Task RemoveDevices()
        {
            string deviceID = null;
            registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            for (int i = 0; i < numberOfDevices; i++)
            {
                try
                {
                    deviceID = prefixDevice + i;
                    await registryManager.RemoveDeviceAsync(deviceID);
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

    }
}
