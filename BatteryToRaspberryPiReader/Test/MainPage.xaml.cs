using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.Data.SqlClient;
using System;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string iothubUri = "IoTProjectCourse.azure-devices.net";
        string deviceId = "myraspberrypi";
        string deviceKey = "tDMzYSieVYKfpOlc9hkG9AMqF+eNgCNbxoD/j9WNmnw=";

        static DeviceClient deviceClient;

        int BATTERY_VOLTAGE_CHANNEL = 0;
        int BATTERY_CURRENT_CHANNEL = 1;

        public MainPage()
        {

            this.InitializeComponent();
            deviceClient = DeviceClient.Create(iothubUri,
                AuthenticationMethodFactory.
                CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey),
                TransportType.Http1);

            Display();

        }

        public async void Display()
        {

            var ads1115 = new Sensor.ADS1115();

            if (await ads1115.Setup())
            {
                while (true)
                {
                    try
                    {
                        SensorData sensorData = new SensorData();
                     
                        sensorData.Date = System.DateTime.Now;
                        sensorData.Voltage = (await ads1115.getReadingAvg(BATTERY_VOLTAGE_CHANNEL,10))*((213.0+117.0)/117.0);
                        sensorData.Current = (await ads1115.getReadingAvg(BATTERY_CURRENT_CHANNEL, 10)) / ((1 + (214.0 / 117.0)) * 0.1);
                        sensorData.Power = sensorData.Current * sensorData.Voltage;
                        //sensorData.Temperature = 0;

                        Voltage.Text = sensorData.Voltage.ToString();
                        Current.Text = sensorData.Current.ToString();

                        var str = sensorData.Date.ToString() + " " +
                            sensorData.Voltage.ToString() + " " +
                            sensorData.Current.ToString() + " " +
                            sensorData.Power.ToString(); // + " " +
                            //sensorData.Temperature.ToString();
                            
                        var messageString = JsonConvert.SerializeObject(sensorData);
                        var message = new Message(Encoding.ASCII.GetBytes(messageString));

                        await deviceClient.SendEventAsync(message);

                        await Task.Delay(10000);
                    }
                    catch(Exception ex)
                    {
                        Voltage.Text = ex.ToString();
                        break;
                    }
                }
            }
            else
            {
                Voltage.Text = "Error in Setup";
            }
        }

    }
}
