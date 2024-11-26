using MicroBeast.I2C;
using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MicroBeast.PIO;

namespace MicroBeast
{
    public class Z84C20
    {
        private PIO mPortA;
        private PIO mPortB;
        private IProcessor mProcessor;

        public Z84C20(I2cBus i2cBus, IProcessor processor)
        {
            mProcessor = processor;
            mPortA = new PIO(i2cBus);
            mPortA.OnInterrupt += MPortA_OnInterrupt;
            mPortA.Reset();
            mPortB = new PIO(i2cBus, PullupLineType.Pullup, PullupLineType.Pullup);
            mPortB.OnInterrupt += MPortB_OnInterrupt;
            mPortB.Reset();
        }

        private void MPortB_OnInterrupt(byte vector)
        {
            mProcessor.FireInterupt("Interrupt:"+vector.ToString("X2"));
        }

        private void MPortA_OnInterrupt(byte vector)
        {
        }

        public PIO PortA {  get { return mPortA; } }
        public PIO PortB { get { return mPortB; } }

        public byte PORTB_CTRL
        {
            get { return mPortB.Control; }
            set { mPortB.Control = value; }
        }
        public byte PORTB_DATA
        {
            get { return mPortB.Data; }
            set { mPortB.Data = value; }
        }

        public byte PORTA_CTRL
        {
            get { return mPortA.Control; }
            set { mPortA.Control = value; }
        }
        public byte PORTA_DATA
        {
            get { return mPortA.Data; }
            set { mPortA.Data = value; }
        }
    }// class Z84C20
}