using System;
using Goosetuv.Snow.NET.Methods;

/// <summary>
/// This is a test app and should not be used in production.
/// Seriously, the code doesn't even make sense, it's just for testing some methods 💀
/// I am sorry to literally anyone that tries to use this to understand Snow.NET
/// </summary>
namespace Goosetuv.Snow.NET.ConsoleApp
{
    class Program
    {
        private static int totalComputerCount, totalComputersPulled = 0;
        private static int totalApplicationCount, totalApplicationsPulled = 0;
        private static int totalAllowListCount, totalAllowListPulled = 0;
        private static int totalDenyListCount, totalDenyListPulled = 0;
        private static int totalOverlicenseCount, totalOverlicensePulled = 0;
        private static int totalUnderlicenseCount, totalUnderlicensePulled = 0;

        private static int AppCount = 0;
        private static int customerCID = 1;

        private static Authenticate auth;

        static void Main(string[] args)
        {
           try
           {
                // start stop watch to time out long it takes
                var watch = System.Diagnostics.Stopwatch.StartNew();

                auth = new Authenticate("http://snow.domain.scot/api/", "Administrator", "SnowNET2022!");

                PlatformData();

                Console.WriteLine("\n ----------");

                GetLicenses();

                Console.WriteLine("\n ----------");

                GetObjectTypes();

                Console.WriteLine("\n ----------");

                GetDataCenters();

                Console.WriteLine("\n ----------");

                GetApplicationsOverlicensed();

                Console.WriteLine("\n ----------");

                GetApplicationsUnderlicensed();

                Console.WriteLine("\n ----------");

                GetApplicationsAllowList();

                Console.WriteLine("\n ----------");

                GetApplicationsDenyList();

                Console.WriteLine("\n ----------");

                GetApplications();

                Console.WriteLine("\n ----------");

                GetUsers();

                Console.WriteLine("\n ----------");

                GetComputers();

                Console.WriteLine("\n ----------");

                GetAgreements();

                Console.WriteLine("\n ----------");

                GetAgreementTypes();

                Console.WriteLine("\n ----------");

                watch.Stop();
                Console.WriteLine($" \nTime to Complete: {watch.ElapsedMilliseconds / 1000}s");

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadLine();
            }

        }

        static void PlatformData()
        {
            var platformData = new PlatformData(auth.Client);

            var _systemInfo = platformData.SystemInformation();

            Console.WriteLine(
                $" Platform Information:\n" +
                $"   > App Version              : {_systemInfo.Body.AppVersion}\n" +
                $"   > Database Schema Version  : {_systemInfo.Body.DbSchemaVersion}\n" +
                $"   > Product                  : {_systemInfo.Body.Product}\n" +
                $"   > API Version              : {_systemInfo.Body.ApiVersion}\n"
            );
            
            var _systemUser = platformData.SystemUser();
            Console.WriteLine(
                $"  > Display Name              : {_systemUser.Body.FirstName} {_systemUser.Body.LastName}\n" +
                $"  > Current Time              : {DateTime.Now}\n" +
                $"  > Assigned Customer ID      : {_systemUser.Body.CustomerId}\n"
            );

            Console.WriteLine("\n Your account permissions are:");

            for (int i = 0; i < _systemUser.Body.GroupMemberships.Count; i++)
            {
                Console.WriteLine($"  > {_systemUser.Body.GroupMemberships[i].SystemGroupName}");
            }

            var _customers = platformData.Customers();

            Console.Write("\n You have access to these customers: \n");

            for (int i = 0; i < _customers.Body.Count; i++)
            {
                Console.WriteLine(string.Format("   CID: {0}, Name: {1}", _customers.Body[i].Body.Id, _customers.Body[i].Body.Name));
            }

            Console.WriteLine("\n Data Update Job Information: \n");

            var _duj = platformData.DataUpdateJob();

            Console.WriteLine(
                    $"   > Last Start Time         : {_duj.Body.LastStartTime}\n" +
                    $"   > Last Execution Time     : {_duj.Body.LastExecutionTime}\n" +
                    $"   > Is Running              : {_duj.Body.IsRunning}\n" +
                    $"   > State                   : {_duj.Body.State}"
            );
        }

        #region Computer
        static void GetComputers(int skipCount = 0)
        {
            ComputerData computerData = new ComputerData(auth.Client);
            var data = computerData.Computers(customerCID, skipCount); //"&$filter=Name eq 'CRY01SNW01'"

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalComputerCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                GetComputer(computerData, data.Body[i].Body.Id);
                GetComputerApplications(computerData, data.Body[i].Body.Id);
                GetComputerObjects(computerData, data.Body[i].Body.Id);
                GetComputerUsers(computerData, data.Body[i].Body.Id);
                GetComputerVirtualMachines(computerData, data.Body[i].Body.Id);
                Console.WriteLine(" -----------------");

                totalComputersPulled++;
            }

            Console.WriteLine($" Pulled {totalComputersPulled}/{totalComputerCount}");

            if (totalComputersPulled < totalComputerCount)
            {
                GetComputers(skipCount: totalComputersPulled);
            }

        }

        static void GetComputer(ComputerData cd, int computerID)
        {
            Console.WriteLine(" Computer Information:");

            var data = cd.Computer(customerCID, computerID);

            Console.WriteLine(
                $"   > Name           : {data.Body.Name}\n" +
                $"   > Client Version : {data.Body.ClientVersion}\n" +
                $"   > Model          : {data.Body.Model}\n" +
                $"   > Manufacturer   : {data.Body.Manufacturer}\n" +
                $"   > Last Scan      : {data.Body.LastScanDate}\n" +
                $"   > Status         : {data.Body.Status}");

            if(data.Body.CustomFields != null)
            {
                for (int i = 0; i < data.Body.CustomFields.Count; i++)
                {
                    Console.WriteLine(
                        $"   > Custom Field {i} : {data.Body.CustomFields[i].Name}\n" +
                        $"   > Custom Field Type: {data.Body.CustomFields[i].DataType}" +
                        $"      > Custom Field Data : {data.Body.CustomFields[i].Value}"
                    );
                }
            }
        }

        static void GetComputerObjects(ComputerData cd, int computerID, int skipCount = 0)
        {
            Console.WriteLine(" Objects:");

            int totalObjects = 0;
            int totalObjectsPulled = 0;

            var data = cd.ComputerObjects(customerCID, computerID, skipCount);

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalObjects = Convert.ToInt32(data.Meta[i].Value.ToString());
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.WriteLine(
                    $"   > Name           : {data.Body[i].Body.Name}\n" +
                    $"   > Type           : {data.Body[i].Body.TypeName}\n" +
                    $"   > Organization   : {data.Body[i].Body.Organization}");

                totalObjectsPulled++;
            }

            if (totalObjects == 0)
            {
                Console.WriteLine($"   No objects found for {computerID}");
            }

            if (totalObjectsPulled < totalObjects)
            {
                GetComputerObjects(cd, totalObjectsPulled);
            }

        }

        static void GetComputerApplications(ComputerData cd, int computerID, int skipCount = 0)
        {
            Console.WriteLine(" Applications:");

            int totalApplications = 0;
            int totalApplicationsPulled = 0;

            var data = cd.ComputerApplications(customerCID, computerID, skipCount);

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalApplications = Convert.ToInt32(data.Meta[i].Value.ToString());

                    AppCount = totalApplications + AppCount;
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.WriteLine($"   > Name           : {data.Body[i].Body.Name}");

                totalApplicationsPulled++;
            }

            if (totalApplicationsPulled < totalApplications)
            {
                GetComputerApplications(cd, computerID, totalApplicationsPulled);
            }
        }

        static void GetComputerUsers(ComputerData cd, int computerID, int skipCount = 0)
        {
            Console.WriteLine(" Computer Users:");

            int totalUsers = 0;
            int totalUsersPulled = 0;

            var data = cd.ComputerUsers(customerCID, computerID, skipCount);

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalUsers = Convert.ToInt32(data.Meta[i].Value.ToString());
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.WriteLine(
                    $"   > Username       : {data.Body[i].Body.Username}\n" +
                    $"   > Full Name      : {data.Body[i].Body.FullName}\n" +
                    $"   > Last Logon     : {data.Body[i].Body.LastLogon}\n" +
                    $"   > Logon Count    : {data.Body[i].Body.LogonCount}\n" +
                    $"   > Updated Date   : {data.Body[i].Body.UpdatedDate}");

                totalUsersPulled++;
            }

            if (totalUsers == 0)
            {
                Console.WriteLine($"   No users found for {computerID}");
            }

            if (totalUsersPulled < totalUsers)
            {
                GetComputerUsers(cd, computerID, totalUsers);
            }
        }

        static void GetComputerVirtualMachines(ComputerData cd, int computerID, int skipCount = 0)
        {
            Console.WriteLine(" Computer Virtual Machines:");

            int totalVMs = 0;
            int totalVMsPulled = 0;

            var data = cd.ComputerVirtualMachines(customerCID, computerID, skipCount);

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalVMs = Convert.ToInt32(data.Meta[i].Value.ToString());
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.WriteLine(
                    $"   > VMID       : {data.Body[i].Body.VirtualMachineId}\n" +
                    $"   > Name       : {data.Body[i].Body.Name}\n" +
                    $"   > OpSystem   : {data.Body[i].Body.OperatingSystem}\n" +
                    $"   > PowerState : {data.Body[i].Body.PowerState}\n");

                totalVMsPulled++;
            }

            if (totalVMs == 0)
            {
                Console.WriteLine($"   No virtual machines found for {computerID}");
            }

            if (totalVMsPulled < totalVMs)
            {
                GetComputerVirtualMachines(cd, computerID, totalVMs);
            }
        }
        #endregion

        #region Agreements
        static void GetAgreements(int skipCount = 0)
        {
            var agreementData = new AgreementData(auth.Client);
            var agreements = agreementData.Agreements(customerCID, skipCount, "&$filter=MasterId eq null");

            int totalAgreementCount = 0;
            int totalAgreementsPulled = 0;

            for (int i = 0; i < agreements.Meta.Count; i++)
            {
                if (agreements.Meta[i].Name == "Count")
                {
                    totalAgreementCount = Convert.ToInt32(agreements.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < agreements.Body.Count; i++)
            {
                GetAgreement(agreementData, agreements.Body[i].Body.Id);
                GetSubAgreements(agreementData, agreements.Body[i].Body.Id);

                totalAgreementsPulled++;
            }

            Console.WriteLine($" Pulled {totalAgreementsPulled}/{totalAgreementCount}");

            if (totalAgreementsPulled < totalAgreementCount)
            {
                GetAgreements(skipCount: totalAgreementsPulled);
            }
        }

        static void GetSubAgreements(AgreementData ad, int agreementID, int skipCount = 0)
        {
            var subAgreements = ad.SubAgreements(customerCID, agreementID, skipCount);

            int totalSubAgreementsCount = 0;
            int totalSubAgreementsPulled = 0;

            for (int i = 0; i < subAgreements.Meta.Count; i++)
            {
                if (subAgreements.Meta[i].Name == "Count")
                {
                    totalSubAgreementsCount = Convert.ToInt32(subAgreements.Meta[i].Value.ToString());
                }
            }

            for (int i = 0; i < subAgreements.Body.Count; i++)
            {
                Console.WriteLine(" Sub Agreement Found..");
                GetAgreement(ad, subAgreements.Body[i].Body.Id);

                totalSubAgreementsPulled++;
            }

            if(totalSubAgreementsCount == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            } else
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.WriteLine($" Sub Agreements Pulled {totalSubAgreementsPulled}/{totalSubAgreementsCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalSubAgreementsPulled < totalSubAgreementsCount)
            {
                GetSubAgreements(ad, agreementID, totalSubAgreementsPulled);
            }

        }

        static void GetAgreement(AgreementData ad, int agreementID)
        {
            Console.WriteLine(" Agreement Information:");

            var data = ad.Agreement(customerCID, agreementID);

            Console.WriteLine(
                $"   > Agreement ID   : {data.Body.Id}\n" +
                $"   > AgreementNumber: {data.Body.AgreementNumber}\n" +
                $"   > AgreementType  : {data.Body.AgreementType}\n" +
                $"   > Name           : {data.Body.Name}\n" +
                $"   > Master ID      : {data.Body.MasterId}\n" +
                $"   > Organization   : {data.Body.Organization}");

            for (int i = 0; i < data.Body.AgreementPeriods.Count; i++)
            {
                Console.WriteLine(
                    $"   > Period {i}       : {data.Body.AgreementPeriods[i].ValidFrom} - {data.Body.AgreementPeriods[i].ValidTo}");
            }

            if (data.Body.RestrictedToRoles != null)
            {
                for (int i = 0; i < data.Body.RestrictedToRoles.Count; i++)
                {
                    Console.WriteLine(
                        $"   > Restrictions {i} : {data.Body.RestrictedToRoles[i].Name}");
                }
            }

            if (data.Body.CustomFields != null)
            {
                for (int i = 0; i < data.Body.CustomFields.Count; i++)
                {
                    Console.WriteLine(
                        $"   > Custom Field {i} : {data.Body.CustomFields[i].Name}\n" +
                        $"      > Custom Field Data : {data.Body.CustomFields[i].Value}"
                    );
                }
            }

            GetAgreementObjects(ad, data.Body.Id, 0);

            if (data.Body.AgreementType == "Support" || data.Body.AgreementType == "Oracle" || data.Body.AgreementType == "AgreementTypeTest")
            {

            } else
            {
                GetAgreementLicenses(ad, data.Body.Id, 0);

            }

            GetAgreementComputers(ad, data.Body.Id, 0);
        }

        static void GetAgreementTypes(int skipCount = 0)
        {
            var agreementData = new AgreementData(auth.Client);
            var agreementTypes = agreementData.AgreementTypes(customerCID, skipCount);

            int totalAgreementTypeCount = 0;
            int totalAgreementTypePulled = 0;

            for (int i = 0; i < agreementTypes.Meta.Count; i++)
            {
                if(agreementTypes.Meta[i].Name == "Count")
                {
                    totalAgreementTypeCount = Convert.ToInt32(agreementTypes.Meta[i].Value.ToString());
                }
            }

            for (int i = 0; i < agreementTypes.Body.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  Agreement Type Found...");
                Console.ForegroundColor = ConsoleColor.White;

                GetAgreementType(agreementData, agreementTypes.Body[i].Body.Id);

                totalAgreementTypePulled++;
            }

            Console.WriteLine($" Agreement Type Pulled {totalAgreementTypePulled}/{totalAgreementTypeCount}");

            if(totalAgreementTypePulled < totalAgreementTypeCount)
            {
                GetAgreementTypes(totalAgreementTypePulled);
            }
        }

        static void GetAgreementType(AgreementData ad, int agreementTypeID)
        {
            var agreementType = ad.AgreementType(customerCID, agreementTypeID);

            Console.WriteLine(
                $"   > Type ID                : {agreementType.Body.Id}\n" +
                $"   > Type Name              : {agreementType.Body.Name}\n" +
                $"   > Type Description       : {agreementType.Body.Description}\n" +
                $"   > Type ComputersActive   : {agreementType.Body.ComputersActive}\n" +
                $"   > Type ObjectsActive     : {agreementType.Body.ObjectsActive}\n"
            );
        }

        static void GetAgreementComputers(AgreementData ad, int agreementID, int skipCount)
        {
            var data = ad.AgreementComputers(customerCID, agreementID, skipCount);
            var computerData = new ComputerData(auth.Client);
            int totalComputerACount = 0;
            int totalComputersAPulled = 0;

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalComputerACount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                GetComputer(computerData, data.Body[i].Body.Id);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" -----------------");

                totalComputersAPulled++;
            }

            if (totalComputersAPulled < totalComputerACount)
            {
                GetAgreementComputers(ad, agreementID, totalComputersAPulled);
            }
        }

        static void GetAgreementObjects(AgreementData ad, int agreementID, int skipCount)
        {
            Console.WriteLine(" Agreement Objects Information:");

            var data = ad.AgreementObjects(customerCID, agreementID, skipCount);

            int totalAgreementObjectCount = 0;
            int totalAgreementObjectPulled = 0;

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalAgreementObjectCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"    > {data.Body[i].Body.Name}");

                totalAgreementObjectPulled++;
            }

            if (totalAgreementObjectPulled < totalAgreementObjectCount)
            {
                GetAgreementObjects(ad, agreementID, totalAgreementObjectPulled);
            }
        }

        static void GetAgreementLicenses(AgreementData ad, int agreementID, int skipCount)
        {
            Console.WriteLine(" Agreement Licenses Information:");

            var data = ad.AgreementLicenses(customerCID, agreementID, skipCount);

            int totalAgreementLicensesCount = 0;
            int totalAgreementLicensesPulled = 0;

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalAgreementLicensesCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"    > {data.Body[i].Body.ApplicationName}");
                Console.ForegroundColor = ConsoleColor.White;

                totalAgreementLicensesPulled++;
            }

            if (totalAgreementLicensesPulled < totalAgreementLicensesCount)
            {
                GetAgreementLicenses(ad, agreementID, totalAgreementLicensesPulled);
            }
        }
        #endregion

        #region Users
        static void GetUsers(int skipCount = 0)
        {
            var userData = new UserData(auth.Client);
            var data = userData.Users(customerCID, skipCount); //"&$filter=Name eq 'CRY01SNW01'"
            int totalUserCount = 0;
            int totalUserPulled = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalUserCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                GetUser(userData, data.Body[i].Body.Id);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"      >>>>>>>>>>>>>>>>>");

                Console.ForegroundColor = ConsoleColor.Green;
                GetUserApplications(userData, data.Body[i].Body.Id);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"      >>>>>>>>>>>>>>>>>");

                Console.ForegroundColor = ConsoleColor.Green;
                GetUserObjects(userData, data.Body[i].Body.Id);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"      >>>>>>>>>>>>>>>>>");
                
                Console.ForegroundColor = ConsoleColor.Green;
                GetUserComputers(userData, data.Body[i].Body.Id);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" -----------------");

                totalUserPulled++;
            }

            Console.WriteLine($" Pulled {totalUserPulled}/{totalUserCount}");

            if (totalUserPulled < totalUserCount)
            {
                GetUsers(skipCount: totalUserPulled);
            }
        }

        static void GetUser(UserData ud, int UserID)
        {
            var data = ud.User(customerCID, UserID);

            Console.WriteLine(
                    $"  ID                  : {data.Body.Id}\n" +
                    $"  CID                 : {data.Body.CustomerId}\n" +
                    $"  Username            : {data.Body.Username}\n" +
                    $"  FullName            : {data.Body.FullName}\n" +
                    $"  LastLogon           : {data.Body.LastLogon}\n" +
                    $"  Status Code         : {data.Body.StatusCode}\n" +
                    $"  Updated             : {data.Body.UpdatedDate}\n" +
                    $"  UpdatedBy           : {data.Body.UpdatedBy}\n" +
                    $"  Email               : {data.Body.Email}\n" +
                    $"  Phone               : {data.Body.PhoneNumber}\n" +
                    $"  Org                 : {data.Body.Organization}\n" +
                    $"  OrgChecksum         : {data.Body.OrgChecksum}\n" +
                    $"      ---------- \n" +
                    $"  LastUsedComputerID  : {data.Body.LastUsedComputerId}\n" +
                    $"  LastUsedComputerName: {data.Body.LastUsedComputerName}"
             );
        }

        static void GetUserApplications(UserData ud, int UserID, int skipCount = 0)
        {
            var data = ud.UserApplications(customerCID, UserID, skipCount);
            int counter = 0;
            int pulledCounter = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    counter = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.WriteLine(
                    $"  Application Name            : {data.Body[i].Body.Name}\n" +
                    $"  Application Manufacturer    : {data.Body[i].Body.ManufacturerName}\n" +
                    $"  ----------"
                );

                pulledCounter++;
            }

            if(pulledCounter < counter)
            {
                GetUserApplications(ud, UserID, pulledCounter);
            }
        }

        static void GetUserObjects(UserData ud, int UserID, int skipCount = 0)
        {
            var data = ud.UserObjects(customerCID, UserID, skipCount);
            int counter = 0;
            int pulledCounter = 0;

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    counter = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                Console.WriteLine(
                    $"  Object Name            : {data.Body[i].Body.Name}\n" +
                    $"  Object Type            : {data.Body[i].Body.TypeName}\n" +
                    $"  Object Organization    : {data.Body[i].Body.Organization}\n" +
                    $"  ----------"
                );

                pulledCounter++;
            }

            if (pulledCounter < counter)
            {
                GetUserObjects(ud, UserID, pulledCounter);
            }
        }

        static void GetUserComputers(UserData ud, int UserID, int skipCount = 0)
        {
            var data = ud.UserComputers(customerCID, UserID, skipCount);
            int counter = 0;
            int pulledCounter = 0;

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    counter = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                Console.WriteLine(
                    $"  Computer Name            : {data.Body[i].Body.Name}\n" +
                    $"  Computer Manufacturer    : {data.Body[i].Body.Manufacturer}\n" +
                    $"  Computer Model           : {data.Body[i].Body.Model}\n" +
                    $"  Computer Organization    : {data.Body[i].Body.Organization}\n" +
                    $"  ----------"
                );

                pulledCounter++;
            }

            if (pulledCounter < counter)
            {
                GetUserObjects(ud, UserID, pulledCounter);
            }
        }

        #endregion

        #region Applications
        static void GetApplications(int skipCount = 0)
        {
            var appData = new ApplicationData(auth.Client);
            var data = appData.Applications(customerCID, skipCount); //"&$filter=Id eq guid'8aa9240d-8c2f-4d1f-816e-48f574890904'"

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalApplicationCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                GetApplication(appData, data.Body[i].Body.Id);
                GetApplicationCompliance(appData, data.Body[i].Body.Id);
                GetApplicationUsers(appData, data.Body[i].Body.Id);
                GetApplicationComputers(appData, data.Body[i].Body.Id);
                GetApplicationLicenses(appData, data.Body[i].Body.Id);

                totalApplicationsPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalApplicationsPulled}/{totalApplicationCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalApplicationsPulled < totalApplicationCount)
            {
                GetApplications(skipCount: totalApplicationsPulled);
            }
        }

        static void GetApplicationsAllowList(int skipCount = 0)
        {
            var appData = new ApplicationData(auth.Client);
            var data = appData.ApplicationsAllowList(customerCID, skipCount); //"&$filter=Id eq guid'8aa9240d-8c2f-4d1f-816e-48f574890904'"

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalAllowListCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                Console.WriteLine(data.Body[i].Body.Id);

                //GetApplication(appData, data.Body[i].Body.Id);

                totalAllowListPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalAllowListPulled}/{totalAllowListCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalAllowListPulled < totalAllowListCount)
            {
                GetApplicationsAllowList(skipCount: totalAllowListPulled);
            }
        }

        static void GetApplicationsDenyList(int skipCount = 0)
        {
            var appData = new ApplicationData(auth.Client);
            var data = appData.ApplicationsDenyList(customerCID, skipCount); //"&$filter=Id eq guid'8aa9240d-8c2f-4d1f-816e-48f574890904'"

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalDenyListCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                Console.WriteLine(data.Body[i].Body.Id);

                totalDenyListPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalDenyListPulled}/{totalDenyListCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalDenyListPulled < totalDenyListCount)
            {
                GetApplicationsDenyList(skipCount: totalDenyListPulled);
            }
        }

        static void GetApplicationsOverlicensed(int skipCount = 0)
        {
            var appData = new ApplicationData(auth.Client);
            var data = appData.ApplicationsOverlicensed(customerCID, skipCount); //"&$filter=Id eq guid'8aa9240d-8c2f-4d1f-816e-48f574890904'"

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalOverlicenseCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                Console.WriteLine(
                    $" ID               : {data.Body[i].Body.Id}\n" +
                    $" Name             : {data.Body[i].Body.Name}\n" +
                    $" Manufacturer     : {data.Body[i].Body.ManufacturerName}\n" +
                    $" LicRequirement   : {data.Body[i].Body.LicenseRequirement}\n" +
                    $" LicenseCount     : {data.Body[i].Body.LicenseCount}\n" +
                    $" TLicenseCount    : {data.Body[i].Body.TransferredLicenseCount}\n" +
                    $" Compliance       : {data.Body[i].Body.Compliance}\n"
                );

                totalOverlicensePulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalOverlicensePulled}/{totalOverlicenseCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalOverlicensePulled < totalOverlicenseCount)
            {
                GetApplicationsOverlicensed(skipCount: totalOverlicensePulled);
            }
        }

        static void GetApplicationsUnderlicensed(int skipCount = 0)
        {
            var appData = new ApplicationData(auth.Client);
            var data = appData.ApplicationsUnderlicensed(customerCID, skipCount); //"&$filter=Id eq guid'8aa9240d-8c2f-4d1f-816e-48f574890904'"

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalUnderlicenseCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                Console.WriteLine(
                    $" ID               : {data.Body[i].Body.Id}\n" +
                    $" Name             : {data.Body[i].Body.Name}\n" +
                    $" Manufacturer     : {data.Body[i].Body.ManufacturerName}\n" +
                    $" LicRequirement   : {data.Body[i].Body.LicenseRequirement}\n" +
                    $" LicenseCount     : {data.Body[i].Body.LicenseCount}\n" +
                    $" TLicenseCount    : {data.Body[i].Body.TransferredLicenseCount}\n" +
                    $" Compliance       : {data.Body[i].Body.Compliance}\n"
                );

                totalUnderlicensePulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalUnderlicensePulled}/{totalUnderlicenseCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalUnderlicensePulled < totalUnderlicenseCount)
            {
                GetApplicationsUnderlicensed(skipCount: totalUnderlicensePulled);
            }
        }

        static void GetApplicationUsers(ApplicationData ad, Guid applicationID, int skipCount = 0)
        {
            var data = ad.ApplicationUsers(customerCID, applicationID, skipCount); //"&$filter=Id eq guid'8aa9240d-8c2f-4d1f-816e-48f574890904'"

            int totalCount = 0;
            int totalPulled = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(
                    $" ID               : {data.Body[i].Body.Id}\n" +
                    $" Name             : {data.Body[i].Body.FullName}\n" +
                    $" Last Logon       : {data.Body[i].Body.LastLogon}\n" +
                    $" Organization     : {data.Body[i].Body.Organization}\n" +
                    $" Email            : {data.Body[i].Body.Email}\n"
                );
                Console.ForegroundColor = ConsoleColor.White;

                totalPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalPulled < totalCount)
            {
                GetApplicationUsers(ad, applicationID, totalPulled);
            }
        }

        static void GetApplicationLicenses(ApplicationData ad, Guid applicationID, int skipCount = 0)
        {
            var data = ad.ApplicationLicense(customerCID, applicationID, skipCount); //"&$filter=Id eq guid'8aa9240d-8c2f-4d1f-816e-48f574890904'"

            int totalCount = 0;
            int totalPulled = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(
                    $" ID                   : {data.Body[i].Body.Id}\n" +
                    $" Name                 : {data.Body[i].Body.ApplicationName}\n" +
                    $" Manufacturer         : {data.Body[i].Body.ManufacturerName}\n" +
                    $" Metric               : {data.Body[i].Body.Metric}\n" +
                    $" Assignment           : {data.Body[i].Body.AssignmentType}\n"
                );
                Console.ForegroundColor = ConsoleColor.White;

                totalPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalPulled < totalCount)
            {
                GetApplicationLicenses(ad, applicationID, totalPulled);
            }
        }

        static void GetApplicationComputers(ApplicationData ad, Guid applicationID, int skipCount = 0)
        {
            var data = ad.ApplicationComputers(customerCID, applicationID, skipCount); //"&$filter=Id eq guid'8aa9240d-8c2f-4d1f-816e-48f574890904'"

            int totalCount = 0;
            int totalPulled = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(
                    $" ID                   : {data.Body[i].Body.Id}\n" +
                    $" Name                 : {data.Body[i].Body.Name}\n" +
                    $" Organization         : {data.Body[i].Body.Organization}\n" +
                    $" Virtual              : {data.Body[i].Body.IsVirtual}\n" +
                    $" OEM                  : {data.Body[i].Body.IsOEM}\n"
                );
                Console.ForegroundColor = ConsoleColor.DarkYellow;

                totalPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalPulled < totalCount)
            {
                GetApplicationComputers(ad, applicationID, totalPulled);
            }
        }

        static void GetApplication(ApplicationData ad, Guid applicationID)
        {
            var intData = ad.Application(customerCID, applicationID);

            Console.WriteLine(
                    $"  ID                  : {intData.Body.Id}\n" +
                    $"  App. Name           : {intData.Body.Name}\n" +
                    $"  Manufactuer Id      : {intData.Body.ManufacturerId}\n" +
                    $"  Manufactuer Name    : {intData.Body.ManufacturerName}\n" +
                    $"  Language Name       : {intData.Body.LanguageName}\n" +
                    $"  Created Date        : {intData.Body.CreatedDate}\n" +
                    $"  Created By          : {intData.Body.CreatedBy}\n" +
                    $"  OperatingSystemType : {intData.Body.OperatingSystemType}\n" +
                    $"  OperatingSystem?    : {intData.Body.IsOperatingSystem}\n" +
                    $"  Metric              : {intData.Body.Metric}\n" +
                    $"  UserLicenseCount    : {intData.Body.UserLicenseCost}\n" +
                    $"  ------------"
             );

        }

        static void GetApplicationCompliance(ApplicationData ad, Guid applicationID, int skipCount = 0)
        {
            var data = ad.ApplicationCompliance(customerCID, applicationID, skipCount);
            int totalCompliance = 0;
            int pulledCompliance = 0;

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCompliance = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                Console.WriteLine(
                        $"      > ID                  : {data.Body[i].Body.MetricName}\n" +
                        $"      > App. Name           : {data.Body[i].Body.InitialRequirement}\n" +
                        $"      > Manufactuer Id      : {data.Body[i].Body.AvailableLicenses}\n" +
                        $"      > Manufactuer Name    : {data.Body[i].Body.LicenseRequirement}\n" +
                        $"      > Language Name       : {data.Body[i].Body.LicensesPurchased}\n"
                 );

                if(data.Body[i].Body.LicenseDiscrepancies != null)
                {
                    foreach (var it in data.Body[i].Body.LicenseDiscrepancies)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine(
                            $"      > License Descrep : {it.Name} - {it.Value}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                } else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(
                        $"      > No License Descrepancies.");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                Console.Write($"      ------------");

                pulledCompliance++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n      Pulled {pulledCompliance}/{totalCompliance}");
            Console.ForegroundColor = ConsoleColor.White;

            if (pulledCompliance < totalCompliance)
            {
                GetApplicationCompliance(ad, applicationID, pulledCompliance);
            }
        }
        #endregion

        #region Data Center Clusters
        static void GetDataCenters(int skipCount = 0)
        {
            var dccData = new DataCenterClusterData(auth.Client);
            var data = dccData.DataCenterClusters(customerCID, skipCount); //"&$filter=Id eq guid'8aa9240d-8c2f-4d1f-816e-48f574890904'"

            int totalPulled = 0;
            int totalCount = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                Console.WriteLine(
                    $"  Name         : {data.Body[i].Body.Name}\n" +
                    $"  Organization : {data.Body[i].Body.Organization}\n" +
                    $"  Server       : {data.Body[i].Body.ServerCount}\n" +
                    $"  VM Count     : {data.Body[i].Body.VirtualMachineCount}\n" +
                    $"  IVM Count    : {data.Body[i].Body.InventoriedVirtualMachineCount}\n"
                );

                GetDataCenter(dccData, data.Body[i].Body.Id);
                GetDCCLicenses(dccData, data.Body[i].Body.Id);
                GetDCCHosts(dccData, data.Body[i].Body.Id);

                totalPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalPulled < totalCount)
            {
                GetDataCenters(skipCount: totalPulled);
            }
        }

        static void GetDCCLicenses(DataCenterClusterData dcc, int dccID, int skipCount = 0)
        {
            var data = dcc.DataCenterClusterLicenses(customerCID, dccID, skipCount); //"&$filter=ApplicationName eq 'Windows Server 2016 Datacenter'"

            int totalCount = 0;
            int totalPulled = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(
                    $"      ID                   : {data.Body[i].Body.Id}\n" +
                    $"      Name                 : {data.Body[i].Body.ApplicationName}\n" +
                    $"      Manufacturer         : {data.Body[i].Body.ManufacturerName}\n" +
                    $"      Metric               : {data.Body[i].Body.Metric}\n" +
                    $"      Assignment           : {data.Body[i].Body.AssignmentType}\n"
                );
                Console.ForegroundColor = ConsoleColor.White;

                totalPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalPulled < totalCount)
            {
                GetDCCLicenses(dcc, dccID, totalPulled);
            }
        }

        static void GetDCCHosts(DataCenterClusterData dcc, int dccID, int skipCount = 0)
        {
            var data = dcc.DataCenterClusterHosts(customerCID, dccID, skipCount); //"&$filter=ApplicationName eq 'Windows Server 2016 Datacenter'"

            int totalCount = 0;
            int totalPulled = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(
                    $"      ID                   : {data.Body[i].Body.ComputerId}\n" +
                    $"      Name                 : {data.Body[i].Body.ComputerName}\n" +
                    $"      LastScanDate         : {data.Body[i].Body.LastScanDate}\n" +
                    $"      OperatingSystem      : {data.Body[i].Body.OperatingSystem}\n" +
                    $"      Processor            : {data.Body[i].Body.ProcessorType}\n"
                );
                Console.ForegroundColor = ConsoleColor.White;

                totalPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalPulled < totalCount)
            {
                GetDCCHosts(dcc, dccID, totalPulled);
            }
        }

        static void GetDataCenter(DataCenterClusterData dcc, int dccID)
        {
            var intData = dcc.DataCenterCluster(customerCID, dccID);

            Console.WriteLine(
                    $"  DCC ID              : {intData.Body.Id}\n" +
                    $"  DCC Name            : {intData.Body.Name}\n" +
                    $"  DCC Description     : {intData.Body.Description}\n" +
                    $"  DCC Organisation    : {intData.Body.Organization}\n" +
                    $"  DCC Server Count    : {intData.Body.ServerCount}\n" +
                    $"  Lic. App. Count     : {intData.Body.LicensedApplicationCount}\n" +
                    $"  VM Count            : {intData.Body.VirtualMachineCount}\n" +
                    $"  Inv. VM Count       : {intData.Body.InventoriedVirtualMachineCount}\n" +
                    $"  Updated By          : {intData.Body.UpdatedBy}\n" +
                    $"  Updated Date        : {intData.Body.UpdatedDate}\n" +
                    $"  ------------"
             );
        }
        #endregion

        #region Objects

        static void GetObjectTypes(int skipCount = 0)
        {
            var odData = new ObjectData(auth.Client);
            var data = odData.ObjectTypes(customerCID, skipCount); //"&$filter=Name eq 'TestObjectType'"

            int totalPulled = 0;
            int totalCount = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                GetObjectType(odData, data.Body[i].Body.Id);
                GetObjectTypeObjects(odData, data.Body[i].Body.Id);

                totalPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalPulled < totalCount)
            {
                GetObjectTypes(skipCount: totalPulled);
            }
        }

        static void GetObjectType(ObjectData od, int objectTypeID)
        {
            var intData = od.ObjectType(customerCID, objectTypeID);

            Console.WriteLine(
                    $"  ID              : {intData.Body.Id}\n" +
                    $"  Name            : {intData.Body.Name}\n" +
                    $"  Description     : {intData.Body.Description}\n" +
                    $"  ------------"
             );
        }

        static void GetObjectTypeObjects(ObjectData od, int objectTypeID, int skipCount = 0)
        {
            var data = od.ObjectTypeObjects(customerCID, objectTypeID, skipCount); //"&$filter=Name eq 'TestObjectType'"

            int totalPulled = 0;
            int totalCount = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                GetObject(od, data.Body[i].Body.Id);

                totalPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalPulled < totalCount)
            {
                GetObjectTypeObjects(od, objectTypeID, totalPulled);
            }
        }

        static void GetObject(ObjectData od, int objectID)
        {
            var data = od.Object(customerCID, objectID);

            Console.WriteLine(
                $"  ID              : {data.Body.Id}\n" +
                $"  Name            : {data.Body.Name}\n" +
                $"  TypeName        : {data.Body.TypeName}\n" +
                $"  Organisation    : {data.Body.Organization}\n" +
                $"  CreatedDate     : {data.Body.CreatedDate}\n" +
                $"  CreatedBy       : {data.Body.CreatedBy}");
        }

        #endregion

        #region Licenses
        static void GetLicenses(int skipCount = 0)
        {
            var ldData = new LicenseData(auth.Client);
            var data = ldData.Licenses(customerCID, skipCount); //"&$filter=Name eq 'Windows 10 Professional'"

            int totalPulled = 0;
            int totalCount = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                GetLicense(ldData, data.Body[i].Body.Id);
                Console.WriteLine($"        BaseLicenses");
                GetLicenseBaseLicenses(ldData, data.Body[i].Body.Id);
                Console.WriteLine($"        UpgradingLicenses");
                GetLicenseUpgradingLicense(ldData, data.Body[i].Body.Id);
                Console.WriteLine($"        VirtualMachines");
                //GetLicenseVirtualMachines(ldData, data.Body[i].Body.Id);

                totalPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalPulled < totalCount)
            {
                GetLicenses(skipCount: totalPulled);
            }
        }

        static void GetLicense(LicenseData ld, int licenseID)
        {
            var intData = ld.License(customerCID, licenseID);

            Console.WriteLine(
                    $"  ID              : {intData.Body.Id}\n" +
                    $"  Name            : {intData.Body.ApplicationName}\n" +
                    $"  Metric          : {intData.Body.Metric}\n" +
                    $"  ------------"
             );
        }

        static void GetLicenseBaseLicenses(LicenseData ld, int licenseID, int skipCount = 0)
        {

            var ldData = new LicenseData(auth.Client);
            var data = ldData.LicenseBaseLicenses(customerCID, licenseID, skipCount); //"&$filter=Name eq 'Windows 10 Professional'"

            int totalPulled = 0;
            int totalCount = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                Console.WriteLine(
                    $"  Parent ID   : {licenseID}\n" +
                    $"  ID          : {data.Body[i].Body.Id}"
                );

                totalPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalPulled < totalCount)
            {
                GetLicenseBaseLicenses(ldData, licenseID, totalPulled);
            }
        }

        static void GetLicenseUpgradingLicense(LicenseData ld, int licenseID, int skipCount = 0)
        {

            var ldData = new LicenseData(auth.Client);
            var data = ldData.LicenseUpgradingLicenses(customerCID, licenseID, skipCount); //"&$filter=Name eq 'Windows 10 Professional'"

            int totalPulled = 0;
            int totalCount = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count")
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            for (int i = 0; i < data.Body.Count; i++)
            {

                Console.WriteLine(
                    $"  Child ID   : {licenseID}\n" +
                    $"  ID          : {data.Body[i].Body.Id}"
                );

                totalPulled++;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
            Console.ForegroundColor = ConsoleColor.White;

            if (totalPulled < totalCount)
            {
                GetLicenseUpgradingLicense(ldData, licenseID, totalPulled);
            }
        }

        static void GetLicenseVirtualMachines(LicenseData ld, int licenseID, int skipCount = 0)
        {

            var ldData = new LicenseData(auth.Client);
            var data = ldData.LicenseVirtualMachines(customerCID, licenseID, skipCount); //"&$filter=Name eq 'Windows 10 Professional'"

            int totalPulled = 0;
            int totalCount = 0;

            // loop through all of the meta tags until we find count
            // there is almost definitely a better way to do this

            for (int i = 0; i < data.Meta.Count; i++)
            {
                if (data.Meta[i].Name == "Count" && data.Meta[i] != null)
                {
                    totalCount = Convert.ToInt32(data.Meta[i].Value.ToString()); // lol.
                }
            }

            if(data.Body.Count > 0)
            {
                for (int i = 0; i < data.Body.Count; i++)
                {

                    Console.WriteLine(
                        $"  License ID   : {licenseID}\n" +
                        $"  VM ID        : {data.Body[i].Body.VirtualMachineId}\n" +
                        $"  CM ID        : {data.Body[i].Body.ComputerId}"
                    );

                    totalPulled++;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" Pulled {totalPulled}/{totalCount}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            if (totalPulled < totalCount)
            {
                GetLicenseVirtualMachines(ldData, licenseID, totalPulled);
            }
        }

        #endregion
    }
}
