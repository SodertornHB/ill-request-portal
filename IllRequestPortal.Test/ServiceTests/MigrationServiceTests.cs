
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using IllRequestPortal.Logic.DataAccess;
using IllRequestPortal.Logic.Model;
using IllRequestPortal.Logic.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace IllRequestPortal.Test
{    
    public class MigrationServiceTests
    {
        private Mock<ILogger<MigrationService>> loggerMock;
        private Mock<IMigrationDataAccess> dataAccessMock;

        [SetUp]
        public void Setup()
        {
            loggerMock = new Mock<ILogger<MigrationService>>();
            dataAccessMock = new Mock<IMigrationDataAccess>();
        }

        [Test]
        public void Initiate_SholdNotBeNull()
        {
            var sut = new MigrationService(loggerMock.Object, dataAccessMock.Object);
            Assert.That(sut,!Is.Null);
        }
        
        [Test]
        public async Task GetMigration_SholdNotBeNull()
        {
             
        var id3 = 3;
            dataAccessMock.Setup(x => x.Get(It.IsAny<int>())).Returns(Task.FromResult(new Migration { Id = id3 }));
            var sut = new MigrationService(loggerMock.Object, dataAccessMock.Object);
            var result = await sut.Get(id3);
            Assert.That(sut, !Is.Null);
            Assert.That(id3, Is.EqualTo( result.Id));
            loggerMock.VerifyLoggingExact(LogLevel.Information, $"Fetching entity with id {id3} from data source.");
        }

        [Test]
        public async Task GetAllMigrations_SholdContainEntities()
        {
             
            var id1 = 1;
            var id2 = 2;
            var id3 = 3;
            var testData = new List<Migration>
            {
                new Migration{ Id = id1 },
                new Migration{ Id = id2 },
                new Migration{ Id = id3 }
            };
            dataAccessMock.Setup(x => x.GetAll()).Returns(Task.FromResult(testData.AsEnumerable()));
            var sut = new MigrationService(loggerMock.Object, dataAccessMock.Object);
            var result = await sut.GetAll();
            Assert.That(3, Is.EqualTo( result.Count()));
            Assert.That(id1, Is.EqualTo(result.First().Id));
            Assert.That(id3, Is.EqualTo(result.Last().Id));
            loggerMock.VerifyLoggingExact(LogLevel.Information, "Fetching all entities from data source.");
        }

        [Test]
        public async Task InsertMigration_VerifyInsertIsCalled()
        {
             
            var id1 = 1;
                    var sut = new MigrationService(loggerMock.Object, dataAccessMock.Object);
            await sut.Insert(new Migration { Id = id1 });
            dataAccessMock.Verify(x => x.Insert(It.Is<Migration>(y => y.Id == id1)));
            loggerMock.VerifyLoggingContains(LogLevel.Information, "Saving entity");
        }

        [Test]
        public async Task UpdateMigration_VerifyUpdateIsCalled()
        {
             
        var id1 = 1;
            var sut = new MigrationService(loggerMock.Object, dataAccessMock.Object);
            await sut.Update(new Migration { Id = id1 });
            dataAccessMock.Verify(x => x.Update(It.Is<Migration>(y => y.Id == id1)));
            loggerMock.VerifyLoggingContains(LogLevel.Information, "Update entity");
        }

        [Test]
        public async Task DeleteMigration_VerifyDeleteIsCalled()
        {
             
            var id1 = 1;
            var sut = new MigrationService(loggerMock.Object, dataAccessMock.Object);
            await sut.Delete(id1);
            dataAccessMock.Verify(x => x.Delete(It.Is<int>(y => y == id1)));
            loggerMock.VerifyLoggingExact(LogLevel.Information, $"Deleting entity with id {id1} from data source.");
        }
    }
}
