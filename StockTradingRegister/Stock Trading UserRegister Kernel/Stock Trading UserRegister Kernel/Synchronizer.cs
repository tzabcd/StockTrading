#define DEBUG
//#undef DEBUG
#define RELEASE
//#undef RELEASE

using System;
using System.Text;
using System.Threading;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using zns = EastMoney.StockIndexTrader.RemotingProvider.Protocol;


namespace Stock_Trading_UserRegister_Kernel
{
    /// <summary>
    /// 数据同步
    /// </summary>
    public class Synchronizer
    {
        private SqlConnection sqlConn = null;
        private SqlCommand sqlCmd = null;
        private SqlDataReader sqlReader = null;

        /// <summary>
        /// 初始化对象时建立连接
        /// </summary>
        /// <param name="strConn"></param>
        public Synchronizer(string strConn)
        {
            sqlConn = new SqlConnection(strConn.Trim());
        }

        /// <summary>
        /// 初始化，从数据库加载数据到用户缓存表
        /// </summary>
        /// <param name="mapUser"></param>
        /// <returns></returns>
        public bool Initialize(ref Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>> mapUser)
        {
            try
            {
                if (mapUser == null)
                    mapUser = new Dictionary<int, Dictionary<string, RemotingInterface.UserInfo>>();
                else
                    mapUser.Clear();

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                //获取所有正在进行的活动中已报名的用户信息
                sqlCmd = new SqlCommand(@" select p.playId,p.playName,g.gameId,g.gameName,g.gameState,a.areaId,a.areaName,u.* 
                                        from userlist"+BaseConfig.PlayId+@" u,play p,game g,area a
                                        where u.areaId=a.areaId
                                        and a.gameId=g.gameId
                                        and g.playId=p.playId
                                        and g.gameState = 1", sqlConn);
                sqlReader = sqlCmd.ExecuteReader();

                while (sqlReader.Read())
                {
                    RemotingInterface.UserInfo userInfo = new RemotingInterface.UserInfo();
                    userInfo.Clear();
                    userInfo.UserId = Convert.ToInt32(sqlReader["UserId"]);
                    userInfo.UserPassPortId = sqlReader["UserName"].ToString();
                    userInfo.UserNickName = sqlReader["NickName"].ToString();
                    userInfo.PlayId = Convert.ToInt32(sqlReader["PlayId"]);
                    userInfo.GameId = Convert.ToInt32(sqlReader["GameId"]);
                    userInfo.AreaId = Convert.ToInt32(sqlReader["AreaId"]);
                    userInfo.UserDataBase = sqlReader["UserDataBase"].ToString();

                    //加入RegisterSystem中的mapUser
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
 

                    //加入用户缓存表
                    if (Common.userBuffer != null)
                    {
                        Common.userBuffer.SetUserInfo(userInfo);
                    }
                    else
                    {
                        Loger.Debug("userBuffer is null!");
                    }
 
                }
                sqlReader.Close();

                return true;
            }
            catch (Exception err)
            {
                Loger.Debug(err.ToString());
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }



        /// <summary>
        /// 将用户信息加入用户内存，并将新用户加入新用户队列
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public bool AddNewUser(RemotingInterface.UserInfo User)
        {
            try
            {
                if (BaseConfig.bAllowReg == false)
                    return false;

                if (User.UserPassPortId == "" || User.PlayId <= 0 || User.AreaId <= 0)
                    return false;

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                //先在数据库中查找指定的用户信息
                sqlCmd = new SqlCommand("SELECT AreaId,UserName,UserDataBase FROM [UserList" + BaseConfig.PlayId + "] WHERE AreaId = @AreaId AND UserName = @UserName AND Validity = 1 ", sqlConn);
                sqlCmd.Parameters.Add("@AreaId", SqlDbType.Int); sqlCmd.Parameters["@AreaId"].Value = User.AreaId;
                sqlCmd.Parameters.Add("@UserName", SqlDbType.VarChar, 32); sqlCmd.Parameters["@UserName"].Value = User.UserPassPortId;
                sqlReader = sqlCmd.ExecuteReader();
                if (sqlReader.Read())
                {
                    sqlReader.Close();
                    return false;
                }
                else  //未找到则添加
                {
                    sqlReader.Close();

                    //为用户分配撮合交易服务器
#if DEBUG
                    #region 轮循
                    if (!Regex.IsMatch(BaseConfig.CurrentRmtServerKey, "[A-K]"))
                    {
                        BaseConfig.CurrentRmtServerKey = "K";
                    }
                    char chrBaseChar = char.Parse(BaseConfig.CurrentRmtServerKey);
                    int nBaseChar = (int)'A';
                    int nOffSet = (int)chrBaseChar - nBaseChar;
                    int nNextOffSet = (nOffSet + 1) % BaseConfig.RmtNumber;
                    char chrCurr = (char)(nBaseChar + nNextOffSet);

                    BaseConfig.CurrentRmtServerKey = chrCurr.ToString();
                    #endregion
#else
                    #region 随机 
                    Random rd = new Random();
                    int nDataBaseChar = rd.Next(0, BaseConfig.RmtNumber);

                    int nBaseChar = (int)'A';
                    BaseConfig.CurrentRmtServerKey = ((char)(nDataBaseChar + nBaseChar)).ToString();
                    #endregion
#endif
                    //加入数据库
                    sqlCmd = new SqlCommand("INSERT INTO [UserList" + BaseConfig.PlayId + "] (AreaID, UserName, NickName, UserDataBase,Rtime,RIP) VALUES (@AreaID, @UserName, @NickName, @UserDataBase,@Rtime,@RIP) SELECT @@IDENTITY", sqlConn);
                    sqlCmd.Parameters.Add("@AreaID", SqlDbType.Int); sqlCmd.Parameters["@AreaID"].Value = User.AreaId;
                    sqlCmd.Parameters.Add("@UserName", SqlDbType.VarChar, 32); sqlCmd.Parameters["@UserName"].Value = User.UserPassPortId;
                    sqlCmd.Parameters.Add("@NickName", SqlDbType.VarChar, 32); sqlCmd.Parameters["@NickName"].Value = User.UserNickName;
                    sqlCmd.Parameters.Add("@UserDataBase", SqlDbType.VarChar, 16); sqlCmd.Parameters["@UserDataBase"].Value = BaseConfig.mapNotifySrv[User.PlayId][BaseConfig.CurrentRmtServerKey].DataBaseChar;
                    sqlCmd.Parameters.Add("@Rtime", SqlDbType.DateTime); sqlCmd.Parameters["@Rtime"].Value = User.Rtime;
                    sqlCmd.Parameters.Add("@RIP", SqlDbType.VarChar, 16); sqlCmd.Parameters["@RIP"].Value = User.RIP;
                    int cRet = Convert.ToInt32(sqlCmd.ExecuteScalar());

                    RemotingInterface.UserInfo urkUser = new RemotingInterface.UserInfo();
                    urkUser.Clear();
                    urkUser.UserId = cRet;
                    urkUser.PlayId = User.PlayId;
                    urkUser.GameId = User.GameId;
                    urkUser.AreaId = User.AreaId;
                    urkUser.UserPassPortId = User.UserPassPortId;
                    urkUser.UserNickName = User.UserNickName;
                    urkUser.UserDataBase = BaseConfig.mapNotifySrv[urkUser.PlayId][BaseConfig.CurrentRmtServerKey].DataBaseChar;

                    #region 注册通知
                    try
                    {
                        //撮合系统接口
                        zns.Result flag = Common.znRmtIobj[User.PlayId][BaseConfig.CurrentRmtServerKey].PostUserData(cRet);
                        if (flag ==zns.Result.Existent_User)
                        {
                            Loger.Debug("撮合系统中存在该用户 userId = " + User.UserPassPortId + " status = " + flag);
                        }
                        else  if (flag!=zns.Result.Success)
                        {
                            Loger.Debug("用户加入撮合系统失败 userId = " + User.UserId + " status = " + flag);
                            RemoveDBFailUser(urkUser);
                            return false;
                        }

                        //加入用户缓存
                        if (Common.userBuffer != null)
                        {
                            Common.userBuffer.SetUserInfo(urkUser);
                            Loger.Debug("完成注册 userId = " + cRet.ToString() + "\t userName = " + User.UserPassPortId + "\t DataBase =" + urkUser.UserDataBase);
                        }
                    }
                    catch (Exception ex)
                    {
                        Loger.Debug("通知失败");
                        RemoveDBFailUser(urkUser);
                        Loger.Debug(ex.ToString());
                    }



                    #endregion

                }
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug("注册失败 userName = " + User.UserPassPortId);
                Loger.Debug(err.ToString());
                if (!sqlReader.IsClosed)
                    sqlReader.Close();
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }

        public bool ChangeNickName(RemotingInterface.UserInfo User)
        {
            try
            {
                if (BaseConfig.bAllowReg == false)
                    return false;

                if (User.UserPassPortId == "" || User.PlayId <= 0 || User.AreaId <= 0)
                    return false;

                if (sqlConn.State == ConnectionState.Closed)
                    sqlConn.Open();

                //先在数据库中查找指定的用户信息
                sqlCmd = new SqlCommand("SELECT AreaId,UserName,UserDataBase FROM [UserList" + BaseConfig.PlayId + "] WHERE AreaId = @AreaId AND UserName = @UserName AND Validity = 1 ", sqlConn);
                sqlCmd.Parameters.Add("@AreaId", SqlDbType.Int); sqlCmd.Parameters["@AreaId"].Value = User.AreaId;
                sqlCmd.Parameters.Add("@UserName", SqlDbType.VarChar, 32); sqlCmd.Parameters["@UserName"].Value = User.UserPassPortId;
                sqlReader = sqlCmd.ExecuteReader();
                if (sqlReader.Read())
                {
                    sqlReader.Close();

                    //加入数据库
                    sqlCmd = new SqlCommand("UPDATE [UserList" + BaseConfig.PlayId + "] SET NickName = @NickName WHERE AreaId = @AreaId AND UserName = @UserName AND Validity = 1 ", sqlConn);
                    sqlCmd.Parameters.Add("@AreaID", SqlDbType.Int); sqlCmd.Parameters["@AreaID"].Value = User.AreaId;
                    sqlCmd.Parameters.Add("@UserName", SqlDbType.VarChar, 32); sqlCmd.Parameters["@UserName"].Value = User.UserPassPortId;
                    sqlCmd.Parameters.Add("@NickName", SqlDbType.VarChar, 32); sqlCmd.Parameters["@NickName"].Value = User.UserNickName;
                    sqlCmd.ExecuteNonQuery();

                    RemotingInterface.UserInfo urkUser = new RemotingInterface.UserInfo();
                    urkUser.Clear();
                    urkUser.PlayId = User.PlayId;
                    urkUser.GameId = User.GameId;
                    urkUser.AreaId = User.AreaId;
                    urkUser.UserPassPortId = User.UserPassPortId;
                    urkUser.UserNickName = User.UserNickName;
                    urkUser.UserDataBase = BaseConfig.mapNotifySrv[urkUser.PlayId][BaseConfig.CurrentRmtServerKey].DataBaseChar;

                    #region 注册通知
                    try
                    {
                        //加入用户缓存
                        if (Common.userBuffer != null)
                        {
                            Common.userBuffer.ChangeNickName(urkUser);
                            Loger.Debug("NickName Changed. userId " + "\t userName = " + User.UserPassPortId );
                        }
                    }
                    catch (Exception ex)
                    {
                        RemoveDBFailUser(urkUser);
                        Loger.Debug(ex.ToString());
                    }



                    #endregion

                }
                return true;
            }
            catch (Exception err)
            {
                Loger.Debug("注册失败 userName = " + User.UserPassPortId);
                Loger.Debug(err.ToString());
                if (!sqlReader.IsClosed)
                    sqlReader.Close();
                return false;
            }
            finally
            {
                if (sqlConn.State != ConnectionState.Closed)
                    sqlConn.Close();
            }
        }
        /// <summary>
        /// 从数据库中移除用户
        /// </summary>
        /// <param name="userId"></param>
        private void RemoveDBFailUser(RemotingInterface.UserInfo userInfo)
        {
            SqlConnection DelConn = new SqlConnection(BaseConfig.ConnStr);
            try
            {
                if (DelConn.State == ConnectionState.Closed)
                    DelConn.Open();

                SqlCommand sqlCmdDel = new SqlCommand("DELETE FROM  [UserList" + BaseConfig.PlayId + "] WHERE UserId = @UserId", sqlConn);
                sqlCmdDel.Parameters.Add("@UserId", SqlDbType.Int);
                sqlCmdDel.Parameters["@UserId"].Value = userInfo.UserId;
                sqlCmdDel.ExecuteNonQuery();
                Loger.Debug(" Remove userId = :" + userInfo.UserId);
                Common.userBuffer.RemoveUserInfo(userInfo);
            }
            catch (Exception err)
            {
                Loger.Debug("RemoveDBFailUser :userId =" + userInfo.UserId + "\t" + err.ToString());
            }
            finally
            {
                if (DelConn.State != ConnectionState.Closed)
                    DelConn.Close();
            }
        }
    }
}
