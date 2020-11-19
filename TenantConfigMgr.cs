using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ADMT
{
    public class TenantConfigMgr
    {
        private static string validTenantFile = "~/IssuerConfig.json";
        private static string _tenantTemplate2 = "https://login.microsoftonline.com/{0}/v2.0";
        private static string _tenantTemplate = "https://sts.windows.net/{0}/";
        private static List<Tenant> _validTenants = null;
        public static List<Tenant> ValidTenants
        {
            get
            {
                if (_validTenants == null)
                {
                    var node = "ValidIssuers";
                    var filePath = System.Web.HttpContext.Current.Server.MapPath(validTenantFile);
                    using (var stream = new StreamReader(filePath))
                    {
                        var tenantString = stream.ReadToEnd();

                        var jsonObj = JObject.Parse(tenantString);
                        _validTenants = jsonObj[node].ToObject<List<Tenant>>();
                    }
                }
                return _validTenants;
            }
        }
        private  static List<String> _validTenantIds = null;
        public static List<String> ValidTenantIds 
        {
            get
            { 
                if(_validTenantIds == null)
                {
                    _validTenantIds = ValidTenants.Select(t => string.Format(_tenantTemplate2, t.Id)).ToList();
                }
                return _validTenantIds;
            }
        }
    }
}