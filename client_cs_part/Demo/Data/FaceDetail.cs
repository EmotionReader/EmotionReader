using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionReader
{
    public class FaceDetail
    {
        public int Frame_no { get; private set; }
        // 0-anger / 1-disgust / 2-fear / 3-happy / 4-neutral / 5-sad / 6-surprise
        public int Emotion_code { get; private set; }
        //====================Rect=========================
        public int X1 { get; private set; }
        public int Y1 { get; private set; }
        public int X2 { get; private set; }
        public int Y2 { get; private set; }
        //==================ProgressBar=====================
        public double Current_Anger    { get; private set; }
        public double Current_Disgust  { get; private set; }
        public double Current_Fear     { get; private set; }
        public double Current_Happy    { get; private set; }
        public double Current_Neutral  { get; private set; }
        public double Current_Sad      { get; private set; }
        public double Current_Surprise { get; private set; }
        //===================================================

        public FaceDetail(int frame_no, int emotion_code, int x1, int y1, int x2, int y2, double anger, double disgust, double fear, double happy, double neutral, double sad, double surprise)
        {
            Frame_no = frame_no;
            Emotion_code = emotion_code;
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            Current_Anger    = anger * 100;
            Current_Disgust  = disgust * 100;
            Current_Fear     = fear * 100;
            Current_Happy    = happy * 100;
            Current_Neutral  = neutral * 100;
            Current_Sad      = sad * 100;
            Current_Surprise = surprise * 100;
        }
    }
}
