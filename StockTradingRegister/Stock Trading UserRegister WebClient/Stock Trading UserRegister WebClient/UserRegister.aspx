<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserRegister.aspx.cs" Inherits="Stock_Trading_UserRegister_WebClient._UserRegister" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>无标题页</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    用户名:<asp:TextBox ID="txtUserName" runat="server"></asp:TextBox>
    活动ID:<asp:TextBox ID="txtPlayId" runat="server"></asp:TextBox>
    比赛ID:<asp:TextBox ID="txtGameId" runat="server"></asp:TextBox>
    区域ID:<asp:TextBox ID="txtAreaId" runat="server"></asp:TextBox>
    <asp:Button Text="注册" runat="server" OnClick="btnUserReg" />
    </div>
    </form>
</body>
</html>
