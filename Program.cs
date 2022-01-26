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
    //create the class
    class Soil
    {
        public double temperature { get; set; }

        public double moisture { get; set; }

        public double EC { get; set; }
    }
    class Program
    {
        //set the modbus client
        public static ModbusClient client = new ModbusClient();
        //set the aas client
        public static AasClient aasClient = new AasClient("https://testaas.aasproto.xyz", "ABCD1234");
        //set the ip address from the edge link device connected
        public static string IP = "192.168.0.100";
        //port number default is define to 502
        public static int Port = 502;
        public static bool LogIn = true;
        //after the asset created, get the asset Id and store to the variable 
        public static string assetId = "c891efd5465c4480b2f8c8e9051b8eed";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Connect to PLC....");

            //this three modules need to configured first
            //done the method created by self-define
            client.ConnectedChanged += ClientConnectedChanged;
            client.ReceiveDataChanged += ClientReceivedDataChanged;
            client.SendDataChanged += ClientSendDataChanged;
            //connect to your modbus tcp
            client.Connect(IP, Port);

            //custom create asset using the asset api
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
            //get data every one minute
            while (true)
            { 
                //initialize the instance for soil
                Soil soil = new Soil();
                Console.WriteLine("Press anykey to continue...");
                Console.ReadLine();
                //read the temperature follow your modbus tcp address defined (40001 , here will be 0:cuz array start from 0)
                //and then you take the quantity which is 4 as current data type is 64 bit for double which occupied 4 address space
                var temperatures = client.ReadHoldingRegisters(0, 4);
                //transfer the data from bytes into double with leedian using high low
                var formattedTemperature = DataConvert.RegistersToDouble(temperatures, DataConvert.RegisterOrder.HighLow);
                //assigned the temperature value to your instance
                soil.temperature = formattedTemperature;
                //same as above
                var moistures = client.ReadHoldingRegisters(4, 4);
                var formattedMoisture = DataConvert.RegistersToDouble(moistures, DataConvert.RegisterOrder.HighLow);
                soil.moisture = formattedMoisture;
                //same as above
                var ec = client.ReadHoldingRegisters(8, 4);
                var formattedEC = DataConvert.RegistersToDouble(ec, DataConvert.RegisterOrder.HighLow);
                soil.EC = formattedEC;
                //here is just display to make you more clear about the data you get
                Console.WriteLine("------------------------------");
                Console.WriteLine("Result detect from sensor\n");
                Console.WriteLine($"Temperature:{soil.temperature.ToString("0.00")}'C Moisture:{soil.moisture.ToString("0.00")}% EC:{soil.EC.ToString("0.00")}mS/cm\n");
                Console.WriteLine("------------------------------");

                //add your instance to the octane asset
                await AddInstance(soil);

                //sleep for one minute for every minute update
                Thread.Sleep(60000);
            }

        }

        public static async Task AddInstance(Soil soil)
        {
            //add the instance to your asset
            await aasClient.InstanceApi.AddInstanceData(assetId, soil);

            //get all your instance to verify you got the instance
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
