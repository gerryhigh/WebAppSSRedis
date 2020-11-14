using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebAppSSRedis
{
	public partial class _Default : BasePage
	{
		protected override void Page_Load(object sender, EventArgs e)
		{
			base.Page_Load(sender, e);

			if (UserSession.IsAuthenticated)
			{
				lblUser.Text = UserSession.DisplayName;
			}
		}
	}
}