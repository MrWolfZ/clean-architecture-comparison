﻿using System.IO;
using CAC.Core.Infrastructure.Persistence;
using CAC.Core.TestUtilities;
using CAC.DDD.Web.Persistence;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CAC.DDD.UnitTests.Persistence
{
    [TestFixture]
    [IntegrationTest]
    [Parallelizable(ParallelScope.None)]
    public sealed class FileSystemTaskListRepositoryTests : TaskListRepositoryTests
    {
        [SetUp]
        public void SetUp()
        {
            StorageDir.Create();
        }

        [TearDown]
        public void TearDown()
        {
            StorageDir.Delete(true);
        }

        private static readonly DirectoryInfo StorageDir = new DirectoryInfo(Path.Join(TestContext.CurrentContext.TestDirectory, nameof(FileSystemTaskListRepositoryTests)));

        protected override ITaskListRepository Testee { get; } = new FileSystemTaskListRepository(Options.Create(new FileSystemStoragePersistenceOptions { BaseDir = StorageDir.FullName }));
    }
}
