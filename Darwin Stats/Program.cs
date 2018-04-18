using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Security.AccessControl;
using Newtonsoft.Json;
using System.Timers;
using System.Net;

namespace Darwin_Stats
{
    class Program
    {
        struct ConfigDarwin
        {
            public string steam64ID;
            public int timeout;
            public bool multiFile;
            public string outputDir;
        }

        public class DarwinStats
        {
            public string displayName { get; set; }
            public string elo { get; set; }
            public string rank { get; set; }
            public string gamesPlayed { get; set; }
            public string eloChange { get; set; }
            public string winStreak { get; set; }
            public string dailyKills { get; set; }
            public string dailyGames { get; set; }
            public string dailyEloChange { get; set; }
            public string dailyWinCount { get; set; }
        }

        private ConfigDarwin conf;
        private string configPath, customURL;
        private Timer timer;
        private int mask;

        private string displayName;
        private string elo;
        private string rank;
        private string gamesPlayed;
        private string eloChange;
        private string winStreak;
        private string dailyKills;
        private string dailyGames;
        private string dailyEloChange;
        private string dailyWinCount;

        static void Main(string[] args)
         => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            await TestAPI();
            await Task.Delay(-1);
        }

        private async Task TestAPI()
        {
            conf = new ConfigDarwin()//Standard config
            {
                steam64ID = "Enter your steam64ID here",
                timeout = 30,
                multiFile = true,
                outputDir = Directory.GetCurrentDirectory()
            };

            configPath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");//Gets current path

            if (!File.Exists(configPath))//Create the new config file to be filled out
            {
                createFile(configPath);

                using (StreamWriter sw = File.AppendText(configPath))
                {
                    await sw.WriteLineAsync(JsonConvert.SerializeObject(conf));
                }

                Console.WriteLine("WARNING! New Config initialized! Need to fill in values before running!");
                throw new Exception("NO CONFIG AVAILABLE! Go to executable path and fill out newly created file!");
            }

            using (StreamReader reader = new StreamReader(configPath))
            {
                conf = JsonConvert.DeserializeObject<ConfigDarwin>(reader.ReadLine());
            }

            customURL = @"https://darwintracker.com/rankjson.php?id=stm-" + conf.steam64ID;

            timer = new Timer(conf.timeout * 1000);

            timer.Elapsed += GetData;

            timer.Start();

            await Task.CompletedTask;
        }

        private void createFile(string path)
        {
            using (var f = File.Create(path))//Make sure we have access to the file
            {
                DirectoryInfo dInfo = new DirectoryInfo(path);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule("everyone", FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                dInfo.SetAccessControl(dSecurity);
            }
        }

        private async void GetData(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Getting Darwin Data...");
            // Create a request for the URL.   
            WebRequest request = WebRequest.Create(customURL);
            // Get the response.  
            WebResponse response = await request.GetResponseAsync();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = await reader.ReadToEndAsync();
            DarwinStats info = JsonConvert.DeserializeObject<DarwinStats>(responseFromServer);
            // Clean up the streams and the response.  
            reader.Close();
            response.Close();

            if (!ValidateChange(info))
                return;

            await FileCheck();

            if (conf.multiFile)//Multi File
            {
                if ((mask & 1) == 1)
                {
                    using (StreamWriter sw = new StreamWriter(conf.outputDir + @"\DisplayName.txt", false))
                    {
                        await sw.WriteLineAsync("Name: " + displayName);
                    }
                }
                if ((mask & 2) == 2)
                {
                    using (StreamWriter sw = new StreamWriter(conf.outputDir + @"\Elo.txt", false))
                    {
                        await sw.WriteLineAsync("Elo: " + elo);
                    }
                }
                if ((mask & 4) == 4)
                {
                    using (StreamWriter sw = new StreamWriter(conf.outputDir + @"\Rank.txt", false))
                    {
                        await sw.WriteLineAsync("Rank: " + rank);
                    }
                }
                if ((mask & 8) == 8)
                {
                    using (StreamWriter sw = new StreamWriter(conf.outputDir + @"\GamesPlayed.txt", false))
                    {
                        await sw.WriteLineAsync("Games Played: " + gamesPlayed);
                    }
                }
                if ((mask & 16) == 16)
                {
                    using (StreamWriter sw = new StreamWriter(conf.outputDir + @"\EloChange.txt", false))
                    {
                        await sw.WriteLineAsync("Elo Change: " + eloChange);
                    }
                }
                if ((mask & 32) == 32)
                {
                    using (StreamWriter sw = new StreamWriter(conf.outputDir + @"\WinStreak.txt", false))
                    {
                        await sw.WriteLineAsync("Win Streak: " + winStreak);
                    }
                }
                if ((mask & 64) == 64)
                {
                    using (StreamWriter sw = new StreamWriter(conf.outputDir + @"\DailyKills.txt", false))
                    {
                        await sw.WriteLineAsync("Daily Kills: " + dailyKills);
                    }
                }
                if ((mask & 128) == 128)
                {
                    using (StreamWriter sw = new StreamWriter(conf.outputDir + @"\DailyGames.txt", false))
                    {
                        await sw.WriteLineAsync("Daily Games: " + dailyGames);
                    }
                }
                if ((mask & 256) == 256)
                {
                    using (StreamWriter sw = new StreamWriter(conf.outputDir + @"\DailyEloChange.txt", false))
                    {
                        await sw.WriteLineAsync("Daily Elo Change: " + dailyEloChange);
                    }
                }
                if ((mask & 512) == 512)
                {
                    using (StreamWriter sw = new StreamWriter(conf.outputDir + @"\DailyWinCount.txt", false))
                    {
                        await sw.WriteLineAsync("Daily Win Count: " + dailyWinCount);
                    }
                }
            }
            else//Single File
            {
                using (StreamWriter sw = new StreamWriter(conf.outputDir + @"\DarwinStats.txt", false))
                {
                    await sw.WriteLineAsync("Name: " + displayName +
                        "\nElo: " + elo +
                        "\nRank: " + rank +
                        "\nGames Played: " + gamesPlayed +
                        "\nElo Change: " + eloChange +
                        "\nWin Streak: " + winStreak +
                        "\nDaily Kills: " + dailyKills +
                        "\nDaily Games: " + dailyGames +
                        "\nDaily Elo Change: " + dailyEloChange +
                        "\nDaily Win Count: " + dailyWinCount);
                }
            }
        }

        private bool ValidateChange(DarwinStats inf)
        {
            mask = 0;
            if (displayName != inf.displayName)
                mask |= 1;
            if (elo != inf.elo)
                mask |= 2;
            if (rank != inf.rank)
                mask |= 4;
            if (gamesPlayed != inf.gamesPlayed)
                mask |= 8;
            if (eloChange != inf.eloChange)
                mask |= 16;
            if (winStreak != inf.winStreak)
                mask |= 32;
            if (dailyKills != inf.dailyKills)
                mask |= 64;
            if (dailyGames != inf.dailyGames)
                mask |= 128;
            if (dailyEloChange != inf.dailyEloChange)
                mask |= 256;
            if (dailyWinCount != inf.dailyWinCount)
                mask |= 512;

            displayName = inf.displayName;
            elo = inf.elo;
            rank = inf.rank;
            gamesPlayed = inf.gamesPlayed;
            eloChange = inf.eloChange;
            winStreak = inf.winStreak;
            dailyKills = inf.dailyKills;
            dailyGames = inf.dailyGames;
            dailyEloChange = inf.dailyEloChange;
            dailyWinCount = inf.dailyWinCount;

            return mask > 0;
        }

        private async Task FileCheck()
        {
            if (!File.Exists(conf.outputDir + @"\DarwinStats.txt"))
                createFile(conf.outputDir + @"\DarwinStats.txt");

            if (!File.Exists(conf.outputDir + @"\DisplayName.txt"))
                createFile(conf.outputDir + @"\DisplayName.txt");

            if (!File.Exists(conf.outputDir + @"\Elo.txt"))
                createFile(conf.outputDir + @"\Elo.txt");

            if (!File.Exists(conf.outputDir + @"\Rank.txt"))
                createFile(conf.outputDir + @"\Rank.txt");

            if (!File.Exists(conf.outputDir + @"\GamesPlayed.txt"))
                createFile(conf.outputDir + @"\GamesPlayed.txt");

            if (!File.Exists(conf.outputDir + @"\EloChange.txt"))
                createFile(conf.outputDir + @"\EloChange.txt");

            if (!File.Exists(conf.outputDir + @"\WinStreak.txt"))
                createFile(conf.outputDir + @"\WinStreak.txt");

            if (!File.Exists(conf.outputDir + @"\DailyKills.txt"))
                createFile(conf.outputDir + @"\DailyKills.txt");

            if (!File.Exists(conf.outputDir + @"\DailyGames.txt"))
                createFile(conf.outputDir + @"\DailyGames.txt");

            if (!File.Exists(conf.outputDir + @"\DailyEloChange.txt"))
                createFile(conf.outputDir + @"\DailyEloChange.txt");

            if (!File.Exists(conf.outputDir + @"\DailyWinCount.txt"))
                createFile(conf.outputDir + @"\DailyWinCount.txt");

            await Task.CompletedTask;
        }
    }
}