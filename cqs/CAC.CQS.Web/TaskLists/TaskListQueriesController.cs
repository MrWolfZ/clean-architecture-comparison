﻿using System.Net;
using System.Threading.Tasks;
using CAC.Core.Application;
using CAC.CQS.Application.TaskLists.GetAllTaskLists;
using CAC.CQS.Application.TaskLists.GetAllTaskListsWithPendingEntries;
using CAC.CQS.Application.TaskLists.GetTaskListByIdQuery;
using Microsoft.AspNetCore.Mvc;

namespace CAC.CQS.Web.TaskLists
{
    [ApiController]
    [Route("taskLists")]
    public class TaskListQueriesController : ControllerBase
    {
        private readonly IQueryHandler<GetAllTaskListsQuery, GetAllTaskListsQueryResponse> getAllTaskListsQueryHandler;
        private readonly IQueryHandler<GetAllTaskListsWithPendingEntriesQuery, GetAllTaskListsWithPendingEntriesQueryResponse> getAllTaskListsWithPendingEntriesQueryHandler;
        private readonly IQueryHandler<GetTaskListByIdQuery, GetTaskListByIdQueryResponse> getTaskListByIdQueryHandler;

        public TaskListQueriesController(IQueryHandler<GetAllTaskListsQuery, GetAllTaskListsQueryResponse> getAllTaskListsQueryHandler,
                                         IQueryHandler<GetTaskListByIdQuery, GetTaskListByIdQueryResponse> getTaskListByIdQueryHandler,
                                         IQueryHandler<GetAllTaskListsWithPendingEntriesQuery, GetAllTaskListsWithPendingEntriesQueryResponse> getAllTaskListsWithPendingEntriesQueryHandler)
        {
            this.getAllTaskListsQueryHandler = getAllTaskListsQueryHandler;
            this.getTaskListByIdQueryHandler = getTaskListByIdQueryHandler;
            this.getAllTaskListsWithPendingEntriesQueryHandler = getAllTaskListsWithPendingEntriesQueryHandler;
        }

        [HttpPost("getAllTaskLists")]
        public async Task<GetAllTaskListsQueryResponse> GetAll(GetAllTaskListsQuery query)
        {
            return await getAllTaskListsQueryHandler.ExecuteQuery(query);
        }

        [HttpPost("getTaskListById")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<GetTaskListByIdQueryResponse> GetById(GetTaskListByIdQuery query)
        {
            return await getTaskListByIdQueryHandler.ExecuteQuery(query);
        }

        [HttpPost("getAllTaskListsWithPendingEntries")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<GetAllTaskListsWithPendingEntriesQueryResponse> GetAllWithPendingEntries(GetAllTaskListsWithPendingEntriesQuery query)
        {
            return await getAllTaskListsWithPendingEntriesQueryHandler.ExecuteQuery(query);
        }
    }
}