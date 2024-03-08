using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServoNew_Black.Service
{
    internal class ServoNew
    {
        public SerialPort ServoNew_SerialPort;//用于串口通信的对象
        private static readonly ServoNew _instance = new ServoNew();
        public static ServoNew Instance
        {
            get { return _instance; }
        }
        public ServoNew() //构造函数
        {
            ServoNew_SerialPort = new SerialPort();
        }
        public ServoNew(string portName, int baudRate)//重载构造函数
        {
            // 初始化串口通信，接受串口名称和波特率作为参数，并打开串口
            ServoNew_SerialPort = new SerialPort(portName, baudRate);
            ServoNew_SerialPort.Open();
        }

        /// <summary>
        /// 等待串口返回数据，并比较长度
        /// </summary>
        /// <param name="ResLength">串口接收字节数组的长度</param>
        /// <param name="timeoutms">超时时间(ms)</param>
        /// <param name="receivedData">接收到的信息</param>
        /// <returns>true-接收到了足够信息，false-失败</returns>
        public bool WaitForResponse(int ResLength, int timeoutms, out byte[] receivedData)
        {

            try
            {
                ServoNew_SerialPort.ReadTimeout = timeoutms;
                byte[] receivedDatas = new byte[ResLength];
                int bytesCount = 0;

                while (bytesCount < ResLength)
                {
                    try
                    {
                        int bytesRead = ServoNew_SerialPort.Read(receivedDatas, bytesCount, ResLength - bytesCount);
                        bytesCount += bytesRead;
                    }
                    catch (TimeoutException)
                    {
                        receivedData = null;
                        return false;
                    }
                }
                receivedData = receivedDatas;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                receivedData = null;
                return false;
            }
        }



        public bool SendMessage(byte[] message)
        {
            try
            {
                // 发送消息到下位机
                ServoNew_SerialPort.Write(message, 0, message.Length);
                return true;
            }
            catch (Exception ex)
            {
                // 处理异常
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }
        public void Close()
        {
            // 关闭串口
            if (ServoNew_SerialPort.IsOpen)
            {
                ServoNew_SerialPort.Close();
            }
        }
    }
}
