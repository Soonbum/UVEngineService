using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using static UVEngine.Controllers.CyUSBSerial.CyUSBSerial;

namespace UVEngineService.Controllers;

public enum EngineType
{
    NQM,
    Anhua,
    Visitech
}

/// <summary>
/// 연결 요청 시 사용할 데이터 구조
/// </summary>
/// <param name="Type"></param>
/// <param name="Id"></param>
/// <param name="Ip"></param>
/// <param name="Port"></param>
public record ConnectionRequest(EngineType Type, int Id, string? Ip = null, int Port = 5000);

public interface ILightEngineController
{
    /// <summary>
    /// 장치를 연결하고, 연결 상태 활성화 상태로 변경
    /// </summary>
    /// <param name="index">정수 타입, 장치의 ID 값을 의미함</param>
    void Connect(int index);
    /// <summary>
    /// 장치 연결을 끊고, 연결 상태 비활성화 상태로 변경
    /// </summary>
    void Disconnect();

    /// <summary>
    /// GetTemperatureSensor 함수에 의해 수집된 특정 센서의 온도 값을 가져옴 (실제 현재 값과 다를 수 있음)
    /// </summary>
    /// <param name="sensor">정수 값, 엔진에 따라 다를 수 있으며 보통 LED 온도 혹은 PCB 온도 등이 있음</param>
    /// <returns>섭씨 온도 값</returns>
    double GetTemperatureSensorValue(int sensor);

    /// <summary>
    /// LED DAC 설정 요청 여부를 가져옴
    /// </summary>
    /// <returns>LED DAC 설정 요청을 했으면 true, 그 외에는 false</returns>
    bool GetSettingLEDDAC();
    /// <summary>
    /// LED DAC 변수에 저장된 값을 가져옴 (현재 엔진에 설정된 값과 다를 수 있음)
    /// </summary>
    /// <returns>현재 LED DAC 변수의 값</returns>
    int GetLEDDACValue();

    /// <summary>
    /// 연결 상태를 가져옴
    /// </summary>
    /// <returns>연결되어 있으면 true, 그 외에는 false</returns>
    bool GetDeviceConnected();
    /// <summary>
    /// 현재 LED On/Off 상태인지 가져옴
    /// </summary>
    /// <returns>On 상태이면 true, Off 상태이면 false</returns>
    bool GetDeviceLEDOn();
    /// <summary>
    /// 엔진 투영 뒤집기 요청 상태를 가져옴
    /// </summary>
    /// <returns>요청을 했으면 true, 아니면 false</returns>
    bool GetEngineFlipOn();

    /// <summary>
    /// LED DAC 설정 요청 여부를 설정함
    /// </summary>
    /// <param name="isSet">LED DAC 설정을 요청할 경우 true, 아니면 false</param>
    void SetSettingLEDDAC(bool isSet);
    /// <summary>
    /// LED DAC 변수에 값을 설정함 (현재 엔진에 설정된 값과 다를 수 있음)
    /// </summary>
    /// <param name="value">설정하고자 하는 LED DAC 값</param>
    void SetLEDDACValue(int value);
    /// <summary>
    /// 장비 연결 상태를 설정함
    /// </summary>
    /// <param name="isConnected">연결되어 있으면 true, 아니면 false</param>
    void SetDeviceConnected(bool isConnected);
    /// <summary>
    /// LED On/Off 상태 제어를 요청함
    /// </summary>
    /// <param name="isOn">LED On 요청시 true, LED Off 요청시 false</param>
    void SetDeviceLEDOn(bool isOn);
    /// <summary>
    /// 엔진 투영 뒤집기 설정을 요청함
    /// </summary>
    /// <param name="isOn">요청을 할 경우 true, 아니면 false</param>
    void SetEngineFlipOn(bool isOn);
    /// <summary>
    /// 엔진 투영 뒤집기 설정 값을 지정함
    /// </summary>
    /// <param name="isOn">뒤집을 경우 true, 뒤집지 않을 경우 false</param>
    /// <param name="isX">X 방향으로 뒤집을 경우 true, Y 방향으로 뒤집을 경우 false</param>
    void SetEngineFilpValue(bool isOn, bool isX);

    /// <summary>
    /// 연결할 때 설정한 엔진의 ID 값을 가져옴
    /// </summary>
    /// <returns>연결시 설정한 엔진 ID 값</returns>
    int GetEngineID();
    /// <summary>
    /// 통신 오류가 발생한 횟수를 가져옴 (I2C 통신 오류 기준), 단 연결이 재개되면 0으로 초기화됨
    /// </summary>
    /// <returns>통신이 끊어지고 나서부터 발생한 통신 오류 횟수</returns>
    int GetErrorCount();

    /// <summary>
    /// 백그라운드 스레드 작동
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken);
}

public class VirtualLightEngineController(ILightEngineController controller) : ILightEngineController
{
    private readonly ILightEngineController controller = controller;

    public ILightEngineController UnderlyingController => controller;

    /// <summary>
    /// 장치를 연결하고, 연결 상태 활성화 상태로 변경
    /// </summary>
    /// <param name="index">정수 타입, 장치의 ID 값을 의미함</param>
    public void Connect(int index) => controller.Connect(index);
    /// <summary>
    /// 장치 연결을 끊고, 연결 상태 비활성화 상태로 변경
    /// </summary>
    public void Disconnect() => controller.Disconnect();

    /// <summary>
    /// GetTemperatureSensor 함수에 의해 수집된 특정 센서의 온도 값을 가져옴 (실제 현재 값과 다를 수 있음)
    /// </summary>
    /// <param name="sensor">정수 값, 엔진에 따라 다를 수 있으며 보통 LED 온도 혹은 PCB 온도 등이 있음</param>
    /// <returns>섭씨 온도 값</returns>
    public double GetTemperatureSensorValue(int sensor) => controller.GetTemperatureSensorValue(sensor);

    /// <summary>
    /// LED DAC 설정 요청 여부를 가져옴
    /// </summary>
    /// <returns>LED DAC 설정 요청을 했으면 true, 그 외에는 false</returns>
    public bool GetSettingLEDDAC() => controller.GetSettingLEDDAC();
    /// <summary>
    /// LED DAC 변수에 저장된 값을 가져옴 (현재 엔진에 설정된 값과 다를 수 있음)
    /// </summary>
    /// <returns>현재 LED DAC 변수의 값</returns>
    public int GetLEDDACValue() => controller.GetLEDDACValue();

    /// <summary>
    /// 연결 상태를 가져옴
    /// </summary>
    /// <returns>연결되어 있으면 true, 그 외에는 false</returns>
    public bool GetDeviceConnected() => controller.GetDeviceConnected();
    /// <summary>
    /// 현재 LED On/Off 상태인지 가져옴
    /// </summary>
    /// <returns>On 상태이면 true, Off 상태이면 false</returns>
    public bool GetDeviceLEDOn() => controller.GetDeviceLEDOn();
    /// <summary>
    /// 엔진 투영 뒤집기 요청 상태를 가져옴
    /// </summary>
    /// <returns>요청을 했으면 true, 아니면 false</returns>
    public bool GetEngineFlipOn() => controller.GetEngineFlipOn();

    /// <summary>
    /// LED DAC 설정 요청 여부를 설정함
    /// </summary>
    /// <param name="isSet">LED DAC 설정을 요청할 경우 true, 아니면 false</param>
    public void SetSettingLEDDAC(bool isSet) => controller.SetSettingLEDDAC(isSet);
    /// <summary>
    /// LED DAC 변수에 값을 설정함 (현재 엔진에 설정된 값과 다를 수 있음)
    /// </summary>
    /// <param name="value">설정하고자 하는 LED DAC 값</param>
    public void SetLEDDACValue(int value) => controller.SetLEDDACValue(value);
    /// <summary>
    /// 장비 연결 상태를 설정함
    /// </summary>
    /// <param name="isConnected">연결되어 있으면 true, 아니면 false</param>
    public void SetDeviceConnected(bool isConnected) => controller.SetDeviceConnected(isConnected);
    /// <summary>
    /// LED On/Off 상태 제어를 요청함
    /// </summary>
    /// <param name="isOn">LED On 요청시 true, LED Off 요청시 false</param>
    public void SetDeviceLEDOn(bool isOn) => controller.SetDeviceLEDOn(isOn);
    /// <summary>
    /// 엔진 투영 뒤집기 설정을 요청함
    /// </summary>
    /// <param name="isOn">요청을 할 경우 true, 아니면 false</param>
    public void SetEngineFlipOn(bool isOn) => controller.SetEngineFlipOn(isOn);
    /// <summary>
    /// 엔진 투영 뒤집기 설정 값을 지정함
    /// </summary>
    /// <param name="isOn">뒤집을 경우 true, 뒤집지 않을 경우 false</param>
    /// <param name="isX">X 방향으로 뒤집을 경우 true, Y 방향으로 뒤집을 경우 false</param>
    public void SetEngineFilpValue(bool isOn, bool isX) => controller.SetEngineFilpValue(isOn, isX);

    /// <summary>
    /// 연결할 때 설정한 엔진의 ID 값을 가져옴
    /// </summary>
    /// <returns>연결시 설정한 엔진 ID 값</returns>
    public int GetEngineID() => controller.GetEngineID();
    /// <summary>
    /// 통신 오류가 발생한 횟수를 가져옴 (I2C 통신 오류 기준), 단 연결이 재개되면 0으로 초기화됨
    /// </summary>
    /// <returns>통신이 끊어지고 나서부터 발생한 통신 오류 횟수</returns>
    public int GetErrorCount() => controller.GetErrorCount();

    /// <summary>
    /// 백그라운드 스레드 작동
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken) => controller.StartAsync(cancellationToken);
}

/// <summary>
/// 테스트 모델: NQM+ 4K
/// 해상도: 3840x2160 (4K UHD)
/// 통신 방식: USB 인터페이스를 통한 I2C 통신
/// </summary>
public class NQMEngineController : ILightEngineController
{
    private CancellationTokenSource? _cts;
    private Task? _runTask;

    // NQM+에 대한 CyUSBSerial Device 정보
    public const int VID = 0x04B4;
    public const int PID = 0x000A;

    private CY_RETURN_STATUS cyReturnStatus;
    private CY_DEVICE_INFO cyDeviceInfo;
    private CY_VID_PID cyVidPid;
    private CY_DEVICE_INFO[] cyDeviceInfoList = new CY_DEVICE_INFO[16];
    private byte cyNumDevices = 0;
    private byte[] deviceID = new byte[16];
    private uint[] deviceArray = new uint[6];

    public IntPtr cy_HANDLE;
    byte deviceNumber;

    public byte gpioNum;
    public byte gpioValue;

    public byte[] Engine_WriteBuffer = new byte[7];
    public byte[] Engine_ReadBuffer = new byte[7];

    public CY_I2C_DATA_CONFIG cy_I2C_DATA_CONFIG;
    public CY_DATA_BUFFER cy_DATA_BUFFER_Engine_Read;
    public CY_DATA_BUFFER cy_DATA_BUFFER_Engine_Write;

    public enum LightEngineControllerCommand
    {
        GetTemperatures = 0,
        PowerOn,
        PowerOff,
        LEDOn,
        LEDOff,
        SetLEDDAC,
        SetFanSpeed,
        SetMotorControl,
        SetEngineFlip,
    };
    public enum FanSpeedOption
    {
        DMD = 0,
        LED1,
        LED2
    };
    public enum TemperatureSensor
    {
        DMD = 0,
        LED,
        LEDDriverBoard
    };

    private Queue<string> commandQueue;
    private int[] commandCount;

    private volatile bool isDeviceConnected;
    private volatile bool isProjectorPowerOn;            // 프로젝터 전원 상태

    private volatile bool isEngineLEDOn;                 // 엔진 UV 라이트를 켰는지 여부

    private volatile bool isSetLECDAC;                   // LED DAC 설정 여부
    private volatile int LEDDACValue;                    // LED DAC 값 (범위: 0 ~ 1023)

    private volatile bool isEngineFlip;                  // 투영 뒤집기 설정 여부
    private volatile bool IsFlipOn;                      // 투영 뒤집기 On/Off
    private volatile bool IsFlipX;                       // 투영 뒤집기 (true이면 X방향, false이면 Y방향)

    private double[] TemperatureSensorValue;
    private int id;

    public bool isMotorControl;                 // 모터 제어 설정 여부
    public bool IsFront;
    public bool IsDown;
    public int StepSize;

    private int errorCount = 0;                 // I2C 통신 오류 횟수
    private int errorCountPrev = 0;             // 이전 I2C 통신 오류 횟수

    public int GetEngineID()
    {
        return id;
    }

    public int GetErrorCount()
    {
        return errorCount;
    }

    public struct SlicingByte
    {
        private uint _value;

        // uint에 접근
        public uint Value
        {
            get => _value;
            set => _value = value;
        }

        // byte 배열로 접근
        public byte[] Bytes
        {
            get
            {
                // uint 값을 byte 배열로 변환
                return BitConverter.GetBytes(_value);
            }
            set
            {
                if (value.Length != 4)
                    throw new ArgumentException("Byte array must have exactly 4 elements.");
                // byte 배열 값을 uint로 변환
                _value = BitConverter.ToUInt32(value, 0);
            }
        }

        // 생성자
        public SlicingByte(uint value)
        {
            _value = value;
        }

        public SlicingByte(byte[] bytes)
        {
            if (bytes.Length != 4)
                throw new ArgumentException("Byte array must have exactly 4 elements.");
            _value = BitConverter.ToUInt32(bytes, 0);
        }
    }

    SlicingByte slicingByte;

    public int Engine_Count;
    public int EnableByte;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runTask = Task.Run(() => RunControlLoop(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    private void RunControlLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                errorCountPrev = errorCount;

                if (!isProjectorPowerOn)
                {
                    if (ProjectorPowerOn())
                    {
                        gpioNum = 15;
                        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

                        if (cyReturnStatus == CY_RETURN_STATUS.CY_SUCCESS && gpioValue == 1)
                            isProjectorPowerOn = true;
                    }
                }

                if (isProjectorPowerOn && isDeviceConnected)
                {
                    // 모든 주요 온도를 주기적으로 갱신
                    TemperatureSensorValue[0] = GetTemperatureSensor(0); // DMD
                    TemperatureSensorValue[1] = GetTemperatureSensor(1); // LED
                    TemperatureSensorValue[2] = GetTemperatureSensor(2); // PCB

                    // 통신 오류 체크
                    if (TemperatureSensorValue[(int)TemperatureSensor.LED] == 0.0 || TemperatureSensorValue[(int)TemperatureSensor.LED] == -1)
                        errorCount++;

                    bool returnValue;
                    if (isEngineLEDOn)
                        returnValue = LEDPowerOn();
                    else
                        returnValue = LEDPowerOff();

                    // 통신 오류 체크
                    if (!returnValue)
                        errorCount++;

                    if (isSetLECDAC)
                    {
                        SetLEDDAC(LEDDACValue);
                        isSetLECDAC = false;
                    }

                    if (isEngineFlip)
                    {
                        SetEngineFlip(IsFlipOn, IsFlipX);
                        isEngineFlip = false;
                    }

                    if (isMotorControl)
                    {
                        SetMotorControl(IsFront, IsDown, StepSize);
                        isMotorControl = false;
                    }
                }

                // 오류가 더 이상 발생하지 않는다면 카운트 변수 초기화
                if (errorCount == errorCountPrev)
                {
                    errorCount = 0;
                    errorCountPrev = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Thread.Sleep(100);
        }
    }

    public NQMEngineController()
    {
        isDeviceConnected = false;
        isProjectorPowerOn = false;
        isEngineLEDOn = false;
        isSetLECDAC = false;

        commandQueue = new Queue<string>(1000);

        commandCount = new int[9];
        for (int i = 0; i < 9; i++) commandCount[i] = 0;

        TemperatureSensorValue = new double[3];
    }

    public void Connect(int index)
    {
        if (isDeviceConnected) return;

        cyReturnStatus = CyGetListofDevices(ref cyNumDevices);

        if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            return;

        for (byte i = 0; i < cyNumDevices; i++)
        {
            cyReturnStatus = CyGetDeviceInfo(i, ref cyDeviceInfo);

            //Console.WriteLine("CyGetDeviceInfo: {0}/{1}", index + 1, cyNumDevices);
            //Console.WriteLine("Number of interfaces: {0}\nVid: 0x{1:X} \nPid: 0x{2:X}\nSerial Number is: {3}\nManufacturer name: {4}\nProduct Name: {5}\nSCB Number: 0x{6:X}\nDevice Type: {7}\nDevice Class: {8}\n\n",
            //    cyDeviceInfo.numInterfaces,
            //    cyDeviceInfo.vidPid.vid,
            //    cyDeviceInfo.vidPid.pid,
            //    cyDeviceInfo.serialNum,
            //    cyDeviceInfo.manufacturerName,
            //    cyDeviceInfo.productName,
            //    cyDeviceInfo.deviceBlock,
            //    cyDeviceInfo.deviceType[0],
            //    cyDeviceInfo.deviceClass[0]);
        }

        cyVidPid.vid = VID;
        cyVidPid.pid = PID;
        for (int i = 0; i < cyDeviceInfoList.Length; i++)
        {
            cyDeviceInfoList[i].vidPid.vid = 0;
            cyDeviceInfoList[i].vidPid.pid = 0;
            cyDeviceInfoList[i].numInterfaces = 0;
            cyDeviceInfoList[i].manufacturerName = new string('\0', CY_STRING_DESCRIPTOR_SIZE);
            cyDeviceInfoList[i].productName = new string('\0', CY_STRING_DESCRIPTOR_SIZE);
            cyDeviceInfoList[i].serialNum = new string('\0', CY_STRING_DESCRIPTOR_SIZE);
            cyDeviceInfoList[i].deviceFriendlyName = new string('\0', CY_STRING_DESCRIPTOR_SIZE);
            cyDeviceInfoList[i].deviceType = new CY_DEVICE_TYPE[CY_MAX_DEVICE_INTERFACE];
            cyDeviceInfoList[i].deviceClass = new CY_DEVICE_CLASS[CY_MAX_DEVICE_INTERFACE];
            cyDeviceInfoList[i].deviceBlock = CY_DEVICE_SERIAL_BLOCK.SerialBlock_MFG;
        }

        cyReturnStatus = CyGetDeviceInfoVidPid(cyVidPid, deviceID, cyDeviceInfoList, ref cyNumDevices, 16);

        int arrindex = 0;

        for (int i = 0; i < cyNumDevices; i++)
        {
            // Find the device at device index at SCB0
            if (cyDeviceInfoList[i].deviceBlock == CY_DEVICE_SERIAL_BLOCK.SerialBlock_SCB0)
            {
                //Console.WriteLine("CyGetDeviceInfoVidPid: {0}/{1}", index + 1, cyNumDevices);
                //Console.WriteLine("Number of interfaces: {0}\nVid: 0x{1:X} \nPid: 0x{2:X}\nSerial Number is: {3}\nManufacturer name: {4}\nProduct Name: {5}\nSCB Number: 0x{6:X}\nDevice Type: {7}\nDevice Class: {8}\n\n",
                //    cyDeviceInfoList[index].numInterfaces,
                //    cyDeviceInfoList[index].vidPid.vid,
                //    cyDeviceInfoList[index].vidPid.pid,
                //    cyDeviceInfoList[index].serialNum,
                //    cyDeviceInfoList[index].manufacturerName,
                //    cyDeviceInfoList[index].productName,
                //    cyDeviceInfoList[index].deviceBlock,
                //    cyDeviceInfoList[index].deviceType[0],
                //    cyDeviceInfoList[index].deviceClass[0]);

                deviceArray[arrindex] = (uint)i;    // 원하는 장치 인덱스가 deviceArray 안에 있음
                arrindex++;
            }
        }

        if (index < arrindex)
        {
            deviceNumber = (byte)deviceArray[index];

            cyReturnStatus = CyOpen(deviceNumber, 0, ref cy_HANDLE);

            if (cyReturnStatus == CY_RETURN_STATUS.CY_SUCCESS)
            {
                isDeviceConnected = true;
            }
            else
            {
                isDeviceConnected = false;
            }
        }
        else
        {
            isDeviceConnected = false;
        }

        errorCount = 0;

        id = index;
    }

    public void Disconnect()
    {
        if (isDeviceConnected)
        {
            _cts?.Cancel();
            LEDPowerOff();
            ProjectorPowerOff();

            if (cy_HANDLE != IntPtr.Zero) CyClose(cy_HANDLE);
            cy_HANDLE = IntPtr.Zero;
            isDeviceConnected = false;
        }
    }

    public bool ProjectorPowerOn()
    {
        gpioNum = 0;
        gpioValue = 1;

        cyReturnStatus = CySetGpioValue(cy_HANDLE, gpioNum, gpioValue);

        if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            return false;

        return true;
    }

    public bool ProjectorPowerOff()
    {
        gpioNum = 0;
        gpioValue = 0;

        cyReturnStatus = CySetGpioValue(cy_HANDLE, gpioNum, gpioValue);

        if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            return false;

        return true;
    }

    public bool LEDPowerOn()
    {
        gpioNum = 14;
        gpioValue = 1;

        cyReturnStatus = CySetGpioValue(cy_HANDLE, gpioNum, gpioValue);

        if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
        {
            return false;
        }

        return true;
    }

    public bool LEDPowerOff()
    {
        gpioNum = 14;
        gpioValue = 0;

        cyReturnStatus = CySetGpioValue(cy_HANDLE, gpioNum, gpioValue);

        if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
        {
            return false;
        }

        return true;
    }

    public bool SetLEDDAC(int value)
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            // I2C Bus is busy
            return false;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 3);

            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)0xD1;
            Engine_WriteBuffer[1] = (byte)((value >> 8) & 0xFF);
            Engine_WriteBuffer[2] = (byte)(value & 0xFF);

            // Set LED DAC Write:
            // "0x{0:X}\t0x{1:X}", Engine_WriteBuffer[1], Engine_WriteBuffer[2]

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 3;

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            // TransCount: {0}", cy_DATA_BUFFER_Engine_Write.transferCount
        }

        if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
        {
            return false;
        }

        return true;
    }

    public int GetLEDDAC()
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            // I2C Device is busy
            return -4;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 2);
            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)0x15;
            Engine_WriteBuffer[1] = (byte)0xD1;

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            // Get LED DAC Write:
            // "0x{0:X}\t0x{1:X}", Engine_WriteBuffer[0], Engine_WriteBuffer[1]);

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            // "TransCount: {0}", cy_DATA_BUFFER_Engine_Write.transferCount

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                // "Write Error = {0}", cy_RETURN_STATUS
                return -3;
            }

            Array.Clear(Engine_ReadBuffer, 0, 2);

            bufferHandle = GCHandle.Alloc(Engine_ReadBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Read.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_ReadBuffer, 0);
            cy_DATA_BUFFER_Engine_Read.length = 2;

            cyReturnStatus = CyI2cRead(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Read, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                // "Read Error = {0}", cy_RETURN_STATUS
                return -2;
            }

            // Get LED DAC Read:
            // "0x{0:X}\t0x{1:X}", Engine_WriteBuffer[0], Engine_WriteBuffer[1]);

            int _dac = (Engine_ReadBuffer[0] << 8) + Engine_ReadBuffer[1];

            if (cyReturnStatus == CY_RETURN_STATUS.CY_SUCCESS)
                return _dac;
            else
            {
                return -1;
            }
        }
    }

    public double GetTemperatureSensor(int option)      // DMD = 0, LED = 1, LED Driver Board = 2
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            //I2C Device is busy
            return 0.0;
        }
        else
        {
            byte[] writeOptionData = [0x9C, 0x9F, 0x9E];
            int[] readOptionDataSize = [1, 4, 2];
            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Array.Clear(Engine_WriteBuffer, 0, 2);

            Engine_WriteBuffer[0] = (byte)0x15;
            Engine_WriteBuffer[1] = (byte)writeOptionData[option];

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = (uint)2;

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                // $"Write Error: {cy_RETURN_STATUS}";
                // $"Slave Address: 0x{cy_I2C_DATA_CONFIG.slaveAddress:X}";
                // $"Buffer Length: {cy_DATA_BUFFER_Engine_Write.length}";
                // $"Buffer Content: {string.Join(" ", Engine_WriteBuffer.Select(b => $"0x{b:X2}"))}";

                return 0.0;
            }

            Array.Clear(Engine_ReadBuffer, 0, readOptionDataSize[option]);

            bufferHandle = GCHandle.Alloc(Engine_ReadBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Read.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_ReadBuffer, 0);
            cy_DATA_BUFFER_Engine_Read.length = (uint)readOptionDataSize[option];

            cyReturnStatus = CyI2cRead(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Read, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                // $"Read Error: {cy_RETURN_STATUS}";
                // $"Slave Address: 0x{cy_I2C_DATA_CONFIG.slaveAddress:X}";
                // $"Buffer Length: {cy_DATA_BUFFER_Engine_Read.length}";
                // $"Buffer Content: {string.Join(" ", Engine_ReadBuffer.Select(b => $"0x{b:X2}"))}";

                return 0.0;
            }

            if (option == 0)
            {
                return (double)(ReverseBYTE(Engine_ReadBuffer[0]));
            }
            else if (option == 1)
            {
                return (double)((Engine_ReadBuffer[3] << 8) + Engine_ReadBuffer[2] + (float)((((Engine_ReadBuffer[1] << 8) + Engine_ReadBuffer[0])) / 65536));
            }
            else if (option == 2)
            {
                return (double)(((Engine_ReadBuffer[1] & 0x0F) << 4) + ((Engine_ReadBuffer[0] & 0xF0) >> 4) + (float)((Engine_ReadBuffer[0] & 0x0F) / 16));
            }
            else
            {
                return -1;
            }
        }
    }

    public void SetEngineFlip(bool isOn, bool isFlipX)    // isFlipX = true (x축 반전; 좌우 반전) /isFlipX = false (y축 반전; 상하 반전) 
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            // I2C Device is busy
            return;
        }
        else
        {
            // GPIO is Not BUSY
            Array.Clear(Engine_WriteBuffer, 0, 2);

            //EnableByte |= 0;

            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)(isFlipX ? 0x10 : 0x1F);         // Flip 축 설정 
            Engine_WriteBuffer[1] = (byte)(isOn ? 0x01 : 0x00);            // Flip On Off 설정

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                // "Write Error = {0}", cy_RETURN_STATUS
                return;
            }
        }
    }

    public static byte ReverseBYTE(byte inputByte)
    {
        byte outputByte = 0;

        for (int i = 0; i < 7; i++)
        {
            outputByte |= (byte)(inputByte & 1);

            inputByte = (byte)(inputByte >> 1);
            outputByte = (byte)(outputByte << 1);
        }
        outputByte |= (byte)(inputByte & 1);

        return outputByte;
    }

    public double GetTemperatureSensorValue(int sensor)
    {
        //return TemperatureSensorValue[sensor];
        return TemperatureSensorValue[1];
    }

    public bool GetSettingLEDDAC()
    {
        return isSetLECDAC;
    }
    public int GetLEDDACValue()
    {
        return LEDDACValue;
    }
    public bool GetDeviceConnected()
    {
        return isDeviceConnected;
    }
    public bool GetDeviceLEDOn()
    {
        return isEngineLEDOn;
    }
    public bool GetEngineFlipOn()
    {
        return isEngineFlip;
    }

    public void SetSettingLEDDAC(bool isSet)
    {
        isSetLECDAC = isSet;
    }
    public void SetLEDDACValue(int value)
    {
        LEDDACValue = value;
    }
    public void SetDeviceConnected(bool isConnected)
    {
        isDeviceConnected = isConnected;
    }

    public void SetDeviceLEDOn(bool isOn)
    {
        isEngineLEDOn = isOn;
    }
    public void SetEngineFlipOn(bool isOn)
    {
        isEngineFlip = isOn;
    }
    public void SetEngineFilpValue(bool isOn, bool isX)
    {
        IsFlipOn = isOn;
        IsFlipX = isX;
    }

    // 사용하지 않는 함수 모음 =========================================================================================================================
    public int SetFanSpeed(int value, int option)     // value: 1~100, option: DMD = 0, LED1 = 1, LED2 = 2
    {
        if (value > 100) value = 100;
        else if (value < 0) value = 0;

        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            //Console.WriteLine("I2C Device is busy");
            return -1;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 2);
            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)(0xEB + option);
            Engine_WriteBuffer[1] = (byte)value;

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            //Console.WriteLine("Set Fan Speed Write: ");
            //Console.WriteLine("0x{0:X}\t0x{1:X}", Engine_WriteBuffer[0], Engine_WriteBuffer[1]);

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);

            if (bufferHandle.IsAllocated) bufferHandle.Free();

            //Console.WriteLine("TransCount: {0}", cy_DATA_BUFFER_Engine_Write.transferCount);

            if (cyReturnStatus == CY_RETURN_STATUS.CY_SUCCESS)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }

    public int SetMotorControl(bool isFront, bool isDown, int stepSize)
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            //Console.WriteLine("I2C Device is busy");
            return -1;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 2);

            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)(isFront ? 0xB5 : 0xBA);
            Engine_WriteBuffer[1] = (byte)((isDown == true) ? 1 : 0);

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            //Console.WriteLine("Set Motor Control Write: ");
            //Console.WriteLine("0x{0:X}\t0x{1:X}", Engine_WriteBuffer[0], Engine_WriteBuffer[1]);

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                return -1;
            }

            Array.Clear(Engine_WriteBuffer, 0, 5);

            Engine_WriteBuffer[0] = (byte)(isFront ? 0xB6 : 0xBB);
            slicingByte.Value = (uint)stepSize;
            Engine_WriteBuffer[1] = (byte)(stepSize & 0xFF);
            Engine_WriteBuffer[2] = (byte)((stepSize >> 8) & 0xFF);
            Engine_WriteBuffer[3] = (byte)0x32;
            Engine_WriteBuffer[4] = (byte)0x00;

            //Console.WriteLine("Set Motor Control Write: ");
            //Console.WriteLine("0x{0:X}\t0x{1:X}", Engine_WriteBuffer[0], Engine_WriteBuffer[1]);

            bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 5;

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus == CY_RETURN_STATUS.CY_SUCCESS)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }

    public int GetLightSensor()
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            //Console.WriteLine("I2C Device is busy");
            return -1;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 2);
            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)0x15;
            Engine_WriteBuffer[1] = (byte)0xF7;

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            //Console.WriteLine("Get Light Sensor Write: ");
            //Console.WriteLine("0x{0:X}\t0x{1:X}", Engine_WriteBuffer[0], Engine_WriteBuffer[1]);

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                return -1;
            }

            Array.Clear(Engine_ReadBuffer, 0, 2);

            bufferHandle = GCHandle.Alloc(Engine_ReadBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Read.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_ReadBuffer, 0);
            cy_DATA_BUFFER_Engine_Read.length = 2;

            cyReturnStatus = CyI2cRead(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Read, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                return -1;
            }

            //Console.WriteLine("Get Light Sensor Read: ");
            //Console.WriteLine("0x{0:X}\t0x{1:X}", Engine_ReadBuffer[0], Engine_ReadBuffer[1]);

            int lightSensorValue = (Engine_ReadBuffer[1] << 8) + Engine_ReadBuffer[0];

            return lightSensorValue;
        }
    }

    public void SetProjectorsourceandTestpattern_HDMI()
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            //Console.WriteLine("I2C Device is busy");
            return;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 2);
            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)0x01;
            Engine_WriteBuffer[1] = (byte)0x00;

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            //Console.WriteLine("Set HDMI Write: ");
            //Console.WriteLine("0x{0:X}\t0x{1:X}", Engine_WriteBuffer[0], Engine_WriteBuffer[1]);

            //Console.WriteLine("TransCount: {0}", cy_DATA_BUFFER_Engine_Write.transferCount);
            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                return;
            }
        }
    }

    public void SetProjectorsourceandTestpattern_Ramp()
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            //Console.WriteLine("I2C Device is busy");
            return;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 2);
            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)0x01;
            Engine_WriteBuffer[1] = (byte)0x01;

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            //Console.WriteLine("Set Ramp Write1: ");
            //Console.WriteLine("0x{0:X}\t0x{1:X}", Engine_WriteBuffer[0], Engine_WriteBuffer[1]);

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            //Console.WriteLine("TransCount: {0}", cy_DATA_BUFFER_Engine_Write.transferCount);

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                return;
            }

            Array.Clear(Engine_WriteBuffer, 0, 2);
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)0x11;
            Engine_WriteBuffer[1] = (byte)0x01;

            bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            //Console.WriteLine("Set Ramp Write2: ");
            //Console.WriteLine("0x{0:X}\t0x{1:X}", Engine_WriteBuffer[0], Engine_WriteBuffer[1]);

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            //Console.WriteLine("TransCount: {0}", cy_DATA_BUFFER_Engine_Write.transferCount);
        }
    }

    public void SetProjectorsourceandTestpattern_CheckerBoard()
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            //Console.WriteLine("I2C Device is busy");
            return;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 2);
            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)0x01;
            Engine_WriteBuffer[1] = (byte)0x01;

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            //Console.WriteLine("Set Checkerboard Write1: ");
            //Console.WriteLine("0x{0:X}\t0x{1:X}", Engine_WriteBuffer[0], Engine_WriteBuffer[1]);

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            //Console.WriteLine("TransCount: {0}", cy_DATA_BUFFER_Engine_Write.transferCount);

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                //Console.WriteLine("Write Error = {0}", cy_RETURN_STATUS);
                return;
            }

            Array.Clear(Engine_WriteBuffer, 0, 2);
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)0x11;
            Engine_WriteBuffer[1] = (byte)0x07;

            bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            //Console.WriteLine("Set Checkerboard Write2: ");
            //Console.WriteLine("0x{0:X}\t0x{1:X}", Engine_WriteBuffer[0], Engine_WriteBuffer[1]);

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            //Console.WriteLine("TransCount: {0}", cy_DATA_BUFFER_Engine_Write.transferCount);
        }
    }

    public void SetProjectorsourceandTestpattern_SolidField()
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            //Console.WriteLine("I2C Device is busy");
            return;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 7);
            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)0x13;
            Engine_WriteBuffer[1] = (byte)0xFF;
            Engine_WriteBuffer[2] = (byte)0x03;
            Engine_WriteBuffer[3] = (byte)0xFF;
            Engine_WriteBuffer[4] = (byte)0x03;
            Engine_WriteBuffer[5] = (byte)0xFF;
            Engine_WriteBuffer[6] = (byte)0x03;

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 7;

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                return;
            }

            Array.Clear(Engine_WriteBuffer, 0, 2);
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)0x01;
            Engine_WriteBuffer[1] = (byte)0x02;

            bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                return;
            }
        }
    }

    public void SetEngineCurrent(int engineCurrent)
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            //Console.WriteLine("I2C Device is busy");
            return;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 7);
            cy_I2C_DATA_CONFIG.slaveAddress = 0x1A;
            cy_I2C_DATA_CONFIG.isNakBit = true;
            cy_I2C_DATA_CONFIG.isStopBit = true;

            Engine_WriteBuffer[0] = (byte)0x54;
            Engine_WriteBuffer[1] = (byte)(engineCurrent & 0xFF);  // R
            Engine_WriteBuffer[2] = (byte)((engineCurrent >> 8) & 0xFF);
            Engine_WriteBuffer[3] = (byte)(engineCurrent & 0xFF);  // G
            Engine_WriteBuffer[4] = (byte)((engineCurrent >> 8) & 0xFF);
            Engine_WriteBuffer[5] = (byte)(engineCurrent & 0xFF);  // B
            Engine_WriteBuffer[6] = (byte)((engineCurrent >> 8) & 0xFF);

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 7;

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();
        }
    }

    public void SetEngineSeqOn()
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            //Console.WriteLine("I2C Device is busy");
            return;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 2);

            EnableByte |= 0x01 | 0x02 | 0x04;

            Engine_WriteBuffer[0] = (byte)0x52;
            Engine_WriteBuffer[1] = (byte)EnableByte;

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            cy_I2C_DATA_CONFIG.slaveAddress = (byte)0x1A;

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();

            if (cyReturnStatus != CY_RETURN_STATUS.CY_SUCCESS)
            {
                return;
            }
        }
    }

    public void SetEngineSeqOff()
    {
        gpioNum = 1;
        cyReturnStatus = CyGetGpioValue(cy_HANDLE, gpioNum, out gpioValue);

        if (gpioValue == 0)
        {
            //Console.WriteLine("I2C Device is busy");
            return;
        }
        else
        {
            Array.Clear(Engine_WriteBuffer, 0, 2);

            EnableByte |= 0;

            Engine_WriteBuffer[0] = (byte)0x52;
            Engine_WriteBuffer[1] = (byte)EnableByte;

            GCHandle bufferHandle = GCHandle.Alloc(Engine_WriteBuffer, GCHandleType.Pinned);
            cy_DATA_BUFFER_Engine_Write.buffer = Marshal.UnsafeAddrOfPinnedArrayElement(Engine_WriteBuffer, 0);
            cy_DATA_BUFFER_Engine_Write.length = 2;

            cy_I2C_DATA_CONFIG.slaveAddress = (byte)0x1A;

            cyReturnStatus = CyI2cWrite(cy_HANDLE, ref cy_I2C_DATA_CONFIG, ref cy_DATA_BUFFER_Engine_Write, 5000);
            if (bufferHandle.IsAllocated) bufferHandle.Free();
        }
    }
}

/// <summary>
/// 테스트 모델: Anhua 4K UV 엔진, 펌웨어 DF78 기준
/// 해상도: 3840x2160 (4K UHD)
/// 통신 방식: 라즈베리파이-HIFUN 보드를 경유한 TCP/IP (원래는 시리얼 통신)
/// </summary>
public class AnhuaEngineController : ILightEngineController
{
    private CancellationTokenSource? _cts;
    private Task? _runTask;

    // 모듈명: DF78 기준
    public enum FanSpeedOption
    {
        DMD = 0,    // Get: CM+STAT=4, Set: CM+FAN1=85
        PCB1,       // Get: CM+STAT=5, Set: CM+FAN2=85
        PCB2        // Get: CM+STAT=6, Set: CM+FAN3=85
    };
    public enum TemperatureSensor
    {
        LED = 0,
        PCB = 1
    };

    private TcpClient? tcpClient;
    private NetworkStream? stream;
    private StreamWriter? writer;
    private StreamReader? reader;

    private string ipAddress;
    private int port;

    private volatile bool isDeviceConnected;
    private volatile bool isProjectorPowerOn;            // 프로젝터 전원 상태

    private volatile bool isEngineLEDOn;                 // 엔진 UV 라이트를 켰는지 여부

    private volatile bool isSetLECDAC;                   // LED DAC 설정 여부
    private volatile int LEDDACValue;                    // LED DAC 값 (범위: 0 ~ 1023)

    private volatile bool isEngineFlip;                  // 투영 뒤집기 설정 여부
    private volatile bool IsFlipOn = false;              // 투영 뒤집기 On/Off
    private volatile bool IsFlipX = false;               // 투영 뒤집기 (true이면 X방향, false이면 Y방향)

    private double[] TemperatureSensorValue;
    private int id;

    private int errorCount = 0;                 // 통신 오류 횟수
    private int errorCountPrev = 0;             // 이전 통신 오류 횟수

    public int GetEngineID()
    {
        return id;
    }

    public int GetErrorCount()
    {
        return errorCount;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runTask = Task.Run(() => RunControlLoop(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    public void Stop()
    {
        if (isDeviceConnected)
        {
            _cts?.Cancel();
            LEDPowerOff();
            ProjectorPowerOff();

            DisconnectTcp();
            //[수정] : isProjectorPowerOn 미 초기화시
            //Disconnect()후 재 연결 시 isProjectorPowerOn이 true로 남아있어서 LED 제어가 안되는 문제 발생
            //DisconnectTcp() 내부에서 false로 초기화 하므로 여기서는 별도 처리 불필요함
            isDeviceConnected = false;
        }
    }

    private void RunControlLoop(CancellationToken token)
    {
        // UV 엔진 Power On 재시도 타이머
        Stopwatch retryTimer = new();
        bool isWaitingRetry = false;

        // 마지막으로 성공적으로 설정된 LED 상태 저장 변수
        bool? lastEngineLEDState = null;
        bool returnValue;

        while (!token.IsCancellationRequested)
        {
            try
            {
                //[수정] : 재연결 로직 추가:
                //기존에는 isDeviceConnected가 false가 되어도 루프가 아무것도 하지 않은 채 계속 돌아감.
                //수정 된 코드에서는 연결이 끊어지면 자동으로 재연결을 시도함.
                //Connect(id)는 id필드에 저장된 마지막 연결 인덱스로 재시도.
                if (!isDeviceConnected)
                {
                    Connect(id);
                    if (isDeviceConnected)
                    {
                        lastEngineLEDState = null;   // 재연결 후 LED 상태 초기화하여 재전송 유도
                        isWaitingRetry = false;     // projectorPowerOn 재시도 타이머 초기화
                        retryTimer.Stop();
                        retryTimer.Reset();
                        Thread.Sleep(500);
                        continue;
                    }
                    Thread.Sleep(3000); //연결 실패 시 3초 후 재시도
                    continue;
                }
                errorCountPrev = errorCount;

                if (!isProjectorPowerOn)
                {
                    // 1분 대기 중인지 확인
                    if (isWaitingRetry)
                    {
                        // 1분(60,000ms)이 지났는지 확인
                        if (retryTimer.ElapsedMilliseconds >= 60000)
                        {
                            isWaitingRetry = false;
                            retryTimer.Stop();
                            retryTimer.Reset();
                        }
                    }

                    // 대기 중이 아닐 때만 실행
                    if (!isWaitingRetry)
                    {
                        if (ProjectorPowerOn())
                        {
                            isProjectorPowerOn = true;
                        }
                        else
                        {
                            // 실패 시 타이머 시작
                            isWaitingRetry = true;
                            retryTimer.Start();
                        }
                    }
                }

                if (isProjectorPowerOn && isDeviceConnected)
                {
                    // --- LED 제어 로직 수정 시작 ---
                    // 현재 상태가 이전 상태와 다를 때만 통신 수행
                    if (lastEngineLEDState != isEngineLEDOn)
                    {
                        if (isEngineLEDOn)
                            returnValue = LEDPowerOn();
                        else
                            returnValue = LEDPowerOff();

                        // 통신 오류 체크
                        if (!returnValue)
                            errorCount++;

                        // 통신 성공 시에만 마지막 상태를 업데이트 (실패 시 다음 루프에서 재시도)
                        if (returnValue)
                        {
                            lastEngineLEDState = isEngineLEDOn;
                        }

                        // 테스트 중 - 안정성 테스트를 위해 온도 체크 코멘트 처리
                        //// LED Off 상태 직후 온도 체크
                        //if (lastEngineLEDState == false)
                        //{
                        //    Thread.Sleep(400);

                        //    TemperatureSensorValue[(int)TemperatureSensor.LED] = GetTemperatureSensor((int)TemperatureSensor.LED);

                        //    // 통신 오류 체크
                        //    if (TemperatureSensorValue[(int)TemperatureSensor.LED] == 0.0)
                        //        errorCount++;

                        //    Thread.Sleep(400);
                        //}
                    }
                    // --- LED 제어 로직 수정 끝 ---

                    if (isSetLECDAC)
                    {
                        SetLEDDAC(LEDDACValue);
                        isSetLECDAC = false;
                    }

                    if (isEngineFlip)
                    {
                        SetEngineFlip(IsFlipOn, IsFlipX);
                        isEngineFlip = false;
                    }
                }

                // 오류가 더 이상 발생하지 않는다면 카운트 변수 초기화
                if (errorCount == errorCountPrev)
                {
                    errorCount = 0;
                    errorCountPrev = 0;
                }
            }
            catch (Exception ex)
            {
                //[수정] : try / catch 추가
                //기존 코드에서 예외 발생 시 백그라운드 스레드가 조용히 종료되고 그 이후 어떤 명령도 처리되지 않는 상태가 됨.
                //예외를 잡아 TCP만 해제하고 루프 상단의 재연결 로직으로 이어짐.
                //에러복구시에는 TCP만 끊어야 하므로 Disconnect()가 아닌 DisconnectTcp() 호출함
                Console.WriteLine($"Anhua Engine: Thread Error ({ex.Message})");
                errorCount++;
                // 통신 오류 발생 시 연결 해제 및 재연결 시도
                DisconnectTcp();
            }

            Thread.Sleep(100);
        }
    }

    public AnhuaEngineController(string ipAddress, int port)
    {
        this.ipAddress = ipAddress;
        this.port = port;

        isDeviceConnected = false;
        isProjectorPowerOn = false;
        isEngineLEDOn = false;
        isSetLECDAC = false;

        TemperatureSensorValue = new double[2];
    }

    public void Connect(int index)
    {
        if (isDeviceConnected) return;

        try
        {
            tcpClient = new TcpClient();
            // 타임아웃 설정 (연결 시도 3초)
            var result = tcpClient.BeginConnect(ipAddress, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

            if (!success || !tcpClient.Connected)
            {
                throw new Exception("[Anhua Engine] TCP Connection Timeout");
            }

            stream = tcpClient.GetStream();
            // AutoFlush를 true로 설정하여 즉시 전송되게 함
            writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            reader = new StreamReader(stream, Encoding.ASCII);

            //[수정] Timeout 미설정 시 SendCommand()의 stream.Read()가 영원히 블로킹됨.
            //네트워크가 half-open 상태(연결은 살아있지만 상대방이 응답 안함)가 되면
            //백그라운드 스레드가 이 지점에서 완전히 멈추고 모든 명령 처리가 중단됨.
            //Timeout 설정하면 지정 시간 내 응답 없을 시 IOException 발생하여 catch로 빠져나올 수 있음.
            tcpClient.ReceiveTimeout = 500;
            tcpClient.SendTimeout = 500;
            stream.ReadTimeout = 2000;
            stream.WriteTimeout = 2000;

            this.isDeviceConnected = true;
            id = index;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Anhua Engine] 연결 오류 {ex.Message}");
            this.isDeviceConnected = false;
        }
    }

    //[수정] DisconnectTcp() 분리 : Thread_DoWork의 catch에서 에러 복구 시 호출하기 위함.
    //기존 Disconnect()를 그대로 쓰면 스레드가 종료되게 됨.
    //TCP연결만 끊고 스레드는 살려둬서 다음 루프에서 재연결 시도할 수 있게 함.
    public void DisconnectTcp()
    {
        writer?.Close();
        reader?.Close();
        stream?.Close();
        tcpClient?.Close();
        this.isDeviceConnected = false;
        this.isProjectorPowerOn = false; // [수정] : 아래 Disconnect()와 동일한 이유로 초기화
    }

    public void Disconnect()
    {
        if (isDeviceConnected)
        {
            _cts?.Cancel();
            LEDPowerOff();
            ProjectorPowerOff();

            DisconnectTcp();
            //[수정] : isProjectorPowerOn 미 초기화시
            //Disconnect()후 재 연결 시 isProjectorPowerOn이 true로 남아있어서 LED 제어가 안되는 문제 발생
            //DisconnectTcp() 내부에서 false로 초기화 하므로 여기서는 별도 처리 불필요함
            isDeviceConnected = false;
        }
    }

    private string SendCommand(string cmd)
    {
        if (!isDeviceConnected || tcpClient == null || !tcpClient.Connected)
            return "Error: Socket Closed";

        if (stream == null)
            return "Error: Network Stream Unavailable";

        try
        {
            // 기존에 남아있던 쓰레기 데이터 청소 (선택 사항)
            //while (stream.DataAvailable) { stream.ReadByte(); }
            //[수정] : 기존 byte by byte 방식은 데이터가 많을 때 비효율적이므로 한번에 읽어버림
            if (stream.DataAvailable)
            {
                byte[] dummy = new byte[tcpClient.ReceiveBufferSize];
                stream.Read(dummy, 0, tcpClient.ReceiveBufferSize);
            }

            // 명령어 전송
            byte[] data = Encoding.ASCII.GetBytes(cmd + "\r\n");
            stream.Write(data, 0, data.Length);
            stream.Flush();
            /*
            // 응답 대기 및 읽기 (ReadLine 스타일)
            using StreamReader reader = new(stream, Encoding.ASCII, leaveOpen: true);
            // 응답이 올 때까지 대기 (타임아웃은 아래 '참고' 섹션 확인)
            // ReadLine()은 \n 또는 \r\n을 만날 때까지 블로킹(대기)됩니다.
            string? response = reader.ReadLine();

            if (response != null)
            {
                return response.Trim();
            }

            return "No Response";
            */
            //[수정] : StreamReader 방식 제거 :
            //기존 코드는 SendCommand()를 호출할 때마다 new StreamReader를 생성하고 using으로 폐기함.
            //StreamReader는 내부적으로 선행 읽기 버퍼를 가지므로, 응답 이후의 데이터까지 미리 읽어 버퍼에 저장함
            //using 블록 종료 시 leaveOpen : true 여도 이 버퍼는 소실되어 다음 호출에서 엉뚱한 데이터를 읽거나 프로토콜이 탈동기 될 확률이 있음.
            //또한 ReadLine()은 \n을 받을 때까지 무한 블로킹하므로, 엔진이 응답하지 않으면 백그라운드 스레드가 영구적으로 멈춤.
            //stream.Read()로 교체하여 두 문제를 모두 해결.

            //응답 대기 (최대 500ms 폴링)
            int retry = 0;
            while (!stream.DataAvailable && retry < 50)
            {
                Thread.Sleep(10);
                retry++;
            }

            //데이터 읽기
            if (stream.DataAvailable)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
            }
            return "No Response";
        }
        catch (Exception ex)
        {
            //[수정] : 통신 예외 발생 시 연결을 끊어 재연결 로직이 트리거되도록
            DisconnectTcp();
            return $"[Anhua Engine] SendCommand({cmd}): {ex.Message}";
        }
    }

    public bool ProjectorPowerOn()
    {
        // 전원 상태 Get: CM+STAT=0 보내서 1 오면 ON, 0 오면 OFF
        int val = 1;
        string cmd = $"CM+PWRE={val}";
        string returnStr = SendCommand(cmd);
        if (returnStr.Contains("OK"))
            return true;
        else
            return false;
    }

    public bool ProjectorPowerOff()
    {
        // 전원 상태 Get: CM+STAT=0 보내서 1 오면 ON, 0 오면 OFF
        int val = 0;
        string cmd = $"CM+PWRE={val}";
        string returnStr = SendCommand(cmd);
        if (returnStr.Contains("OK"))
            return true;
        else
            return false;
    }

    public bool LEDPowerOn()
    {
        // 전원 상태 Get: CM+STAT=1 보내서 1 오면 ON, 0 오면 OFF
        int val = 1;
        string cmd = $"CM+LEDE={val}";
        string returnStr = SendCommand(cmd);
        if (returnStr.Contains("OK"))
            return true;
        else
            return false;
    }

    public bool LEDPowerOff()
    {
        // 전원 상태 Get: CM+STAT=1 보내서 1 오면 ON, 0 오면 OFF
        int val = 0;
        string cmd = $"CM+LEDE={val}";
        string returnStr = SendCommand(cmd);
        if (returnStr.Contains("OK"))
            return true;
        else
            return false;
    }

    public bool SetLEDDAC(int value)
    {
        // Clamp (0 ~ 1023)
        int val = Math.Max(0, Math.Min(1023, value));

        string cmd = $"CM+LEDS={val}";
        string returnStr = SendCommand(cmd);
        if (returnStr.Contains("OK"))
            return true;
        else
            return false;
    }

    public int GetLEDDAC()
    {
        string cmd = "CM+STAT=3";
        string returnStr = SendCommand(cmd);

        // 성공하면 0~1023 범위의 숫자가 오고, 실패하면 ERROR 문자열이 옴
        if (int.TryParse(returnStr, out int value))
            return value;

        return 0;
    }

    public double GetTemperatureSensor(int option)  // LED = 0, PCB = 1
    {
        string returnStr = "0.0";

        if (option == 0)
            returnStr = SendCommand("CM+GTMP"); // LED
        else if (option == 1)
            returnStr = SendCommand("CM+GTMB"); // PCB

        if (double.TryParse(returnStr, out double value))
            return (double)value;

        return 0.0;
    }

    public void SetEngineFlip(bool isOn, bool isFlipX)
    {
        // dummy
    }

    public bool GetDeviceConnected()
    {
        return this.isDeviceConnected;
    }

    public bool GetDeviceLEDOn()
    {
        return isEngineLEDOn;
    }

    public bool GetEngineFlipOn()
    {
        // dummy
        return false;
    }

    public int GetLEDDACValue()
    {
        return LEDDACValue;
    }

    public bool GetSettingLEDDAC()
    {
        return isSetLECDAC;
    }

    public double GetTemperatureSensorValue(int sensor)
    {
        return TemperatureSensorValue[sensor];
    }

    public void SetDeviceConnected(bool isConnected)
    {
        this.isDeviceConnected = isConnected;
    }

    public void SetDeviceLEDOn(bool isOn)
    {
        isEngineLEDOn = isOn;
    }

    public void SetEngineFilpValue(bool isOn, bool isX)
    {
        // dummy
    }

    public void SetEngineFlipOn(bool isOn)
    {
        // dummy
    }

    public void SetLEDDACValue(int value)
    {
        LEDDACValue = value;
    }

    public void SetSettingLEDDAC(bool isSet)
    {
        isSetLECDAC = isSet;
    }
}

/// <summary>
/// 테스트 모델: LuxBeam LRS WQ
/// 해상도: 2560x1600 (WQXGA)
/// 통신 방식: TCP/IP
/// 기타: 제어용 포트 5000, 웹 UI 포트 8080
/// </summary>
public class VisitechWQXGAEngineController : ILightEngineController
{
    private CancellationTokenSource? _cts;
    private Task? _runTask;

    private TcpClient? tcpClient;
    private NetworkStream? stream;
    private StreamWriter? writer;
    private StreamReader? reader;

    private readonly string ipAddress;
    private readonly int port;

    private volatile bool isDeviceConnected;
    private volatile bool isProjectorPowerOn;            // 프로젝터 전원 상태

    private volatile bool isEngineLEDOn;                 // 엔진 UV 라이트를 켰는지 여부

    private volatile bool isSetLECDAC;                   // LED DAC 설정 여부
    private volatile int LEDDACValue;                    // LED DAC 값 (범위: 0 ~ 1023)

    private volatile bool isEngineFlip;                  // 투영 뒤집기 설정 여부
    private volatile bool IsFlipOn;                      // 투영 뒤집기 On/Off
    private volatile bool IsFlipX;                       // 투영 뒤집기 (true이면 X방향, false이면 Y방향)

    private double TemperatureSensorValue;
    private int id;

    private int errorCount = 0;                 // 통신 오류 횟수
    private int errorCountPrev = 0;             // 이전 통신 오류 횟수

    public int GetEngineID()
    {
        return id;
    }

    public int GetErrorCount()
    {
        return errorCount;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runTask = Task.Run(() => RunControlLoop(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    private void RunControlLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            errorCountPrev = errorCount;

            if (!isProjectorPowerOn)
            {
                if (ProjectorPowerOn())
                {
                    isProjectorPowerOn = true;
                }
            }

            if (isProjectorPowerOn && isDeviceConnected)
            {
                TemperatureSensorValue = GetTemperatureSensor();

                // 통신 오류 체크
                if (TemperatureSensorValue == 0.0 || TemperatureSensorValue == -1)
                    errorCount++;

                bool returnValue;
                if (isEngineLEDOn)
                    returnValue = LEDPowerOn();
                else
                    returnValue = LEDPowerOff();

                // 통신 오류 체크
                if (!returnValue)
                    errorCount++;

                if (isSetLECDAC)
                {
                    SetLEDDAC(LEDDACValue);
                    isSetLECDAC = false;
                }

                if (isEngineFlip)
                {
                    SetEngineFlip(IsFlipOn, IsFlipX);
                    isEngineFlip = false;
                }
            }

            // 오류가 더 이상 발생하지 않는다면 카운트 변수 초기화
            if (errorCount == errorCountPrev)
            {
                errorCount = 0;
                errorCountPrev = 0;
            }

            Thread.Sleep(100);
        }
    }

    public VisitechWQXGAEngineController(string ipAddress, int port = 5000)
    {
        this.ipAddress = ipAddress;
        this.port = port;

        isDeviceConnected = false;
        isProjectorPowerOn = false;
        isEngineLEDOn = false;
        isSetLECDAC = false;

        TemperatureSensorValue = 0.0;
    }

    public void Connect(int index)
    {
        if (isDeviceConnected) return;

        try
        {
            tcpClient = new TcpClient();
            // 타임아웃 설정 (연결 시도 3초)
            var result = tcpClient.BeginConnect(ipAddress, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

            if (!success || !tcpClient.Connected)
            {
                throw new Exception("[Visitech Engine] TCP Connection Timeout");
            }

            stream = tcpClient.GetStream();
            // AutoFlush를 true로 설정하여 즉시 전송되게 함
            writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            reader = new StreamReader(stream, Encoding.ASCII);

            this.isDeviceConnected = true;
            id = index;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Visitech Engine] Connect Error ({ex.Message})");
            this.isDeviceConnected = false;
        }
    }

    public void Disconnect()
    {
        if (isDeviceConnected)
        {
            _cts?.Cancel();
            LEDPowerOff();
            ProjectorPowerOff();

            writer?.Close();
            reader?.Close();
            stream?.Close();
            tcpClient?.Close();

            this.isDeviceConnected = false;
        }
    }

    private void Init()
    {
        // 엔진 초기화 동작
        // 하나씩 보내고 응답을 하나씩 확인할 필요가 있음
        string response;

        response = SendCommand("GET LED TEMP");
        Console.WriteLine($"[Visitech Engine] {id}: GET LED TEMP --> {response}");

        response = SendCommand("INIT HDMI");
        Console.WriteLine($"[Visitech Engine] {id}: INIT HDMI --> {response}");

        response = SendCommand("SET OPERATION MODE VIDEO_PATTERN_MODE");
        Console.WriteLine($"[Visitech Engine] {id}: SET OPERATION MODE VIDEO_PATTERN_MODE --> {response}");

        response = SendCommand("INIT HDMI");
        Console.WriteLine($"[Visitech Engine] {id}: INIT HDMI --> {response}");

        response = SendCommand("SET OPERATION MODE VIDEO_PATTERN_MODE");
        Console.WriteLine($"[Visitech Engine] {id}: SET OPERATION MODE VIDEO_PATTERN_MODE --> {response}");

        response = SendCommand("SET LUT DEFINITION 180000,0,0,8,0,0,0");
        Console.WriteLine($"[Visitech Engine] {id}: SET LUT DEFINITION 180000,0,0,8,0,0,0 --> {response}");

        response = SendCommand("SET LUT CONFIG 1 1000");
        Console.WriteLine($"[Visitech Engine] {id}: SET LUT CONFIG 1 1000 --> {response}");

        response = SendCommand("GET VERSION");
        Console.WriteLine($"[Visitech Engine] {id}: GET VERSION --> {response}");
    }

    private string SendCommand(string cmd)
    {
        if (!isDeviceConnected || tcpClient == null || !tcpClient.Connected)
            return "Error: Socket Closed";

        if (stream == null)
            return "Error: Network Stream Unavailable";

        try
        {
            // 기존에 남아있던 쓰레기 데이터 청소 (선택 사항)
            while (stream.DataAvailable) { stream.ReadByte(); }

            // 명령어 전송
            byte[] data = Encoding.ASCII.GetBytes(cmd + "\r\n\r\n");
            stream.Write(data, 0, data.Length);
            stream.Flush();

            // 장비가 응답을 준비할 충분한 시간을 줍니다.
            Thread.Sleep(200);

            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            // 수신된 전체 문자열 확인
            string fullResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            if (fullResponse != null)
                return fullResponse.Trim();

            return "No Response";
        }
        catch (Exception ex)
        {
            return $"[Visitech Engine] SendCommand({cmd}): {ex.Message}";
        }
    }

    public bool ProjectorPowerOn()
    {
        // dummy
        return true;
    }

    public bool ProjectorPowerOff()
    {
        // dummy
        return true;
    }

    public bool LEDPowerOn()
    {
        string returnStr = SendCommand("SET SEQ ON");
        Console.WriteLine($"[Visitech Engine] {id}: LED ON --> {returnStr}");
        return true;
    }

    public bool LEDPowerOff()
    {
        string returnStr = SendCommand("SET SEQ OFF");
        Console.WriteLine($"[Visitech Engine] {id}: LED OFF --> {returnStr}");
        return true;
    }

    public bool SetLEDDAC(int value)
    {
        int val = Math.Max(0, Math.Min(1023, value));
        LEDDACValue = val;
        string cmd = $"SET AMPLITUDE {val}";
        string returnStr = SendCommand(cmd);
        Console.WriteLine($"[Visitech Engine] {id}: SET AMPLITUDE --> {returnStr}");
        return true;
    }

    public int GetLEDDAC()
    {
        string cmd = $"GET AMPLITUDE";
        string returnStr = SendCommand(cmd);

        // 응답 문자열에서 LED DAC 값 추출
        if (int.TryParse(returnStr.AsSpan(5), out int value))
            return value;

        return 0;
    }

    public double GetTemperatureSensor()
    {
        string returnStr = SendCommand("GET LED TEMP");
        Console.WriteLine($"[Visitech Engine] {id}: GET LED TEMP --> {returnStr}");

        // 응답 문자열에서 온도 값 추출
        if (double.TryParse(returnStr.AsSpan(5, 5), out double value))
            return (double)value;

        return 0.0;
    }

    public void SetEngineFlip(bool isOn, bool isFlipX)
    {
        // dummy
    }

    public bool GetDeviceConnected()
    {
        return this.isDeviceConnected;
    }

    public bool GetDeviceLEDOn()
    {
        return isEngineLEDOn;
    }

    public bool GetEngineFlipOn()
    {
        return isEngineFlip;
    }

    public int GetLEDDACValue()
    {
        return LEDDACValue;
    }

    public bool GetSettingLEDDAC()
    {
        return isSetLECDAC;
    }

    public double GetTemperatureSensorValue(int sensor)
    {
        return TemperatureSensorValue;
    }

    public void SetDeviceConnected(bool isConnected)
    {
        this.isDeviceConnected = isConnected;
    }

    public void SetDeviceLEDOn(bool isOn)
    {
        isEngineLEDOn = isOn;
    }

    public void SetEngineFlipOn(bool isOn)
    {
        isEngineFlip = isOn;
    }

    public void SetEngineFilpValue(bool isOn, bool isX)
    {
        IsFlipOn = isOn;
        IsFlipX = isX;
    }

    public void SetLEDDACValue(int value)
    {
        LEDDACValue = value;
    }

    public void SetSettingLEDDAC(bool isSet)
    {
        isSetLECDAC = isSet;
    }
}