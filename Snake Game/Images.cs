using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Snake_Game
{
    public static class Images
    {

        public static readonly ImageSource Empty = LoadImage("Empty.png");
        public static readonly ImageSource Body = LoadImage("Body.png");
        public static readonly ImageSource DeadHead = LoadImage("DeadHead.png");
        public static readonly ImageSource DeadBody = LoadImage("DeadBody.png");
        public static readonly ImageSource Head = LoadImage("Head.png");
        public static readonly ImageSource Food = LoadImage("Food.png");
        public static readonly ImageSource Crown = LoadImage("Crown.png");
        public static readonly ImageSource Gold_Body = LoadImage("Gold_body.png");
        public static readonly ImageSource Gold_Head = LoadImage("Gold_head.png");
        public static readonly ImageSource Bomb = LoadImage("Bomb.png");
        private static ImageSource LoadImage(string fileName)
        {
            return new BitmapImage(new Uri($"Assets/{fileName}", UriKind.Relative));
        }
    }
}
