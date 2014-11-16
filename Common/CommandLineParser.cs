using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkMultiPlayerCommon
{
    public class CommandLineParser
    {
        public static bool ParseIp(string aCommand, out string aAddress, out int aPort)
        {
            aAddress = null;
            aPort = 0;

            if (aCommand.Contains("dmp://"))
            {
                if (aCommand.Contains("[") && aCommand.Contains("]"))
                {
                    //IPv6 literal
                    aAddress = aCommand.Substring("dmp://[".Length);
                    aAddress = aAddress.Substring(0, aAddress.LastIndexOf("]"));
                    if (aCommand.Contains("]:"))
                    {
                        //With port
                        string portString = aCommand.Substring(aCommand.LastIndexOf("]:") + 1);
                        if (!Int32.TryParse(portString, out aPort))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //IPv4 literal or hostname
                    if (aCommand.Substring("dmp://".Length).Contains(":"))
                    {
                        //With port
                        aAddress = aCommand.Substring("dmp://".Length);
                        aAddress = aAddress.Substring(0, aAddress.LastIndexOf(":"));
                        string portString = aCommand.Substring(aCommand.LastIndexOf(":") + 1);
                        if (!Int32.TryParse(portString, out aPort))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //Without port
                        aAddress = aCommand.Substring("dmp://".Length);
                    }
                }
                return true;
            }

            return false;
        }
    }
}
