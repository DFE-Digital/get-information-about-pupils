using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Validation;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace DfE.GIAP.Web.Helpers.SearchDownload
{
    public static class SearchDownloadHelper
    {
        /// <summary>
        /// Disables specific download data types in the model.
        /// </summary>
        public static void DisableDownloadDataTypes<TDownloadDataType>(
            LearnerDownloadViewModel model, IEnumerable<TDownloadDataType> downloadDataTypes) where TDownloadDataType : Enum
        {
            foreach (var downloadDataType in downloadDataTypes)
            {
                var searchDownloadDataType = model.SearchDownloadDatatypes
                    .SingleOrDefault(dataType => dataType.Value.Equals(downloadDataType.ToString()));

                if (searchDownloadDataType is not null)
                {
                    searchDownloadDataType.Disabled = true;
                }
            }
        }

        /// <summary>
        /// Adds download data types to the model based on user permissions and organization age bounds.
        /// </summary>
        public static void AddDownloadDataTypes(
            LearnerDownloadViewModel model,
            ClaimsPrincipal user,
            int organisationLowAge,
            int organisationHighAge,
            bool isOrganisationLocalAuthority,
            bool isOrganisationAllAges)
        {
            SetSearchDownloadDatatypesForModel(model);

            foreach (var downloadType in Enum.GetValues(typeof(DownloadDataType)).Cast<DownloadDataType>())
            {
                bool canDownload =
                    HasRequiredDownloadAccess(user, isOrganisationLocalAuthority, isOrganisationAllAges) ||
                    IsBetweenRequiredAgeBoundsForDownload(downloadType, organisationLowAge, organisationHighAge);

                AddSearchDownloadDataTypeToModel(model, downloadType, canDownload);
            }
        }

        /// <summary>
        /// Adds ULN-specific download data types to the model based on user permissions and organization high age.
        /// </summary>
        public static void AddUlnDownloadDataTypes(
            LearnerDownloadViewModel model,
            ClaimsPrincipal user,
            int organisationHighAge,
            bool isDfeUser)
        {
            foreach (var downloadType in Enum.GetValues(typeof(DownloadUlnDataType)).Cast<DownloadUlnDataType>())
            {
                bool canDownload =
                    user.IsAdmin() ||
                    DownloadValidation.ValidateUlnDownloadDataType(downloadType, organisationHighAge) ||
                    isDfeUser;

                AddSearchDownloadDataTypeToModel(model, downloadType, canDownload);
            }
        }

        /// <summary>
        /// Prepares a file for download by returning an IActionResult.
        /// </summary>
        public static IActionResult DownloadFile(ReturnFile downloadFile)
        {
            string contentType = GetDownloadContentType(downloadFile);

            return new FileContentResult(downloadFile.Bytes, contentType)
            {
                FileDownloadName = downloadFile.FileName
            };
        }

        /// <summary>
        /// Initializes the model's search download data types with default values.
        /// </summary>
        private static void SetSearchDownloadDatatypesForModel(LearnerDownloadViewModel model)
        {
            model.SearchDownloadDatatypes = Enum.GetValues(typeof(AllSchoolsDownloadDataType))
                .Cast<AllSchoolsDownloadDataType>()
                .Select(downloadType => new SearchDownloadDataType
                {
                    Name = StringHelper.StringValueOfEnum(downloadType),
                    Value = downloadType.ToString(),
                    CanDownload = true
                })
                .ToList();
        }

        /// <summary>
        /// Adds a specific download data type to the model.
        /// </summary>
        private static void AddSearchDownloadDataTypeToModel<TDownloadType>(
            LearnerDownloadViewModel model, TDownloadType downloadType, bool canDownload = true) where TDownloadType : Enum
        {
            model.SearchDownloadDatatypes.Add(new SearchDownloadDataType
            {
                Name = StringHelper.StringValueOfEnum(downloadType),
                Value = downloadType.ToString(),
                CanDownload = canDownload
            });
        }

        /// <summary>
        /// Checks if the user has the required access to download data.
        /// </summary>
        private static bool HasRequiredDownloadAccess(
            ClaimsPrincipal user,
            bool isOrganisationLocalAuthority,
            bool isOrganisationAllAges)
        {
            return user.IsAdmin() || isOrganisationLocalAuthority || isOrganisationAllAges;
        }

        /// <summary>
        /// Validates if the download data type is within the required age bounds.
        /// </summary>
        private static bool IsBetweenRequiredAgeBoundsForDownload(
            DownloadDataType downloadDataType,
            int organisationLowAge,
            int organisationHighAge)
        {
            return DownloadValidation.ValidateDownloadDataType(downloadDataType, organisationLowAge, organisationHighAge);
        }

        /// <summary>
        /// Determines the content type for the file being downloaded.
        /// </summary>
        private static string GetDownloadContentType(ReturnFile downloadFile)
        {
            return downloadFile.FileType == FileType.ZipFile
                ? FileType.ZipContentType
                : $"text/{downloadFile.FileType}";
        }
    }
}