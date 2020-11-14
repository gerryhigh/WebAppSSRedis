<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebAppSSRedis._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
		Test SS-Redis custom auth, use username of Bob or Mary from 2 browsers, then recycle redis<br />
		Then refresh page from each browser		
    </div>

    <div class="row">
        <div class="col-md-4">
			User:&nbsp;<asp:TextBox ID="txtUser" runat="server" /><br />
			<input type="button" value="Login" onclick="javascript:doAuth();" />
        </div>
        <div class="col-md-4">
			<asp:Label ID="lblUser" runat="server" />
        </div>
    </div>

	<script type="text/javascript">
		function doAuth() {
			var userName = $('#<%=txtUser.ClientID%>').val();
			console.log(userName);
			$.post('/api/auth?format=json', { Username: userName, Password: 'blank' }, function (resp) {
				console.dir(resp);
				$('#<%=lblUser.ClientID%>').text(resp.DisplayName);
			});

		}
	</script>

</asp:Content>
