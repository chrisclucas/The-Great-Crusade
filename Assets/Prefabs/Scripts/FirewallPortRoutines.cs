using System;
using NetFwTypeLib;
using UnityEngine;


public class FirewallPortRoutines : MonoBehaviour
{
    private const string CLSIDFireWallManager = "{304CE942-6E39-40D8-943A-B913C40C9CD4}";
    private static NetFwTypeLib.INetFwMgr GetFirewallManager()
    {
        Debug.Log("Getting CLSID objectType");
        Type objectType = Type.GetTypeFromCLSID(new Guid(CLSIDFireWallManager));
        Debug.Log("Got CLSID objectType");
        INetFwMgr manager = Activator.CreateInstance(objectType) as NetFwTypeLib.INetFwMgr;
        if (manager == null)
        {
            throw new NotSupportedException("Could not load firewall manager");
        }

        return manager;
    }

    private static INetFwProfile GetCurrentProfile()
    {
        INetFwProfile profile;
        try
        {
            Debug.Log("Running GetFirewallManager()");
            profile = GetFirewallManager().LocalPolicy.CurrentProfile;
            Debug.Log("Completed Running GetFirewallManager()");
        }
        catch (System.Runtime.InteropServices.COMException e)
        {
            throw new NotSupportedException("Could not get the current profile (COMException)", e);
        }
        catch (System.Runtime.InteropServices.InvalidComObjectException e)
        {
            throw new NotSupportedException("Could not get the current profile (InvalidComObjectException)", e);
        }

        return profile;
    }

    public static bool IsWindowsFirewallOn
    {
        get
        {
            return GetCurrentProfile().FirewallEnabled;
        }

        set
        {
            GetCurrentProfile().FirewallEnabled = value;
        }
    }

    public static bool RemovePort(int portNumber, NET_FW_IP_PROTOCOL_ protocol)
    {
        if (IsPortEnabled(portNumber, protocol))
        {
            try
            {
                GetCurrentProfile().GloballyOpenPorts.Remove(portNumber, protocol);
            }
            catch (Exception)
            {
                return false;
            }
        }

        return true;
    }

    private const string ProgramIDOpenPort = "HNetCfg.FWOpenPort";
    public static bool AddPort(string title, int portNumber, NET_FW_SCOPE_ scope, NET_FW_IP_PROTOCOL_ protocol, NET_FW_IP_VERSION_ ipversion)
    {
        if (string.IsNullOrEmpty(title))
        {
            throw new ArgumentNullException("title");
        }

        if (!IsPortEnabled(portNumber, protocol))
        {
            // Get the type based on program ID
            Type type = Type.GetTypeFromProgID(ProgramIDOpenPort);
            INetFwOpenPort port = Activator.CreateInstance(type) as INetFwOpenPort;

            port.Name = title;
            port.Port = portNumber;
            port.Scope = scope;
            port.Protocol = protocol;
            port.IpVersion = ipversion;

            try
            {
                GetCurrentProfile().GloballyOpenPorts.Add(port);
            }
            catch (Exception)
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsPortEnabled(int portNumber, NET_FW_IP_PROTOCOL_ protocol)
    {
        // Retrieve the open ports collection
        INetFwOpenPorts openPorts = GetCurrentProfile().GloballyOpenPorts;
        if (openPorts == null)
        {
            return false;
        }

        // Get the open port
        try
        {
            INetFwOpenPort openPort = openPorts.Item(portNumber, protocol);
            if (openPort == null)
            {
                return false;
            }
        }
        catch (System.IO.FileNotFoundException)
        {
            return false;
        }

        return true;
    }

    private const string ProgramIDAuthorizedApplication = "HNetCfg.FwAuthorizedApplication";
    public static bool AddApplication(string title, string applicationPath, NET_FW_SCOPE_ scope, NET_FW_IP_VERSION_ ipversion)
    {
        if (String.IsNullOrEmpty(title))
        {
            throw new ArgumentNullException("title");
        }

        if (String.IsNullOrEmpty(applicationPath))
        {
            throw new ArgumentNullException("applicationPath");
        }

        if (!IsApplicationEnabled(applicationPath))
        {
            // Get the type based on program ID
            Type type = Type.GetTypeFromProgID(ProgramIDAuthorizedApplication);
            INetFwAuthorizedApplication auth = Activator.CreateInstance(type) as INetFwAuthorizedApplication;

            auth.Name = title;
            auth.ProcessImageFileName = applicationPath;
            auth.Scope = scope;
            auth.IpVersion = ipversion;
            auth.Enabled = true;

            try
            {
                GetCurrentProfile().AuthorizedApplications.Add(auth);
            }
            catch (Exception)
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsApplicationEnabled(string applicationPath)
    {
        if (String.IsNullOrEmpty(applicationPath))
        {
            throw new ArgumentNullException("applicationPath");
        }

        try
        {
            INetFwAuthorizedApplication application = GetCurrentProfile().AuthorizedApplications.Item(applicationPath);

            if (application == null)
            {
                return false;
            }
        }
        catch (System.IO.FileNotFoundException)
        {
            return false;
        }

        return true;
    }
}
