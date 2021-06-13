using System;
using System.IO;
using CAC.Core.Infrastructure.Persistence;
using CAC.Core.TestUtilities;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Infrastructure.TaskLists;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CAC.CQS.UnitTests.Infrastructure.TaskLists
{
    [TestFixture]
    [IntegrationTest]
    public sealed class FileSystemTaskListRepositoryTests : TaskListRepositoryTests
    {
        [SetUp]
        public void SetUp()
        {
            _ = Directory.CreateDirectory(storageDir);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(storageDir, true);
        }

        private readonly string storageDir;

        public FileSystemTaskListRepositoryTests()
        {
            storageDir = new(Path.Join(TestContext.CurrentContext.TestDirectory, Guid.NewGuid().ToString()));
            Testee = new FileSystemTaskListRepository(Options.Create(new FileSystemStoragePersistenceOptions { BaseDir = storageDir }), DomainEventPublisher);
        }

        protected override ITaskListRepository Testee { get; }
    }
}