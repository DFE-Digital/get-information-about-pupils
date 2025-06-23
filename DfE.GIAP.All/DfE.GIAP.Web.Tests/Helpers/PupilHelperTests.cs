using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Search;
using DfE.GIAP.Web.Helpers.Search;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers
{
    public class PupilHelperTests
    {

        #region CheckIfStarredPupil - string
        [Fact]
        public void CheckIfStarredPupil_enter_null_return_false()
        {
            // Arrange
            string inputData = null;

            // Act
            var acting = PupilHelper.CheckIfStarredPupil(inputData);

            // Assert
            Assert.False(acting);
        }

        [Fact]
        public void CheckIfStarredPupil_enter_empty_return_false()
        {
            // Arrange
            string inputData = string.Empty;

            // Act
            var acting = PupilHelper.CheckIfStarredPupil(inputData);

            // Assert
            Assert.False(acting);
        }

        [Fact]
        public void CheckIfStarredPupil_enter_equals_return_true()
        {
            // Arrange
            string inputData = Global.EncodedSuffixMarker;

            // Act
            var acting = PupilHelper.CheckIfStarredPupil(inputData);

            // Assert
            Assert.True(acting);
        }

        #endregion

        #region CheckIfStarredPupil - string[]
        [Fact]
        public void CheckIfStarredPupil_Array_enter_empty_return_false()
        {
            // Arrange
            var inputData = new string[] { };

            // Act
            var acting = PupilHelper.CheckIfStarredPupil(inputData);

            // Assert
            Assert.False(acting);
        }

        [Fact]
        public void CheckIfStarredPupil_Array_enter_null_return_false()
        {
            // Arrange
            string[] inputData = null;

            // Act
            var acting = PupilHelper.CheckIfStarredPupil(inputData);

            // Assert
            Assert.False(acting);
        }

        [Fact]
        public void CheckIfStarredPupil_Array_enter_equals_return_true()
        {
            // Arrange
            var inputData = new string[] { Global.EncodedSuffixMarker };

            // Act
            var acting = PupilHelper.CheckIfStarredPupil(inputData);

            // Assert
            Assert.True(acting);
        }

        [Fact]
        public void CheckIfStarredPupil_Array_enter_sequence_equals_return_true()
        {
            // Arrange
            var inputData = new string[] { " ", "", string.Empty, "!!", Global.EncodedSuffixMarker };

            // Act
            var acting = PupilHelper.CheckIfStarredPupil(inputData);

            // Assert
            Assert.True(acting);
        }

        #endregion

        [Theory]
        [InlineData("<span style='display:none'>1</span>", SearchErrorMessages.EnterUPNs)]
        [InlineData("You have not entered any UPNs", SearchErrorMessages.EnterUPNs)]
        [InlineData("<span style='display:none'>2</span>", SearchErrorMessages.TooManyUPNs)]
        [InlineData("UPNs have been entered, please review and reduce to the maximum of", SearchErrorMessages.TooManyUPNs)]
        [InlineData("<span style='display:none'>3</span>", SearchErrorMessages.UPNLength)]
        [InlineData("<span style='display:none'>4</span>", SearchErrorMessages.UPNFormat)]
        [InlineData("<span style='display:none'>5</span>", SearchErrorMessages.UPNMustBeUnique)]
        [InlineData("The following UPN(s) are duplicated", SearchErrorMessages.UPNMustBeUnique)]
        public void GenerateValidationMessageUpnSearch_CheckOutcome(string inputData, string outputValue)
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("test", inputData);

            // Act
            var acting = PupilHelper.GenerateValidationMessageUpnSearch(modelState);

            // Assert
            Assert.Equal(outputValue, acting);
        }

        [Theory]
        [InlineData("<span style='display:none'>1</span>", SearchErrorMessages.EnterULNs)]
        [InlineData("You have not entered any ULNs", SearchErrorMessages.EnterULNs)]
        [InlineData("<span style='display:none'>2</span>", SearchErrorMessages.TooManyULNs)]
        [InlineData("ULNs have been entered, please review and reduce to the maximum of", SearchErrorMessages.TooManyULNs)]
        [InlineData("<span style='display:none'>3</span>", SearchErrorMessages.ULNLength)]
        [InlineData("<span style='display:none'>4</span>", SearchErrorMessages.ULNFormat)]

        public void GenerateValidationMessageUlnSearch_CheckOutcome(string inputData, string outputValue)
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("test", inputData);

            // Act
            var acting = PupilHelper.GenerateValidationMessageUlnSearch(modelState);

            // Assert
            Assert.Equal(outputValue, acting);
        }

        [Theory]
        [InlineData("31/12/2002", 31, 12, 2002)]
        [InlineData("2002", 0, 0, 2002)]
        [InlineData("12/2002", 0, 12, 2002)]
        [InlineData("", 0, 0, 0)]
        public void ConvertFilterNameToCustomDOBFilterText_CheckOutcome(string dobValue, int day, int month, int year)
        {
            // Arrange

            // Act
            PupilHelper.ConvertFilterNameToCustomDOBFilterText(dobValue, out int dayOut, out int monthOut, out int yearOut);

            // Assert
            Assert.Equal(day, dayOut);
            Assert.Equal(month, monthOut);
            Assert.Equal(year, yearOut);
        }
    }
}
