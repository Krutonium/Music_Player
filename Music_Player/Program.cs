using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFmpeg.NET;
using ManagedBass;


namespace Music_Player
{
    class Program
    {
        public static double volume = 0.1;
        static async Task Main(string[] args)
        {
            Program prog = new Program();
            List<string> songs = prog.LoadSongs();
            Random rand = new Random();
            while (true)
            {
                await prog.PlaySong(songs, rand.Next(0, songs.Count - 1));
            }
        }

        private List<string> LoadSongs()
        {
            Console.WriteLine("Loading Songs");
            List<string> AllFiles = Directory.GetFiles("/home/krutonium/Music/", "*.*", SearchOption.AllDirectories)
                .ToList();
            List<string> wantedFiles = new List<string>();
            foreach (var song in AllFiles)
            {
                var tmpSong = song.ToUpper();
                if (tmpSong.EndsWith(".MP3") | tmpSong.EndsWith(".WAV") | tmpSong.EndsWith(".FLAC"))
                {
                    wantedFiles.Add(song);
                }
            }
            Console.WriteLine("Got {0} Songs", wantedFiles.Count);
            if (wantedFiles.Count == 0)
            {
                Console.WriteLine("Found no valid music! Must be MP3, WAV, or FLAC");
                Environment.Exit(1);
            }
            return wantedFiles;
        }

        private async Task PlaySong(List<string>song, int index)
        {
            MediaPlayer player = new MediaPlayer();
            var ffmpeg = new Engine("/usr/bin/ffmpeg");
            if (song[index].ToUpper().EndsWith(".FLAC"))
            {
                
                string tmpPath = Path.GetTempPath() + "tempSong.wav";
                if(File.Exists(tmpPath)){File.Delete(tmpPath);}
                await ffmpeg.ConvertAsync(new InputFile(song[index]), new OutputFile(tmpPath));
         
                System.Threading.Thread.Sleep(2000);
                await player.LoadAsync(tmpPath);
            }
            else
            {
                await player.LoadAsync(song[index]);
            }

            player.Volume = volume;
            player.Play();
            //await ffmpeg.GetThumbnailAsync(new InputFile(song[index]), new OutputFile("/home/krutonium/currentsong.png"));
            
            Console.WriteLine("Playing \"{0}\" by \"{1}\" from \"{2}\"", player.Title, player.Artist, player.Album);
            while (player.State == PlaybackState.Paused | player.State == PlaybackState.Playing)
            {
                System.Threading.Thread.Sleep(1000);
                Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
                Console.Write("{0:mm\\:ss} | {1:mm\\:ss}", player.Position, player.Duration);
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.F7:
                            if (player.State == PlaybackState.Paused)
                            { player.Play(); } else
                            { player.Pause(); }
                            break;
                        case ConsoleKey.F8:
                            player.Stop();
                            break;
                        case ConsoleKey.UpArrow:
                            volume += 0.05;
                            player.Volume = volume;
                            break;
                        case ConsoleKey.DownArrow:
                            volume -= 0.05;
                            player.Volume = volume;
                            break;
                    }
                }

                if (player.State == PlaybackState.Paused)
                {
                    File.WriteAllText("/home/krutonium/currentsong.txt","");
                }
                else
                {
                    File.WriteAllText("/home/krutonium/currentsong.txt", player.Title + " | " + player.Artist);
                }
            }
            Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
            player.Dispose();
        }
    }
}
