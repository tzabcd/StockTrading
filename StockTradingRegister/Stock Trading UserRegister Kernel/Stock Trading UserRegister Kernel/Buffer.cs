#define DEBUG
#undef DEBUG

using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;

//////////////////////////////////////////////////////////////////////////
// 1.用户缓存只有一个，当用户注册数越来越多的时候，性能势必会下降，每次查询都会遍历所有用户
// 2.检测用户存在和返回用户信息因为程序代码几乎完全一样，可合为一个函数，提高性能。
//
//////////////////////////////////////////////////////////////////////////
namespace Stock_Trading_UserRegister_Kernel
{
    /// <summary>
    /// 用户表缓存类
    /// </summary>
    public class UserBuffer
    {
        //用户表缓存 结构：Dictionary<区域ID,Dictionary<用户名, 用户信息>>
        private Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>> mapUser = null;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            try
            {
                mapUser = new Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>>();
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 清除用户表缓存
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            try
            {
                if (mapUser == null)
                    mapUser = new Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>>();
                else
                {
                    lock (mapUser)
                    {
                        mapUser.Clear();
                    }
                }

                Loger.Debug("Interface Buffer Cleared");
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err.ToString());
                return false;
            }
        }

        /// <summary>
        /// 获取指定活动ID和用户名的用户信息
        /// </summary>
        /// <param name="playId">活动ID</param>
        /// <param name="userName">用户名</param>
        /// <returns></returns>
        public RemotingInterface.UserInfo GetUserInfo(int playId, string userName)
        {
            RemotingInterface.UserInfo userInfo = new RemotingInterface.UserInfo();
            userInfo.Clear();
            try
            {
                if (playId <= 0 || userName == string.Empty)
                    return userInfo;

                Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>> mapTempUser = null;
                lock (mapUser)
                {
                    mapTempUser = new Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>>(mapUser);
                }

                if (mapTempUser == null)
                    return userInfo;

                foreach (KeyValuePair<int, Dictionary<string, RemotingInterface.UserInfo>> amap in mapTempUser)
                {
                    foreach (KeyValuePair<string, RemotingInterface.UserInfo> umap in amap.Value)
                    {
                        if (playId == umap.Value.PlayId && userName == umap.Value.UserPassPortId)  //找到与指定PlayId和UserName对应的用户，直接返回
                            return umap.Value;
                    }
                }
                return userInfo;
            }
            catch (Exception err)
            {
                Loger.Debug("GetUserInfo error :" + err.ToString());
                return userInfo;
            }
        }

        /// <summary>
        /// 检测用户是否存在
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns></returns>
        public bool IsExistUser(int playId, string userName)
        {
            try
            {
                Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>> mapTempUser = null;
                mapTempUser = new Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>>(mapUser);
                if (mapTempUser == null)
                    return false;

                foreach (KeyValuePair<int, Dictionary<string, RemotingInterface.UserInfo>> amap in mapTempUser)
                {
                    foreach (KeyValuePair<string, RemotingInterface.UserInfo> umap in amap.Value)
                    {
                        if (playId == umap.Value.PlayId && userName == umap.Value.UserPassPortId)
                            return true;
                    }
                }

                return false;
            }
            catch (Exception err)
            {
                Loger.Debug("IsExistUser(" + playId + "," + userName + ") error :" + err.ToString());
                return false;
            }
        }

        /// <summary>
        /// 检测用户是否存在，如果存在则返回该用户信息
        /// </summary>
        /// <param name="playId">活动ID</param>
        /// <param name="userName">用户名</param>
        /// <param name="userInfo">用户信息</param>
        /// <returns></returns>
        public bool IsExistUser(int playId, string userName, out RemotingInterface.UserInfo userInfo)
        {
            userInfo = new RemotingInterface.UserInfo();
            userInfo.Clear();
            try
            {
                Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>> mapTempUser = null;
                mapTempUser = new Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>>(mapUser);
                if (mapTempUser == null)
                    return false;

                foreach (KeyValuePair<int, Dictionary<string, RemotingInterface.UserInfo>> amap in mapTempUser)
                {
                    foreach (KeyValuePair<string, RemotingInterface.UserInfo> umap in amap.Value)
                    {
                        if (playId == umap.Value.PlayId && userName == umap.Value.UserPassPortId)
                        {
                            userInfo = umap.Value;
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception err)
            {
                Loger.Debug("IsExistUser(int playId, string userName,out RemotingInterface.UserInfo userInfo) error :" + err.ToString());
                return false;
            }
        }

        /// <summary>
        /// 用户信息加入用户缓存表
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public bool SetUserInfo(RemotingInterface.UserInfo userInfo)
        {
            try
            {
                if (userInfo.AreaId <= 0)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(userInfo.UserPassPortId))
                {
                    return false;
                }
                lock (mapUser)
                {

                    if (!mapUser.ContainsKey(userInfo.AreaId))
                    {
                        Dictionary<string, RemotingInterface.UserInfo> mapTempUser = new Dictionary<string, RemotingInterface.UserInfo>();
                        mapTempUser[userInfo.UserPassPortId] = userInfo;
                        mapUser.Add(userInfo.AreaId, mapTempUser);
                    }
                    else
                    {
                        if (!mapUser[userInfo.AreaId].ContainsKey(userInfo.UserPassPortId))
                        {
                            mapUser[userInfo.AreaId].Add(userInfo.UserPassPortId, userInfo);
                        }
                    }
                    return true;
                }
            }
            catch (Exception err)
            {
                Loger.Debug("SetUserInfo error : " + err.ToString());
                return false;
            }
        }

        public bool ChangeNickName(RemotingInterface.UserInfo userInfo)
        {
            try
            {
                if (userInfo.AreaId <= 0)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(userInfo.UserPassPortId))
                {
                    return false;
                }
                lock (mapUser)
                {
                    if (!mapUser.ContainsKey(userInfo.AreaId))
                    {
                        return false;
                    }

                    if (!mapUser[userInfo.AreaId].ContainsKey(userInfo.UserPassPortId))
                    {
                        return false;
                    }

                    mapUser[userInfo.AreaId][userInfo.UserPassPortId] = userInfo;
                    return true;
                }
            }
            catch (Exception err)
            {
                Loger.Debug("NickName change error : " + err.ToString());
                return false;
            }
        }

        public bool RemoveUserInfo(RemotingInterface.UserInfo userInfo)
        {
            try
            {
                if (userInfo.AreaId <= 0)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(userInfo.UserPassPortId))
                {
                    return false;
                }
                lock (mapUser)
                {
                    if (mapUser.ContainsKey(userInfo.AreaId))
                    {
                        if (mapUser[userInfo.AreaId].ContainsKey(userInfo.UserPassPortId))
                        {
                            mapUser[userInfo.AreaId].Remove(userInfo.UserPassPortId);
                            Loger.Debug(" remove cache success , userName = " + userInfo.UserPassPortId);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception err)
            {
                Loger.Debug("remove user error : userName = "+userInfo.UserPassPortId+" error : " + err.ToString());
                return false;
            }
        }
    }
}
