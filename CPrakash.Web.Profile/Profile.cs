using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Web;

namespace CPrakash.Web.Profile
{
    /// <summary>
    /// Profile
    /// </summary>
    public class Profile
    {
        #region Constructor
        
        /// <summary>
        /// Profile
        /// </summary>
        public Profile(IProfileService profileService)
            : this(profileService, HttpContext.Current.Request.IsAuthenticated ? HttpContext.Current.User.Identity.Name 
            : HttpContext.Current.Request.AnonymousID, HttpContext.Current.Request.IsAuthenticated)
        {
        }

        /// <summary>
        /// Profile
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="isAuthenticated"></param>
        public Profile(IProfileService profileService, string userName, bool isAuthenticated)
        {
            this.ProfileService = profileService;
            this.UserName = userName;
            this.IsAuthenticated = isAuthenticated;
            Init();
        }

        #endregion

        #region public Members

        public string UserName { get; private set; }

        public bool IsAuthenticated { get; private set; }

        public dynamic Properties { get; private set; }

        #endregion

        #region Private Members

        private IProfileService ProfileService;

        #endregion

        #region Private Methods

        /// <summary>
        /// Init
        /// </summary>
        private void Init()
        {
            if (ProfileService != null)
            {
                var _members = ProfileService.GetPropertyValues(this.UserName);
                Properties = new ProfileProperties(_members);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Save
        /// </summary>
        public void Save()
        {
            if (ProfileService != null)
                ProfileService.SetPropertyValues(this.UserName, this.Properties.Members);
        }

        #endregion
    }
}
