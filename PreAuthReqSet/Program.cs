using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace PreAuthReqSet
{
    class Program
    {
        static void Main(string[] args)
        {
            // Parse arguments
            var arguments = new Dictionary<string, string>();
            try
            {
                foreach (var argument in args)
                {
                    var idx = argument.IndexOf(':');

                    if (idx > 0)
                        arguments[argument.Substring(0, idx)] = argument.Substring(idx + 1);
                    else
                        arguments[argument] = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }

            if (!arguments.ContainsKey("/user") || String.IsNullOrEmpty(arguments["/user"]))
            {
                ShowUsage();
                Environment.Exit(1);
            }

            string userName = arguments["/user"];

            // Get user from AD
            DirectoryEntry userDirEntry = GetUserDirectoryEntry(userName);

            // Get UAC value, print current value of "Do not use Kerberos pre-authentication"
            int uacValue = (int)userDirEntry.Properties["userAccountControl"].Value;

            bool curState = ((uacValue & 4194304) != 0);
            Console.WriteLine("[*] Currently Set: " + curState);

            // Determine to set, unset, or finish op
            if (arguments.ContainsKey("/set") && !String.IsNullOrEmpty(arguments["/set"]))
            {
                bool set = bool.Parse(arguments["/set"]);

                if (set == curState)
                {
                    Console.WriteLine("[*] Nothing to do");
                }
                else
                {
                    SetUnsetPreauth(userDirEntry, set);
                }
            }
        }

        static DirectoryEntry GetUserDirectoryEntry(string userName)
        {
            // Get user principal
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, userName);
            if (userPrincipal != null)
            {
                try
                {
                    // Get user directory entry
                    DirectoryEntry userDirEntry = new DirectoryEntry("LDAP://" + userPrincipal.DistinguishedName);

                    return userDirEntry;
                }
                catch (System.DirectoryServices.DirectoryServicesCOMException E)
                {
                    Console.WriteLine("[!] Something went wrong getting the user directory entry");
                    Console.WriteLine(E.Message.ToString());

                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        static void SetUnsetPreauth(DirectoryEntry userDirEntry, bool set)
        {
            try
            {
                // Set or unset value of "Do not use Kerberos pre-authentication"
                int uacValue = (int)userDirEntry.Properties["userAccountControl"].Value;

                if (set)
                {
                    Console.WriteLine("[*] Setting...");
                    userDirEntry.Properties["userAccountControl"].Value = uacValue | 4194304;
                }
                else
                {
                    Console.WriteLine("[*] Unsetting...");
                    userDirEntry.Properties["userAccountControl"].Value = uacValue & ~4194304;
                }

                userDirEntry.CommitChanges();
                userDirEntry.Close();
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                Console.WriteLine("[!] Something went wrong setting the user directory entry");
                Console.WriteLine(E.Message.ToString());
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("PreAuthReqSet.exe /user:USER [/set:True|False]");
        }
    }
}
