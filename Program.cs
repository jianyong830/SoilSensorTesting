using AAS.Client;
using AAS.Common.AssetModel;
using AAS.Common.Parameters.MetadataParams;
using EasyModbus.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace SoilSensorTesting
{
    class Soil
    {
        public double temperature { get; set; }

        public double moisture { get; set; }

        public double EC { get; set; }
    }
    class Program
    {

        public static ModbusClient client = new ModbusClient();
        public static AasClient aasClient = new AasClient("https://testaas.aasproto.xyz", "ABCD1234");
        public static string IP = "192.168.0.100";
        public static int Port = 502;
        public static bool LogIn = true;
        public static string assetId = "c891efd5465c4480b2f8c8e9051b8eed";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Connect to PLC....");

            client.ConnectedChanged += ClientConnectedChanged;
            client.ReceiveDataChanged += ClientReceivedDataChanged;
            client.SendDataChanged += ClientSendDataChanged;
            client.Connect(IP, Port);

            //var asset = await aasClient.AssetApi.AddAsset(new AddAssetParam()
            //{
            //    Name = "TestingSoilSensor",
            //    Description = "Sample Testing Soil Sensor",
            //    Properties = new List<Property>()
            //    {
            //        new Property()
            //        {
            //            Name = "Temperature",
            //            Description = "Temperature of soil",
            //            PropertyType = PropertyType.Number

            //        },
            //        new Property()
            //        {
            //            Name = "Moisture",
            //            Description = "Moisture of soil",
            //            PropertyType = PropertyType.Number
            //        },
            //        new Property()
            //        {
            //            Name = "EC",
            //            Description = "EC of soil",
            //            PropertyType = PropertyType.Number
            //        }
            //    }


            //});

            await ReadData();
        }

        public static async Task ReadData()
        {
            while (true)
            {
                Soil soil = new Soil();
                Console.WriteLine("Press anykey to continue...");
                Console.ReadLine();
                var temperatures = client.ReadHoldingRegisters(0, 4);
                var formattedTemperature = DataConvert.RegistersToDouble(temperatures, DataConvert.RegisterOrder.HighLow);
                soil.temperature = formattedTemperature;
                var moistures = client.ReadHoldingRegisters(4, 4);
                var formattedMoisture = DataConvert.RegistersToDouble(moistures, DataConvert.RegisterOrder.HighLow);
                soil.moisture = formattedMoisture;
                var ec = client.ReadHoldingRegisters(8, 4);
                var formattedEC = DataConvert.RegistersToDouble(ec, DataConvert.RegisterOrder.HighLow);
                soil.EC = formattedEC;
                Console.WriteLine("------------------------------");
                Console.WriteLine("Result detect from sensor\n");
                Console.WriteLine($"Temperature:{soil.temperature.ToString("0.00")}'C Moisture:{soil.moisture.ToString("0.00")}% EC:{soil.EC.ToString("0.00")}mS/cm\n");
                Console.WriteLine("------------------------------");

                //client.WriteMultipleRegisters()
                //await AddInstance(soil);

                Thread.Sleep(60000);
            }

        }

        public static async Task AddInstance(Soil soil)
        {
            await aasClient.InstanceApi.AddInstanceData(assetId, soil);

            var allInstance = await aasClient.InstanceApi.GetAllInstances<Soil>(assetId);
            Console.WriteLine("Success retrieve all instance data");
            Console.WriteLine("--------------------------------------");
            foreach (var a in allInstance)
            {
                Console.WriteLine($"Temperature: {a.Data.temperature.ToString("0.00")}'C");
                Console.WriteLine($"Moisture: {a.Data.moisture.ToString("0.00")} %");
                Console.WriteLine($"EC: {a.Data.EC.ToString("0.00")} mS/cm");
                Console.WriteLine("\n");
            }

        }

        private static void ClientSendDataChanged(object sender)
        {
            if (LogIn) Console.WriteLine("Success send data");
        }

        private static void ClientReceivedDataChanged(object sender)
        {
            if (LogIn) Console.WriteLine("Success received data");
        }

        private static void ClientConnectedChanged(object sender)
        {
            var c = (ModbusClient)sender;
            if (LogIn) Console.WriteLine($"Success connect data.{c.Connected}.{c.IPAddress}");
        }
    }
}
