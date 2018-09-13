using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VRChatApi.Classes;
using Flurl;
using System.Collections.Generic;

namespace VRChatAutoConnect
{
    class Program
    {
        static VRChatApi.VRChatApi api;
        static string worldId;

        static string SearchWorld()
        {
            Console.Write("Search for a world: ");
            string keyword = Console.ReadLine();
            // Console.WriteLine("Search Results for \"{0}\":", keyword);
            List<WorldBriefResponse> worlds = api.WorldApi.Search(keyword: keyword).Result;
            var i = 1;
            foreach (var world in worlds)
            {
                Console.WriteLine("{0}: {1}", i, world.name);
                i += 1;
            }
            Console.Write("Enter number of server to select: ");
            string result = Console.ReadLine();
            bool success = int.TryParse(result, out int int_result);
            if (!success) return "";
            worldId = worlds[int_result-1].id;
            Console.WriteLine();
            Console.WriteLine("To save this, start the application as:");
            Console.WriteLine("VRChatAutoConnect.exe {0}", worldId);
            Console.WriteLine();
            return worldId;
        }
        
        static async Task Main(string[] args)
        {
            using (WebClient wc = new WebClient())
            {
               var online_users = wc.DownloadString("https://api.vrchat.cloud/api/1/visits");
                Console.WriteLine("Initializing API... ({0} users online)", online_users);
            }
            api = new VRChatApi.VRChatApi("", "");
            if (args.Length < 1) {
                worldId = SearchWorld();
                if (string.IsNullOrWhiteSpace(worldId)) throw new Exception("Missing world ID!");
            } else worldId = args[0]; // wrld_b805006c-bec7-4179-958a-5a9351e48d5c
            Console.WriteLine("Getting world \"{0}\"...", worldId);
            WorldResponse world = await api.WorldApi.Get(worldId);
            Console.WriteLine("Name: {0}", world.name);
            Console.WriteLine("Players: {0} / {1}", world.occupants, world.capacity);
            if (world.occupants < 1) { await CreateNew("No one seems to be online, sorry!");return; }
            Console.WriteLine("Instances:");
            string instance_pattern = @"^(\d+)~hidden";
            //string instance_pattern_private = instance_pattern + @"\((usr_[\w\d]{8}-[\w\d]{4}-[\w\d]{4}-[\w\d]{4}-[\w\d]{12})\)~nonce\(([0-9A-Z]{64})\)$";
            string fullest = "";
            int fullest_count = 0;
            foreach (var instance in world.instances)
            {
                if (!Regex.Match(instance.id, instance_pattern).Success)
                {
                    Console.WriteLine("{0}: {1}", instance.id, instance.occupants);
                    if (instance.occupants > fullest_count)
                        fullest = instance.id; fullest_count = instance.occupants;
                } /* else {
                    var parsed = Regex.Split(instance.id, instance_pattern_private);
                    var parsed_id = parsed[1];var parsed_usr_id = parsed[2]; var parsed_nonce = parsed[3];
                    UserBriefResponse creator = await api.UserApi.GetById(parsed_usr_id);
                    Console.WriteLine("{0} (private) by {1}: {2}", parsed_id, creator.displayName, instance.occupants);
                } */
            }
            if (fullest_count < 1) { await CreateNew("No public instances found, sorry!"); return; }
            await StartGame(worldId, fullest);
        }
        static async Task CreateNew(string reason)
        {
            Console.WriteLine(reason);
            await StartGame(worldId);
        }

        private static async Task StartGame(string id = null, string instance = null)
        {
            StringBuilder builder = new StringBuilder("");
            if (!(id is null)) {
                builder.Append(id);
                if (!(instance is null)) {
                    builder.Append(":");
                    builder.Append(instance);
                }
            }
            string innerString = builder.ToString();
            var url = "vrchat://".AppendPathSegment("launch");
            if (!string.IsNullOrWhiteSpace(innerString)) {
                url.SetQueryParam("id", innerString);
            }
            url.SetQueryParam("ref", "vrchat.com");
            Console.WriteLine("Connecting to {0}", url);
            Console.ReadKey(true);
            System.Diagnostics.Process.Start(url.ToString());
        }
    }
}
