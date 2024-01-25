using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServoNew_Black.Service
{
    public class CRC16
    {

        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="data">校验数据</param>
        /// <returns>高低8位</returns>
        public static (byte high, byte low) CRCCalc(byte[] data)
        {
            byte[] crcbuf = data;
            //计算并填写CRC校验码
            int crc = 0xffff;
            int len = crcbuf.Length;
            for (int n = 0; n < len; n++)
            {
                byte i;
                crc = crc ^ crcbuf[n];
                for (i = 0; i < 8; i++)
                {
                    int TT;
                    TT = crc & 1;
                    crc = crc >> 1;
                    crc = crc & 0x7fff;
                    if (TT == 1)
                    {
                        crc = crc ^ 0xa001;
                    }
                    crc = crc & 0xffff;
                }

            }
            byte[] redata = new byte[2];
            redata[1] = (byte)((crc >> 8) & 0xff);
            redata[0] = (byte)((crc & 0xff));
            return (redata[0], redata[1]);
        }

    }
}
