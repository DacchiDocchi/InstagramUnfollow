using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;

namespace InstagramConsoleUnfollowers
{
    internal class Program
    {
        private static IInstaApi _instaApi;
        
        public static async Task Main(string[] args)
        {
            string username, password;
            
            Console.WriteLine("Enter your Instagram Username");
            username = Console.ReadLine();
            
            Console.WriteLine("Now enter your Instagram Password");
            password = Console.ReadLine();
            
            _instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(UserSessionData.ForUsername(username).WithPassword(password))
                .SetRequestDelay(RequestDelay.FromSeconds(2, 2))
                .Build();
            
            await _instaApi.SendRequestsBeforeLoginAsync();
            var result = await _instaApi.LoginAsync();
            
            if (!result.Succeeded || !_instaApi.IsUserAuthenticated)
            {
                Console.WriteLine("Failed logging, check your instagram username or password, try again!");
            }
            
            if (result.Succeeded || _instaApi.IsUserAuthenticated)
            {
                Console.WriteLine("Logged, please wait a moment...");
                await GetNotFollowingBack(username);
            }
        }
        
        private static async Task<List<string>> GetNotFollowingBack(string username)
        {
            var followingResult = await _instaApi.UserProcessor.GetUserFollowingAsync(username, PaginationParameters.MaxPagesToLoad(int.MaxValue));

            if (!followingResult.Succeeded)
            {
                Console.WriteLine("Error!");
            }

            var followingSet = followingResult.Value.Select(x => x.UserName).ToHashSet();

            var followersResult = await _instaApi.UserProcessor.GetUserFollowersAsync(username, PaginationParameters.MaxPagesToLoad(int.MaxValue));

            if (!followersResult.Succeeded)
            {
                Console.WriteLine("Error!");
            }

            foreach (var item in followersResult.Value.Select(x => x.UserName))
            {
                followingSet.Remove(item);
            }
            
            Console.Write("The following people do not follow you back\n" + string.Join(System.Environment.NewLine, followingSet));
            return followingSet.ToList();
        }
    }
}