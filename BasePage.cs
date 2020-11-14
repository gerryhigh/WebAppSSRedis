using System;
using ServiceStack;
using ServiceStack.Caching;

namespace WebAppSSRedis
{
	/// <summary>
	/// Summary description for BasePage.
	/// </summary>
	public class BasePage : System.Web.UI.Page
	{
		#region Properties

		public bool IsLoggedIn { get { return UserSession.IsAuthenticated; } }

		/// <summary>
		/// Typed UserSession
		/// </summary>
		private object userSession;
		protected virtual TUserSession SessionAs<TUserSession>()
		{
			return (TUserSession)(userSession ?? (userSession = SessionFeature.GetOrCreateSession<CustomUserSession>(Cache)));
			//			return (TUserSession)(userSession ?? (userSession = Cache.SessionAs<TUserSession>()));
		}

		public CustomUserSession UserSession
		{
			get
			{
				//				return SessionFeature.GetOrCreateSession<Common.Models.CustomUserSession>(Cache);
				return SessionAs<CustomUserSession>();
			}
		}

		public new ICacheClient Cache
		{
			get { return ServiceStack.HostContext.Resolve<ServiceStack.Caching.ICacheClient>(); }
		}

		//private ISessionFactory sessionFactory;
		//public virtual ISessionFactory SessionFactory
		//{
		//	get { return sessionFactory ?? (sessionFactory = ServiceStack.HostContext.Resolve<ISessionFactory>()) ?? new SessionFactory(Cache); }
		//}

		/// <summary>
		/// Dynamic Session Bag
		/// </summary>
		//		private ISession session;
		//		public new ISession Session
		//		{
		//			get
		//			{
		//				SessionFeature.GetOrCreateSession()
		////				return session ?? (session = SessionFeature.GetOrCreateSession<AuthUserSession>(Cache);
		//				return session ?? (session = SessionFactory.GetOrCreateSession());
		//			}
		//		}

		public void ClearSession()
		{
			userSession = null;
			this.Cache.Remove(SessionFeature.GetSessionKey());
		}
		#endregion

		public BasePage()
		{
		}
		protected override void OnPreLoad(EventArgs e)
		{
			base.OnPreLoad(e);
			if (UserSession.IsAuthenticated)
			{
				var ts = TimeSpan.FromMinutes(120);
				UserSession.LastModified = DateTime.UtcNow;
				var httpReq = ServiceStack.HostContext.AppHost.TryGetCurrentRequest();
				httpReq.SaveSession(UserSession, ts);
				//				Cache.Set<Common.Models.CustomUserSession>(SessionFeature.GetSessionKey(), UserSession, ts);
			}
			//else
			//{
			//	Common.Logger.LogInfo("OnPreLoad:Session Not authenticated for " + UserSession.UserName + "," +UserSession.);

			//}
		}
		protected virtual void Page_Load(object sender, EventArgs e)
		{
			Response.Cache.SetExpires(System.DateTime.Now);
		}
	}
}
