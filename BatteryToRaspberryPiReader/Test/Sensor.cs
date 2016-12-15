using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Devices.SerialCommunication;


namespace Sensor
{
    
    class ADS1115
    {
        private byte I2C_ADDR = 0x48;
        private byte CONFIG_POINTER = 0x01;
        private byte CONVERT_POINTER = 0x00;
        private byte CONVERT_DELAY = 8;

        private ushort CONFIG_CQUE_NONE = 0x0003;
        private ushort CONFIG_CLAT_NONLAT = 0x0000;
        private ushort CONFIG_CPOL_ACTVLOW = 0x0000;
        private ushort CONFIG_CMODE_TRAD = 0x0000;
        private ushort CONFIG_DR_1600SPS = 0x0080;
        private ushort CONFIG_MODE_SINGLE = 0x0100;
        private ushort CONFIG_PGA_6_144V = 0x0000;
        private ushort CONFIG_MUX_SINGLE_0 = 0x4000;
        private ushort CONFIG_MUX_SINGLE_1 = 0x5000;
        private ushort CONFIG_MUX_SINGLE_2 = 0x6000;
        private ushort CONFIG_MUX_SINGLE_3 = 0x7000;
        private ushort CONFIG_OS_SINGLE = 0x8000;

        private I2cDevice ads1115;

        public async Task<Boolean> Setup()
        {
            ads1115 = await InitializeI2CAsync();

            if(ads1115 != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<I2cDevice> InitializeI2CAsync()
        {
            string aqs = I2cDevice.GetDeviceSelector("I2C1");
            DeviceInformationCollection device_information_collection = await DeviceInformation.FindAllAsync(aqs);
            string deviceId = device_information_collection[0].Id;

            I2cConnectionSettings connection = new I2cConnectionSettings(I2C_ADDR);
            connection.BusSpeed = I2cBusSpeed.FastMode;
            connection.SharingMode = I2cSharingMode.Shared;

            return await I2cDevice.FromIdAsync(deviceId, connection);
        }

        public async Task<double> getReading(int channel)
        {
            short raw_data = 0;
            byte[] i2c_data = new byte[2];

            ushort config = (ushort)(CONFIG_CQUE_NONE | // Disable the comparator (default val)
                  CONFIG_CLAT_NONLAT | // Non-latching (default val)
                  CONFIG_CPOL_ACTVLOW | // Alert/Rdy active low   (default val)
                  CONFIG_CMODE_TRAD | // Traditional comparator (default val)
                  CONFIG_DR_1600SPS | // 1600 samples per second (default)
                  CONFIG_MODE_SINGLE |   // Single-shot mode (default)
                  CONFIG_PGA_6_144V | // Setting ref voltage
                  CONFIG_OS_SINGLE); // Single reading

            switch (channel)
            {
                case (0):
                    config |= CONFIG_MUX_SINGLE_0;
                    break;
                case (1):
                    config |= CONFIG_MUX_SINGLE_1;
                    break;
                case (2):
                    config |= CONFIG_MUX_SINGLE_2;
                    break;
                case (3):
                    config |= CONFIG_MUX_SINGLE_3;
                    break;
                default: break;
            }

            byte config_upper = (byte)(config >> 8);
            byte config_lower = (byte)(config & 0xFF);

            try
            {
                ads1115.Write(new byte[] { CONFIG_POINTER, config_upper, config_lower });
                await Task.Delay(CONVERT_DELAY);
                ads1115.WriteRead(new byte[] { CONVERT_POINTER }, i2c_data);
            }
            catch(Exception ex)
            {
                //Exception handler, maybe do something here
            }

            raw_data = (short)(i2c_data[0] << 8);
            raw_data |= (short)(i2c_data[1] & 0xFF);

            double min_v = (6.114 * 2) / Math.Pow(2, 16);

            return (raw_data * min_v);
        }

        public async Task<double> getReadingAvg(int channel, int n)
        {
            double voltage = await getReading(channel);

            for(int i = 1; i < n; i++)
            {
                voltage = (voltage + await getReading(channel)) / 2;
            }

            return voltage;
        }
    }

    
}
