using HalconDotNet;
using peak.ipl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uEye.Types.Focus;
using Vision_Align._2.FormViewer;
using VoAlgorithm;

namespace Vision_Align
{
    public enum Viewer { Main, Alram, IO, Log, MAX };
    public enum LogType { SYSTEM = 0, TCP, PLC, EXCEPTION, DB, Serial, ALARM, MAX };

    public enum CamInfo { CAM_1, CAM_2, MAX};

    public enum CamSet { CALIBRATION, PRE_ALIGN, CONTACT_ALIGN, MAX };

    public enum HWindowType { MAIN_1, MAIN_2, PRE_1, PRE_2, LOG_1, LOG_2, MAX };

    public static class Global
    {
        // 기본 메뉴, 타이틀 사용 여부
        public static bool bUseFormTitle = true ;
        public static bool bUseFormMenu  = true;

        public static bool bMovePgm = false;
        public static System.Drawing.Point point;

        #region Define
        public static readonly int MAX_CAM = 2;
        #endregion

        #region FilePaht
        public static string strRecipePath = Environment.CurrentDirectory + "\\RECIPE";
        public static string strConfigPath = Environment.CurrentDirectory + "\\CONFIG";
        public static string strRsltPath   = Environment.CurrentDirectory + "\\RESULT";

        public static string[] strMatchPath = new string[(int)MatchingType.Max];
        #endregion

        #region Form Variable
        public static FormBase formBase;
        public static FormMenu formMenu;
        public static FormTitle formTitle;

        public static FormMain formMain;
        public static FormIO formIO;
        public static FormLogView formLog;

        public static FormSetCam formSetCam;

        public static FormDisplay[] formDisplay = new FormDisplay[MAX_CAM];
        public static FormPreview[] formPreView = new FormPreview[2];

        public static FormModel formModel;//= new FormModel();
        public static FormAlarmView formAlarmView;

        public static FormHalconDisplay[] formHDisplay = new FormHalconDisplay[(int)HWindowType.MAX];

        #endregion

        #region Config
        public static Infor.Camera inforCam = new Infor.Camera();
        public static Infor.Result inforResult = new Infor.Result();
        public static Infor.PLC_INFO inforPLC = new Infor.PLC_INFO();
        public static Infor.MotionState inforMotion = new Infor.MotionState();
        public static Infor.UVW_Info inforUvw = new Infor.UVW_Info();

        public static Config.SYSTEM_LIST Sys_Param = new Config.SYSTEM_LIST();
        public static Config.CAMSET_LIST CamSet_Param = new Config.CAMSET_LIST();
        public static Config.SERIAL_LIST Serial_Param = new Config.SERIAL_LIST();
        public static Config.PRE_CONFIG_LIST PreConfig_Param = new Config.PRE_CONFIG_LIST();
        public static Config.CALIBRATION_LIST Calibration_Param = new Config.CALIBRATION_LIST();

        public static Config.SHM_List[] recipeMask_Shm  = new Config.SHM_List[(int)CamInfo.MAX];
        public static Config.SHM_List[] recipeGlass_Shm = new Config.SHM_List[(int)CamInfo.MAX];
        public static Config.NCC_List[] recipeMask_Ncc  = new Config.NCC_List[(int)CamInfo.MAX];
        #endregion

        #region Class Variable
        public static ClsLight clsLight = new ClsLight();
        public static ClsOmron clsOmron = new ClsOmron();
        public static Dictionary<CamInfo, ClsMotion> dicClsMotion = new Dictionary<CamInfo, ClsMotion>();
        public static Dictionary<CamInfo, ClsCamera> dicClsCam = new Dictionary<CamInfo, ClsCamera>();

        public static ClsAutoThread clsAutoThread;
        public static ClsOmronThread  clsOmronThread;
        public static ClsFolderThread clsFolderThread;

        public static ClsAlarmManager clsAlarmManager = new ClsAlarmManager();
        public static VisionAlgorithm[] clsAlgorithm = new VisionAlgorithm[(int)CamInfo.MAX];
        public static ClsCsvFile clsCSV = new ClsCsvFile();
        internal static ClsUvwStage clsUvwStage = new ClsUvwStage();
        internal static ClsVision clsVision = new ClsVision();
        #endregion

        #region Variable

        public static HModelInfo[,] hModelInfo = new HModelInfo[(int)CamInfo.MAX, (int)MatchingType.Max];

        /// <summary>
        /// 동작 모드
        /// </summary>
        public static bool bAutoMode = false;   // false : Manual  / true : Auto

        /// <summary>
        /// LOG Queue
        /// </summary>
        public static Queue<string> queueProcessLog = new Queue<string>();

        /// <summary>
        /// Cam 연결 상태
        /// </summary>
        public static bool bConnectCam;

        /// <summary>
        /// 모션 연결 상태
        /// </summary>
        public static bool bConnectAxisZ;

        /// <summary>
        /// PLC 연결 상태
        /// </summary>
        public static bool bConnectPLC;

        /// <summary>
        /// 조명 연결 상태
        /// </summary>
        public static bool bConnectLight;

        /// <summary>
        /// CalibrationManualMode
        /// </summary>
        public static bool bCalibrationManualMode;

        /// <summary>
        /// Log 변수 LogType : SYSTEM = 0, PROCESS = 1, MOTION = 2, TCP, PLC, EXCEPTION, DB, MAX
        /// </summary>
        public static Dictionary<LogType, ClsLogger> logger;

        /// <summary>
        /// 현재 화면 위치 Viewer : Main, Alram, Manual, IO, Recipe, Comm, MAX
        /// </summary>
        public static Viewer m_nCurrViewer = Viewer.MAX;
        public static Viewer m_nChangViewer = Viewer.Main;
        #endregion

        #region Func
        public static void IsLoadConfig(bool IsLoad)
        {
            Sys_Param         = IsLoad_List(IsLoad, Sys_Param        );
            CamSet_Param      = IsLoad_List(IsLoad, CamSet_Param     );
            Serial_Param      = IsLoad_List(IsLoad, Serial_Param     );
            PreConfig_Param   = IsLoad_List(IsLoad, PreConfig_Param  );
            Calibration_Param = IsLoad_List(IsLoad, Calibration_Param);
            inforUvw          = IsLoad_List(IsLoad, inforUvw         );

            strMatchPath[(int)MatchingType.MarkGray  ] = Environment.CurrentDirectory + "\\RECIPE\\MaskG" ;
            strMatchPath[(int)MatchingType.MaskShape ] = Environment.CurrentDirectory + "\\RECIPE\\MaskS" ;
            strMatchPath[(int)MatchingType.GlassShape] = Environment.CurrentDirectory + "\\RECIPE\\GlassS";

            string strPath = "";
            for (int nType = 0; nType < (int)MatchingType.Max; nType++)
            {
                for (int nCam = 0; nCam < (int)CamInfo.MAX; nCam++)
                {
                    Global.clsAlgorithm[nCam] = new VisionAlgorithm();
                    strPath = string.Format("{0}\\{1}\\{2}", strMatchPath[nType], Sys_Param.strCurrModel[nCam, nType], Sys_Param.strCurrModel[nCam, nType]);

                    Global.clsAlgorithm[nCam].LoadModel((MatchingType)nType, strPath, out Global.hModelInfo[nCam, nType]);

                    if      (nType == (int)MatchingType.MarkGray  ) { recipeMask_Ncc [nCam] = new Config.NCC_List(); recipeMask_Ncc [nCam] = IsLoad_List(true, recipeMask_Ncc [nCam], strPath); }
                    else if (nType == (int)MatchingType.MaskShape ) { recipeMask_Shm [nCam] = new Config.SHM_List(); recipeMask_Shm [nCam] = IsLoad_List(true, recipeMask_Shm [nCam], strPath); }
                    else if (nType == (int)MatchingType.GlassShape) { recipeGlass_Shm[nCam] = new Config.SHM_List(); recipeGlass_Shm[nCam] = IsLoad_List(true, recipeGlass_Shm[nCam], strPath); }
                
                }
            }

            clsAlgorithm[0].StageBasicInfo(inforUvw.pinDeg, inforUvw.stageDeg, inforUvw.dRadius);
            clsAlgorithm[1].StageBasicInfo(inforUvw.pinDeg, inforUvw.stageDeg, inforUvw.dRadius);
        }

        public static T IsLoad_List<T>(bool isLoad, T obj, string strFullPaht = "")
        {
            string strPath = "";
            if(strFullPaht == "") strPath = System.IO.Path.Combine(Global.strConfigPath, typeof(T).Name + ".json");
            else                  strPath = System.IO.Path.Combine(strFullPaht + ".json"); ;
                try
                {
                    if (isLoad)
                    {
                        if (System.IO.File.Exists(strPath))
                            obj = JsonConvertor.JsonToClass<T>(strPath);
                    }
                    else
                    {
                        JsonConvertor.ClassToJson(obj, strPath);
                    }
                }
                catch (Exception e)
                {
                    Global.logger[LogType.EXCEPTION].Write($"IsLoad_List<{typeof(T).Name}> : " + e.ToString());
                }
            return obj;
        }
        #endregion
    }

    public class Infor
    {
        public class Camera
        {
            public bool bState = false;

            public bool bLive = false;

            public string[] strCamSerial = new string[(int)CamInfo.MAX]; 

            public Camera()
            {
                strCamSerial[0] = "4104018345";
                strCamSerial[1] = "4103133522";
            }
        }

        public class MotionState
        {
            public bool[] bHome   = new bool[(int)CamInfo.MAX];
            public bool[] bServo  = new bool[(int)CamInfo.MAX];
            public bool[] bLimitP = new bool[(int)CamInfo.MAX];
            public bool[] bLimitM = new bool[(int)CamInfo.MAX];
            public bool[] bInPos  = new bool[(int)CamInfo.MAX];
            public bool[] bBasy   = new bool[(int)CamInfo.MAX];
            public bool[] bAlram  = new bool[(int)CamInfo.MAX];
            public double[] dCmdPos = new double[(int)CamInfo.MAX];  

            public MotionState()
            {
                for (int n = 0; n < (int)CamInfo.MAX; n++)
                {
                    bHome  [n] = false;
                    bServo [n] = false;
                    bLimitP[n] = false;
                    bLimitM[n] = false;
                    bInPos [n] = false;
                    bAlram [n] = false;
                    bBasy  [n] = false;
                    dCmdPos[n] = 0;
                }
            }
        }

        public class UVW_Info
        {
            public double dRadius;//μm 단위
            public double[] pinDeg = new double[4]; //deg 단위
            public double[] stageDeg = new double[4]; //deg 단위


            public UVW_Info()
            {
                dRadius = 445112;
                pinDeg[0] = -51.842773;
                pinDeg[1] = 128.157227;
                pinDeg[2] = 231.842773;
                pinDeg[3] = 51.842773;
                stageDeg[0] = 0;
                stageDeg[1] = 0;
                stageDeg[2] = -90;
                stageDeg[3] = -90;
            }
        }

        public class Result
        {
            public double[] dTarget_X        = new double[(int)CamInfo.MAX]; // 글라스
            public double[] dTarget_Y        = new double[(int)CamInfo.MAX];
            public double[] dTarget_Scale    = new double[(int)CamInfo.MAX];
            public double[] dTarget_Score    = new double[(int)CamInfo.MAX];
            public string[] strTarget_Match  = new string[(int)CamInfo.MAX];
            public double[] dCenterToTargetX = new double[(int)CamInfo.MAX];  // 이미지 중심에서 글라스 까지의 거리
            public double[] dCenterToTargetY = new double[(int)CamInfo.MAX];  // 이미지 중심에서 글라스 까지의 거리

            public double[] dMark_X          = new double[(int)CamInfo.MAX];
            public double[] dMark_Y          = new double[(int)CamInfo.MAX];
            public double[] dMark_Scale      = new double[(int)CamInfo.MAX];
            public double[] dMark_Score      = new double[(int)CamInfo.MAX];
            public string[] strMark_Match    = new string[(int)CamInfo.MAX];
            public double[] dCenterToMarkX   = new double[(int)CamInfo.MAX];
            public double[] dCenterToMarkY   = new double[(int)CamInfo.MAX];

            public double[] dGray            = new double[(int)CamInfo.MAX];
            public double[] dFocus           = new double[(int)CamInfo.MAX];
            public double[] dProcessTime     = new double[(int)CamInfo.MAX];
            public string[] strSaveImgNo     = new string[(int)CamInfo.MAX];  // 이미지 저장 번호



            public double dFullProcessTime = 0;                               // 모든 프로세싱 타임

            public string strJudg         = "-";
            public int    nRetry          = 3;
            public string strMarkID       = "No Mark ID";
            public string strGlassID      = "No Glass ID";

            public double resultErrorX = 0;
            public double resultErrorY = 0;
            public double resultErrorT = 0;

            public double ContactErrorX = 0;
            public double ContactErrorY = 0;
            public double ContactErrorT = 0;

            public double TargetX = 0;
            public double TargetY = 0;
            public double TargetT = 0;

            public double dMove_UVW_X1 = 0;
            public double dMove_UVW_X2 = 0;
            public double dMove_UVW_Y1 = 0;
            public double dMove_UVW_Y2 = 0;

            public Result()
            {
                for(int n = 0; n < (int)CamInfo.MAX; n++ )
                {
                    dTarget_X       [n] = 0;
                    dTarget_Y       [n] = 0;
                    dTarget_Scale   [n] = 0;
                    dTarget_Score   [n] = 0;
                    dCenterToTargetX[n] = 0;
                    dCenterToTargetY[n] = 0;
                    dMark_X         [n] = 0;
                    dMark_Y         [n] = 0;
                    dMark_Scale     [n] = 0;
                    dMark_Score     [n] = 0;
                    dCenterToMarkX  [n] = 0;
                    dCenterToMarkY  [n] = 0;
                    dGray           [n] = 0;
                    dFocus          [n] = 0;
                    dProcessTime    [n] = 0;

                    strSaveImgNo    [n] = "";
                    strTarget_Match [n] = "";
                    strMark_Match   [n] = "";
                }
            }
        }

        public class PLC_INFO
        {
            #region DI_Stagepublic bool Alive;
            public bool InAlive;              // 0
            public bool InReady;              // 1
            public bool InAlarm;              // 2
            public bool InStageEnable;        // 4
            public bool InCalibrationRequest; // 6
            public bool InPreAlignRequest;    // 16
            public bool InContactRequest;     // 20
            public bool InUvwMoveReply;       // 25
            public bool InUvwMoveOk;          // 26
            public bool InUvwMoveNg;          // 27
            public bool InMaskModelRequest;   // 28

            public int InPmcCurrentRetry = 0;   // retry 카운터
            public int InPmcRetry = 0;   // retry 카운터
            #endregion

            #region DO_Vision
            public bool OutAlive;              // 0
            public bool OutReady;              // 1
            public bool OutAlarm;              // 2
            public bool OutBusy;               // 3
            public bool OutCancel;             // 5
            public bool OutCalibrationReply;   // 6
            public bool OutCalibrationStop;   // 7

            public bool OutPrealignReply;      // 17
            public bool OutPrealignOk;         // 18
            public bool OutPrealignNg;         // 19

            public bool OutContactReply;       // 21
            public bool OutContactOk;          // 22
            public bool OutContactNg;          // 23

            public bool OutUvwMoveRequest;     // 24

            public bool OutModelReply;         // 29
            public bool OutModelOk;            // 30
            public bool OutModelNg;            // 31

            public int OutAlarmCode = 0;
            #endregion

            #region AI_Stage
            /// <summary>
            /// X1
            /// </summary>
            public double InStageCurrentU = 0;  // 현재 위치 값
            /// <summary>
            /// Y1
            /// </summary>
            public double InStageCurrentV = 0;  // 현재 위치 값
            /// <summary>
            /// X2
            /// </summary>
            public double InStageCurrentW = 0;  // 현재 위치 값
            #endregion

            #region AO_Vision
            public double OutStageTargetU = 0;  // 0 목표 위치 값
            public double OutStageTargetV = 0;  // 2 목표 위치 값
            public double OutStageTargetW = 0;  // 4 목표 위치 값

            public double OutPreAlignTime = 0;              // 40

            public double OutPreAlignCam1GlassX = 0;        // 42
            public double OutPreAlignCam1GlassY = 0;        // 44        
            public double OutPreAlignCam1GlassScore = 0;    // 46
            public double OutPreAlignCam1MaskX = 0;         // 48
            public double OutPreAlignCam1MaskY = 0;         // 50
            public double OutPreAlignCam1MaskScore = 0;     // 52

            public double OutPreAlignCam2GlassX = 0;        // 54
            public double OutPreAlignCam2GlassY = 0;        // 56
            public double OutPreAlignCam2GlassScore = 0;    // 58
            public double OutPreAlignCam2MaskX = 0;         // 60
            public double OutPreAlignCam2MaskY = 0;         // 62
            public double OutPreAlignCam2MaskScore = 0;     // 64

            public double OutPreAlignErrorX = 0;            // 66
            public double OutPreAlignErrorY = 0;            // 68
            public double OutPreAlignErrorT = 0;            // 70


            public double OutContactAlignTime = 0;          // 80

            public double OutContactAlignCam1GlassX = 0;    // 82
            public double OutContactAlignCam1GlassY = 0;    // 84
            public double OutContactAlignCam1GlassScore = 0;    // 86
            public double OutContactAlignCam1MaskX = 0;     // 88
            public double OutContactAlignCam1MaskY = 0;     // 90
            public double OutContactAlignCam1MaskScore = 0; // 92

            public double OutContactAlignCam2GlassX = 0;    // 94
            public double OutContactAlignCam2GlassY = 0;    // 96
            public double OutContactAlignCam2GlassScore = 0;    // 98
            public double OutContactAlignCam2MaskX = 0;     // 100
            public double OutContactAlignCam2MaskY = 0;     // 102
            public double OutContactAlignCam2MaskScore = 0; // 104

            public double OutContactAlignErrorX = 0;        // 106
            public double OutContactAlignErrorY = 0;        // 108
            public double OutContactAlignErrorT = 0;        // 110
            #endregion

            public void ClearOutput()
            {
                OutAlive = false;                 // 0
                OutReady = false;                 // 1
                OutAlarm = false;                 // 2
                OutBusy = false;                  // 3
                OutCancel = false;                // 5
                OutCalibrationReply = false;      // 6
                OutCalibrationStop = false;       // 7
                OutPrealignReply = false;         // 17
                OutPrealignOk = false;            // 18
                OutPrealignNg = false;            // 19
                OutContactReply = false;          // 21
                OutContactOk = false;             // 22
                OutContactNg = false;             // 23
                OutUvwMoveRequest = false;        // 24
                OutModelReply = false;            // 29
                OutModelOk = false;               // 30
                OutModelNg = false;               // 31

                OutAlarmCode = 0;

                OutStageTargetU = 0;              // 0 목표 위치 값
                OutStageTargetV = 0;              // 2 목표 위치 값
                OutStageTargetW = 0;              // 4 목표 위치 값

                OutPreAlignTime = 0;              // 40

                OutPreAlignCam1GlassX = 0;        // 42
                OutPreAlignCam1GlassY = 0;        // 44        
                OutPreAlignCam1GlassScore = 0;    // 46
                OutPreAlignCam1MaskX = 0;         // 48
                OutPreAlignCam1MaskY = 0;         // 50
                OutPreAlignCam1MaskScore = 0;     // 52

                OutPreAlignCam2GlassX = 0;        // 54
                OutPreAlignCam2GlassY = 0;        // 56
                OutPreAlignCam2GlassScore = 0;    // 58
                OutPreAlignCam2MaskX = 0;         // 60
                OutPreAlignCam2MaskY = 0;         // 62
                OutPreAlignCam2MaskScore = 0;     // 64

                OutPreAlignErrorX = 0;            // 66
                OutPreAlignErrorY = 0;            // 68
                OutPreAlignErrorT = 0;            // 70


                OutContactAlignTime = 0;          // 80

                OutContactAlignCam1GlassX = 0;    // 82
                OutContactAlignCam1GlassY = 0;    // 84
                OutContactAlignCam1GlassScore = 0;    // 86
                OutContactAlignCam1MaskX = 0;     // 88
                OutContactAlignCam1MaskY = 0;     // 90
                OutContactAlignCam1MaskScore = 0; // 92

                OutContactAlignCam2GlassX = 0;    // 94
                OutContactAlignCam2GlassY = 0;    // 96
                OutContactAlignCam2GlassScore = 0;    // 98
                OutContactAlignCam2MaskX = 0;     // 100
                OutContactAlignCam2MaskY = 0;     // 102
                OutContactAlignCam2MaskScore = 0; // 104

                OutContactAlignErrorX = 0;        // 106
                OutContactAlignErrorY = 0;        // 108
                OutContactAlignErrorT = 0;        // 110
            }

        }
    }


    public class Config
    {
        public class SYSTEM_LIST
        {
            public string[,] strCurrModel = new string[(int)CamInfo.MAX, (int)MatchingType.Max];

            public bool bImgSave_Bmp = false;
            public bool bImgSave_jpg = false;
        }

        public class CAMSET_LIST
        {
            public Dictionary<CamInfo, double[]> dicExposure ;
            public Dictionary<CamInfo, double[]> dicGamma    ;
            public Dictionary<CamInfo, int   []> dicGain     ;
                                                             
            public Dictionary<CamInfo, bool  []> dicRotation ;
            public Dictionary<CamInfo, bool  []> dicFlipX    ;
            public Dictionary<CamInfo, bool  []> dicFlipY    ;

            public Dictionary<CamInfo, int   []> dicLightSpot;
            public Dictionary<CamInfo, int   []> dicLightRing;

            public Dictionary<CamInfo, double[]> dicZ_Pos    ;
            public Dictionary<CamInfo, double[]> dicZ_Speed  ;

            public CAMSET_LIST()
            {
                dicExposure  = new Dictionary<CamInfo, double[]>();
                dicGamma     = new Dictionary<CamInfo, double[]>();
                dicGain      = new Dictionary<CamInfo, int[]>();
                             
                dicRotation  = new Dictionary<CamInfo, bool[]>();
                dicFlipX     = new Dictionary<CamInfo, bool[]>();
                dicFlipY     = new Dictionary<CamInfo, bool[]>();

                dicLightSpot = new Dictionary<CamInfo, int[]>();
                dicLightRing = new Dictionary<CamInfo, int[]>();

                dicZ_Pos     = new Dictionary<CamInfo, double[]>();
                dicZ_Speed   = new Dictionary<CamInfo, double[]>();

                for(CamInfo n = 0; n < CamInfo.MAX; n++)
                {
                    dicExposure .Add(n, new double[(int)CamSet.MAX]);
                    dicGamma    .Add(n, new double[(int)CamSet.MAX]);
                    dicGain     .Add(n, new int   [(int)CamSet.MAX]);
                    dicRotation .Add(n, new bool  [(int)CamSet.MAX]);
                    dicFlipX    .Add(n, new bool  [(int)CamSet.MAX]);
                    dicFlipY    .Add(n, new bool  [(int)CamSet.MAX]);
                    dicLightSpot.Add(n, new int   [(int)CamSet.MAX]);
                    dicLightRing.Add(n, new int   [(int)CamSet.MAX]);
                    dicZ_Pos    .Add(n, new double[(int)CamSet.MAX]);
                    dicZ_Speed  .Add(n, new double[(int)CamSet.MAX]);
                }
            }
        }

        public class CALIBRATION_LIST
        {
            public double MaskHoleDistanceX = 440000.0;       //단위 μm
            public double MaskHoleDistanceY = 340000.0;       //단위 μm
            public double MaskHoleDistanceC = 556057.5509;    //단위 μm
            public double MarkScoreLimit;

            public int AverageImageCount = 10;

            public double MoveOffsetXY = 1000;
            public double MoveOffsetAngle = 0.2;
            public int MoveOffsetAngleCount = 10;

            public double StartPositionX = 0.0;
            public double StartPositionY = 0.0;
            public double StartPositionAngle = 0.0;

            //Camera Calibration Result
            public double[] CameraPixelResolutionX;
            public double[] CameraPixelResolutionY;
            public double[] CameraTilt;

            /// <summary>
            /// Camera Image Center 기준 UVW 중심과 거리 X
            /// </summary>
            public double[] CameraCoordinateOffsetX;
            /// <summary>
            /// Camera Image Center 기준 UVW 중심과 거리 X
            /// </summary>
            public double[] CameraCoordinateOffsetY;



            public CALIBRATION_LIST()
            {
                MaskHoleDistanceC = 556057.5509; //단위 μm
                MarkScoreLimit = 0.7;

                CameraTilt = new double[(int)CamSet.MAX];
                CameraPixelResolutionX = new double[(int)CamSet.MAX];
                CameraPixelResolutionY = new double[(int)CamSet.MAX];
                CameraCoordinateOffsetX = new double[(int)CamSet.MAX];
                CameraCoordinateOffsetY = new double[(int)CamSet.MAX];
            }
        }

        public class PRE_CONFIG_LIST
        {
            public double dLimitMark ;
            public double dLimitGlass;

            public double dPreAlignXY;
            public double dPreAlignTh;
            public double dContactAlignXY;
            public double dContactAlignTh;

            public int  iPreAlignMaskAverageImageCount;
            public bool bPreAlignUseMedian;

            public int  nStorageMinFreeGB;      // 잔여 공간 경고 임계값 [GB]
            public int  nStorageRetentionDays;  // 결과 파일 보관 기간 [일]

            public double dUVW_LimitPosX1;
            public double dUVW_LimitPosX2;
            public double dUVW_LimitPosY1;
            public double dUVW_LimitPosY2;
            public double dUVW_LimitNegX1;
            public double dUVW_LimitNegX2;
            public double dUVW_LimitNegY1;
            public double dUVW_LimitNegY2;

            public double dUVW_OffsetX1;
            public double dUVW_OffsetX2;
            public double dUVW_OffsetY1;
            public double dUVW_OffsetY2;

            public bool   bAlways_Live;

            public int nInPositionGap;

            public PRE_CONFIG_LIST()
            {
                dLimitMark = 0.9;
                dLimitGlass = 0.9;
                dPreAlignXY = 10;
                dPreAlignTh = 0.001;
                dContactAlignXY = 10;
                dContactAlignTh = 0.001;

                iPreAlignMaskAverageImageCount = 10;
                bPreAlignUseMedian             = false;

                nStorageMinFreeGB     = 10;
                nStorageRetentionDays = 90;

                dUVW_LimitPosX1 = 4500;
                dUVW_LimitPosX2 = 4500;
                dUVW_LimitPosY1 = 4500;
                dUVW_LimitPosY2 = 4500;
                dUVW_LimitNegX1 = -4500;
                dUVW_LimitNegX2 = -4500;
                dUVW_LimitNegY1 = -4500;
                dUVW_LimitNegY2 = -4500;

                dUVW_OffsetX1 = .10;
                dUVW_OffsetX2 = .20;
                dUVW_OffsetY1 = .30;
                dUVW_OffsetY2 = .40;

                bAlways_Live = false;

                nInPositionGap = 10;

            }
        }

        public class SERIAL_LIST
        {
            /// <summary>
            /// Com port No
            /// </summary>
            public int nPort = 0;

            /// <summary>
            /// baud 
            /// </summary>
            public int nbaud = 9600;
        }

        public class SHM_List
        {
            public double dRadius         ;
            public double dAngleMin       ;
            public double dAngleMax       ;
            public double dMaxOverlap     ;
            public double dMinScore       ;
            public double dMultiOption    ;
            public bool   bMultiOption    ;
            public int    nNumLevels      ;
            public string strMetric       ;
            public string strSubPixel     ;
            public string strBackGround   ;
            public double dYSize          ;
            public double dXSize          ;
            public double dDepth          ;
            public bool   bAutoContrast   ;
            public bool   bAutoContrastMin;
            public double dContrast       ;
            public double dContrastMin    ;
            public double dScaleMin       ;
            public double dScaleMax       ;
            public double dGreedNess      ;
            public bool   bDirLeft        ;
        }

        public class NCC_List
        {
            public double dRow          ;
            public double dCol          ;
            public double dRadius       ;
            public double dAngleMin     ;
            public double dAngleMax     ;
            public double dMaxOverlap   ;
            public double dMinScore     ;
            public double dInner_Circle ;
            public double dMultiOption  ;
            public bool   bMultiOption  ;
            public int    nNumLevels    ;
            public string strMetric     ;
            public string strSubPixel   ;
        }

        public class M_G_MODEL_LIST
        {
            public double[] dRow          = new double[(int)CamInfo.MAX];
            public double[] dCol          = new double[(int)CamInfo.MAX];
            public double[] dRadius       = new double[(int)CamInfo.MAX];
            public double[] dAngleMin     = new double[(int)CamInfo.MAX];
            public double[] dAngleMax     = new double[(int)CamInfo.MAX];
            public double[] dMaxOverlap   = new double[(int)CamInfo.MAX];
            public double[] dMinScore     = new double[(int)CamInfo.MAX];
            public double[] dInner_Circle = new double[(int)CamInfo.MAX];
            public double[] dMultiOption  = new double[(int)CamInfo.MAX];
            public bool  [] bMultiOption  = new bool  [(int)CamInfo.MAX];
            public int   [] nNumLevels    = new int   [(int)CamInfo.MAX];
            public string[] strMetric     = new string[(int)CamInfo.MAX];
            public string[] strSubPixel   = new string[(int)CamInfo.MAX];

            public M_G_MODEL_LIST()
            {
                for(int n = 0; n < (int) CamInfo.MAX; n++)
                {
                    dRow   [n] = 0;
                    dCol   [n] = 0;
                    dRadius[n] = 0;

                    dAngleMin    [n] = .0;
                    dAngleMax    [n] = .0;
                    dMaxOverlap  [n] = .0;
                    dMinScore    [n] = .0;
                    dInner_Circle[n] = 0;
                    dMultiOption [n] = .0;
                    bMultiOption [n] = false;

                    nNumLevels[n] = 0;

                    strMetric  [n] = "";
                    strSubPixel[n] = "";
                }
            }
        }

        public class M_S_MODEL_LIST
        {
            public double[] dRadius       = new double[(int)CamInfo.MAX];
            public double[] dAngleMin     = new double[(int)CamInfo.MAX];
            public double[] dAngleMax     = new double[(int)CamInfo.MAX];
            public double[] dMaxOverlap   = new double[(int)CamInfo.MAX];
            public double[] dMinScore     = new double[(int)CamInfo.MAX];
            public double[] dMultiOption  = new double[(int)CamInfo.MAX];
            public bool  [] bMultiOption  = new bool  [(int)CamInfo.MAX];
            public int   [] nNumLevels    = new int   [(int)CamInfo.MAX];
            public string[] strMetric     = new string[(int)CamInfo.MAX];
            public string[] strSubPixel   = new string[(int)CamInfo.MAX];
            public double[] dBoader       = new double[(int)CamInfo.MAX];
            public double[] dPixel        = new double[(int)CamInfo.MAX];
            public string[] strBackGround = new string[(int)CamInfo.MAX];

            public M_S_MODEL_LIST()
            {
                for (int n = 0; n < (int)CamInfo.MAX; n++)
                {
                    dRadius      [n] = .0;
                    dAngleMin    [n] = .0;
                    dAngleMax    [n] = .0;
                    dMaxOverlap  [n] = .0;
                    dMinScore    [n] = .0;
                    dMultiOption [n] = .0;
                    bMultiOption [n] = false;
                    nNumLevels   [n] = 0;
                    strMetric    [n] = "";
                    strSubPixel  [n] = "";
                    dBoader      [n] = .0;
                    dPixel       [n] = .0;
                    strBackGround[n] = "";
                }

            }
        }

        public class G_S_MODEL_LIST
        {
            public double[] dYSize           = new double[(int)CamInfo.MAX];
            public double[] dXSize           = new double[(int)CamInfo.MAX];
            public double[] dDepth           = new double[(int)CamInfo.MAX];
            public double[] dAngleMin        = new double[(int)CamInfo.MAX];
            public double[] dAngleMax        = new double[(int)CamInfo.MAX];
            public double[] dMaxOverlap      = new double[(int)CamInfo.MAX];
            public double[] dMinScore        = new double[(int)CamInfo.MAX];
            public int   [] nNumLevels       = new int   [(int)CamInfo.MAX];
            public string[] strMetric        = new string[(int)CamInfo.MAX];
            public string[] strSubPixel      = new string[(int)CamInfo.MAX];
            public double[] dBoader          = new double[(int)CamInfo.MAX];
            public double[] dPixel           = new double[(int)CamInfo.MAX];
            public string[] strBackGround    = new string[(int)CamInfo.MAX];
            public bool  [] bAutoContrast    = new bool  [(int)CamInfo.MAX]; 
            public bool  [] bAutoContrastMin = new bool  [(int)CamInfo.MAX]; 
            public double[] dContrast        = new double[(int)CamInfo.MAX];
            public double[] dContrastMin     = new double[(int)CamInfo.MAX];
            public double[] dScaleMin        = new double[(int)CamInfo.MAX];
            public double[] dScaleMax        = new double[(int)CamInfo.MAX];
            public double[] dGreedNess       = new double[(int)CamInfo.MAX];

            public G_S_MODEL_LIST()
            {
                for (int n = 0; n < (int)CamInfo.MAX; n++)
                {
                    dYSize          [n] = .0;
                    dXSize          [n] = .0;
                    dDepth          [n] = .0;
                    dAngleMin       [n] = .0;
                    dAngleMax       [n] = .0;
                    dMaxOverlap     [n] = .0;
                    dMinScore       [n] = .0;
                    nNumLevels      [n] = 0;
                    strMetric       [n] = "";
                    strSubPixel     [n] = "";
                    dBoader         [n] = .0;
                    dPixel          [n] = .0;
                    strBackGround   [n] = "";
                    bAutoContrast   [n] = false;
                    bAutoContrastMin[n] = false;
                    dContrast       [n] = 0;
                    dContrastMin    [n] = 0;
                    dScaleMin       [n] = 0;
                    dScaleMax       [n] = 0;
                    dGreedNess      [n] = 0;    
                }

            }
        }

    }
}
