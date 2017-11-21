using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BgTaxi.Models.Models;
using System.Collections.Generic;
using System.Data.Entity;
using Moq;
using System.Linq;
using BgTaxi.Services;

namespace BgTaxi.Tests.Services
{
    [TestClass]
    public class AccessTokenServiceTests
    {
        [TestMethod]
        public void AccessToken_AddAnAccessToken_ShouldAddCorrectly()
        {
            var data = new List<AccessToken>().AsQueryable();

            var mock = new Mock<DbSet<AccessToken>>();
               var mockContext = new Mock<Models.Models.Database>();
            mockContext.Setup(m => m.AccessTokens).Returns(mock.Object);

            AccessTokenService accTokService = new AccessTokenService(mockContext.Object);
            AccessToken accToken = new AccessToken()
            {
                Device = new Device() { Id = Guid.NewGuid(), LastRequestDateTime = DateTime.Now },
                UniqueAccesToken = Guid.NewGuid().ToString("D"),
                CreatedDateTime = DateTime.Now
            };
            accTokService.AddAccessToken(accToken);

            mock.Verify(m => m.Add(It.IsAny<AccessToken>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());


        }

        [TestMethod]
        public void AccessToken_GetAll_ShouldContainsAll()
        {
            var data = new List<AccessToken>()
            {
                new AccessToken () {
                Device = new Device() { Id = Guid.NewGuid(), LastRequestDateTime = DateTime.Now },
                UniqueAccesToken = Guid.NewGuid().ToString("D"),
                CreatedDateTime = DateTime.Now
            },
                new AccessToken () {
                Device = new Device() { Id = Guid.NewGuid(), LastRequestDateTime = DateTime.Now },
                UniqueAccesToken = Guid.NewGuid().ToString("D"),
                CreatedDateTime = DateTime.Now
            },
                new AccessToken () {
                Device = new Device() { Id = Guid.NewGuid(), LastRequestDateTime = DateTime.Now },
                UniqueAccesToken = Guid.NewGuid().ToString("D"),
                CreatedDateTime = DateTime.Now
            }

        }.AsQueryable();

            var mockSet = new Mock<DbSet<AccessToken>>();
            mockSet.As<IQueryable<AccessToken>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<AccessToken>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<AccessToken>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<AccessToken>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<Models.Models.Database>();
            mockContext.Setup(c => c.AccessTokens).Returns(mockSet.Object);

            AccessTokenService accTokService = new AccessTokenService(mockContext.Object);
            Assert.AreEqual(3, accTokService.GetAll().Count());
        }

        [TestMethod]
        public void AccessToken_IsUserLoggedIn_ShouldReturnTrue()
        {
            
            var userId = Guid.NewGuid().ToString("D");
            var id = Guid.NewGuid();
            var accessTokenString = Guid.NewGuid().ToString("D");
            var dataDevices = new List<Device>()
            {
               new Device()
               {
                    Id = id,
                     UserId = userId
               }
            }.AsQueryable();

            var dataAccessTokens = new List<AccessToken>()
            {
                new AccessToken () {
                Device = new Device() {  Id = id,  UserId = userId },
                UniqueAccesToken = accessTokenString,
                CreatedDateTime = DateTime.Now
                
            }
            }.AsQueryable();
            var mockSetAccessTokens = new Mock<DbSet<AccessToken>>();
            mockSetAccessTokens.As<IQueryable<AccessToken>>().Setup(m => m.Provider).Returns(dataAccessTokens.Provider);
            mockSetAccessTokens.As<IQueryable<AccessToken>>().Setup(m => m.Expression).Returns(dataAccessTokens.Expression);
            mockSetAccessTokens.As<IQueryable<AccessToken>>().Setup(m => m.ElementType).Returns(dataAccessTokens.ElementType);
            mockSetAccessTokens.As<IQueryable<AccessToken>>().Setup(m => m.GetEnumerator()).Returns(dataAccessTokens.GetEnumerator());

            var mockSetDevices = new Mock<DbSet<Device>>();
            mockSetDevices.As<IQueryable<Device>>().Setup(m => m.Provider).Returns(dataDevices.Provider);
            mockSetDevices.As<IQueryable<Device>>().Setup(m => m.Expression).Returns(dataDevices.Expression);
            mockSetDevices.As<IQueryable<Device>>().Setup(m => m.ElementType).Returns(dataDevices.ElementType);
            mockSetDevices.As<IQueryable<Device>>().Setup(m => m.GetEnumerator()).Returns(dataDevices.GetEnumerator());

            var mockContext = new Mock<Models.Models.Database>();
            mockContext.Setup(c => c.AccessTokens).Returns(mockSetAccessTokens.Object);
            mockContext.Setup(c => c.Devices).Returns(mockSetDevices.Object);

            AccessTokenService accTokService = new AccessTokenService(mockContext.Object);
           
            Assert.AreEqual(true, accTokService.IsUserLoggedIn(accessTokenString));
        }
    }
}
