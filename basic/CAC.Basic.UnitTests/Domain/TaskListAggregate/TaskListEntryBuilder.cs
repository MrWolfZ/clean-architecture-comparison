﻿using System.Threading;
using CAC.Basic.Domain.TaskListAggregate;

namespace CAC.Basic.UnitTests.Domain.TaskListAggregate
{
    public sealed record TaskListEntryBuilder
    {
        private static long taskListEntryIdCounter;

        public TaskListEntryBuilder()
        {
            Description = $"task {Id.NumericValue}";
        }

        public TaskListEntryId Id { get; init; } = Interlocked.Increment(ref taskListEntryIdCounter);

        public string Description { get; init; }
        
        public bool IsDone { get; init; }

        public TaskListEntry Build() => TaskListEntry.FromRawData(Id, Description, IsDone);
    }
}