using System;
using System.Text;

namespace ConsoleSimulation
{
    class TelemetryData
    {
        public string deviceId { get; set; }
        public DateTime sendingDateTime { get; set; }
        public int protocolVersion { get; set; }
        public string messageData { get; set; }
        public string pointInfo { get; set; }
        public string level { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }

        // Empty constructor
        public TelemetryData() { }
        
        // Contructor with random values
        public TelemetryData(int protocolVersion, string deviceId)
        {
            this.protocolVersion = protocolVersion;
            this.deviceId = deviceId;

            // set random values for temperature, humidity, level and pointInfo
            SetRandomValues();

            // set body of message
            messageData = Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes(pointInfo));
            
            // set sending date-time in universal time format
            sendingDateTime = DateTime.UtcNow;
        }

        // set random values for temperature, humidity, level and pointInfo
        private void SetRandomValues()
        {
            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();

            temperature = minTemperature + rand.NextDouble() * 15;
            humidity = minHumidity + rand.NextDouble() * 20;

            if (rand.NextDouble() > 0.8)
            {
                if (rand.NextDouble() > 0.5)
                {
                    level = "critical_temperature";
                    pointInfo = "This is a critical temperature message.";
                }
                else
                {
                    level = "critical_humidity";
                    pointInfo = "This is a critical humidity message.";
                }
            }
            else
            {
                level = "normal";
                pointInfo = "This is a normal message.";
            }
        }
    }
}
