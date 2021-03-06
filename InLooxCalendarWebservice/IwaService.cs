﻿using Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InLoox.ODataClient;
using InLoox.ODataClient.Data.BusinessObjects;
using InLoox.ODataClient.Extensions;

namespace InLooxCalendarWebservice
{
    public class IwaService
    {
        private const string ODATA_ROUTE = "odata/";
        Container _context;

        public bool IsConnected { get; private set; }

        public Contact CurrentUser { get; private set; }

        public IwaService()
        {
            IsConnected = false;
            CurrentUser = null;
        }

        public async Task<bool> Connect(Uri endPoint, string username, string password)
        {
            var tokenResponse = ODataBasics.GetToken(endPoint, username, password)
                .Result;

            var token = tokenResponse?.AccessToken;

            if (token == null)
            {
                // credentials invalid
                IsConnected = false;
                return IsConnected;
            }

            return await Connect(endPoint, token);
        }

        public async Task<bool> Connect(Uri endPoint, string token)
        {
            var endPointOdata = new Uri(endPoint, ODATA_ROUTE);

            try
            {
                // set context
                _context = ODataBasics.GetInLooxContext(endPointOdata, token);

                // get current user, also test for connection
                CurrentUser = await GetCurrentContact();
            }
            catch
            {
                // token invalid
                IsConnected = false;
                return IsConnected;
            }

            IsConnected = true;
            return IsConnected;
        }

        private async Task<Contact> GetCurrentContact()
        {
            var userRequest = _context.contact.getauthenticated();
            var users = await userRequest.ExecuteAsync();
            return users.First();
        }

        public async Task<IEnumerable<WorkPackageView>> GetMyTasks()
        {
            if (!IsConnected) { 
                return null;
            }

            // query all tasks assigned to the currently authenticated user
            var taskQuery = _context.workpackageview
                .Where(t => t.ContactId == CurrentUser.ContactId)
                .ToDataServiceQuery();

            var tasks = await taskQuery.ExecuteAsync();
            return tasks;
        }
    }
}
