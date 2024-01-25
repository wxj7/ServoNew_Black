using Modbus;

using ServoNew_Black.Service;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;


namespace ServoNew_Black.ViewModel
{
    public class ServoNewVM : BaseViewModel
    {
        private byte Address = 0x01;
        public bool _isReading = false;
        public bool _isSetting = false; //设置地址和读取
        private System.Timers.Timer _speedTimer;
        
        private ServoNew ServoNew = ServoNew.Instance;
        public static readonly ServoNewVM _instance = new ServoNewVM();
        public static ServoNewVM Instance
        {
            get { return _instance; }
        }
        public ServoNewVM()//构造函数中调用方法，初始化串口相关界面和命令
        {
            GetCom();
            CommandInit();
        }
        public void CommandInit()
        {
            RefreshComCommand = new Command(new Action<object>((object param) =>
            {
                RefreshCom();
            }));
            OpenComCommand = new Command(new Action<object>((object param) =>
            {
                OpenCom();
            }));
            CloseComCommand = new Command(new Action<object>((object param) =>
            {
                CloseCom();
            }));
            StartCommand = new Command(new Action<object>((object param) =>
            {
                Start();
            }));
            StopCommand = new Command(new Action<object>((object param) =>
            {
                Stop();
            }));
            StartReadCommand = new Command(new Action<object>((object param) =>
            {
                StartRead();
            }));
            StopReadCommand = new Command(new Action<object>((object param) =>
            {
                StopReading();
            }));
            

        }
        public Command RefreshComCommand { private set; get; }
        public Command OpenComCommand { private set; get; }
        public Command CloseComCommand { private set; get; }
        public Command StartCommand { private set; get; }
        public Command StopCommand{ private set; get; }   
        public Command StartReadCommand { private set; get; }
        public Command StopReadCommand { private set; get; }

        //使能按钮
        private bool _isOnEnable = false;
        public bool IsOnEnable
        {
            get => _isOnEnable;
            private set
            {
                _isOnEnable = value;
                OnPropertyChanged(nameof(IsOnEnable));
                OnPropertyChanged(nameof(IsOffEnable));
            }
        }
        public bool IsOffEnable
        {
            get
            {
                if (OpenComEnable == true) return false; //串口还没打开时返回false
                else return !_isOnEnable; //其他情况就和On取反
            }
        }

        //电机按钮
        private bool _isStartEnable;
        public bool IsStartEnable
        {
            get => _isStartEnable;
            private set
            {
                _isStartEnable = value;
                OnPropertyChanged(nameof(IsStartEnable));
                OnPropertyChanged(nameof(IsStopEnable));
            }
        }
        public bool IsStopEnable
        {
            get
            {
                if (OpenComEnable == true) //串口还没打开时返回false
                { return false; }
                return !_isStartEnable;
            }

        }

        private string _sp; // 声明 sp 变量
        public string Sp
        {
            get => _sp;
            set
            {
                _sp = value;
                OnPropertyChanged(nameof(Sp));
            }
        }
        //设置速度属性
        private int _speed = 100;
        public int Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged(nameof(Speed));
            }
        }

        //实际速度属性
        private float _nowspeed;
        public float NowSpeed
        {
            get => _nowspeed;
            private set
            {
                _nowspeed = value;
                OnPropertyChanged(nameof(NowSpeed));
            }
        }
        //电机使能状态
        private string _servoNewStatus;
        public string ServoNewStatus
        {
            get => _servoNewStatus;
            private set
            {
                _servoNewStatus = value;
                OnPropertyChanged(nameof(ServoNewStatus));
            }
        }

        //电机使能并设置为速度模式
        public void Start()
        {
            IsOnEnable = false;
            IsStartEnable = true;
            if (_isSetting) { return; }
            //使能指令
            byte[] StartReadCommand = BulidMessage.BuildMessage(Address, FunctionCode.WriteSingle, 0x0000, 0x0001);
            _isReading = true;
            byte[] resMessage;
            ServoNew.SendMessage(StartReadCommand);
            ServoNew.WaitForResponse(8, 1000, out resMessage);
            //速度模式指令
            byte[] StartReadCommand2 = BulidMessage.BuildMessage(Address, FunctionCode.WriteSingle, 0x0019, 0x0003);
            byte[] resMessage2;
            ServoNew.SendMessage(StartReadCommand2);
            ServoNew.WaitForResponse(8, 1000, out resMessage2);
            ServoNewStatus = "速度模式";
        }
        public void Stop()
        {
            StopReading();
            byte[] StartReadCommand = BulidMessage.BuildMessage(Address, FunctionCode.WriteSingle, 0x0000, 0x0000);
            byte[] resMessage;
            ServoNew.SendMessage(StartReadCommand);
            ServoNew.WaitForResponse(8, 1000, out resMessage);
            IsOnEnable = true;
            _isReading = false;
            ServoNewStatus = "使能关闭";
        }

        public void StartRead()
        {
           
            if (_isSetting) { return; }
            //电机设置速度指令
            // 将十进制速度转换为十六进制字符串
            string hexSpeed = Speed.ToString("X");

            // 这里的 hexSpeed 就是转换后的十六进制速度值，你可以将它用作需要的 sp 值
            Sp = hexSpeed;
            // 假设 sp 是十六进制字符串
            ushort spValue = ushort.Parse(Sp, System.Globalization.NumberStyles.HexNumber);

            byte[] StartReadCommand = BulidMessage.BuildMessage(Address, FunctionCode.WriteSingle, 0x0002, spValue);
            byte[] resMessage;
            ServoNew.SendMessage(StartReadCommand);
            ServoNew.WaitForResponse(8, 1000, out resMessage);
            

            // 设置定时器，每隔一定时间触发一次 Elapsed 事件
            _speedTimer = new System.Timers.Timer(300); // 这里的1000表示1秒，你可以根据需要调整时间间隔

            _speedTimer.Elapsed += OnTimerElapsed;
            _speedTimer.AutoReset = true;
            _speedTimer.Start();
        }


        //读取实时速度
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_isSetting) { return; }
            byte[] StartReadCommand = BulidMessage.BuildMessage(Address, FunctionCode.ReadHolding, 0x0010, 0x0001);

            byte[] resMessage;
            ServoNew.SendMessage(StartReadCommand);
            ServoNew.WaitForResponse(7, 1000, out resMessage);

            if (resMessage != null && resMessage.Length >= 7)
            {
                byte[] tempBytes = new byte[] { resMessage[3], resMessage[4] };
                Array.Reverse(tempBytes);
                short tempInt16 = BitConverter.ToInt16(tempBytes, 0);
                float tempFloat = tempInt16 / 10.0f;
                NowSpeed = (float)Math.Round(tempFloat, 1);
            }
        }


        // 停止定时器的方法
        public void StopReading()
        {
            _speedTimer?.Stop();
            byte[] StartReadCommand = BulidMessage.BuildMessage(Address, FunctionCode.WriteSingle, 0x0002, 0x0000);
            byte[] resMessage;
            ServoNew.SendMessage(StartReadCommand);
            ServoNew.WaitForResponse(8, 1000, out resMessage);

            IsStartEnable = true;
        }




        #region 串口相关界面
        //==========================================伺服电机串口界面======================================//
        ObservableCollection<string> _comNameCollection = new ObservableCollection<string>();
        public ObservableCollection<string> ComNameCollection
        {
            get { return _comNameCollection; }
            set
            {
                _comNameCollection = value;
                OnPropertyChanged(nameof(ComNameCollection));
            }
        }
        private string _selectedComServo;
        public string SelectedComServo
        {
            get { return _selectedComServo; }
            set
            {
                _selectedComServo = value;
                OnPropertyChanged(nameof(SelectedComServo));
            }
        }

 

        public bool _openComEnable = false;
        public bool CloseComEnable
        {
            get { return !_openComEnable; }
            set
            {
                _openComEnable = !value;
                OnPropertyChanged(nameof(OpenComEnable));
                OnPropertyChanged(nameof(CloseComEnable));
            }
        }
        public bool OpenComEnable
        {
            get { return _openComEnable; }
            set
            {
                _openComEnable = value;
                OnPropertyChanged(nameof(OpenComEnable));
                OnPropertyChanged(nameof(CloseComEnable));
            }
        }

        private void GetCom() //获得已连接的串口，并且传到下拉菜单
        {
            string[] portNames = SerialPort.GetPortNames();

            foreach (string portName in portNames)
            {
                ComNameCollection.Add(portName);//将串口名加入CoMNameColletion，并且判断是否为0，来执行串口可行
            }
            this.OpenComEnable = (this.ComNameCollection.Count != 0);
        }

        private void OpenCom()//打开串口
        {
            OpenComEnable = false;
            if (ServoNew.ServoNew_SerialPort == null)
            {
                OpenComEnable = true;
                return;
            }
            if (ServoNew.ServoNew_SerialPort.IsOpen)
            {
                ServoNew.Close();
                OpenComEnable = true;
            }
            else
            {
                if (this.ComNameCollection.Count == 0)
                {
                    MessageBox.Show("无可用COM口");
                    OpenComEnable = true;
                    return;
                }
                try
                {
                    ServoNew = new ServoNew(SelectedComServo, 19200);
                    OpenComEnable = false;
                    IsOnEnable = true;
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("打开串口失败\r\n" + ex.Message);
                    OpenComEnable = true;
                    return;
                }
            }
        }
        private void CloseCom()//关闭串口
        {
            if (ServoNew.ServoNew_SerialPort == null)
            {
                OpenComEnable = true;
                IsOnEnable = false;
                return;
            }
            if (ServoNew.ServoNew_SerialPort.IsOpen)
            {
               
                OpenComEnable = true;
                // 停止定时器
                StopReading();
                IsOnEnable = false;

                ServoNew.Close();
            }
        }

        private void RefreshCom()//刷新串口
        {
            OpenComEnable = false;
            if (ServoNew.ServoNew_SerialPort == null)
            {
                OpenComEnable = true;
                return;
            }
            if (ServoNew.ServoNew_SerialPort.IsOpen)
            {
                ServoNew.Close();
                OpenComEnable = true;
            }
            

            if (ComNameCollection.Count != 0)
            {
                this.ComNameCollection.Clear();
            }
            this.GetCom();
            this.OpenComEnable = (this.ComNameCollection.Count != 0);
           
        }
        #endregion
        //========================================串口界面===================================//
    }
}

namespace Modbus
{
    //从站地址：01（默认地址01）
    //功能码：04（读保持寄存器功能码）
    //寄存器起始地址：00 00（16位无符号整数，对应寄存器地址0）
    //寄存器数量：00 01（16位无符号整数，对应寄存器数量1）
    //CRC校验：31 CA（16位CRC校验值）

    //从站地址：01（默认地址01）
    //功能码：04（读保持寄存器功能码）
    //字节数：02（表示后续数据的字节数）
    //数据：08 2A（16位无符号整数，对应氧气浓度的数值）
    //CRC校验：3F 2F（16位CRC校验值）
    public class FunctionCode //定义了Modbus通信中常用的功能码
    {
        public static byte ReadHolding = 0x03;
        public static byte ReadInput = 0x04;
        public static byte WriteCoil = 0x05;//写单个开关，通常用于写入单个线圈的状态
        public static byte WriteSingle = 0x06;
        public static byte WriteMultiple = 0x10;
    }
    public class RegistersAddress //定义了一些常用的寄存器地址
    {
        public const ushort P_AbsolutePos = 0x1507;//泵绝对位置
        public const ushort P_ElectAngle = 0x1509;//泵电气角度
        public const ushort P_Unknow = 0x1516;//泵手册没写

        public const ushort P_AccTime = 0x030E;//泵加速时间
        public const ushort P_DecTime = 0x030F;//泵减速时间
        public const ushort P_Operation = 0x1408;//泵命令写入（设置速度和正反转）

        public const ushort A_EnergyEnable = 0x3201;//使能
        public const ushort A_NowSpeed = 0x0B00;//电机转速
        public const ushort A_ClockRotation = 0x0309;//伺服搅拌器正转-顺时针
        public const ushort A_AntiClockRotation = 0x030B;//伺服搅拌器反转-逆时针
        public const ushort A_SetSpeed = 0x0604;//设置速度

    }
    public class RegisterCount //定义了一些常用的寄存器数量
    {
        public const ushort One = 0x0001;//读16位
        public const ushort Two = 0x0002;//读32位（读两个寄存器）
    }

    public class SpecialData //特殊数据的寄存器地址
    {
        public const ushort P_Reverse = 0x1103;//泵命令写入-反转
        public const ushort P_Stop = 0x1104;//泵命令写入-停止

    }

    public class CalcData
    {
        /// <summary>
        /// 进行9位modbus报文的32位数据计算并返回值
        /// </summary>
        /// <param name="resMessage">接收的9位报文</param>
        /// /// <returns>报文中数据转化出的32位int值</returns>
        public static int GetInt32(byte[] resMessage)
        {
            int lengthToConvert = 4; // 要转化的32位数据的字节长度
            byte[] dataToConvert = new byte[lengthToConvert];

            // 复制要转化的字节到新数组
            Array.Copy(resMessage, 3, dataToConvert, 0, lengthToConvert);
            // 将前两个字节组合成一个 16 位整数
            ushort register0 = BitConverter.ToUInt16(dataToConvert, 0);

            // 将后两个字节组合成一个 16 位整数
            ushort register1 = BitConverter.ToUInt16(dataToConvert, 2);

            // 将两个 16 位整数组合成一个 32 位整数,调换0,1可实现高低位变换
            int Value = (int)((uint)register1 << 16 | register0);
            return Value;
        }

        /// <summary>
        /// 进行7位Modbus报文的16位数据计算并返回值，修复高低位顺序
        /// </summary>
        /// <param name="resMessage">接受的7位报文</param>
        /// <returns>报文中数据转化出的16位int值</returns>
        public static int GetInt16(byte[] resMessage)
        {
            int lengthToConvert = 2; // 要转化的16位数据的字节长度
            byte[] dataToConvert = new byte[lengthToConvert];

            // 复制要转化的字节到新数组
            Array.Copy(resMessage, 3, dataToConvert, 0, lengthToConvert);

            // 手动调换高低位顺序
            byte temp = dataToConvert[0];
            dataToConvert[0] = dataToConvert[1];
            dataToConvert[1] = temp;

            int Value = BitConverter.ToInt16(dataToConvert, 0);
            return Value;
        }
    }

    public static class BulidMessage
    {
        /// <summary>
        /// 拼接03(读单个或多个寄存器)和06(写单个寄存器)的报文
        /// </summary>
        /// <param name="slaveAddress">从机地址</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="registerAddress">读取/写入的寄存器地址</param>
        /// <param name="registerCount">读取寄存器地址个数/写入寄存器的数据</param>
        /// <returns>报文</returns>
        public static byte[] BuildMessage(byte slaveAddress, byte functionCode, ushort registerAddress, ushort registerCount)
        {
            // 将寄存器地址和寄存器数量拆分成高位和低位字节
            byte registerAddressHigh = (byte)(registerAddress >> 8);
            byte registerAddressLow = (byte)(registerAddress & 0xFF);
            byte registerCountHigh = (byte)(registerCount >> 8);
            byte registerCountLow = (byte)(registerCount & 0xFF);

            //CRC校验
            (byte high, byte low) crcResult = CRC16.CRCCalc(new byte[] { slaveAddress, functionCode, registerAddressHigh, registerAddressLow, registerCountHigh, registerCountLow });
            // 构建完整的Modbus RTU报文
            byte[] modbusMessage = new byte[]
            {
                 slaveAddress,
                 functionCode,
                 registerAddressHigh,registerAddressLow,//寄存器地址
                 registerCountHigh,registerCountLow,//03时为读取信息的位数，06时为写入寄存器的数据
                 crcResult.high,crcResult.low,
             };
            return modbusMessage;
        }

        //例：01 10 06 04 00 01 02 00 96 41 BA 数据为0x0096
        /// <summary>
        /// 拼接10功能码(写入多个寄存器)报文
        /// </summary>
        /// <param name="slaveAddress">从机地址</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="registerStartAddress">写入寄存器起始地址</param>
        /// <param name="registerCount">写入寄存器地址个数</param>
        /// <param name="registerData">写入寄存器的数据(字节数组)</param>
        /// <returns>报文</returns>
        public static byte[] BuildMessage(byte slaveAddress, byte functionCode, ushort registerStartAddress, ushort registerCount, byte[] registerData)
        {
            // 将寄存器地址和寄存器数量拆分成高位和低位字节
            byte registerAddressHigh = (byte)(registerStartAddress >> 8);
            byte registerAddressLow = (byte)(registerStartAddress & 0xFF);
            byte registerCountHigh = (byte)(registerCount >> 8);
            byte registerCountLow = (byte)(registerCount & 0xFF);

            byte[] inputBytes = new byte[]
            {
                slaveAddress,
                functionCode,
                registerAddressHigh, registerAddressLow,
                registerCountHigh, registerCountLow,
                (byte)registerData.Length
            };
            byte[] subMessage = inputBytes.Concat(registerData).ToArray();//先是合成一个序列，然后再转成字节数组
            //CRC校验
            (byte high, byte low) crcResult = CRC16.CRCCalc(subMessage);

            byte[] modbusMessage = subMessage.Concat(new byte[] { crcResult.high, crcResult.low }).ToArray();

            return modbusMessage;
        }
    }
}
