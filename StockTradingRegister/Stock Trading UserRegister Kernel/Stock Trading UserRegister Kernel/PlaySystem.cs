using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace Stock_Trading_UserRegister_Kernel
{
    /// <summary>
    /// 活动系统
    /// </summary>
    class PlaySystem
    {
        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <returns></returns>
        public List<RemotingInterface.PlayInfo> GetPlayList()
        {
            SqlConnection sqlConn = new SqlConnection(Common.strConn);
            if (sqlConn.State == ConnectionState.Closed)
                sqlConn.Open();
            try
            {
                SqlCommand sqlCmd = new SqlCommand("SELECT * FROM [Play] ORDER BY PlayID", sqlConn);
                SqlDataReader sqlReader = sqlCmd.ExecuteReader();

                List<RemotingInterface.PlayInfo> listPlay = new List<RemotingInterface.PlayInfo>();
                RemotingInterface.PlayInfo playInfo = new RemotingInterface.PlayInfo();
                playInfo.Clear();

                while (sqlReader.Read())
                {
                    playInfo.PlayId = Convert.ToInt32(sqlReader["PlayId"]);
                    playInfo.PlayName = sqlReader["PlayName"].ToString();
                    playInfo.PlayState = Convert.ToByte(sqlReader["PlayState"]);
                    playInfo.Description = sqlReader["Description"].ToString();

                    listPlay.Add(playInfo);

                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": Load the play list : PlayId = " + playInfo.PlayId + "\t PlayName = " + playInfo.PlayName);
                }

                return listPlay;
            }
            catch (Exception ex)
            {
                Loger.Debug(ex.ToString());
                return null;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        /// <summary>
        /// 获取比赛列表
        /// </summary>
        /// <param name="playId"></param>
        /// <returns></returns>
        public List<RemotingInterface.GameInfo> GetGameList(int playId)
        {
            SqlConnection sqlConn = new SqlConnection(Common.strConn);
            if (sqlConn.State == ConnectionState.Closed)
                sqlConn.Open();
            try
            {
                if (playId == 0)
                    return null;

                SqlCommand sqlCmd = new SqlCommand("SELECT * FROM [Game] where playId=" + playId + " ORDER BY GameID", sqlConn);
                SqlDataReader sqlReader = sqlCmd.ExecuteReader();

                List<RemotingInterface.GameInfo> listGame = new List<RemotingInterface.GameInfo>();
                RemotingInterface.GameInfo gameInfo = new RemotingInterface.GameInfo();
                gameInfo.Clear();

                while (sqlReader.Read())
                {
                    gameInfo.GameId = Convert.ToInt32(sqlReader["GameId"]);
                    gameInfo.PlayId = Convert.ToInt32(sqlReader["PlayId"]);
                    gameInfo.GameName = sqlReader["GameName"].ToString();
                    gameInfo.GameType = Convert.ToByte(sqlReader["GameType"]);
                    gameInfo.RegDateStart = Convert.ToDateTime(sqlReader["RegDateStart"]);
                    gameInfo.RegDateEnd = Convert.ToDateTime(sqlReader["RegDateEnd"]);
                    gameInfo.GameDateStart = Convert.ToDateTime(sqlReader["GameDateStart"]);
                    gameInfo.GameDateEnd = Convert.ToDateTime(sqlReader["GameDateEnd"]);
                    gameInfo.GameLevel = Convert.ToByte(sqlReader["GameLevel"]);
                    gameInfo.GameState = Convert.ToByte(sqlReader["GameState"]);
                    gameInfo.Description = sqlReader["Description"].ToString();

                    listGame.Add(gameInfo);

                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+": Load the Game list : PlayId = " + gameInfo.PlayId + "\t GameId = " + gameInfo.GameId + " \t GameName = " + gameInfo.GameName);
                }

                return listGame;
            }
            catch (Exception ex)
            {
                Loger.Debug(ex.ToString());
                return null;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        /// <summary>
        /// 获取区域列表
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        public List<RemotingInterface.AreaInfo> GetAreaList(int gameId)
        {
            SqlConnection sqlConn = new SqlConnection(Common.strConn);
            if (sqlConn.State == ConnectionState.Closed)
                sqlConn.Open();
            try
            {
                if (gameId == 0)
                    return null;

                SqlCommand sqlCmd = new SqlCommand("SELECT * FROM [Area] where GameId=" + gameId + " ORDER BY AreaID", sqlConn);
                SqlDataReader sqlReader = sqlCmd.ExecuteReader();

                List<RemotingInterface.AreaInfo> listArea = new List<RemotingInterface.AreaInfo>();
                RemotingInterface.AreaInfo areaInfo = new RemotingInterface.AreaInfo();
                areaInfo.Clear();

                while (sqlReader.Read())
                {
                    areaInfo.AreaId = Convert.ToInt32(sqlReader["AreaId"]);
                    areaInfo.AreaName = sqlReader["AreaName"].ToString();
                    areaInfo.AreaRegDateStart = Convert.ToDateTime(sqlReader["AreaRegDateStart"]);
                    areaInfo.AreaRegDateEnd = Convert.ToDateTime(sqlReader["AreaRegDateEnd"]);
                    areaInfo.AreaDateStart = Convert.ToDateTime(sqlReader["AreaDateStart"]);
                    areaInfo.AreaDateEnd = Convert.ToDateTime(sqlReader["AreaDateEnd"]);
                    areaInfo.AreaState = Convert.ToByte(sqlReader["AreaState"]);
                    areaInfo.CreateDate = Convert.ToDateTime(sqlReader["CreateDate"]);
                    areaInfo.AreaDescription = sqlReader["AreaDescription"].ToString();

                    if (listArea.Contains(areaInfo))
                        listArea[areaInfo.AreaId] = areaInfo;
                    else
                        listArea.Add(areaInfo);

                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": Load the Area list : GameId = " + gameId + "\t AreaId = " + areaInfo.AreaId + " \t AreaName = " + areaInfo.AreaName);

                }

                return listArea;
            }
            catch (Exception ex)
            {
                Loger.Debug(ex.ToString());
                return null;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }
    }
}
