using EmotionReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionReader
{
    public class VideoInfo
    {
        public double Fps { get; private set; }
        public int Frame_max_count { get; private set; }
        public double Frame_Gap { get; private set; }
        public List<Face> Faces { get; private set; }
        public string VideoPath { get; private set; }

        public VideoInfo() { }

        public VideoInfo(double fps, int fmc, double fg)
        {
            Fps = fps;
            Frame_max_count = fmc;
            Frame_Gap = fg;
            VideoPath = string.Empty;
            Faces = new List<Face>();
        }
        public int Get_Face_code_idx(int face_code)
        {
            try
            {
                for (int i = 0; i < Faces.Count; i++)
                {
                    if (Faces[i].Face_code == face_code)
                    {
                        return i;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void Set_VideoPath(string path)
        {
            VideoPath = path;
        }
    }
}
