using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPrakash.Web.Profile
{
    /// <summary>
    /// IProfileService
    /// </summary>
    public interface IProfileService
    {
        /// <summary>
        /// GetPropertyValues
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Dictionary<string, object> GetPropertyValues(string userName);

        /// <summary>
        /// SetPropertyValues
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="properties"></param>
        void SetPropertyValues(string userName, Dictionary<string, object> properties);
    }
}
