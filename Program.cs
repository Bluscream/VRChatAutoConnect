using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VRChatApi;
using VRChatApi.Classes;
using Newtonsoft.Json;

namespace VRChatAutoConnect
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 1) throw new Exception("Missing world ID");
            string worldId = args[0];
            System.Console.WriteLine("Initializing API...");
            VRChatApi.VRChatApi api = new VRChatApi.VRChatApi("", "");
            System.Console.WriteLine("Getting world...");
            WorldResponse world = await api.WorldApi.Get(worldId);
            Console.WriteLine("World ID: {0}", worldId);
            Console.WriteLine("Name: {0}", world.name);
            Console.WriteLine("Players: {0} / {1}", world.occupants, world.capacity);
            Console.WriteLine("Instances:");
            string fullest = "";
            int fullest_count = 0;
            string instance_pattern = @"^(\d+)~hidden";
            string instance_pattern_private = instance_pattern + @"\((usr_[\w\d]{8}-[\w\d]{4}-[\w\d]{4}-[\w\d]{4}-[\w\d]{12})\)~nonce\(([0-9A-Z]{64})\)$";
            foreach (var instance in world.instances)
            {
                if (!Regex.Match(instance.id, instance_pattern).Success)
                {
                    Console.WriteLine("{0}: {1}", instance.id, instance.occupants);
                    if (instance.occupants > fullest_count)
                        fullest = instance.id; fullest_count = instance.occupants;
                } else
                {
                    var parsed = Regex.Split(instance.id, instance_pattern_private);
                    var parsed_id = parsed[1];var parsed_usr_id = parsed[2]; var parsed_nonce = parsed[3];
                    /*using (WebClient wc = new WebClient())
                    {
                        var url = "https://vrchat.com/api/1/users/" + parsed_usr_id;
                        Console.WriteLine("Requesting {0}...", url);
                        var json = wc.DownloadString(url);
                        //JsonSerializer serializer = new JsonSerializer();
                        dynamic array = JsonConvert.DeserializeObject(json);
                        Console.WriteLine("{0} (private) by {1}: {2}", parsed_id, array["displayName"], instance.occupants);
                    }*/
                }
            }
            var url = string.Format("vrchat://launch?ref=vrchat.com&id={0}:{1}", worldId, fullest);
            Console.WriteLine("Connecting to {0}...", url);
            System.Console.ReadKey(true);
            System.Diagnostics.Process.Start(url);
            //  
        }
    }
}
