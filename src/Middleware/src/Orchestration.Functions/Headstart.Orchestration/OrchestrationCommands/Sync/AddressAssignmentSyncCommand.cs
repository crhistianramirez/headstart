﻿using System;
using System.Threading.Tasks;
using Headstart.Common.Exceptions;
using Headstart.Common.Helpers;
using Headstart.Common.Models;
using Newtonsoft.Json.Linq;
using Headstart.Common.Queries;
using Headstart.Models;
using OrderCloud.SDK;
using Headstart.Common;

namespace Headstart.Orchestration
{
    public class AddressAssignmentSyncCommand : SyncCommand, IWorkItemCommand
    {
        private readonly IOrderCloudClient _oc;
        public AddressAssignmentSyncCommand(AppSettings settings, LogQuery log, IOrderCloudClient oc) : base(settings, oc, log)
        {
            _oc = oc;
        }

        public async Task<JObject> CreateAsync(WorkItem wi)
        {
            var obj = wi.Current.ToObject<HSAddressAssignment>();
            try
            {
                await _oc.Addresses.SaveAssignmentAsync(wi.ResourceId, obj, wi.Token);
                return JObject.FromObject(obj);
            }
            catch (OrderCloudException exId) when (IdExists(exId))
            {
                // handle 409 errors by refreshing cache
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateExistsError,
                    Message = exId.Message,
                    Level = LogLevel.Error
                });
                return await GetAsync(wi);
            }
            catch (OrderCloudException ex)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateGeneralError,
                    Message = ex.Message,
                    Level = LogLevel.Error
                });
                throw new Exception(OrchestrationErrorType.CreateGeneralError.ToString(), ex);
            }
            catch (Exception e)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.CreateGeneralError,
                    Message = e.Message,
                    Level = LogLevel.Error
                });
                throw new Exception(OrchestrationErrorType.CreateGeneralError.ToString(), e);
            }
        }

        public async Task<JObject> UpdateAsync(WorkItem wi)
        {
            var obj = wi.Current.ToObject<HSAddressAssignment>(OrchestrationSerializer.Serializer);
            try
            {
                await _oc.Addresses.SaveAssignmentAsync(wi.ResourceId, obj, wi.Token);
                return JObject.FromObject(obj);
            }
            catch (OrderCloudException ex)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.UpdateGeneralError,
                    Message = ex.Message,
                    Level = LogLevel.Error
                });
                throw new Exception(OrchestrationErrorType.UpdateGeneralError.ToString(), ex);
            }
        }

        public async Task<JObject> PatchAsync(WorkItem wi)
        {
            var obj = wi.Current.ToObject<AddressAssignment>(OrchestrationSerializer.Serializer);
            try
            {
                await _oc.Addresses.SaveAssignmentAsync(wi.ResourceId, obj, wi.Token);
                return JObject.FromObject(obj);
            }
            catch (OrderCloudException ex)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.PatchGeneralError,
                    Message = ex.Message,
                    Level = LogLevel.Error
                });
                throw new Exception(OrchestrationErrorType.PatchGeneralError.ToString(), ex);
            }
        }

        public Task<JObject> DeleteAsync(WorkItem wi)
        {
            throw new NotImplementedException();
        }

        public async Task<JObject> GetAsync(WorkItem wi)
        {
            try
            {
                var response = await _oc.Addresses.ListAssignmentsAsync(wi.ResourceId, addressID: wi.RecordId, wi.Token);
                return JObject.FromObject(response);
            }
            catch (OrderCloudException ex)
            {
                await _log.Save(new OrchestrationLog(wi)
                {
                    ErrorType = OrchestrationErrorType.GetGeneralError,
                    Message = ex.Message,
                    Level = LogLevel.Error
                });
                throw new Exception(OrchestrationErrorType.GetGeneralError.ToString(), ex);
            }
        }
    }
}
