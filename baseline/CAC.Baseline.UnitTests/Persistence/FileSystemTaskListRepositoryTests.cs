using System;
using System.IO;
using CAC.Baseline.Web.Persistence;
using CAC.Core.Infrastructure.Persistence;
using CAC.Core.TestUtilities;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Persistence
{
    [TestFixture]
    [IntegrationTest]
    public sealed class FileSystemTaskListRepositoryTests : TaskListRepositoryTests
    {
        private readonly string storageDir; 
        
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

        public FileSystemTaskListRepositoryTests()
        {
            storageDir = new(Path.Join(TestContext.CurrentContext.TestDirectory, Guid.NewGuid().ToString()));
            Testee = new FileSystemTaskListRepository(Options.Create(new FileSystemStoragePersistenceOptions { BaseDir = storageDir }));
        }

        protected override ITaskListRepository Testee { get; }
    }
}
