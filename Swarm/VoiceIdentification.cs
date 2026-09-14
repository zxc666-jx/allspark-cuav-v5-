using MissionPlanner.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissionPlanner.Swarm
{
    class VoiceIdentification
    {

        public SpeechRecognitionEngine recognizer;
        private DictationGrammar dictationGrammar;

        public void init()
        {

            string[] a = { };
            SRecognition(a, 1);

        }
        public void start()
        {
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }
        public void stop()
        {
            recognizer.RecognizeAsyncStop();
            recognizer.Dispose();
        }
        private void SRecognition(string[] fg, int i) //创建关键词语列表  
        {
            CultureInfo myCIintl = new CultureInfo("zh-CN");
            foreach (RecognizerInfo config in SpeechRecognitionEngine.InstalledRecognizers())//获取所有语音引擎  
            {
                Console.WriteLine(config.Culture.EnglishName);
                if (config.Culture.Equals(myCIintl))
                {
                    recognizer = new SpeechRecognitionEngine(config);
                    break;
                }//选择识别引擎
            }
            if (recognizer != null)
            {
                InitializeSpeechRecognitionEngine(fg);//初始化语音识别引擎  
                dictationGrammar = new DictationGrammar();
            }
            else
            {
                CustomMessageBox.Show("创建语音识别失败");
            }
        }

        private void InitializeSpeechRecognitionEngine(string[] s)
        {
            // Create and load a dictation grammar.
            //recognizer.LoadGrammar(new DictationGrammar());

            // Configure input to the speech recognizer.
            recognizer.SetInputToDefaultAudioDevice();

            // Modify the initial silence time-out value.
            recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(5);
            
            Choices digits = new Choices();
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    SemanticResultValue temp = new SemanticResultValue(mav.ToString() + "号机", (int)mav.sysid);
                    digits.Add(temp);
                }
            }
            GrammarBuilder GB = new GrammarBuilder();
            // 一级语法
            GB.Append(new SemanticResultKey("目标飞机", digits));
            // 二级语法
            List<string> actionLists = new List<string> { "解锁", "起飞", "降落", "定点", "升高", "下降", "前进", "后退", "左飞", "右飞", "左偏航", "右偏航" };
            GB.Append(new Choices(actionLists.ToArray()));
            // 三级语法，可缺省
            Choices distances_cm = new Choices();
            int maxdiatancescm = 500;
            for (int i=10;i<= maxdiatancescm;i+=10)
            {
                SemanticResultValue temp = new SemanticResultValue(i.ToString() + "厘米", i);
                distances_cm.Add(temp);
            }
            GB.Append(new SemanticResultKey("目标距离", distances_cm),0,1);
            // 四级语法，可缺省
            Choices degrees = new Choices();
            int maxdegres = 30;
            for (int i = 1; i <= maxdegres; i++)
            {
                SemanticResultValue temp = new SemanticResultValue(i.ToString() + "度", i);
                degrees.Add(temp);
            }
            GB.Append(new SemanticResultKey("目标角度", degrees), 0, 1);

            Grammar G = new Grammar(GB);
            Grammar confirmGrammar = new Grammar(new GrammarBuilder("确定执行"));
            Console.WriteLine(G.RuleName);
            //G.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(G_SpeechRecognized);
            recognizer.LoadGrammar(G);
            recognizer.LoadGrammar(confirmGrammar);
            // Add a handler for the speech recognized event.
            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(G_SpeechRecognized);
        }
        string lastResult = "确定执行";
        int last_navdistance_cm;
        int last_navdegree = 0;
        int last_navmavid = 0;
        void G_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // 更新界面显示
            if (FormationControl.instance != null)
            {
                FormationControl.instance.voice_identification_show(e.Result.Text);
            }
            // 解析特定数字
            int navmavid = 0;
            int navdistance_cm = 0;
            int navdegree = 0;
            if (e.Result != null)
            {
                if (e.Result.Semantics != null)
                {
                    if (e.Result.Semantics.ContainsKey("目标飞机"))
                    {
                        navmavid = (int)e.Result.Semantics["目标飞机"].Value;
                    }
                    if (e.Result.Semantics.ContainsKey("目标距离"))
                    {
                        navdistance_cm = (int)e.Result.Semantics["目标距离"].Value;
                    }
                    if (e.Result.Semantics.ContainsKey("目标角度"))
                    {
                        navdegree = (int)e.Result.Semantics["目标角度"].Value;
                    }
                }
                else
                {
                    navmavid = 0;
                    navdistance_cm = 0;
                    navdegree = 0;
                }
                if (e.Result.Text.Equals("确定执行"))
                {
                    ParseRecognized(lastResult, last_navmavid, last_navdistance_cm, last_navdegree);
                }
                else
                {
                    lastResult = e.Result.Text;
                    last_navmavid = navmavid;
                    last_navdistance_cm = navdistance_cm;
                    last_navdegree = navdegree;
                }
            }
        }
        double wrap_360(double input)
        {
            if (input > 360)
                return input - 360;
            if (input < 0)
                return input + 360;
            return input;
        }
        PointLatLngAlt mavlocation = new PointLatLngAlt();
        float mavyaw=0;
        void ParseRecognized(string recognizedtext,int navmavid,int navdistance_cm,int navdegree)
        {
            if (recognizedtext.Equals("确定执行"))
            {
                return;
            }
            if (navmavid==0)
            {
                return;
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav.sysid == (byte)navmavid)
                    {
                        mavlocation = new PointLatLngAlt(mav.cs.lat,mav.cs.lng,mav.cs.alt);
                        mavyaw = mav.cs.yaw;
                        if (recognizedtext.Contains("解锁"))
                        {
                            if (!mav.cs.armed)
                            {
                                port.doARM(mav.sysid, mav.compid, true);
                            }
                        }
                        else if (recognizedtext.Contains("起飞"))
                        {
                            // 收到用户起飞指令 切引导模式 解锁 默认起飞2米
                            // 第一步确实是否为guided模式
                            if (!mav.cs.mode.ToLower().Equals("guided"))
                            {
                                port.setMode(mav.sysid, mav.compid, "Guided");
                            }
                            // 解锁
                            if (!mav.cs.armed)
                            {
                                port.doARM(mav.sysid, mav.compid, true);
                            }
                            port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.TAKEOFF, 0, 0, 0, 0, 0, 0, 2);
                        }
                        else if (recognizedtext.Contains("降落"))
                        {
                            port.setMode(mav.sysid, mav.compid, "Land");
                        }
                        else if (recognizedtext.Contains("定点"))
                        {
                            // 第一步确实是否为guided模式
                            if (!mav.cs.mode.ToLower().Equals("guided"))
                            {
                                port.setMode(mav.sysid, mav.compid, "Guided");
                            }
                            port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false, MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT,
                                mavlocation.Lat, mavlocation.Lng, mavlocation.Alt, 0, 0, 0, 0, 0);
                        }
                        else if (recognizedtext.Contains("升高"))
                        {
                            double navalt = mavlocation.Alt;
                            // 只听到升高，默认升高0.5m
                            if (navdistance_cm == 0)
                            {
                                navalt += 0.5;
                            }
                            else
                            {
                                navalt += (double)navdistance_cm / 100;
                            }
                            // 第一步确实是否为guided模式
                            if (!mav.cs.mode.ToLower().Equals("guided"))
                            {
                                port.setMode(mav.sysid, mav.compid, "Guided");
                            }
                            port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false, MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT,
                                mavlocation.Lat, mavlocation.Lng, navalt, 0, 0, 0, 0, 0);
                        }
                        else if (recognizedtext.Contains("下降"))
                        {
                            double navalt = mavlocation.Alt;
                            // 只听到升高，默认升高0.5m
                            if (navdistance_cm == 0)
                            {
                                navalt -= 0.5;
                            }
                            else
                            {
                                navalt -= (double)navdistance_cm / 100;
                            }
                            // 第一步确实是否为guided模式
                            if (!mav.cs.mode.ToLower().Equals("guided"))
                            {
                                port.setMode(mav.sysid, mav.compid, "Guided");
                            }
                            port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false, MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT,
                                mavlocation.Lat, mavlocation.Lng, navalt, 0, 0, 0, 0, 0);
                        }
                        else if (recognizedtext.Contains("前进"))
                        {
                            double distance = 0;
                            // 没有说出几米，默认为1米
                            if (navdistance_cm == 0)
                            {
                                distance = 1;
                            }
                            else
                            {
                                distance = (double)navdistance_cm / 100;
                            }
                            var newtarget = mavlocation.newpos(mavyaw, distance);
                            // 第一步确实是否为guided模式
                            if (!mav.cs.mode.ToLower().Equals("guided"))
                            {
                                port.setMode(mav.sysid, mav.compid, "Guided");
                            }
                            port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false, MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT,
                                newtarget.Lat, newtarget.Lng, newtarget.Alt, 0, 0, 0, 0, 0);
                        }
                        else if (recognizedtext.Contains("后退"))
                        {
                            double distance = 0;
                            // 没有说出几米，默认为1米
                            if (navdistance_cm == 0)
                            {
                                distance = -1;
                            }
                            else
                            {
                                distance = -(double)navdistance_cm / 100;
                            }
                            var newtarget = mavlocation.newpos(mavyaw, distance);
                            // 第一步确实是否为guided模式
                            if (!mav.cs.mode.ToLower().Equals("guided"))
                            {
                                port.setMode(mav.sysid, mav.compid, "Guided");
                            }
                            port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false, MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT,
                                newtarget.Lat, newtarget.Lng, newtarget.Alt, 0, 0, 0, 0, 0);
                        }
                        else if (recognizedtext.Contains("左飞"))
                        {
                            double distance = 0;
                            // 没有说出几米，默认为1米
                            if (navdistance_cm == 0)
                            {
                                distance = 1;
                            }
                            else
                            {
                                distance = (double)navdistance_cm / 100;
                            }
                            double navyaw = mavyaw - 90;
                            navyaw = wrap_360(navyaw);
                            var newtarget = mavlocation.newpos(navyaw, distance);
                            // 第一步确实是否为guided模式
                            if (!mav.cs.mode.ToLower().Equals("guided"))
                            {
                                port.setMode(mav.sysid, mav.compid, "Guided");
                            }
                            port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false, MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT,
                                newtarget.Lat, newtarget.Lng, newtarget.Alt, 0, 0, 0, 0, 0);
                        }
                        else if (recognizedtext.Contains("右飞"))
                        {
                            double distance = 0;
                            // 没有说出几米，默认为1米
                            if (navdistance_cm == 0)
                            {
                                distance = 1;
                            }
                            else
                            {
                                distance = (double)navdistance_cm / 100;
                            }
                            double navyaw = mavyaw + 90;
                            navyaw = wrap_360(navyaw);
                            var newtarget = mavlocation.newpos(navyaw, distance);
                            // 第一步确实是否为guided模式
                            if (!mav.cs.mode.ToLower().Equals("guided"))
                            {
                                port.setMode(mav.sysid, mav.compid, "Guided");
                            }
                            port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false, MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT,
                                newtarget.Lat, newtarget.Lng, newtarget.Alt, 0, 0, 0, 0, 0);
                        }
                        else if (recognizedtext.Contains("左偏航"))
                        {
                            double navyaw = mavyaw;
                            // 默认5度
                            if (navdegree == 0)
                            {
                                navyaw -= 5;
                                navyaw = wrap_360(navyaw);
                            }
                            else
                            {
                                navyaw -= navdegree;
                                navyaw = wrap_360(navyaw);
                            }
                            // 第一步确实是否为guided模式
                            if (!mav.cs.mode.ToLower().Equals("guided"))
                            {
                                port.setMode(mav.sysid, mav.compid, "Guided");
                            }
                            port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.CONDITION_YAW, (float)navyaw, 100.0f, 0, 0, 0, 0, 0, false);
                        }
                        else if (recognizedtext.Contains("右偏航"))
                        {
                            double navyaw = mavyaw;
                            // 默认5度
                            if (navdegree == 0)
                            {
                                navyaw += 5;
                                navyaw = wrap_360(navyaw);
                            }
                            else
                            {
                                navyaw += navdegree;
                                navyaw = wrap_360(navyaw);
                            }
                            // 第一步确实是否为guided模式
                            if (!mav.cs.mode.ToLower().Equals("guided"))
                            {
                                port.setMode(mav.sysid, mav.compid, "Guided");
                            }
                            port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.CONDITION_YAW, (float)navyaw, 100.0f, 0, 0, 0, 0, 0, false);
                        }
                    }
                }
            }
        }
    }
}
