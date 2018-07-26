using System;

namespace ConsoleSimulation
{
    class TelemetryData
    {
        public int protocolVersion { get; set; }
        public DateTime sendingDateTime { get; set; }
        public string deviceId { get; set; }
        public string messageData { get; set; }
        public double temperature { get; set; }
        public double humidity { get; set; }
        public string pointInfo { get; set; }

    }
}
