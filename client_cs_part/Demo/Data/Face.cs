using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmotionReader;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;


namespace EmotionReader
{
    public class Face
    {
        public int Face_code { get; private set; }
        public List<FaceDetail> Face_details { get; private set; }
        public Face(int face_code, FaceDetail facedetail)
        {
            Face_code = face_code;
            Face_details = new List<FaceDetail>();
            Face_details.Add(facedetail);
        }
        public void Sort_Face_details()
        {
            Face_details = Face_details.OrderBy(FaceDetail => FaceDetail.Frame_no).ToList();
        }
        public int Get_Frame_No_idx(int frame_no)
        {
            for (int i = 0; i < Face_details.Count; i++)
            {
                if (Face_details[i].Frame_no >= frame_no - 6 && Face_details[i].Frame_no <= frame_no + 6)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}