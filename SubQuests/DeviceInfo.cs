using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;

namespace SubQuests.UWP
{
    class DeviceInfo
    {
        EasClientDeviceInformation eas = new EasClientDeviceInformation();

        public string devmanufacturer
        {
            get { return eas.SystemManufacturer; }
            set { }
        }

        public string devmodel
        {
            get { return eas.SystemProductName; }
            set { }
        }

        public string devname
        {
            get { return eas.FriendlyName; }
            set { }
        }

        AnalyticsVersionInfo ai = AnalyticsInfo.VersionInfo;

        public string sysfam
        {
            get { return ai.DeviceFamily; }
            set { }
        }

        public string sysfamdev
        {
            get { return ai.DeviceFamilyVersion; }
            set { }
        }

        public string GetSV()
        {
            ulong v = ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion.ToString());
            ulong v1 = (v & 0xFFFF000000000000L) >> 48;
            ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
            ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
            ulong v4 = (v & 0x000000000000FFFFL);
            string sv = $"{v1}.{v2}.{v3}.{v4}";
            return sv;
        }

        public string sysversion
        {
            get { return GetSV(); }
            set { }
        }

        public string GetSA()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            return package.Id.Architecture.ToString();
        }

        public string sysarch
        {
            get { return GetSA(); }
            set { }
        }

        public string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            // Set app version to textblock
            return "Version " + string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        public string appversion
        {
            get { return GetAppVersion(); }
            set { }
        }
    }
}
