using Processors;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Policy;
using System.Xml.Linq;
using static MicroBeast.PIO;
using static System.Windows.Forms.AxHost;

namespace MicroBeast.I2C
{
    public class I2cBus
    {
        private List<I2CDevice> mDevices = new List<I2CDevice>();

        // 0 - output, 1 - input
        //Host side scl and sda line
        public bool HostScl;
        public bool HostSda;

        //Device side scl and sda line
        public LineLevel DeviceScl;
        public LineLevel DeviceSda;

        private IProcessor mIProcessor;
        public I2cBus(IProcessor processor)
        {
            mIProcessor = processor;
        }

        public LineLevel FindSDALevel()
        {
            foreach (var dev in mDevices)
            {
                var level = dev.FindSDALevel();
                if(level != LineLevel.Float)
                    return level;
            }
            return LineLevel.Float;
        }

        public void FireInterrupt()
        {
            mIProcessor.FireInterupt(":02");
        }

        public void onTick()
        {
            foreach (var dev in mDevices)
                dev.onTick(HostScl, HostSda);
        }

        public I2CDevice ActiveDevice()
        {
            foreach (var dev in mDevices)
                if (dev.IsActive)
                    return dev;
            return null;
        }

        public I2CDevice FindDevice(byte address)
        {
            foreach (var dev in mDevices)
                if(dev.CompareAddress(address) != null)
                    return dev;
            return null;
        }

        public void AddDevice(I2CDevice device)
        {
            mDevices.Add(device);
        }
    }//class I2cBus
}