using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionReader
{
    internal class Packet
    {
        // Packet 
        // @ LIVESTART   @ 
        // @ RECORDSTART @ Path
        // @ RECORDEND   @
        // @ LIVEEND     @
        public static string SendCutVid(string msg)
        {
            string packet = string.Format("@VIDEOCUT@{0}", msg);
            return packet;
        }
        public static string SendDirectory(string msg)
        {
            string packet = string.Format("@START@{0}", msg);
            return packet;
        }
        public static string SendLiveStart()
        {
            string packet = string.Format("@LIVESTART@");
            return packet;
        }
        public static string SendRecordStart(string msg)
        {
            string packet = string.Format("@RECORDSTART@{0}", msg);
            return packet;
        }
        public static string SendRecordEnd()
        {
            string packet = string.Format("@RECORDEND@");
            return packet;
        }
        public static string SendLiveEnd()
        {
            string packet = string.Format("@LIVEEND@");
            return packet;
        }
    }
}
