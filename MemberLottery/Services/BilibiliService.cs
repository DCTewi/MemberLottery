using MemberLottery.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MemberLottery.Services
{
    public class BilibiliService
    {
        private readonly HttpClient http = new();

        public async Task<UserInfoData> GetUserInfoAsync(string mid)
        {
            var apiurl = "https://api.bilibili.com/x/space/acc/info" +
                $"?mid={mid}";

            var response = await http.GetAsync(apiurl);

            if (response.IsSuccessStatusCode)
            {
                var rawjson = await response.Content.ReadAsStringAsync();

                var info = JsonConvert.DeserializeObject<UserInfo>(rawjson);

                if (info.Code == 0)
                {
                    return info.Data;
                }
            }

            return null;
        }

        public async Task<List<MemberInfo>> GetAllMembersAsync(string uid)
        {
            var userinfo = await GetUserInfoAsync(uid);

            if (userinfo != null)
            {
                var roomid = userinfo.Liveroom.RoomId;

                var apiurl = "https://api.live.bilibili.com/xlive/app-room/v2/guardTab/topList" +
                    $"?roomid={roomid}" +
                    $"&page=1" +
                    $"&ruid={uid}" +
                    $"&page_size=29";

                var response = await http.GetAsync(apiurl);

                if (response.IsSuccessStatusCode)
                {
                    var rawjson = await response.Content.ReadAsStringAsync();

                    var info = JsonConvert.DeserializeObject<MemberListInfo>(rawjson);

                    var totalPageCount = info.Data.Info.TotalPage;

                    List<MemberInfo> result = new(info.Data.CurrentPage);

                    // B站真有你的
                    result.AddRange(info.Data.Top3);

                    for (int i = 2; i <= totalPageCount; i++)
                    {
                        result.AddRange(await GetMemberOfPageAsync(uid, roomid, i));
                    }

                    return result;
                }
            }
            return null;
        }

        private async Task<List<MemberInfo>> GetMemberOfPageAsync(string uid, string roomid, int page)
        {
            var apiurl = "https://api.live.bilibili.com/xlive/app-room/v2/guardTab/topList" +
                    $"?roomid={roomid}" +
                    $"&page={page}" +
                    $"&ruid={uid}" +
                    $"&page_size=29";

            var response = await http.GetAsync(apiurl);

            if (response.IsSuccessStatusCode)
            {
                var rawjson = await response.Content.ReadAsStringAsync();

                var info = JsonConvert.DeserializeObject<MemberListInfo>(rawjson);

                return info.Data.CurrentPage;
            }

            return null;
        }
    }
}
