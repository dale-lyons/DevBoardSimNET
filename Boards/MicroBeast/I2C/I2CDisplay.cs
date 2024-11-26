using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MicroBeast.I2C
{
    public class I2CDisplay : I2CDevice
    {
        public delegate void onSetCharacterDisplay(byte col, ushort pattern);
        private byte mAddress;
        private onSetCharacterDisplay mCharacterDisplay;

        private const int MAX_BYTES = 3;
        private byte[] RecievedBytes;
        private int mBytesToRecieve;
        int count = 0;

        private enum LCD_STATE
        {
            None,
            Unlock,
            Brightness,
            Config,
            DisplayPage,
            DisplayCharacter,
        }
        private LCD_STATE mLCDState = LCD_STATE.None;

        public I2CDisplay(I2cBus i2cBus, byte address, onSetCharacterDisplay characterDisplay) : base(i2cBus)
        {
            mAddress = address;
            mCharacterDisplay = characterDisplay;
            RecievedBytes = new byte[MAX_BYTES];
        }

        public override I2CDevice CompareAddress(byte address)
        {
            if (address == mAddress)
                return this;
            mLCDState = LCD_STATE.None;
            return null;
        }

        public override void onReset()
        {
        }

        public override void onDataNotify(byte address, byte data, bool rwBit)// 1- read, 0 - write
        {
            switch (mLCDState)
            {
                case LCD_STATE.None:
                    if (data == 0xfd)
                    {
                        mLCDState = LCD_STATE.DisplayPage;
                        mBytesToRecieve = 1;
                        return;
                    }
                    else if (data == 0xfe)
                    {
                        mLCDState = LCD_STATE.Unlock;
                        mBytesToRecieve = 1;
                        return;
                    }
                    else
                    {
                        mLCDState = LCD_STATE.DisplayCharacter;
                        RecievedBytes[0] = data;
                        mBytesToRecieve = 2;
                        return;
                    }
                case LCD_STATE.Unlock:
                    mLCDState = LCD_STATE.None;
                    break;
                case LCD_STATE.DisplayPage:
                    if (data == 0x00)
                    {
                        mLCDState = LCD_STATE.None;
                    }
                    else if(data == 0x01)
                    {
                        mLCDState = LCD_STATE.Brightness;
                        mBytesToRecieve = 17;
                        count = 12;
                    }
                    else if (data == 0x03)
                    {
                        mBytesToRecieve = 3;
                        mLCDState = LCD_STATE.Config;
                    }
                    break;
                case LCD_STATE.DisplayCharacter:
                    {
                        int index = MAX_BYTES - mBytesToRecieve;
                        RecievedBytes[index] = data;
                        if (--mBytesToRecieve == 0)
                        {
                            mCharacterDisplay(RecievedBytes[0], BitConverter.ToUInt16(RecievedBytes, 1));
                            mLCDState = LCD_STATE.None;
                        }
                    }
                    break;
                case LCD_STATE.Brightness:
                    {
                        if (--mBytesToRecieve == 0)
                        {
                            mBytesToRecieve = 17;
                            if (--count == 0)
                                mLCDState = LCD_STATE.None;
                        }
                    }
                    break;
                case LCD_STATE.Config:
                    {
                        if (--mBytesToRecieve == 0)
                            mLCDState = LCD_STATE.None;
                    }
                    break;
            }
        }//onDataNotify
    }//class I2CDisplay
}