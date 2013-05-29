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
    public class Profile : DynamicObject
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

        #region Private Members

        public string UserName { get; private set; }

        public bool IsAuthenticated { get; private set; }

        private Dictionary<string, object> _members = new Dictionary<string, object>();

        private IProfileService ProfileService;

        #endregion

        #region Private Methods

        /// <summary>
        /// Init
        /// </summary>
        private void Init()
        {
            if (ProfileService != null)
                _members = ProfileService.GetPropertyValues(this.UserName);
        }

        #endregion

        #region Public Methods

        #region Overriden DynamicObject Method

        /// <summary>
        /// When a new property is set, 
        /// add the property name and value to the dictionary
        /// </summary>     
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!_members.ContainsKey(binder.Name))
                _members.Add(binder.Name, value);
            else
                _members[binder.Name] = value;

            return true;
        }

        /// <summary>
        /// When user accesses something, return the value if we have it
        /// </summary>      
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_members.ContainsKey(binder.Name))
            {
                result = _members[binder.Name];
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        /// <summary>
        /// If a property value is a delegate, invoke it
        /// </summary>     
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (_members.ContainsKey(binder.Name) && _members[binder.Name] is Delegate)
            {
                result = (_members[binder.Name] as Delegate).DynamicInvoke(args);
                return true;
            }
            return base.TryInvokeMember(binder, args, out result);
        }

        /// <summary>
        /// Return all dynamic member names
        /// </summary>
        /// <returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _members.Keys;
        }

        #endregion

        /// <summary>
        /// Save
        /// </summary>
        public void Save()
        {
            if (ProfileService != null)
                ProfileService.SetPropertyValues(this.UserName, _members);
        }

        #endregion
    }
}
