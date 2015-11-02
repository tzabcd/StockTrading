using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Stock_Trading_UserRegister_Kernel
{
    /// <summary>
    /// 接口
    /// </summary>
    public class RemotingInterface : MarshalByRefObject
    {
        #region 用户信息
        [Serializable]
        public struct UserInfo
        {
            public int UserId;
            public string UserPassPortId;
            public string UserNickName;
            public int PlayId;
            public int GameId;
            public int AreaId;
            public DateTime Rtime;
            public string RIP;
            public string UserDataBase;
            public byte Validity;

            public void Clear()
            {
                UserId = 0;
                UserPassPortId = "";
                UserNickName = "";
                PlayId = 0;
                GameId = 0;
                AreaId = 0;
                Rtime = DateTime.MinValue;
                RIP = "";
                UserDataBase = "";
                Validity = 0;
            }
        }
        #endregion

        #region 活动信息
        [Serializable]
        public struct PlayInfo
        {
            public int PlayId;
            public string PlayName;
            public byte PlayState;
            public string Description;

            public void Clear()
            {
                PlayId = 0;
                PlayName = "";
                PlayState = 0;
                Description = "";
            }
        }
        #endregion

        #region 比赛信息
        [Serializable]
        public struct GameInfo
        {
            public int GameId;
            public int PlayId;
            public string GameName;
            public byte GameType;
            public DateTime RegDateStart;
            public DateTime RegDateEnd;
            public DateTime GameDateStart;
            public DateTime GameDateEnd;
            public string Description;
            public byte GameLevel;
            public byte GameState;

            public void Clear()
            {
                GameId = 0;
                PlayId = 0;
                GameName = "";
                GameType = 0;
                RegDateStart = DateTime.MinValue;
                RegDateEnd = DateTime.MinValue;
                GameDateStart = DateTime.MinValue;
                GameDateEnd = DateTime.MinValue;
                Description = "";
                GameLevel = 0;
                GameState = 0;
            }
        }
        #endregion

        #region 区域信息
        [Serializable]
        public struct AreaInfo
        {
            public int AreaId;
            public string AreaName;
            public string AreaDescription;
            public DateTime CreateDate;
            public byte AreaState;
            public int GameId;
            public DateTime AreaRegDateStart;
            public DateTime AreaRegDateEnd;
            public DateTime AreaDateStart;
            public DateTime AreaDateEnd;

            public void Clear()
            {
                AreaId = 0;
                AreaName = "";
                AreaDescription = "";
                CreateDate = DateTime.MinValue;
                AreaState = 0;
                GameId = 0;
                AreaRegDateStart = DateTime.MinValue;
                AreaRegDateEnd = DateTime.MinValue;
                AreaDateStart = DateTime.MinValue;
                AreaDateEnd = DateTime.MinValue;
            }
        }
        #endregion


        #region 添加新用户
        public bool RequestNewUser(string strRemoteKey,RemotingInterface.UserInfo userInfo)
        {
            try
            {
                if (BaseConfig.bAllowReg == false)
                    return false;

                if (userInfo.AreaId<=0 || userInfo.UserPassPortId.Length<=0)
                {
                    Loger.Debug("userInfo areaId or UserPassPortId error ");
                    return false;
                }

                if (IsExistUser(userInfo.PlayId, userInfo.UserPassPortId))
                {
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": 用户 [ " + userInfo.UserPassPortId + " ] 已经存在");
                    return false;
                }

                bool bRet = Common.DBSync.AddNewUser(userInfo);
                return bRet;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region 修改昵称
        public bool ChangeNickName(RemotingInterface.UserInfo userInfo)
        {
            if (!IsExistUser(userInfo.PlayId, userInfo.UserPassPortId))
            {
                Loger.Debug("ChangeNickName user [" + userInfo.UserPassPortId + "] dose not in the buffer !");
                return false;
            }
            bool bRet = Common.DBSync.ChangeNickName(userInfo);
            return bRet;
        }
        #endregion

        #region 判断用户存在
        public bool IsExistUser(int playId, string userName)
        {
            return Common.userBuffer.IsExistUser(playId, userName);
        }
        #endregion

        #region 移除指定用户
        public bool RemoveUser(int playId, string userName)
        {
            if (!IsExistUser(playId, userName))
            {
                Loger.Debug("user [" + userName + "] dose not in the buffer !");
                return true;
            }

            Loger.Debug("user [" + userName + "] has been removed from buffer !");
            return false;
        }
        #endregion

        #region 获取相关列表

        #region 获取活动列表
        public List<RemotingInterface.PlayInfo> GetPlayList()
        {
            return new PlaySystem().GetPlayList();
        }
        #endregion


        #region 获取比赛列表
        public List<RemotingInterface.GameInfo> GetGameList(int playId)
        {
            return new PlaySystem().GetGameList(playId);
        }
        #endregion

        #region 获取区域列表
        public List<RemotingInterface.AreaInfo> GetAreaList(int gameId)
        {
            return new PlaySystem().GetAreaList(gameId);
        }
        #endregion

        #endregion

        #region 获取单条记录信息

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="playId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public UserInfo GetUserInfo(int playId, string userName)
        {
            return Common.userBuffer.GetUserInfo(playId, userName);
        }

        /// <summary>
        /// 获取活动信息
        /// </summary>
        /// <param name="playId"></param>
        /// <returns></returns>
        public RemotingInterface.PlayInfo GetPlayInfo(int playId)
        {
            RemotingInterface.PlayInfo playInfo = new PlayInfo();
            playInfo.Clear();

            IList<RemotingInterface.PlayInfo> _list = new List<PlayInfo>();
            _list = GetPlayList();
            _list = (from row in _list
                     where row.PlayId == playId
                     select row).ToList<RemotingInterface.PlayInfo>();
            if (_list.Count == 1)
            {
                playInfo = _list[0];
            }
            return playInfo;
        }

        /// <summary>
        /// 获取比赛信息
        /// </summary>
        /// <param name="playId"></param>
        /// <param name="gameId"></param>
        /// <returns></returns>
        public RemotingInterface.GameInfo GetGameInfo(int playId,int gameId)
        {
            RemotingInterface.GameInfo gameInfo = new GameInfo();
            gameInfo.Clear();

            IList<RemotingInterface.GameInfo> _list = new List<GameInfo>();
            _list = GetGameList(playId);
            _list = (from row in _list
                     where row.GameId== gameId
                     select row).ToList<RemotingInterface.GameInfo>();
            if (_list.Count == 1)
            {
                gameInfo = _list[0];
            }
            return gameInfo;
        }

        /// <summary>
        /// 获取区域信息
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public RemotingInterface.AreaInfo GetAreaInfo(int gameId, int areaId)
        {
            RemotingInterface.AreaInfo areaInfo = new AreaInfo();
            areaInfo.Clear();

            IList<RemotingInterface.AreaInfo> _list = new List<AreaInfo>();
            _list = GetAreaList(gameId);
            _list = (from row in _list
                     where row.AreaId == areaId
                     select row).ToList<RemotingInterface.AreaInfo>();
            if (_list.Count == 1)
            {
                areaInfo = _list[0];
            }
            return areaInfo;
        }
        #endregion
    }
}
