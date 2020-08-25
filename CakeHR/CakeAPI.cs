using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CakeHR
{
    class CakeAPI
    {
        private static String API_TOKEN = "c3ba623c626f8593fb3bc184057a5b7dd3e4918375cc94f869b4595213da0596f5949e4bc59218df";

        //private static List<KeyValuePair<int, String>> allEmployees = new List<KeyValuePair<int, string>>();

        private static Dictionary<int, String> allEmployees = new Dictionary<int, string>();
        private static Dictionary<int, String> employeeNames = new Dictionary<int, string>();
        private static Dictionary<int, String> leavePolicies = new Dictionary<int, string>();

        public static dynamic getEmployeesOutofOffice()
        {
            var client = new RestClient("https://srb.cake.hr/api/leave-management/out-of-office-today");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-Auth-Token", API_TOKEN);
            IRestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject(response.Content);
        }

       

        private static string runCmd(String command)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "createNotifications";
            string b = Properties.Settings.Default.userName;
            string c = Properties.Settings.Default.password;
            start.Arguments = string.Format("{0} {1}",b, c);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.CreateNoWindow = true;
            start.LoadUserProfile = true;
            Process process = Process.Start(start);
            System.IO.StreamReader reader = process.StandardOutput;
            string result = reader.ReadToEnd();
            return result;
        }

        public static List<List<String>> getEmployeesLeavingSoon() {
            List<List<String>> leavingEmployees = new List<List<String>>();

            DateTime dateNow = DateTime.Now;
            DateTime dateTomorrow = dateNow.AddDays(1);
            DateTime date5DaysAfter = dateNow.AddDays(5);
            String dateNowStr = dateNow.ToString("yyyy-MM-dd");
            String dateTomorrowStr = dateTomorrow.ToString("yyyy-MM-dd");
            String date5DaysAfterStr = date5DaysAfter.ToString("yyyy-MM-dd");

            var client = new RestClient("https://srb.cake.hr/api/leave-management/requests?from="+dateTomorrowStr+"&to="+date5DaysAfterStr);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-Auth-Token", API_TOKEN);
            IRestResponse response = client.Execute(request);
            dynamic deserializedResponse = JsonConvert.DeserializeObject(response.Content);

            for (int i = 0; i < deserializedResponse.data.Count; i++)
            {
                if (JsonConvert.SerializeObject(deserializedResponse.data[i].status) == "\"Approved\"") {
                    String employee_id = JsonConvert.SerializeObject(deserializedResponse.data[i].employee_id);
                    String leaveStartStr = JsonConvert.SerializeObject(deserializedResponse.data[i].start_date).Replace("\"","");
                    DateTime leaveStart = DateTime.ParseExact(leaveStartStr, "yyyy-MM-dd", null);
                    String daysToLeave = (leaveStart.Subtract(dateNow).Days).ToString();
                    Console.WriteLine(daysToLeave);
                    int leavePolicyId = int.Parse(JsonConvert.SerializeObject(deserializedResponse.data[i].policy_id).Replace("\"", ""));
                    String policyname = getLeavePolicy(leavePolicyId);

                    List<String> employee = new List<String>();
                    employee.Add(employee_id);
                    employee.Add(daysToLeave);
                    employee.Add(policyname);

                    leavingEmployees.Add(employee);
                }
            }

            return leavingEmployees;
        }

        public static List<List<string>> fetchNotifications()
        {
            List<List<String>> outputList = new List<List<String>>();
            string outputStr = runCmd("");
            if (outputStr.Contains("invalidsignin"))
            {
                return null;
            }
            dynamic outputJson = JsonConvert.DeserializeObject(outputStr);

            for (int i = 0; i < outputJson.data.Count; i++) {
                List<string> currentList = new List<string>();
                currentList.Add(JsonConvert.SerializeObject(outputJson.data[i].title).Replace("\"",""));
                currentList.Add(JsonConvert.SerializeObject(outputJson.data[i].body).Replace("\"", ""));
                outputList.Add(currentList);
            }
            

            return outputList; 

        }

        private static void initiateEmployees() {
                var client = new RestClient("https://srb.cake.hr/api/employees");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("X-Auth-Token", API_TOKEN);
                IRestResponse response = client.Execute(request);
                dynamic deserializedResponse = JsonConvert.DeserializeObject(response.Content);
                for (int i = 0; i < deserializedResponse.data.Count; i++)
                {
                    String imgUrl = JsonConvert.SerializeObject(deserializedResponse.data[i].picture_url);
                    imgUrl = imgUrl.Replace("\"", "");
                    allEmployees[int.Parse(JsonConvert.SerializeObject(deserializedResponse.data[i].id))] = imgUrl;
                    String employeeName = JsonConvert.SerializeObject(deserializedResponse.data[i].first_name);
                    employeeName = employeeName.Replace("\"", "");
                    employeeNames[int.Parse(JsonConvert.SerializeObject(deserializedResponse.data[i].id))] = employeeName;
            }
        }

        private static void initiatePolicies()
        {
            var client = new RestClient("https://srb.cake.hr/api/leave-management/policies");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-Auth-Token", API_TOKEN);
            IRestResponse response = client.Execute(request);
            dynamic deserializedResponse = JsonConvert.DeserializeObject(response.Content);
            for (int i = 0; i < deserializedResponse.data.Count; i++)
            {
                String leave = JsonConvert.SerializeObject(deserializedResponse.data[i].name);
                leave = leave.Replace("\"", "");
                leavePolicies[int.Parse(JsonConvert.SerializeObject(deserializedResponse.data[i].id))] = leave;
            }
        }

        public static String getLeavePolicy(int policyId)
        {
            if (leavePolicies.Count == 0)
            {
                initiatePolicies();
            }

            return leavePolicies[policyId];
        }

        public static String getEmployeeProfileUrl(int employeeId) {
            if (allEmployees.Count == 0)
            {
                initiateEmployees();
            }

            return allEmployees[employeeId];
        }

        public static String getEmployeeName(int employeeId)
        {
            if (employeeNames.Count == 0)
            {
                initiateEmployees();
            }

            return employeeNames[employeeId];
        }
    }
}
