namespace ConsoleSimulation
{

    using System;

    public class DeviceEntity 
    {
        public string Id { get; set; }
        public string ConnectionString { get; set; }
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
        public string PrimaryThumbPrint { get; set; }
        public string SecondaryThumbPrint { get; set; }

        public override string ToString()
        {
            return$"Device ID = {Id}, Primary Key = {PrimaryKey}, Secondary Key = {SecondaryKey}, Primary Thumbprint = {PrimaryThumbPrint}, Secondary Thumbprint = {SecondaryThumbPrint}, ConnectionString = {ConnectionString}\r\n";
        }
    }
}