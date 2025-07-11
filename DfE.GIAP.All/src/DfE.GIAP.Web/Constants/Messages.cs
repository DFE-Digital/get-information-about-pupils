﻿namespace DfE.GIAP.Web.Constants;

public static class Messages
{
    public static class Common
    {
        public static class Errors
        {
            public const string NoAdminSelection = "You have not selected an admin option";
            public const string NoOrganisationSelection = "You have not selected an organisation option";
            public const string NoConfirmationSelection = "You have not selected a confirmation option";
            public const string NoContinueSelection = "Please select an option";
            public const string MyPupilListLimitExceeded = "You have reached the limit for my pupil list, please contact the Department for Education to increase this";
            public const string TitleRequired = "Title must be entered";
            public const string BodyRequired = "Body must be entered";
            public const string DocumentRequired = "Please select a document";
            public const string NewsArticleRequired = "Please select a news article";
            public const string SecurityReportRequired = "A report must be selected";
            public const string TitleLength = "Title must be must be 64 characters or fewer";
            public const string NoPupilsSelected = "You have not selected any pupils";
        }
    }

    public static class NewsArticle
    {
        public static class Success
        {
            public const string DeleteTitle = "News article was deleted successfully";
            public const string DeleteBody = "Your news article was deleted successfully and your change will be reflected immediately";
            public const string PublishTitle = "News article was published successfully";
            public const string PublishBody = "Your news article was published successfully and your change will be reflected immediately";
            public const string CreateTitle = "News article was created successfully";
            public const string CreateBody = "Your news article was created successfully and your change will be reflected immediately";
            public const string UpdateTitle = "News article was updated successfully";
            public const string UpdateBody = "Your news article was updated successfully and your change will be reflected immediately";
            public const string DocumentUpdatedTitle = "Document was updated successfully";
            public const string DocumentUpdatedBody = "Your document was updated successfully and your change will be reflected immediately";
        }

        public static class Errors
        {
            public const string SaveDraftError = "We have been unable to save the draft article.";
            public const string PublishError = "We have been unable to publish the article.";
            public const string CreatedError = "We have been unable to create the article.";
            public const string UpdatedError = "We have been unable to update the article.";
        }
    }

    public static class Search
    {
        public static class Errors
        {
            public const string DobInvalid = "The date of birth field is invalid";
            public const string FilterEmpty = "You have not entered any text";
            public const string SelectOneOrMoreDataTypes = "Select one or more data types";
            public const string SelectFileType = "Select the file type to download";
            public const string EnterUPNs = "Enter one or more UPNs";
            public const string EnterULNs = "Enter one or more ULNs";
            public const string TooManyUPNs = "Too many UPNs";
            public const string TooManyULNs = "Too many ULNs";
            public const string UPNLength = "The UPN must be 13 characters";
            public const string UPNFormat = "The UPN must be in the correct format";
            public const string UPNMustBeUnique = "The ULN must be unique";
            public const string ULNLength = "The ULN must be 10 digits";
            public const string ULNFormat = "The ULN must be in the correct format";
        }
    }

    public static class Downloads
    {
        public static class Errors
        {
            public const string UPNLimitExceeded = "A CTF download can only have up to 500 pupils. You have selected more than 500 pupils, remove some pupils and try again";
            public const string NoDataForDownload = "No results found";
            public const string NoDataForSelectedPupils = "No pupil data of the requested type exists for the selected pupils";
            public const string NoPupilSelected = "No pupil was selected to download";
            public const string NoLearnerSelected = "No learner was selected to download";
            public const string NoDataForOrganisationDownload = "No data exists for the report you have selected";
        }
    }
}
