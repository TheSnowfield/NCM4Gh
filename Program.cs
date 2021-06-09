using System;

namespace NCM4Gh
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(NcmPlayer.NowPlaying?.WndTitle);
        }
    }
}
