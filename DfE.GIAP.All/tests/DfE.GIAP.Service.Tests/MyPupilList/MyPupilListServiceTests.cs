using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.MPL;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Service.Tests.MyPupilList
{
    public class MyPupilListServiceTests
    {
        [Fact]
        public async Task GetMyPupilListLearnerNumbers_returns_numbers_with_blanks_filtered()
        {
            // arrange
            var commonService = Substitute.For<ICommonService>();
            var dodgyData = new UserProfile()
            {
                UserId = "testUser",
                MyPupilList = new List<MyPupilListItem>
                {
                    new MyPupilListItem("UPN", false),
                    new MyPupilListItem("0", false),
                    new MyPupilListItem("", false),
                }
            };
            commonService.GetUserProfile(Arg.Any<UserProfile>()).Returns(dodgyData);

            var sut = new MyPupilListService(commonService);

            // act

            var pupilList = await sut.GetMyPupilListLearnerNumbers("testUser");

            // assert
            Assert.Single(pupilList);
            await commonService.Received().GetUserProfile(Arg.Is<UserProfile>(u => u.UserId.Equals("testUser")));
        }

        [Fact]
        public async Task GetMyPupilListLearnerNumbers_returns_empty_list_if_PupilList_null()
        {
            // arrange
            var commonService = Substitute.For<ICommonService>();
            var dodgyData = new UserProfile()
            {
                UserId = "testUser",
                PupilList = null
            };
            commonService.GetUserProfile(Arg.Any<UserProfile>()).Returns(dodgyData);

            var sut = new MyPupilListService(commonService);

            // act

            var pupilList = await sut.GetMyPupilListLearnerNumbers("testUser");

            // assert
            Assert.NotNull(pupilList);
        }

        [Fact]
        public async Task UpdateMyPupilList_carries_variables_correctly()
        {
            // arrange
            var commonService = Substitute.For<ICommonService>();

            var sut = new MyPupilListService(commonService);

            var list = new List<MyPupilListItem>();
            list.Add(new MyPupilListItem("test", false));

            var headerDeets = new AzureFunctionHeaderDetails();

            // act
            await sut.UpdateMyPupilList(list, "testUser", headerDeets);

            // assert
            await commonService.Received().CreateOrUpdateUserProfile(
                Arg.Is<UserProfile>(u => u.UserId.Equals("testUser") && u.MyPupilList.SequenceEqual(list.ToArray()) && u.IsPupilListUpdated),
                Arg.Is<AzureFunctionHeaderDetails>(headerDeets));
        }

        [Fact]
        public async Task GetMyPupilListLearnerNumbers_return_emptylist_on_null_pupilList()
        {
            // arrange
            var commonService = Substitute.For<ICommonService>();
            var dodgyData = new UserProfile()
            {
                UserId = "testUser",
                MyPupilList = null
            };
            commonService.GetUserProfile(Arg.Any<UserProfile>()).Returns(dodgyData);

            var sut = new MyPupilListService(commonService);

            // act

            var pupilList = await sut.GetMyPupilListLearnerNumbers("testUser");

            // assert
            Assert.NotNull(pupilList);
        }

        [Fact]
        public async Task GetMyPupilListLearnerNumbers_return_emptylist_on_null_userProfile()
        {
            // arrange
            var commonService = Substitute.For<ICommonService>();
            UserProfile dodgyData = null;

            commonService.GetUserProfile(Arg.Any<UserProfile>()).Returns(dodgyData);

            var sut = new MyPupilListService(commonService);

            // act

            var pupilList = await sut.GetMyPupilListLearnerNumbers("testUser");

            // assert
            Assert.NotNull(pupilList);
        }

        [Fact]
        public async Task UpdatePupilMasks_UpdateListCorrectly()
        {
            // arrange
            var originalMPL = new List<MyPupilListItem>();
            originalMPL.Add(new MyPupilListItem("1234567890", false));
            originalMPL.Add(new MyPupilListItem("2345678901", false));
            originalMPL.Add(new MyPupilListItem("3456789012", false));
            originalMPL.Add(new MyPupilListItem("4567890123", false));


            var expectedMPL = new List<MyPupilListItem>();
            expectedMPL.Add(new MyPupilListItem("1234567890", true));
            expectedMPL.Add(new MyPupilListItem("2345678901", false));
            expectedMPL.Add(new MyPupilListItem("3456789012", true));
            expectedMPL.Add(new MyPupilListItem("4567890123", false));

            var inputList = new List<string>();
            inputList.Add("1234567890");
            inputList.Add("3456789012");


            var commonService = Substitute.For<ICommonService>();
            var happyProfile = new UserProfile
            {
                UserId = "Nigel Testersmith",
                MyPupilList = originalMPL
            };

            commonService.GetUserProfile(Arg.Any<UserProfile>()).Returns(happyProfile);

            var headerDeets = new AzureFunctionHeaderDetails();

            var sut = new MyPupilListService(commonService);

            // act

            await sut.UpdatePupilMasks(inputList, true, happyProfile.UserId, headerDeets);

            // assert
            await commonService.Received().CreateOrUpdateUserProfile(
                Arg.Is<UserProfile>(
                    u => u.UserId.Equals("Nigel Testersmith")
                    && u.MyPupilList.SequenceEqual(expectedMPL)
                    && u.IsPupilListUpdated),
                Arg.Is<AzureFunctionHeaderDetails>(headerDeets));
        }
    }
}
