using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Runtime.Serialization;

using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Caching;
using ServiceStack.Auth;
using ServiceStack.Web;

namespace WebAppSSRedis
{
	public class MyAppHost : AppHostBase
	{
		//Tell Service Stack the name of your application and where to find your web services
		public MyAppHost() : base("Service Stack Web Services", typeof(MyAppHost).Assembly) { }

		public override void Configure(Funq.Container container)
		{
			container.Register<IRedisClientsManager>(c =>
				new PooledRedisClientManager("localhost:6379"));

			container.Register(c => c.Resolve<IRedisClientsManager>().GetCacheClient());
			container.Register<ISessionFactory>(c => new SessionFactory(c.Resolve<ICacheClient>()));

			var path = "api";

			SetConfig(new HostConfig
			{
				EnableFeatures = Feature.All.Remove(Feature.Metadata),
				HandlerFactoryPath = path, // used to set path to web services,
				StrictMode = false
			});
			Plugins.Add(new AuthFeature(() => new CustomUserSession(), new IAuthProvider[] {
					new CustomCredentialsAuthProvider()
					{
						SessionExpiry = TimeSpan.FromMinutes(120)
					}
				}));
		}
	}

	public class CustomAuthResponse : ServiceStack.AuthenticateResponse
	{
		public CustomUserSession UserSess { get; set; }
	}
	public class CustomCredentialsAuthProvider : ServiceStack.Auth.CredentialsAuthProvider
	{
		public override object Authenticate(IServiceBase authService, IAuthSession session, Authenticate request)
		{
			var response = base.Authenticate(authService, session, request);
			// grab the session
			var customSession = authService.GetSession() as CustomUserSession;

			// if response can be cast and customSession exists
			if (response is ServiceStack.AuthenticateResponse && customSession != null)
			{
				// cast
				var authResponse = response as ServiceStack.AuthenticateResponse;

				//// build custom response
				var customAuthResponse = new CustomAuthResponse
				{
					ReferrerUrl = authResponse.ReferrerUrl,
					SessionId = authResponse.SessionId,
					ResponseStatus = authResponse.ResponseStatus,
					UserName = authResponse.UserName,
					DisplayName = customSession.FirstName + " " + customSession.LastName,
					UserSess = customSession
				};

				return customAuthResponse;
			}

			// return the standard response
			return response;
		}
		public override bool TryAuthenticate(IServiceBase authService, string userName, string password)
		{
			var session = authService.GetSession() as CustomUserSession;
			// test only auth username bob or mary
			var user = userName.ToLower().Trim();
			bool bLogin = false;
			if (user == "bob" || user == "mary")
			{
				var isBob = user == "bob";
				bLogin = true;
				session.UserName = userName;
				session.FirstName = isBob ? "Bob" : "Mary";
				session.LastName = isBob ? "Davis" : "Smith";
				session.DisplayName = session.FirstName + " " + session.LastName;
				// just add something extra
				session.ExtraMetaData = isBob ? "USA" : "Scottland";
			}
			session.IsAuthenticated = bLogin;

			return bLogin;
		}

		public override IHttpResult OnAuthenticated(IServiceBase authService, IAuthSession session, IAuthTokens tokens, Dictionary<string, string> authInfo)
		{
			authService.Request.RemoveSession();
			// Override session options
			authService.Request.AddSessionOptions(SessionOptions.Temporary);
			// Create new session cookies
			session.Id = authService.Request.Response.CreateSessionIds(authService.Request);

//			authInfo = new Dictionary<string, string>();
//			authInfo.Add("Security", (authService.GetSession() as CustomUserSession).SecurityLevel.ToString());

			try
			{
				session.IsAuthenticated = true;
				session.OnAuthenticated(authService, session, tokens, authInfo);
				AuthEvents.OnAuthenticated(authService.Request, session, authService, tokens, authInfo);
			}
			finally
			{
				authService.SaveSession(session, SessionExpiry);
			}

			return null;
			//				return base.OnAuthenticated(authService, session, tokens, authInfo);
		}

		public override object Logout(IServiceBase service, Authenticate request)
		{
			var session = service.GetSession() as CustomUserSession;
			// do something
			return base.Logout(service, request);
		}
	}
}