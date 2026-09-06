using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if !TEST
using OMRON.Compolet.CIPCompolet64;
using OMRON.Compolet.CIPCompolet64.DataAccess;
#endif

namespace Vision_Align
{
    public enum TAG_NAME
    {
        DI_Stage,
        AI_Stage,
        DO_Vision,
        AO_Vision,
    }
    public enum DI_STAGE_BIT
    {
        ALIVE = 0,
        READY = 1,
        ALRAM = 2,

        STAGE_ENABLE = 4,

        CALIBRATION_REQ = 6,

        PREALIGN_REQ = 16,

        CONTACT_REQ = 20,

        UVW_MOVE_REPLY = 25,
        UVW_MOVE_OK = 26,
        UVW_MOVE_NG = 27,

        MASK_MODEL_REQ = 28,

        MAX
    }

    public enum DO_VISION_BIT
    {
        ALIVE = 0,
        READY = 1,
        ALARM = 2,
        BUSY  = 3,
        CANCEL = 5,
        CALIBRATION_REPLY = 6,
        CALIBRATION_STOP = 7,


        PREALIGN_REPLY = 17,
        PREALIGN_OK = 18,
        PREALIGN_NG = 19,

        CONTACT_REPLY = 21,
        CONTACT_OK = 22,
        CONTACT_NG = 23,

        UVW_MOVE_REQ = 24,

        MODEL_REPLY = 29,
        MODEL_OK = 30,
        MODEL_NG = 31,

        MAX,
    }

    public enum DI_STAGE_WORD
    {
        PRE_ALIGN_RETRY_COUNT = 201,
        PRE_ALIGN_RETRY_CURRENT_COUNT = 202,

        MAX,
    }
    public enum DO_VISION_WORD
    {
        ALARM_CODE = 200,

        MAX,
    }

    public enum AI_STAGE
    {
        ALIGN_STAGE_U_CURRENT_POSITION = 0, // 2 Word, Float형, X1 축
        ALIGN_STAGE_V_CURRENT_POSITION = 2, // 2 Word, Float형, Y1 축
        ALIGN_STAGE_W_CURRENT_POSITION = 4, // 2 Word, Float형, X2 축

        MAX,
    }

    public enum AO_VISION
    {
        ALIGN_STAGE_U_TARGET_POSITION = 0, // 2 Word, Float형, X1 축
        ALIGN_STAGE_V_TARGET_POSITION = 2, // 2 Word, Float형, Y1 축
        ALIGN_STAGE_W_TARGET_POSITION = 4, // 2 Word, Float형, X2 축

        PRE_ALIGN_TIME                  =40,
        PRE_ALIGN_CAM1_GLASS_X          =42,
        PRE_ALIGN_CAM1_GLASS_Y          =44,
        PRE_ALIGN_CAM1_GLASS_SCORE      =46,
        PRE_ALIGN_CAM1_MASK_X           =48,
        PRE_ALIGN_CAM1_MASK_Y           =50,
        PRE_ALIGN_CAM1_MASK_SCORE       =52,
        PRE_ALIGN_CAM2_GLASS_X          =54,
        PRE_ALIGN_CAM2_GLASS_Y          =56,
        PRE_ALIGN_CAM2_GLASS_SCORE      =58,
        PRE_ALIGN_CAM2_MASK_X           =60,
        PRE_ALIGN_CAM2_MASK_Y           =62,
        PRE_ALIGN_CAM2_MASK_SCORE       =64,
        PRE_ALIGN_ERROR_X               =66,
        PRE_ALIGN_ERROR_Y               =68,
        PRE_ALIGN_ERROR_T               =70,

        CONTACT_ALIGN_TIME              =80,
        CONTACT_ALIGN_CAM1_GLASS_X      =82,
        CONTACT_ALIGN_CAM1_GLASS_Y      =84,
        CONTACT_ALIGN_CAM1_GLASS_SCORE  =86,
        CONTACT_ALIGN_CAM1_MASK_X       =88,
        CONTACT_ALIGN_CAM1_MASK_Y       =90,
        CONTACT_ALIGN_CAM1_MASK_SCORE   =92,
        CONTACT_ALIGN_CAM2_GLASS_X      =94,
        CONTACT_ALIGN_CAM2_GLASS_Y      =96,
        CONTACT_ALIGN_CAM2_GLASS_SCORE  =98,
        CONTACT_ALIGN_CAM2_MASK_X       =100,
        CONTACT_ALIGN_CAM2_MASK_Y       =102,
        CONTACT_ALIGN_CAM2_MASK_SCORE   =104,
        CONTACT_ALIGN_ERROR_X           =106,
        CONTACT_ALIGN_ERROR_Y           =108,
        CONTACT_ALIGN_ERROR_T           =110,

        MAX,
    }


    public class ClsOmron
    {

        bool[] _DI_Bit = new bool[32];
        bool[] _DO_Bit = new bool[32];
        int[] _DI_Word = new int[700];
        int[] _DO_Word = new int[700];

        int[] _AI_Word = new int[700];
        int[] _AO_Word = new int[700];

        bool[] _DI_Bit_Prev = new bool[32];
        bool[] _DO_Bit_Prev = new bool[32];
        int[] _DI_Word_Prev = new int[700];
        int[] _DO_Word_Prev = new int[700];

        int[] _AI_Word_Prev = new int[700];
        int[] _AO_Word_Prev = new int[700];

        public bool[] DI_Bit { get => _DI_Bit; }
        public bool[] DO_Bit { get => _DO_Bit; }

        public int[] DI_Word { get => _DI_Word; }
        public int[] DO_Word { get => _DO_Word; }
        public int[] AI_Word { get => _AI_Word; }
        public int[] AO_Word { get => _AO_Word; }

        public event EventHandler<string> ChangedIoMsg;

#if !TEST
        public bool IsConnect { get => DataAccessCompolet1.IsConnected; }

        List<string> m_listName = new List<string>();

        DataAccessCompolet DataAccessCompolet1;
        System.ComponentModel.IContainer components;


        public ClsOmron()
        {
            components = new System.ComponentModel.Container();
            DataAccessCompolet1 = new DataAccessCompolet(components);
        }


        public void Open(string strIP)
        {
            DataAccessCompolet1.AccessType = AccessType.SysmacGateway;
            DataAccessCompolet1.ConnectionType = ConnectionType.UCMM;
            DataAccessCompolet1.UseRoutePath = false;
            DataAccessCompolet1.PeerAddress = strIP;
            DataAccessCompolet1.LocalPort = 2;
            DataAccessCompolet1.Active = true;

            GetVariableNames();
        }

        private bool IsMustElementAccess(VariableInfo info)
        {
            bool toReturn = false;
            if (info.IsArray)
            {
                if (info.Type == VariableType.STRING || info.Type == VariableType.UNION)
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        private bool IsMustMemberAccess(VariableInfo info)
        {
            bool toReturn = false;

            if (info.Type == VariableType.UNION)
            {
                toReturn = true;
            }

            return toReturn;
        }

        private string GetAccessablePath(string path)
        {
            string newPath = "";
            newPath += path;
            VariableInfo info = this.DataAccessCompolet1.GetVariableInfo(path);
            if (this.IsMustElementAccess(info))
            {
                // get only first element
                for (int i = 0; i < info.Dimension; i++)
                {
                    newPath += "[" + info.StartArrayElements[i].ToString() + "]";
                }
                return this.GetAccessablePath(newPath);
            }
            else if (this.IsMustMemberAccess(info))
            {
                // get only first member
                newPath += "." + info.StructMembers[0].Name;
                return this.GetAccessablePath(newPath);
            }
            else
            {
                return newPath;
            }
        }

        public void GetVariableNames()
        {
            if (!DataAccessCompolet1.IsConnected) return;
            //List< string > listName = new List<string> ();
            foreach (string var in this.DataAccessCompolet1.VariableNames)
            {
                string accessablePath = this.GetAccessablePath(var);
                m_listName.Add(var);
            }
        }

        public void ReadVariable(string strVariable, out object val)
        {
            val = new object();
            val = DataAccessCompolet1.ReadVariable(strVariable);
            if (val == null)
            {
                throw new NotSupportedException();
            }
        }

        public void WriteVariable(string strVariable, object val)
        {
            this.DataAccessCompolet1.WriteVariable(strVariable, val);
        }

#else
        // TEST 모드용 스텁 구현
        public bool IsConnect { get => false; }

        public ClsOmron()
        {
        }

        public void Open(string strIP)
        {
        }

        public void GetVariableNames()
        {
        }

        public void ReadVariable(string strVariable, out object val)
        {
            val = null;
        }

        public void WriteVariable(string strVariable, object val)
        {
        }
#endif

        public bool[] GetBits(ushort word, bool bigEndian = false)
        {
            bool[] bits = new bool[16];

            for (int i = 0; i < 16; i++)
            {
                int bitIndex = bigEndian ? (15 - i) : i;
                bits[i] = ((word >> bitIndex) & 1) == 1;
            }

            return bits;
        }

        public ushort BitsToShort(bool[] bits, bool bigEndian = false)
        {
            if (bits == null || bits.Length != 16)
                throw new ArgumentException("bits must be 16 length");

            int word = 0;

            for (int i = 0; i < 16; i++)
            {
                int bitIndex = bigEndian ? (15 - i) : i;

                if (bits[i])
                    word |= (1 << bitIndex);
            }

            return (ushort)word;
        }
        public float WordToFloat(int highWord, int lowWord)
        {
            return WordToFloat((ushort)highWord, (ushort)lowWord);
        }
        public float WordToFloat(ushort highWord, ushort lowWord)
        {
            byte[] bytes = new byte[4];

            bytes[0] = (byte)(highWord & 0xFF);
            bytes[1] = (byte)(highWord >> 8);
            bytes[2] = (byte)(lowWord & 0xFF);
            bytes[3] = (byte)(lowWord >> 8);

            return BitConverter.ToSingle(bytes, 0);
        }

        public float WordToFloat(AI_STAGE idx)
        {
            return WordToFloat(_AI_Word[(int)idx], _AI_Word[(int)idx + 1]);
        }

        public float WordToFloat(AO_VISION idx)
        {
            return WordToFloat(_AO_Word[(int)idx], _AO_Word[(int)idx + 1]);
        }

        public void FloatToWord(float value, ref int[] ints, AO_VISION idx)
        {
            FloatToWord(value, ref ints[(int)idx], ref ints[(int)idx + 1]);
        }

        public void FloatToWord(float value, ref int highWord, ref int lowWord)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            highWord = BitConverter.ToUInt16(bytes, 0);
            lowWord = BitConverter.ToUInt16(bytes, 2);
        }

        object _UpdateIoLock = new object();
        public void UpdateIO()
        {
            lock (_UpdateIoLock)
            {
                if (!IsConnect)
                    return;
                UpdateInput();
                UpdateOutput();
                UpdateAI();
                UpdateAO();
            }
        }

        private void WriteOuput()
        {

        }

        private void UpdateInput()
        {
#if !TEST
            ReadVariable(TAG_NAME.DI_Stage.ToString(), out object val);

            if (val == null) return;

            if (val.GetType().IsArray)
            {
                _DI_Word = val as int[];

                bool[] bits0 = GetBits((ushort)_DI_Word[0]);
                bool[] bits1 = GetBits((ushort)_DI_Word[1]);
                Array.Copy(bits0, 0, _DI_Bit, 0, 16);
                Array.Copy(bits1, 0, _DI_Bit, 16, 16);

                // DI_Stage Bit 업데이트
                Global.inforPLC.InAlive = _DI_Bit[(int)DI_STAGE_BIT.ALIVE];
                Global.inforPLC.InReady = _DI_Bit[(int)DI_STAGE_BIT.READY];
                Global.inforPLC.InAlarm = _DI_Bit[(int)DI_STAGE_BIT.ALRAM];
                Global.inforPLC.InStageEnable = _DI_Bit[(int)DI_STAGE_BIT.STAGE_ENABLE];
                Global.inforPLC.InCalibrationRequest = _DI_Bit[(int)DI_STAGE_BIT.CALIBRATION_REQ];
                Global.inforPLC.InPreAlignRequest = _DI_Bit[(int)DI_STAGE_BIT.PREALIGN_REQ];
                Global.inforPLC.InContactRequest = _DI_Bit[(int)DI_STAGE_BIT.CONTACT_REQ];
                Global.inforPLC.InUvwMoveReply = _DI_Bit[(int)DI_STAGE_BIT.UVW_MOVE_REPLY];
                Global.inforPLC.InUvwMoveOk = _DI_Bit[(int)DI_STAGE_BIT.UVW_MOVE_OK];
                Global.inforPLC.InUvwMoveNg = _DI_Bit[(int)DI_STAGE_BIT.UVW_MOVE_NG];
                Global.inforPLC.InMaskModelRequest = _DI_Bit[(int)DI_STAGE_BIT.MASK_MODEL_REQ];

                // DI_Stage Word 업데이트
                Global.inforPLC.InPmcRetry = _DI_Word[(int)DI_STAGE_WORD.PRE_ALIGN_RETRY_COUNT];
                Global.inforPLC.InPmcCurrentRetry = _DI_Word[(int)DI_STAGE_WORD.PRE_ALIGN_RETRY_CURRENT_COUNT];

                CheckChangedIO_DI(_DI_Bit, ref _DI_Bit_Prev);
                CheckChangedIO_DIWORD(_DI_Word, ref _DI_Word_Prev);
            }
#endif
        }

        private void UpdateOutput()
        {
#if !TEST
            ReadVariable(TAG_NAME.DO_Vision.ToString(), out object val);

            if (val == null) return;

            if (val.GetType().IsArray)
            {
                _DO_Word = val as int[];

                bool[] bits0 = GetBits((ushort)_DO_Word[0]);
                bool[] bits1 = GetBits((ushort)_DO_Word[1]);
                Array.Copy(bits0, 0, _DO_Bit, 0, 16);
                Array.Copy(bits1, 0, _DO_Bit, 16, 16);


                int[] writeWords = new int[_DO_Word.Length];
                bool[] bits = new bool[_DO_Bit.Length];

                // DO_Vision Bit 업데이트
                bits[(int)DO_VISION_BIT.ALIVE] = Global.inforPLC.OutAlive;
                bits[(int)DO_VISION_BIT.READY] = Global.inforPLC.OutReady;
                bits[(int)DO_VISION_BIT.ALARM] = Global.inforPLC.OutAlarm;
                bits[(int)DO_VISION_BIT.BUSY] = Global.inforPLC.OutBusy;
                bits[(int)DO_VISION_BIT.CANCEL] = Global.inforPLC.OutCancel;
                bits[(int)DO_VISION_BIT.CALIBRATION_REPLY] = Global.inforPLC.OutCalibrationReply;
                bits[(int)DO_VISION_BIT.CALIBRATION_STOP] = Global.inforPLC.OutCalibrationStop;
                bits[(int)DO_VISION_BIT.PREALIGN_REPLY] = Global.inforPLC.OutPrealignReply;
                bits[(int)DO_VISION_BIT.PREALIGN_OK] = Global.inforPLC.OutPrealignOk;
                bits[(int)DO_VISION_BIT.PREALIGN_NG] = Global.inforPLC.OutPrealignNg;
                bits[(int)DO_VISION_BIT.CONTACT_REPLY] = Global.inforPLC.OutContactReply;
                bits[(int)DO_VISION_BIT.CONTACT_OK] = Global.inforPLC.OutContactOk;
                bits[(int)DO_VISION_BIT.CONTACT_NG] = Global.inforPLC.OutContactNg;
                bits[(int)DO_VISION_BIT.UVW_MOVE_REQ] = Global.inforPLC.OutUvwMoveRequest;
                bits[(int)DO_VISION_BIT.MODEL_REPLY] = Global.inforPLC.OutModelReply;
                bits[(int)DO_VISION_BIT.MODEL_OK] = Global.inforPLC.OutModelOk;
                bits[(int)DO_VISION_BIT.MODEL_NG] = Global.inforPLC.OutModelNg;


                bool[] b0 = new bool[16];
                bool[] b1 = new bool[16];
                Array.Copy(bits, 0, b0, 0, 16);
                Array.Copy(bits, 16, b1, 0, 16);
                writeWords[0] = BitsToShort(b0);
                writeWords[1] = BitsToShort(b1);

                // DO_Vision Word 업데이트
                writeWords[(int)DO_VISION_WORD.ALARM_CODE] = Global.inforPLC.OutAlarmCode;

                WriteVariable(TAG_NAME.DO_Vision.ToString(), writeWords);

                CheckChangedIO_DO(_DO_Bit, ref _DO_Bit_Prev);
                CheckChangedIO_DOWORD(_DO_Word, ref _DO_Word_Prev);
            }
#endif
        }

        private void UpdateAI()
        {
#if !TEST
            ReadVariable(TAG_NAME.AI_Stage.ToString(), out object val);

            if (val == null) return;

            if (val.GetType().IsArray)
            {
                _AI_Word = val as int[];

                Global.inforPLC.InStageCurrentU = WordToFloat(AI_STAGE.ALIGN_STAGE_U_CURRENT_POSITION);
                Global.inforPLC.InStageCurrentV = WordToFloat(AI_STAGE.ALIGN_STAGE_V_CURRENT_POSITION);
                Global.inforPLC.InStageCurrentW = WordToFloat(AI_STAGE.ALIGN_STAGE_W_CURRENT_POSITION);

                CheckChangedIO_AI(_AI_Word, ref _AI_Word_Prev);
            }
#endif
        }

        private void UpdateAO()
        {
#if !TEST
            ReadVariable(TAG_NAME.AO_Vision.ToString(), out object val);

            if (val == null) return;

            if (val.GetType().IsArray)
            {
                _AO_Word = val as int[];

                int[] writeWords = new int[_DO_Word.Length];

                FloatToWord((float)Global.inforPLC.OutStageTargetU, ref writeWords, AO_VISION.ALIGN_STAGE_U_TARGET_POSITION);
                FloatToWord((float)Global.inforPLC.OutStageTargetV, ref writeWords, AO_VISION.ALIGN_STAGE_V_TARGET_POSITION);
                FloatToWord((float)Global.inforPLC.OutStageTargetW, ref writeWords, AO_VISION.ALIGN_STAGE_W_TARGET_POSITION);

                FloatToWord((float)Global.inforPLC.OutPreAlignTime, ref writeWords, AO_VISION.PRE_ALIGN_TIME);

                FloatToWord((float)Global.inforPLC.OutPreAlignCam1GlassX, ref writeWords, AO_VISION.PRE_ALIGN_CAM1_GLASS_X);
                FloatToWord((float)Global.inforPLC.OutPreAlignCam1GlassY, ref writeWords, AO_VISION.PRE_ALIGN_CAM1_GLASS_Y);
                FloatToWord((float)Global.inforPLC.OutPreAlignCam1GlassScore, ref writeWords, AO_VISION.PRE_ALIGN_CAM1_GLASS_SCORE);
                FloatToWord((float)Global.inforPLC.OutPreAlignCam1MaskX, ref writeWords, AO_VISION.PRE_ALIGN_CAM1_MASK_X);
                FloatToWord((float)Global.inforPLC.OutPreAlignCam1MaskY, ref writeWords, AO_VISION.PRE_ALIGN_CAM1_MASK_Y);
                FloatToWord((float)Global.inforPLC.OutPreAlignCam1MaskScore, ref writeWords, AO_VISION.PRE_ALIGN_CAM1_MASK_SCORE);

                FloatToWord((float)Global.inforPLC.OutPreAlignCam2GlassX, ref writeWords, AO_VISION.PRE_ALIGN_CAM2_GLASS_X);
                FloatToWord((float)Global.inforPLC.OutPreAlignCam2GlassY, ref writeWords, AO_VISION.PRE_ALIGN_CAM2_GLASS_Y);
                FloatToWord((float)Global.inforPLC.OutPreAlignCam2GlassScore, ref writeWords, AO_VISION.PRE_ALIGN_CAM2_GLASS_SCORE);
                FloatToWord((float)Global.inforPLC.OutPreAlignCam2MaskX, ref writeWords, AO_VISION.PRE_ALIGN_CAM2_MASK_X);
                FloatToWord((float)Global.inforPLC.OutPreAlignCam2MaskY, ref writeWords, AO_VISION.PRE_ALIGN_CAM2_MASK_Y);
                FloatToWord((float)Global.inforPLC.OutPreAlignCam2MaskScore, ref writeWords, AO_VISION.PRE_ALIGN_CAM2_MASK_SCORE);

                FloatToWord((float)Global.inforPLC.OutPreAlignErrorX, ref writeWords, AO_VISION.PRE_ALIGN_ERROR_X);
                FloatToWord((float)Global.inforPLC.OutPreAlignErrorY, ref writeWords, AO_VISION.PRE_ALIGN_ERROR_Y);
                FloatToWord((float)Global.inforPLC.OutPreAlignErrorT, ref writeWords, AO_VISION.PRE_ALIGN_ERROR_T);

                FloatToWord((float)Global.inforPLC.OutContactAlignTime, ref writeWords, AO_VISION.CONTACT_ALIGN_TIME);

                FloatToWord((float)Global.inforPLC.OutContactAlignCam1GlassX, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM1_GLASS_X);
                FloatToWord((float)Global.inforPLC.OutContactAlignCam1GlassY, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM1_GLASS_Y);
                FloatToWord((float)Global.inforPLC.OutContactAlignCam1GlassScore, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM1_GLASS_SCORE);
                FloatToWord((float)Global.inforPLC.OutContactAlignCam1MaskX, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM1_MASK_X);
                FloatToWord((float)Global.inforPLC.OutContactAlignCam1MaskY, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM1_MASK_Y);
                FloatToWord((float)Global.inforPLC.OutContactAlignCam1MaskScore, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM1_MASK_SCORE);

                FloatToWord((float)Global.inforPLC.OutContactAlignCam2GlassX, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM2_GLASS_X);
                FloatToWord((float)Global.inforPLC.OutContactAlignCam2GlassY, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM2_GLASS_Y);
                FloatToWord((float)Global.inforPLC.OutContactAlignCam2GlassScore, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM2_GLASS_SCORE);
                FloatToWord((float)Global.inforPLC.OutContactAlignCam2MaskX, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM2_MASK_X);
                FloatToWord((float)Global.inforPLC.OutContactAlignCam2MaskY, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM2_MASK_Y);
                FloatToWord((float)Global.inforPLC.OutContactAlignCam2MaskScore, ref writeWords, AO_VISION.CONTACT_ALIGN_CAM2_MASK_SCORE);

                FloatToWord((float)Global.inforPLC.OutContactAlignErrorX, ref writeWords, AO_VISION.CONTACT_ALIGN_ERROR_X);
                FloatToWord((float)Global.inforPLC.OutContactAlignErrorY, ref writeWords, AO_VISION.CONTACT_ALIGN_ERROR_Y);
                FloatToWord((float)Global.inforPLC.OutContactAlignErrorT, ref writeWords, AO_VISION.CONTACT_ALIGN_ERROR_T);

                WriteVariable(TAG_NAME.AO_Vision.ToString(), writeWords);

                CheckChangedIO_AO(_AO_Word, ref _AO_Word_Prev);

            }
#endif
        }

        private void CheckChangedIO_DI(bool[] currentData, ref bool[] prevData) 
        {
            if (currentData == null || prevData == null) return;
            if (currentData.Length != prevData.Length) return;

            for (int i = 0; i < currentData.Length; i++)
            {
                if (currentData[i] != prevData[i])
                {
                    DI_STAGE_BIT enumName;
                    if (Enum.TryParse(i.ToString(), out enumName))
                    {
                        int v;
                        if(int.TryParse(enumName.ToString(),out v))
                            ChangedIoMsg?.Invoke(this, $"[DI_???_{i.ToString()}]\t{(currentData[i] ? "ON" : "OFF")}");
                        else
                            ChangedIoMsg?.Invoke(this, $"[DI_{enumName.ToString()}_{i.ToString()}]\t{(currentData[i] ? "ON" : "OFF")}");
                    }
                    else
                    {
                        ChangedIoMsg?.Invoke(this, $"[DI_???_{i.ToString()}]\t{(currentData[i] ? "ON" : "OFF")}");
                    }
                }
            }

            prevData = (bool[])currentData.Clone();
        }

        private void CheckChangedIO_DO(bool[] currentData, ref bool[] prevData) 
        {
            if (currentData == null || prevData == null) return;
            if (currentData.Length != prevData.Length) return;

            for (int i = 0; i < currentData.Length; i++)
            {
                if (currentData[i] != prevData[i])
                {
                    DO_VISION_BIT enumName;
                    if (Enum.TryParse(i.ToString(), out enumName))
                    {
                        int v;
                        if (int.TryParse(enumName.ToString(), out v))
                            ChangedIoMsg?.Invoke(this, $"[DO_???_{i.ToString()}]\t{(currentData[i] ? "ON" : "OFF")}");
                        else
                            ChangedIoMsg?.Invoke(this, $"[DO_{enumName.ToString()}_{i.ToString()}]\t{(currentData[i] ? "ON" : "OFF")}");
                    }
                    else
                    {
                        ChangedIoMsg?.Invoke(this, $"[DO_???_{i.ToString()}]\t{(currentData[i] ? "ON" : "OFF")}");
                    }
                }
            }

            prevData = (bool[])currentData.Clone();
        }

        private void CheckChangedIO_AI(int[] currentData, ref int[] prevData) 
        {
            if (currentData == null || prevData == null) return;
            if (currentData.Length != prevData.Length) return;

            for (int i = 0; i < currentData.Length; i++)
            {
                if (currentData[i] != prevData[i])
                {
                    AI_STAGE enumName;
                    if (Enum.TryParse(i.ToString(), out enumName))
                    {
                        int v;
                        if (int.TryParse(enumName.ToString(), out v))
                            ChangedIoMsg?.Invoke(this, $"[AI_???_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                        else
                            ChangedIoMsg?.Invoke(this, $"[AI_{enumName.ToString()}_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                    }
                    else
                    {
                        ChangedIoMsg?.Invoke(this, $"[AI_???_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                    }
                }
            }

            prevData = (int[])currentData.Clone();
        }

        private void CheckChangedIO_DIWORD(int[] currentData, ref int[] prevData)
        {
            if (currentData == null || prevData == null) return;
            if (currentData.Length != prevData.Length) return;

            for (int i = 2; i < currentData.Length; i++)
            {
                if (currentData[i] != prevData[i])
                {
                    DI_STAGE_WORD enumName;
                    if (Enum.TryParse(i.ToString(), out enumName))
                    {
                        int v;
                        if (int.TryParse(enumName.ToString(), out v))
                            ChangedIoMsg?.Invoke(this, $"[DI_???_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                        else
                            ChangedIoMsg?.Invoke(this, $"[DI_{enumName.ToString()}_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                    }
                    else
                    {
                        ChangedIoMsg?.Invoke(this, $"[DI_???_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                    }
                }
            }

            prevData = (int[])currentData.Clone();
        }

        private void CheckChangedIO_DOWORD(int[] currentData, ref int[] prevData) 
        {
            if (currentData == null || prevData == null) return;
            if (currentData.Length != prevData.Length) return;

            for (int i = 2; i < currentData.Length; i++)
            {
                if (currentData[i] != prevData[i])
                {
                    DO_VISION_WORD enumName;
                    if (Enum.TryParse(i.ToString(), out enumName))
                    {
                        int v;
                        if (int.TryParse(enumName.ToString(), out v))
                            ChangedIoMsg?.Invoke(this, $"[DO_???_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                        else
                            ChangedIoMsg?.Invoke(this, $"[DO_{enumName.ToString()}_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                    }
                    else
                    {
                        ChangedIoMsg?.Invoke(this, $"[DO_???_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                    }
                }
            }

            prevData = (int[])currentData.Clone();
        }

        private void CheckChangedIO_AO(int[] currentData, ref int[] prevData) 
        {
            if (currentData == null || prevData == null) return;
            if (currentData.Length != prevData.Length) return;

            for (int i = 0; i < currentData.Length; i++)
            {
                if (currentData[i] != prevData[i])
                {
                    AO_VISION enumName;
                    if (Enum.TryParse(i.ToString(), out enumName))
                    {
                        int v;
                        if (int.TryParse(enumName.ToString(), out v))
                            ChangedIoMsg?.Invoke(this, $"[AO_???_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                        else
                            ChangedIoMsg?.Invoke(this, $"[AO_{enumName.ToString()}_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                    }
                    else
                    {
                        ChangedIoMsg?.Invoke(this, $"[AO_???_{i.ToString()}]\t{(prevData[i].ToString())} -> {(currentData[i].ToString())}");
                    }
                }
            }

            prevData = (int[])currentData.Clone();
        }
    }
}
